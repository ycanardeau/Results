namespace Aigamo.Results.Tests;

// Fold collapses both cases to a single value, handing the unwrapped Ok/Error value to each branch.
public class FoldTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(3);
	private static readonly Result<int, string> Err = Result.Error<int, string>("boom");

	private static string OnOk(int x) => $"ok:{x}";

	private static string OnErr(string e) => $"err:{e}";

	private static async Task<string> OnOkAsync(int x)
	{
		await Task.Yield();
		return $"ok:{x}";
	}

	private static async Task<string> OnErrAsync(string e)
	{
		await Task.Yield();
		return $"err:{e}";
	}

	[Fact]
	public void Ok_runs_onOk_branch_with_value() =>
		Assert.Equal("ok:3", Ok.Fold(onOk: OnOk, onError: OnErr));

	[Fact]
	public void Error_runs_onError_branch_with_value() =>
		Assert.Equal("err:boom", Err.Fold(onOk: OnOk, onError: OnErr));

	// Variant A: Task source, sync selectors.
	[Fact]
	public async Task TaskSource_SyncSelectors_Ok() =>
		Assert.Equal("ok:3", await Task.FromResult(Ok).Fold(onOk: OnOk, onError: OnErr));

	[Fact]
	public async Task TaskSource_SyncSelectors_Error() =>
		Assert.Equal("err:boom", await Task.FromResult(Err).Fold(onOk: OnOk, onError: OnErr));

	// Variant B: sync source, async selectors.
	[Fact]
	public async Task SyncSource_AsyncSelectors_Ok() =>
		Assert.Equal("ok:3", await Ok.Fold(onOk: OnOkAsync, onError: OnErrAsync));

	[Fact]
	public async Task SyncSource_AsyncSelectors_Error() =>
		Assert.Equal("err:boom", await Err.Fold(onOk: OnOkAsync, onError: OnErrAsync));

	// Variant C: Task source, async selectors.
	[Fact]
	public async Task TaskSource_AsyncSelectors_Ok() =>
		Assert.Equal("ok:3", await Task.FromResult(Ok).Fold(onOk: OnOkAsync, onError: OnErrAsync));

	[Fact]
	public async Task TaskSource_AsyncSelectors_Error() =>
		Assert.Equal(
			"err:boom",
			await Task.FromResult(Err).Fold(onOk: OnOkAsync, onError: OnErrAsync)
		);
}
