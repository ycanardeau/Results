using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Aigamo.Results.Analyzers;

/// <summary>
/// Enforces a project-wide choice between implicit and explicit Result construction,
/// selected with the <c>aigamo_results_conversion_style</c> .editorconfig key:
/// <list type="bullet">
/// <item><c>implicit</c> — flags explicit <c>Result.Ok</c>/<c>Result.Error</c> calls that a
/// value could be converted implicitly instead (ARS001).</item>
/// <item><c>explicit</c> — flags reliance on the implicit conversion to <c>Result</c> (ARS002).</item>
/// </list>
/// When the key is unset, neither rule fires.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConversionStyleAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
		ImmutableArray.Create(
			Diagnostics.PreferImplicitConversion,
			Diagnostics.PreferExplicitConversion
		);

	private enum Style
	{
		None,
		Implicit,
		Explicit,
	}

	private sealed class KnownSymbols(INamedTypeSymbol resultStatic, INamedTypeSymbol resultGeneric)
	{
		public INamedTypeSymbol ResultStatic { get; } = resultStatic;
		public INamedTypeSymbol ResultGeneric { get; } = resultGeneric;
	}

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(
			GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
		);

		context.RegisterCompilationStartAction(start =>
		{
			var resultStatic = start.Compilation.GetTypeByMetadataName("Aigamo.Results.Result");
			var resultGeneric = start.Compilation.GetTypeByMetadataName("Aigamo.Results.Result`2");
			if (resultStatic is null || resultGeneric is null)
			{
				return;
			}

			var known = new KnownSymbols(resultStatic, resultGeneric);
			start.RegisterOperationAction(ctx => AnalyzeInvocation(ctx, known), OperationKind.Invocation);
			start.RegisterOperationAction(ctx => AnalyzeConversion(ctx, known), OperationKind.Conversion);
		});
	}

	// ARS001: prefer implicit — an explicit Result.Ok/Result.Error whose value could convert implicitly.
	private static void AnalyzeInvocation(OperationAnalysisContext context, KnownSymbols known)
	{
		if (GetStyle(context.Options, context.Operation.Syntax.SyntaxTree) != Style.Implicit)
		{
			return;
		}

		var invocation = (IInvocationOperation)context.Operation;
		var method = invocation.TargetMethod;
		if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, known.ResultStatic))
		{
			return;
		}

		if (method.Name != "Ok" && method.Name != "Error")
		{
			return;
		}

		if (
			invocation.Type is not INamedTypeSymbol resultType
			|| !SymbolEqualityComparer.Default.Equals(resultType.OriginalDefinition, known.ResultGeneric)
		)
		{
			return;
		}

		// When T == TError the implicit conversion is ambiguous (CS0457), so keep the explicit call.
		if (SymbolEqualityComparer.Default.Equals(resultType.TypeArguments[0], resultType.TypeArguments[1]))
		{
			return;
		}

		if (!TargetTypeMatches(invocation, resultType))
		{
			return;
		}

		var display = method.Name == "Ok" ? "Result.Ok" : "Result.Error";
		context.ReportDiagnostic(
			Diagnostic.Create(
				Diagnostics.PreferImplicitConversion,
				invocation.Syntax.GetLocation(),
				display
			)
		);
	}

	// ARS002: prefer explicit — reliance on the user-defined implicit conversion to Result.
	private static void AnalyzeConversion(OperationAnalysisContext context, KnownSymbols known)
	{
		if (GetStyle(context.Options, context.Operation.Syntax.SyntaxTree) != Style.Explicit)
		{
			return;
		}

		var conversion = (IConversionOperation)context.Operation;
		if (!conversion.Conversion.IsUserDefined || !conversion.Conversion.IsImplicit)
		{
			return;
		}

		if (
			conversion.OperatorMethod is not { } op
			|| !SymbolEqualityComparer.Default.Equals(op.ContainingType.OriginalDefinition, known.ResultGeneric)
		)
		{
			return;
		}

		if (
			conversion.Type is not INamedTypeSymbol resultType
			|| !SymbolEqualityComparer.Default.Equals(resultType.OriginalDefinition, known.ResultGeneric)
		)
		{
			return;
		}

		var isError = SymbolEqualityComparer.Default.Equals(
			op.Parameters[0].Type,
			resultType.TypeArguments[1]
		);
		var display = isError ? "Result.Error" : "Result.Ok";
		context.ReportDiagnostic(
			Diagnostic.Create(
				Diagnostics.PreferExplicitConversion,
				conversion.Operand.Syntax.GetLocation(),
				display
			)
		);
	}

	// True when the call sits in a context whose target type is exactly this Result<T, TError>,
	// so replacing the call with its argument still compiles via the implicit conversion.
	private static bool TargetTypeMatches(IOperation operation, INamedTypeSymbol resultType)
	{
		switch (operation.Parent)
		{
			case IReturnOperation ret:
				return GetEnclosingReturnType(ret) is { } target
					&& SymbolEqualityComparer.Default.Equals(target, resultType);
			case IVariableInitializerOperation { Parent: IVariableDeclaratorOperation decl }:
				return SymbolEqualityComparer.Default.Equals(decl.Symbol.Type, resultType);
			case ISimpleAssignmentOperation assign:
				return SymbolEqualityComparer.Default.Equals(assign.Target.Type, resultType);
			case IArgumentOperation { Parameter: { } parameter }:
				return SymbolEqualityComparer.Default.Equals(parameter.Type, resultType);
			default:
				return false;
		}
	}

	// Conservatively resolves the enclosing method/lambda return type; returns null (skip) for
	// async/Task-returning contexts, where it will not equal the plain Result<T, TError>.
	private static ITypeSymbol? GetEnclosingReturnType(IReturnOperation ret)
	{
		var model = ret.SemanticModel;
		if (model is null)
		{
			return null;
		}

		return model.GetEnclosingSymbol(ret.Syntax.SpanStart) is IMethodSymbol method
			? method.ReturnType
			: null;
	}

	private static Style GetStyle(AnalyzerOptions options, SyntaxTree tree)
	{
		var configOptions = options.AnalyzerConfigOptionsProvider.GetOptions(tree);
		if (configOptions.TryGetValue(Diagnostics.ConfigKey, out var value))
		{
			if (string.Equals(value, "implicit", StringComparison.OrdinalIgnoreCase))
			{
				return Style.Implicit;
			}

			if (string.Equals(value, "explicit", StringComparison.OrdinalIgnoreCase))
			{
				return Style.Explicit;
			}
		}

		return Style.None;
	}
}
