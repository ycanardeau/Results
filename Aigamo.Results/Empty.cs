namespace Aigamo.Results;

public static partial class ResultExtensions
{
	// Discards the success value, keeping only success/error: Ok(_) -> Ok(Unit), Error(e) -> Error(e).
	[GenerateAsyncOverloads]
	public static Result<Unit, TError> Empty<T, TError>(this Result<T, TError> source) =>
		source.Map(_ => Unit.Default);
}
