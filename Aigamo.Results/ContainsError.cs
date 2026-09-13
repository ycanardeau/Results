namespace Aigamo.Results;

public static partial class ResultExtensions
{
	// True when the result is Error and its error equals `error`; false on Ok.
	[GenerateAsyncOverloads]
	public static bool ContainsError<T, TError>(this Result<T, TError> source, TError error)
		=> source.ContainsError(error, comparer: null);

	[GenerateAsyncOverloads]
	public static bool ContainsError<T, TError>(this Result<T, TError> source, TError error, IEqualityComparer<TError>? comparer)
		=> source.Fold(
			onOk: _ => false,
			onError: errorValue => (comparer ?? EqualityComparer<TError>.Default).Equals(errorValue, error));
}
