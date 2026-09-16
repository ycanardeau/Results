namespace Aigamo.Results.Benchmarks;

using System.Text.Json;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class ResultBenchmarks
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(42);
	private static readonly Result<int, string> Err = Result.Error<int, string>("boom");

	private static readonly Result<int, string>[] Sequence =
	[
		Result.Ok<int, string>(1),
		Result.Ok<int, string>(2),
		Result.Ok<int, string>(3),
		Result.Ok<int, string>(4),
		Result.Ok<int, string>(5),
	];

	private static readonly string OkJson = JsonSerializer.Serialize(Ok);

	[Benchmark]
	public Result<int, string> CreateOk() => Result.Ok<int, string>(42);

	[Benchmark]
	public Result<int, string> CreateError() => Result.Error<int, string>("boom");

	[Benchmark]
	public string Fold() => Ok.Fold(onOk: value => value.ToString(), onError: error => error);

	[Benchmark]
	public Result<string, string> Map() => Ok.Map(value => value.ToString());

	[Benchmark]
	public Result<int, string> MapError() => Err.MapError(error => error.ToUpperInvariant());

	[Benchmark]
	public Result<int, string> FlatMap() => Ok.FlatMap(value => Result.Ok<int, string>(value + 1));

	[Benchmark]
	public Result<(int, int, int), string> Combine() =>
		Ok.Combine(_ => Ok).Combine(_ => Ok);

	[Benchmark]
	public Result<int[], string> Merge() => Sequence.Merge();

	[Benchmark]
	public bool Contains() => Ok.Contains(42);

	[Benchmark]
	public string Serialize() => JsonSerializer.Serialize(Ok);

	[Benchmark]
	public Result<int, string> Deserialize() =>
		JsonSerializer.Deserialize<Result<int, string>>(OkJson)!;
}
