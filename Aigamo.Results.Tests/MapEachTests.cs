namespace Aigamo.Results.Tests;

public class MapEachTests
{
	private static Result<IEnumerable<int>, string> Ok(params int[] xs) =>
		Result.Ok<IEnumerable<int>, string>(xs);

	private static readonly Result<IEnumerable<int>, string> Err = Result.Error<
		IEnumerable<int>,
		string
	>("boom");

	private static IEnumerable<int> Values(Result<IEnumerable<int>, string> r) =>
		r.GetOr(_ => Enumerable.Empty<int>());

	[Fact]
	public void Ok_maps_each_element() =>
		Assert.Equal(new[] { 2, 4, 6 }, Values(Ok(1, 2, 3).MapEach(x => x * 2)));

	[Fact]
	public void Error_passes_through() => Assert.Equal(Err, Err.MapEach(x => x * 2));

	[Fact]
	public async Task AsyncMapping_Ok_maps_each_element()
	{
		var result = await Ok(1, 2, 3)
			.MapEach(async x =>
			{
				await Task.Yield();
				return x * 2;
			});
		Assert.Equal(new[] { 2, 4, 6 }, Values(result));
	}

	[Fact]
	public async Task AsyncMapping_Error_passes_through()
	{
		var result = await Err.MapEach(async x =>
		{
			await Task.Yield();
			return x * 2;
		});
		Assert.Equal(Err, result);
	}

	[Fact]
	public async Task TaskSource_SyncMapping()
	{
		var result = await Task.FromResult(Ok(1, 2, 3)).MapEach(x => x * 2);
		Assert.Equal(new[] { 2, 4, 6 }, Values(result));
	}
}
