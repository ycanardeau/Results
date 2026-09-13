namespace Aigamo.Results.Tests;

public class AsTaskTests
{
	[Fact]
	public async Task Wraps_result_in_completed_task()
	{
		var source = Result.Ok<int, string>(7);
		Assert.Equal(source, await source.AsTask());
	}
}
