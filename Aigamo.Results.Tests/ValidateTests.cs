namespace Aigamo.Results.Tests;

public class ValidateTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(4);
	private static readonly Result<int, string> Err = Result.Error<int, string>("e");

	[Fact]
	public void Ok_passing_predicate_is_true() => Assert.True(Ok.Validate(x => x % 2 == 0));

	[Fact]
	public void Ok_failing_predicate_is_false() => Assert.False(Ok.Validate(x => x % 2 == 1));

	[Fact]
	public void Error_is_false() => Assert.False(Err.Validate(_ => true));

	[Fact]
	public async Task AsyncPredicate_Ok_true() =>
		Assert.True(
			await Ok.Validate(async x =>
			{
				await Task.Yield();
				return x % 2 == 0;
			})
		);

	[Fact]
	public async Task AsyncPredicate_Error_false() =>
		Assert.False(
			await Err.Validate(async _ =>
			{
				await Task.Yield();
				return true;
			})
		);
}

public class ValidateErrorTests
{
	private static readonly Result<int, string> Ok = Result.Ok<int, string>(1);
	private static readonly Result<int, string> Err = Result.Error<int, string>("boom");

	[Fact]
	public void Error_passing_predicate_is_true() =>
		Assert.True(Err.ValidateError(e => e.StartsWith("bo")));

	[Fact]
	public void Error_failing_predicate_is_false() =>
		Assert.False(Err.ValidateError(e => e.StartsWith("xx")));

	[Fact]
	public void Ok_is_false() => Assert.False(Ok.ValidateError(_ => true));

	[Fact]
	public async Task AsyncPredicate_Error_true() =>
		Assert.True(
			await Err.ValidateError(async e =>
			{
				await Task.Yield();
				return e.Length == 4;
			})
		);
}
