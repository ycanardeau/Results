namespace Aigamo.Results;

public static partial class ResultExtensions
{
	// Collapses a sequence of results into a single result of an array.
	// Short-circuits on the first Error (a generic TError cannot be aggregated).
	[GenerateAsyncOverloads]
	public static Result<T[], TError> Merge<T, TError>(this IEnumerable<Result<T, TError>> source)
	{
		var values = new List<T>();
		foreach (var result in source)
		{
			switch (result)
			{
				case Result<T, TError>.Ok ok:
					values.Add(ok.ResultValue);
					break;
				case Result<T, TError>.Error error:
					return Result.Error<T[], TError>(error.ErrorValue);
			}
		}

		return Result.Ok<T[], TError>([.. values]);
	}
}
