namespace Aigamo.Results.Tests;

public class CombineTests
{
	[Fact]
	public void Two_oks_combine_into_a_pair()
		=> Assert.Equal(
			Result.Ok<(int, string), string>((1, "a")),
			Result.Ok<int, string>(1).Combine(_ => Result.Ok<string, string>("a")));

	[Fact]
	public void Source_error_short_circuits()
		=> Assert.Equal(
			Result.Error<(int, string), string>("src"),
			Result.Error<int, string>("src").Combine(_ => Result.Ok<string, string>("a")));

	[Fact]
	public void Binder_error_short_circuits()
		=> Assert.Equal(
			Result.Error<(int, string), string>("bind"),
			Result.Ok<int, string>(1).Combine(_ => Result.Error<string, string>("bind")));

	[Fact]
	public void Chains_into_a_triple()
	{
		var result = Result.Ok<int, string>(1)
			.Combine(_ => Result.Ok<string, string>("a"))
			.Combine(_ => Result.Ok<bool, string>(true));
		Assert.Equal(Result.Ok<(int, string, bool), string>((1, "a", true)), result);
	}

	[Fact]
	public async Task AsyncBinder_combines_into_a_pair()
	{
		var result = await Result.Ok<int, string>(1)
			.Combine(async _ => { await Task.Yield(); return Result.Ok<string, string>("a"); });
		Assert.Equal(Result.Ok<(int, string), string>((1, "a")), result);
	}

	[Fact]
	public async Task AsyncBinder_error_short_circuits()
	{
		var result = await Result.Ok<int, string>(1)
			.Combine(async _ => { await Task.Yield(); return Result.Error<string, string>("bind"); });
		Assert.Equal(Result.Error<(int, string), string>("bind"), result);
	}
}
