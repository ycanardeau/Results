namespace Aigamo.Results;

public static partial class ResultExtensions
{
	// True when the result is Error and `predicate` holds for its error; false on Ok.
	[GenerateAsyncOverloads]
	public static bool ValidateError<T, TError>(this Result<T, TError> source, Func<TError, bool> predicate)
		=> source.Fold(
			onOk: _ => false,
			onError: errorValue => predicate(errorValue));
}
