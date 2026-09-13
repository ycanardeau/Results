namespace Aigamo.Results;

public static partial class ResultExtensions
{
	// Unwraps a nested result: Ok(inner) -> inner, Error(e) -> Error(e).
	[GenerateAsyncOverloads]
	public static Result<T, TError> Flatten<T, TError>(this Result<Result<T, TError>, TError> source)
		=> source.Fold(
			onOk: inner => inner,
			onError: Result.Error<T, TError>);
}
