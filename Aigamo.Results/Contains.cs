namespace Aigamo.Results;

public static partial class ResultExtensions
{
	// True when the result is Ok and its value equals `value`; false on Error.
	[GenerateAsyncOverloads]
	public static bool Contains<T, TError>(this Result<T, TError> source, T value) =>
		source.Contains(value, comparer: null);

	[GenerateAsyncOverloads]
	public static bool Contains<T, TError>(
		this Result<T, TError> source,
		T value,
		IEqualityComparer<T>? comparer
	) =>
		source.Fold(
			onOk: resultValue =>
				(comparer ?? EqualityComparer<T>.Default).Equals(resultValue, value),
			onError: _ => false
		);
}
