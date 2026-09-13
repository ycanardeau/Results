namespace Aigamo.Results;

public static partial class ResultExtensions
{
	// True when the result is Ok and `predicate` holds for its value; false on Error.
	[GenerateAsyncOverloads]
	public static bool Validate<T, TError>(this Result<T, TError> source, Func<T, bool> predicate)
		=> source.Fold(
			onOk: resultValue => predicate(resultValue),
			onError: _ => false);
}
