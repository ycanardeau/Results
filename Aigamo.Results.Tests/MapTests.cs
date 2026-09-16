namespace Aigamo.Results.Tests;

// Map: Ok(x) -> Ok(f(x)); Error(e) -> Error(e).
public class MapTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(3);
	private static readonly Result<int, string> Err = Result.Error<int, string>("boom");

	private static readonly Result<string, string> MappedOk = Result.Ok<string, string>("3");
	private static readonly Result<string, string> MappedErr = Result.Error<string, string>("boom");

	private static string Sync(int x) => x.ToString();

	private static async Task<string> AsyncFn(int x)
	{
		await Task.Yield();
		return x.ToString();
	}

	[Fact]
	public void Sync_Ok() => Assert.Equal(MappedOk, Ok.Map(Sync));

	[Fact]
	public void Sync_Error() => Assert.Equal(MappedErr, Err.Map(Sync));

	// Variant A: Task source, sync selector.
	[Fact]
	public async Task TaskSource_SyncSelector_Ok() =>
		Assert.Equal(MappedOk, await Task.FromResult(Ok).Map(Sync));

	[Fact]
	public async Task TaskSource_SyncSelector_Error() =>
		Assert.Equal(MappedErr, await Task.FromResult(Err).Map(Sync));

	// Variant B: sync source, async selector.
	[Fact]
	public async Task SyncSource_AsyncSelector_Ok() =>
		Assert.Equal(MappedOk, await Ok.Map(AsyncFn));

	// The regression: on Error the async-lifted method-group sibling must preserve the error value.
	[Fact]
	public async Task SyncSource_AsyncSelector_Error_preserves_error() =>
		Assert.Equal(MappedErr, await Err.Map(AsyncFn));

	// Variant C: Task source, async selector.
	[Fact]
	public async Task TaskSource_AsyncSelector_Ok() =>
		Assert.Equal(MappedOk, await Task.FromResult(Ok).Map(AsyncFn));

	[Fact]
	public async Task TaskSource_AsyncSelector_Error() =>
		Assert.Equal(MappedErr, await Task.FromResult(Err).Map(AsyncFn));
}
