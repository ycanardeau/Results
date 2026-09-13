namespace Aigamo.Results.Tests;

// FlatMap: Ok(x) -> f(x); Error(e) -> Error(e).
public class FlatMapTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(3);
	private static readonly Result<int, string> Err = Result.Error<int, string>("boom");

	private static readonly Result<string, string> Bound = Result.Ok<string, string>("3");
	private static readonly Result<string, string> BoundToError = Result.Error<string, string>("rejected");
	private static readonly Result<string, string> PassedThrough = Result.Error<string, string>("boom");

	// f maps Ok to Ok, unless the value is negative (then it produces an Error).
	private static Result<string, string> Sync(int x) => x < 0 ? Result.Error<string, string>("rejected") : Result.Ok<string, string>(x.ToString());
	private static async Task<Result<string, string>> AsyncFn(int x) { await Task.Yield(); return Sync(x); }

	[Fact]
	public void Sync_Ok_binds() => Assert.Equal(Bound, Ok.FlatMap(Sync));

	[Fact]
	public void Sync_Ok_can_bind_to_error() => Assert.Equal(BoundToError, Result.Ok<int, string>(-1).FlatMap(Sync));

	[Fact]
	public void Sync_Error_passes_through() => Assert.Equal(PassedThrough, Err.FlatMap(Sync));

	// Variant A: Task source, sync selector.
	[Fact]
	public async Task TaskSource_SyncSelector_Ok() => Assert.Equal(Bound, await Task.FromResult(Ok).FlatMap(Sync));

	[Fact]
	public async Task TaskSource_SyncSelector_Error() => Assert.Equal(PassedThrough, await Task.FromResult(Err).FlatMap(Sync));

	// Variant B: sync source, async selector.
	[Fact]
	public async Task SyncSource_AsyncSelector_Ok() => Assert.Equal(Bound, await Ok.FlatMap(AsyncFn));

	// The regression: on Error the async-lifted method-group sibling must preserve the error value.
	[Fact]
	public async Task SyncSource_AsyncSelector_Error_preserves_error() => Assert.Equal(PassedThrough, await Err.FlatMap(AsyncFn));

	// Variant C: Task source, async selector.
	[Fact]
	public async Task TaskSource_AsyncSelector_Ok() => Assert.Equal(Bound, await Task.FromResult(Ok).FlatMap(AsyncFn));

	[Fact]
	public async Task TaskSource_AsyncSelector_Error() => Assert.Equal(PassedThrough, await Task.FromResult(Err).FlatMap(AsyncFn));
}
