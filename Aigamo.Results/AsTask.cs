namespace Aigamo.Results;

public static partial class ResultExtensions
{
	// Bridges a synchronous result into the Task world so it can start an async chain.
	// Not marked [GenerateAsyncOverloads] -- it is itself the sync-to-async bridge.
	public static Task<Result<T, TError>> AsTask<T, TError>(this Result<T, TError> source) =>
		Task.FromResult(source);
}
