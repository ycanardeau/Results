namespace Aigamo.Results.Tests;

// Tap runs a side effect on Ok and returns the source unchanged; TapError mirrors it on Error.
public class TapTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(3);
	private static readonly Result<int, string> Err = Result.Error<int, string>("boom");

	// ---- Tap (fires on Ok) ----

	[Fact]
	public void Tap_Ok_runs_action_and_returns_source()
	{
		int? seen = null;
		var result = Ok.Tap(x => seen = x);
		Assert.Equal(Ok, result);
		Assert.Equal(3, seen);
	}

	[Fact]
	public void Tap_Error_skips_action_and_returns_source()
	{
		var ran = false;
		var result = Err.Tap(_ => ran = true);
		Assert.Equal(Err, result);
		Assert.False(ran);
	}

	// Variant B: sync source, async action.
	[Fact]
	public async Task Tap_AsyncAction_Ok_runs()
	{
		int? seen = null;
		var result = await Ok.Tap(async x => { await Task.Yield(); seen = x; });
		Assert.Equal(Ok, result);
		Assert.Equal(3, seen);
	}

	[Fact]
	public async Task Tap_AsyncAction_Error_skips()
	{
		var ran = false;
		var result = await Err.Tap(async _ => { await Task.Yield(); ran = true; });
		Assert.Equal(Err, result);
		Assert.False(ran);
	}

	// Variant A: Task source, sync action.
	[Fact]
	public async Task Tap_TaskSource_SyncAction_Ok_runs()
	{
		int? seen = null;
		var result = await Task.FromResult(Ok).Tap(x => seen = x);
		Assert.Equal(Ok, result);
		Assert.Equal(3, seen);
	}

	// Variant C: Task source, async action.
	[Fact]
	public async Task Tap_TaskSource_AsyncAction_Ok_runs()
	{
		int? seen = null;
		var result = await Task.FromResult(Ok).Tap(async x => { await Task.Yield(); seen = x; });
		Assert.Equal(Ok, result);
		Assert.Equal(3, seen);
	}

	// ---- TapError (fires on Error) ----

	[Fact]
	public void TapError_Error_runs_action_and_returns_source()
	{
		string? seen = null;
		var result = Err.TapError(e => seen = e);
		Assert.Equal(Err, result);
		Assert.Equal("boom", seen);
	}

	[Fact]
	public void TapError_Ok_skips_action_and_returns_source()
	{
		var ran = false;
		var result = Ok.TapError(_ => ran = true);
		Assert.Equal(Ok, result);
		Assert.False(ran);
	}

	// Variant B: sync source, async action.
	[Fact]
	public async Task TapError_AsyncAction_Error_runs()
	{
		string? seen = null;
		var result = await Err.TapError(async e => { await Task.Yield(); seen = e; });
		Assert.Equal(Err, result);
		Assert.Equal("boom", seen);
	}

	[Fact]
	public async Task TapError_AsyncAction_Ok_skips()
	{
		var ran = false;
		var result = await Ok.TapError(async _ => { await Task.Yield(); ran = true; });
		Assert.Equal(Ok, result);
		Assert.False(ran);
	}
}
