namespace Aigamo.Results.Tests;

public class MergeTests
{
	private static int[] Values(Result<int[], string> r) => r.GetOr(_ => Array.Empty<int>());

	[Fact]
	public void All_ok_collects_values()
	{
		var merged = new[] { Result.Ok<int, string>(1), Result.Ok<int, string>(2), Result.Ok<int, string>(3) }.Merge();
		Assert.Equal(new[] { 1, 2, 3 }, Values(merged));
	}

	[Fact]
	public void First_error_short_circuits()
	{
		var merged = new[]
		{
			Result.Ok<int, string>(1),
			Result.Error<int, string>("first"),
			Result.Error<int, string>("second"),
		}.Merge();
		Assert.Equal(Result.Error<int[], string>("first"), merged);
	}

	[Fact]
	public void Empty_sequence_is_ok_empty()
	{
		var merged = Array.Empty<Result<int, string>>().Merge();
		Assert.Equal(Array.Empty<int>(), Values(merged));
	}

	[Fact]
	public async Task TaskSource_collects_values()
	{
		IEnumerable<Result<int, string>> source = new[] { Result.Ok<int, string>(1), Result.Ok<int, string>(2) };
		var merged = await Task.FromResult(source).Merge();
		Assert.Equal(new[] { 1, 2 }, Values(merged));
	}
}
