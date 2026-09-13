namespace Aigamo.Results;

public static partial class ResultExtensions
{
	// Returns the success value, or the result of `ifError` when the result is an Error.
	// (F#'s Result.defaultWith.)
	[GenerateAsyncOverloads]
	public static T GetOr<T, TError>(this Result<T, TError> source, Func<TError, T> ifError)
		=> source.Fold(
			onOk: resultValue => resultValue,
			onError: errorValue => ifError(errorValue));
}
