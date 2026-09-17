using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Aigamo.Results.CodeFixes;

/// <summary>
/// Code fixes for the conversion-style diagnostics: ARS001 removes a redundant
/// <c>Result.Ok</c>/<c>Result.Error</c> wrapper (leaving the implicit conversion), and
/// ARS002 wraps a value in the matching <c>Result.Ok</c>/<c>Result.Error</c> factory.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConversionStyleCodeFixProvider))]
[Shared]
public sealed class ConversionStyleCodeFixProvider : CodeFixProvider
{
	private const string PreferImplicitId = "ARS001";
	private const string PreferExplicitId = "ARS002";

	public override ImmutableArray<string> FixableDiagnosticIds { get; } =
		ImmutableArray.Create(PreferImplicitId, PreferExplicitId);

	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context
			.Document.GetSyntaxRootAsync(context.CancellationToken)
			.ConfigureAwait(false);
		if (root is null)
		{
			return;
		}

		foreach (var diagnostic in context.Diagnostics)
		{
			var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

			if (diagnostic.Id == PreferImplicitId)
			{
				if (node.FirstAncestorOrSelf<InvocationExpressionSyntax>() is not { } invocation)
				{
					continue;
				}

				context.RegisterCodeFix(
					CodeAction.Create(
						title: "Use implicit conversion",
						createChangedDocument: ct =>
							RemoveWrapperAsync(context.Document, invocation, ct),
						equivalenceKey: "UseImplicitConversion"
					),
					diagnostic
				);
			}
			else if (diagnostic.Id == PreferExplicitId)
			{
				if (
					(node as ExpressionSyntax ?? node.FirstAncestorOrSelf<ExpressionSyntax>())
					is not { } expression
				)
				{
					continue;
				}

				context.RegisterCodeFix(
					CodeAction.Create(
						title: "Use explicit Result factory",
						createChangedDocument: ct =>
							AddWrapperAsync(context.Document, expression, ct),
						equivalenceKey: "UseExplicitConversion"
					),
					diagnostic
				);
			}
		}
	}

	private static async Task<Document> RemoveWrapperAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken
	)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
		if (root is null || argument is null)
		{
			return document;
		}

		var replacement = argument.Expression.WithTriviaFrom(invocation);
		return document.WithSyntaxRoot(root.ReplaceNode(invocation, replacement));
	}

	private static async Task<Document> AddWrapperAsync(
		Document document,
		ExpressionSyntax expression,
		CancellationToken cancellationToken
	)
	{
		var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (model is null || root is null)
		{
			return document;
		}

		if (
			model.GetTypeInfo(expression, cancellationToken).ConvertedType
				is not INamedTypeSymbol resultType
			|| resultType.TypeArguments.Length != 2
		)
		{
			return document;
		}

		if (model.Compilation.GetTypeByMetadataName("Aigamo.Results.Result") is not { } resultStatic)
		{
			return document;
		}

		var valueType = model.GetTypeInfo(expression, cancellationToken).Type;
		var isError =
			valueType is not null
			&& SymbolEqualityComparer.Default.Equals(valueType, resultType.TypeArguments[1])
			&& !SymbolEqualityComparer.Default.Equals(
				resultType.TypeArguments[0],
				resultType.TypeArguments[1]
			);

		var generator = SyntaxGenerator.GetGenerator(document);
		var typeArguments = new[]
		{
			generator.TypeExpression(resultType.TypeArguments[0]),
			generator.TypeExpression(resultType.TypeArguments[1]),
		};
		var factory = generator.MemberAccessExpression(
			generator.TypeExpression(resultStatic),
			generator.GenericName(isError ? "Error" : "Ok", typeArguments)
		);
		var wrapped = generator
			.InvocationExpression(factory, expression.WithoutTrivia())
			.WithTriviaFrom(expression);

		return document.WithSyntaxRoot(root.ReplaceNode(expression, wrapped));
	}
}
