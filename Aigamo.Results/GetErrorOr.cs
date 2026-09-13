namespace Aigamo.Results;

public static partial class ResultExtensions
{
	// Returns the error value, or the result of `ifOk` when the result is an Ok.
	[GenerateAsyncOverloads]
	public static TError GetErrorOr<T, TError>(this Result<T, TError> source, Func<T, TError> ifOk)
		=> source.Fold(
			onOk: resultValue => ifOk(resultValue),
			onError: errorValue => errorValue);
}
