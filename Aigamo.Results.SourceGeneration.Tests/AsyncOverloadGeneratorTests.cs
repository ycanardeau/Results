using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Aigamo.Results.SourceGeneration.Tests;

public class AsyncOverloadGeneratorTests
{
	// A realistic core that mirrors the library: a Fold helper plus a [GenerateAsyncOverloads]
	// method whose *sibling* branch is a method group (Result.Error). This is the exact shape
	// that regressed — the async variant must lift the method group, not just lambda branches.
	private const string MapSource =
		@"
using System;
using System.Threading.Tasks;

namespace Aigamo.Results
{
    public abstract record Result<T, TError>
    {
        public sealed record Ok(T ResultValue) : Result<T, TError>;
        public sealed record Error(TError ErrorValue) : Result<T, TError>;
    }

    public static class Result
    {
        public static Result<T, TError> Ok<T, TError>(T resultValue) => new Result<T, TError>.Ok(resultValue);
        public static Result<T, TError> Error<T, TError>(TError errorValue) => new Result<T, TError>.Error(errorValue);
    }

    public static partial class ResultExtensions
    {
        public static R Fold<T, TError, R>(this Result<T, TError> source, Func<T, R> onOk, Func<TError, R> onError)
            => source switch
            {
                Result<T, TError>.Ok ok => onOk(ok.ResultValue),
                Result<T, TError>.Error err => onError(err.ErrorValue),
                _ => throw new InvalidOperationException(),
            };

        [GenerateAsyncOverloads]
        public static Result<R, TError> Map<T, TError, R>(this Result<T, TError> source, Func<T, R> mapFunc)
            => source.Fold(
                onOk: resultValue => Result.Ok<R, TError>(mapFunc(resultValue)),
                onError: Result.Error<R, TError>);
    }
}";

	private const string NoAttributeSource =
		@"
namespace Aigamo.Results
{
    public static class Plain { public static int Echo(int x) => x; }
}";

	// A Combine-style core where the selector result is chained into a further call
	// (binder(value).Map(...)). The async lift must parenthesize the awaited selector so
	// the .Map applies to the awaited value, not to the whole awaited chain.
	private const string ChainedSource =
		@"
using System;
using System.Threading.Tasks;

namespace Aigamo.Results
{
    public abstract record Result<T, TError>
    {
        public sealed record Ok(T ResultValue) : Result<T, TError>;
        public sealed record Error(TError ErrorValue) : Result<T, TError>;
    }

    public static class Result
    {
        public static Result<T, TError> Ok<T, TError>(T resultValue) => new Result<T, TError>.Ok(resultValue);
        public static Result<T, TError> Error<T, TError>(TError errorValue) => new Result<T, TError>.Error(errorValue);
    }

    public static partial class ResultExtensions
    {
        public static U Fold<T, TError, U>(this Result<T, TError> source, Func<T, U> onOk, Func<TError, U> onError)
            => source switch
            {
                Result<T, TError>.Ok ok => onOk(ok.ResultValue),
                Result<T, TError>.Error err => onError(err.ErrorValue),
                _ => throw new InvalidOperationException(),
            };

        [GenerateAsyncOverloads]
        public static Result<U, TError> Map<T, TError, U>(this Result<T, TError> source, Func<T, U> mapping)
            => source.Fold(onOk: value => Result.Ok<U, TError>(mapping(value)), onError: Result.Error<U, TError>);

        [GenerateAsyncOverloads]
        public static Result<U, TError> FlatMap<T, TError, U>(this Result<T, TError> source, Func<T, Result<U, TError>> binder)
            => source.Fold(onOk: value => binder(value), onError: Result.Error<U, TError>);

        [GenerateAsyncOverloads]
        public static Result<(T0, T1), TError> Combine<T0, T1, TError>(this Result<T0, TError> source, Func<T0, Result<T1, TError>> binder)
            => source.FlatMap(value => binder(value).Map(next => (value, next)));
    }
}";

	// The regression guard: the generated async overloads must *compile*. Before the
	// method-group sibling was lifted, this produced CS0411 (cannot infer type arguments).
	[Fact]
	public void Generated_output_compiles_without_errors()
	{
		var (output, _) = Run(MapSource);

		var errors = output
			.GetDiagnostics()
			.Where(d => d.Severity == DiagnosticSeverity.Error)
			.ToArray();

		Assert.True(
			errors.Length == 0,
			"Generated code did not compile:\n"
				+ string.Join("\n", errors.Select(e => e.ToString()))
		);
	}

	[Fact]
	public void Method_group_sibling_is_lifted_with_TaskFromResult()
	{
		var generated = GeneratedOverloads(Run(MapSource).Result);

		Assert.Contains("Task.FromResult(Result.Error<R, TError>(__arg))", generated);
	}

	[Fact]
	public void Selector_call_is_awaited_in_the_async_branch()
	{
		var generated = GeneratedOverloads(Run(MapSource).Result);

		Assert.Contains("async resultValue", generated);
		Assert.Contains("await mapFunc(resultValue)", generated);
	}

	[Fact]
	public void Emits_three_async_overloads()
	{
		var generated = GeneratedOverloads(Run(MapSource).Result);

		var count = generated.Split("Map<T, TError, R>(").Length - 1;
		Assert.Equal(3, count);
	}

	[Fact]
	public void No_marked_methods_emits_no_overloads()
	{
		var result = Run(NoAttributeSource).Result;

		var overloadFiles = result
			.Results.Single()
			.GeneratedSources.Where(s => s.HintName.EndsWith(".AsyncOverloads.g.cs"));

		Assert.Empty(overloadFiles);
	}

	// A selector call chained into a further member access must compile in the async variant.
	[Fact]
	public void Chained_selector_core_compiles_without_errors()
	{
		var (output, _) = Run(ChainedSource);

		var errors = output
			.GetDiagnostics()
			.Where(d => d.Severity == DiagnosticSeverity.Error)
			.ToArray();

		Assert.True(
			errors.Length == 0,
			"Generated code did not compile:\n"
				+ string.Join("\n", errors.Select(e => e.ToString()))
		);
	}

	[Fact]
	public void Chained_selector_call_is_parenthesized()
	{
		var generated = GeneratedOverloads(Run(ChainedSource).Result);

		Assert.Contains("(await binder(value).ConfigureAwait(false)).Map(", generated);
	}

	// ---- harness ----------------------------------------------------------

	private static readonly MetadataReference[] References =
	[
		.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Where(p => p.Length > 0)
			.Select(p => (MetadataReference)MetadataReference.CreateFromFile(p)),
	];

	private static (Compilation Output, GeneratorDriverRunResult Result) Run(string source)
	{
		var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
		var tree = CSharpSyntaxTree.ParseText(source, parseOptions);
		var compilation = CSharpCompilation.Create(
			"GeneratorTests",
			[tree],
			References,
			new CSharpCompilationOptions(
				OutputKind.DynamicallyLinkedLibrary,
				nullableContextOptions: NullableContextOptions.Enable
			)
		);

		// The driver must parse the generated trees with the same language version as the
		// input, otherwise the updated compilation has inconsistent language versions.
		GeneratorDriver driver = CSharpGeneratorDriver.Create(
			[new AsyncOverloadGenerator().AsSourceGenerator()],
			parseOptions: parseOptions
		);
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
		return (output, driver.GetRunResult());
	}

	private static string GeneratedOverloads(GeneratorDriverRunResult result) =>
		result
			.Results.Single()
			.GeneratedSources.Single(s => s.HintName.EndsWith(".AsyncOverloads.g.cs"))
			.SourceText.ToString();
}
