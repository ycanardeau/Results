namespace Aigamo.Results.Tests;

public class FlattenTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(5);
	private static readonly Result<int, string> Err = Result.Error<int, string>("inner");

	[Fact]
	public void Ok_of_ok_unwraps() =>
		Assert.Equal(Ok, Result.Ok<Result<int, string>, string>(Ok).Flatten());

	[Fact]
	public void Ok_of_error_becomes_error() =>
		Assert.Equal(Err, Result.Ok<Result<int, string>, string>(Err).Flatten());

	[Fact]
	public void Outer_error_passes_through() =>
		Assert.Equal(
			Result.Error<int, string>("outer"),
			Result.Error<Result<int, string>, string>("outer").Flatten()
		);

	[Fact]
	public async Task TaskSource_unwraps() =>
		Assert.Equal(
			Ok,
			await Task.FromResult(Result.Ok<Result<int, string>, string>(Ok)).Flatten()
		);
}
