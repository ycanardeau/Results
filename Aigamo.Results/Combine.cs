namespace Aigamo.Results;

// Combine chains results, accumulating their success values into a growing tuple.
// The first Error short-circuits the chain (via FlatMap). Arities 2..8; extend as needed.
public static partial class ResultExtensions
{
	[GenerateAsyncOverloads]
	public static Result<(T0, T1), TError> Combine<T0, T1, TError>(
		this Result<T0, TError> source,
		Func<T0, Result<T1, TError>> binder
	) => source.FlatMap(value => binder(value).Map(next => (value, next)));

	[GenerateAsyncOverloads]
	public static Result<(T0, T1, T2), TError> Combine<T0, T1, T2, TError>(
		this Result<(T0, T1), TError> source,
		Func<(T0, T1), Result<T2, TError>> binder
	) => source.FlatMap(value => binder(value).Map(next => (value.Item1, value.Item2, next)));

	[GenerateAsyncOverloads]
	public static Result<(T0, T1, T2, T3), TError> Combine<T0, T1, T2, T3, TError>(
		this Result<(T0, T1, T2), TError> source,
		Func<(T0, T1, T2), Result<T3, TError>> binder
	) =>
		source.FlatMap(value =>
			binder(value).Map(next => (value.Item1, value.Item2, value.Item3, next))
		);

	[GenerateAsyncOverloads]
	public static Result<(T0, T1, T2, T3, T4), TError> Combine<T0, T1, T2, T3, T4, TError>(
		this Result<(T0, T1, T2, T3), TError> source,
		Func<(T0, T1, T2, T3), Result<T4, TError>> binder
	) =>
		source.FlatMap(value =>
			binder(value).Map(next => (value.Item1, value.Item2, value.Item3, value.Item4, next))
		);

	[GenerateAsyncOverloads]
	public static Result<(T0, T1, T2, T3, T4, T5), TError> Combine<T0, T1, T2, T3, T4, T5, TError>(
		this Result<(T0, T1, T2, T3, T4), TError> source,
		Func<(T0, T1, T2, T3, T4), Result<T5, TError>> binder
	) =>
		source.FlatMap(value =>
			binder(value)
				.Map(next =>
					(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, next)
				)
		);

	[GenerateAsyncOverloads]
	public static Result<(T0, T1, T2, T3, T4, T5, T6), TError> Combine<
		T0,
		T1,
		T2,
		T3,
		T4,
		T5,
		T6,
		TError
	>(
		this Result<(T0, T1, T2, T3, T4, T5), TError> source,
		Func<(T0, T1, T2, T3, T4, T5), Result<T6, TError>> binder
	) =>
		source.FlatMap(value =>
			binder(value)
				.Map(next =>
					(
						value.Item1,
						value.Item2,
						value.Item3,
						value.Item4,
						value.Item5,
						value.Item6,
						next
					)
				)
		);

	[GenerateAsyncOverloads]
	public static Result<(T0, T1, T2, T3, T4, T5, T6, T7), TError> Combine<
		T0,
		T1,
		T2,
		T3,
		T4,
		T5,
		T6,
		T7,
		TError
	>(
		this Result<(T0, T1, T2, T3, T4, T5, T6), TError> source,
		Func<(T0, T1, T2, T3, T4, T5, T6), Result<T7, TError>> binder
	) =>
		source.FlatMap(value =>
			binder(value)
				.Map(next =>
					(
						value.Item1,
						value.Item2,
						value.Item3,
						value.Item4,
						value.Item5,
						value.Item6,
						value.Item7,
						next
					)
				)
		);
}
