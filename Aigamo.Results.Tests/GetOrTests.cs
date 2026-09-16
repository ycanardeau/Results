namespace Aigamo.Results.Tests;

public class GetOrTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(3);
	private static readonly Result<int, string> Err = Result.Error<int, string>("boom");

	[Fact]
	public void Ok_returns_value() => Assert.Equal(3, Ok.GetOr(_ => -1));

	[Fact]
	public void Error_returns_fallback() => Assert.Equal(4, Err.GetOr(e => e.Length));

	[Fact]
	public async Task Async_Error_returns_fallback() =>
		Assert.Equal(
			4,
			await Err.GetOr(async e =>
			{
				await Task.Yield();
				return e.Length;
			})
		);

	[Fact]
	public async Task Async_Ok_returns_value() =>
		Assert.Equal(
			3,
			await Ok.GetOr(async _ =>
			{
				await Task.Yield();
				return -1;
			})
		);
}

public class GetErrorOrTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(3);
	private static readonly Result<int, string> Err = Result.Error<int, string>("boom");

	[Fact]
	public void Error_returns_error() => Assert.Equal("boom", Err.GetErrorOr(_ => "ok"));

	[Fact]
	public void Ok_returns_fallback() => Assert.Equal("v3", Ok.GetErrorOr(v => "v" + v));

	[Fact]
	public async Task Async_Ok_returns_fallback() =>
		Assert.Equal(
			"v3",
			await Ok.GetErrorOr(async v =>
			{
				await Task.Yield();
				return "v" + v;
			})
		);
}
