namespace Aigamo.Results.Tests;

// FlatMapError: Ok(x) -> Ok(x); Error(e) -> f(e).
public class FlatMapErrorTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(3);
	private static readonly Result<int, string> Err = Result.Error<int, string>("retry");

	private static readonly Result<int, int> PassedThrough = Result.Ok<int, int>(3);
	private static readonly Result<int, int> Recovered = Result.Ok<int, int>(0);      // "retry" recovers to Ok(0)
	private static readonly Result<int, int> BoundToError = Result.Error<int, int>(1); // other errors map to Error(len)

	// f recovers the "retry" error to Ok(0); anything else becomes Error(e.Length).
	private static Result<int, int> Sync(string e) => e == "retry" ? Result.Ok<int, int>(0) : Result.Error<int, int>(e.Length);
	private static async Task<Result<int, int>> AsyncFn(string e) { await Task.Yield(); return Sync(e); }

	[Fact]
	public void Sync_Ok_passes_through() => Assert.Equal(PassedThrough, Ok.FlatMapError(Sync));

	[Fact]
	public void Sync_Error_recovers() => Assert.Equal(Recovered, Err.FlatMapError(Sync));

	[Fact]
	public void Sync_Error_can_bind_to_error() => Assert.Equal(BoundToError, Result.Error<int, string>("x").FlatMapError(Sync));

	// Variant A: Task source, sync selector.
	[Fact]
	public async Task TaskSource_SyncSelector_Ok() => Assert.Equal(PassedThrough, await Task.FromResult(Ok).FlatMapError(Sync));

	[Fact]
	public async Task TaskSource_SyncSelector_Error() => Assert.Equal(Recovered, await Task.FromResult(Err).FlatMapError(Sync));

	// Variant B: sync source, async selector.
	// On Ok the async-lifted method-group sibling (Result.Ok) must preserve the ok value.
	[Fact]
	public async Task SyncSource_AsyncSelector_Ok_preserves_value() => Assert.Equal(PassedThrough, await Ok.FlatMapError(AsyncFn));

	[Fact]
	public async Task SyncSource_AsyncSelector_Error() => Assert.Equal(Recovered, await Err.FlatMapError(AsyncFn));

	// Variant C: Task source, async selector.
	[Fact]
	public async Task TaskSource_AsyncSelector_Ok() => Assert.Equal(PassedThrough, await Task.FromResult(Ok).FlatMapError(AsyncFn));

	[Fact]
	public async Task TaskSource_AsyncSelector_Error() => Assert.Equal(Recovered, await Task.FromResult(Err).FlatMapError(AsyncFn));
}
