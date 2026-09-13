namespace Aigamo.Results.Tests;

// MapError: Ok(x) -> Ok(x); Error(e) -> Error(f(e)).
public class MapErrorTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(3);
	private static readonly Result<int, string> Err = Result.Error<int, string>("boom");

	private static readonly Result<int, int> MappedOk = Result.Ok<int, int>(3);
	private static readonly Result<int, int> MappedErr = Result.Error<int, int>(4); // "boom".Length

	private static int Sync(string e) => e.Length;
	private static async Task<int> AsyncFn(string e) { await Task.Yield(); return e.Length; }

	[Fact]
	public void Sync_Ok() => Assert.Equal(MappedOk, Ok.MapError(Sync));

	[Fact]
	public void Sync_Error() => Assert.Equal(MappedErr, Err.MapError(Sync));

	// Variant A: Task source, sync selector.
	[Fact]
	public async Task TaskSource_SyncSelector_Ok() => Assert.Equal(MappedOk, await Task.FromResult(Ok).MapError(Sync));

	[Fact]
	public async Task TaskSource_SyncSelector_Error() => Assert.Equal(MappedErr, await Task.FromResult(Err).MapError(Sync));

	// Variant B: sync source, async selector.
	// On Ok the async-lifted method-group sibling (Result.Ok) must preserve the ok value.
	[Fact]
	public async Task SyncSource_AsyncSelector_Ok_preserves_value() => Assert.Equal(MappedOk, await Ok.MapError(AsyncFn));

	[Fact]
	public async Task SyncSource_AsyncSelector_Error() => Assert.Equal(MappedErr, await Err.MapError(AsyncFn));

	// Variant C: Task source, async selector.
	[Fact]
	public async Task TaskSource_AsyncSelector_Ok() => Assert.Equal(MappedOk, await Task.FromResult(Ok).MapError(AsyncFn));

	[Fact]
	public async Task TaskSource_AsyncSelector_Error() => Assert.Equal(MappedErr, await Task.FromResult(Err).MapError(AsyncFn));
}
