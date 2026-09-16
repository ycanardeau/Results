namespace Aigamo.Results.Tests;

public class ContainsTests
{
	[Fact]
	public void Ok_matching_value_is_true() => Assert.True(Result.Ok<int, string>(3).Contains(3));

	[Fact]
	public void Ok_non_matching_value_is_false() =>
		Assert.False(Result.Ok<int, string>(3).Contains(4));

	[Fact]
	public void Error_is_false() => Assert.False(Result.Error<int, string>("e").Contains(3));

	[Fact]
	public void Uses_supplied_comparer() =>
		Assert.True(
			Result.Ok<string, string>("ABC").Contains("abc", StringComparer.OrdinalIgnoreCase)
		);

	[Fact]
	public async Task TaskSource_matches() =>
		Assert.True(await Task.FromResult(Result.Ok<int, string>(3)).Contains(3));
}

public class ContainsErrorTests
{
	[Fact]
	public void Error_matching_value_is_true() =>
		Assert.True(Result.Error<int, string>("boom").ContainsError("boom"));

	[Fact]
	public void Error_non_matching_value_is_false() =>
		Assert.False(Result.Error<int, string>("boom").ContainsError("bang"));

	[Fact]
	public void Ok_is_false() => Assert.False(Result.Ok<int, string>(1).ContainsError("boom"));

	[Fact]
	public void Uses_supplied_comparer() =>
		Assert.True(
			Result
				.Error<int, string>("BOOM")
				.ContainsError("boom", StringComparer.OrdinalIgnoreCase)
		);
}
