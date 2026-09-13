namespace Aigamo.Results;

// MapEach maps every element of a successful sequence result. Its async-selector
// overloads are hand-written: the [GenerateAsyncOverloads] generator lifts a selector
// call at the call site, but here the selector is applied inside `Select`, which the
// generator cannot rewrite. The async element mapping uses Task.WhenAll.
public static partial class ResultExtensions
{
	public static Result<IEnumerable<U>, TError> MapEach<T, TError, U>(this Result<IEnumerable<T>, TError> source, Func<T, U> mapping)
		=> source.Map(items => items.Select(mapping));

	// Task source, sync mapping.
	public static async Task<Result<IEnumerable<U>, TError>> MapEach<T, TError, U>(this Task<Result<IEnumerable<T>, TError>> source, Func<T, U> mapping)
		=> (await source.ConfigureAwait(false)).MapEach(mapping);

	// Sync source, async mapping.
	public static async Task<Result<IEnumerable<U>, TError>> MapEach<T, TError, U>(this Result<IEnumerable<T>, TError> source, Func<T, Task<U>> mapping)
		=> await source.Fold(
			onOk: async items => Result.Ok<IEnumerable<U>, TError>(await Task.WhenAll(items.Select(mapping)).ConfigureAwait(false)),
			onError: error => Task.FromResult(Result.Error<IEnumerable<U>, TError>(error))).ConfigureAwait(false);

	// Task source, async mapping.
	public static async Task<Result<IEnumerable<U>, TError>> MapEach<T, TError, U>(this Task<Result<IEnumerable<T>, TError>> source, Func<T, Task<U>> mapping)
		=> await (await source.ConfigureAwait(false)).MapEach(mapping).ConfigureAwait(false);
}
