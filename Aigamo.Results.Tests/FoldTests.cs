namespace Aigamo.Results.Tests;

// Fold collapses both cases to a single value, handing the unwrapped Ok/Error value to each branch.
public class FoldTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(3);
	private static readonly Result<int, string> Err = Result.Error<int, string>("boom");

	[Fact]
	public void Ok_runs_onOk_branch_with_value()
		=> Assert.Equal("ok:3", Ok.Fold(onOk: x => $"ok:{x}", onError: e => $"err:{e}"));

	[Fact]
	public void Error_runs_onError_branch_with_value()
		=> Assert.Equal("err:boom", Err.Fold(onOk: x => $"ok:{x}", onError: e => $"err:{e}"));
}
