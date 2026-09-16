using System.Text.Json.Serialization;

namespace Aigamo.Results;

[GenerateMatch]
[JsonDerivedType(typeof(Result<,>.Ok), nameof(Ok))]
[JsonDerivedType(typeof(Result<,>.Error), nameof(Error))]
public closed record Result<T, TError>
{
	private Result() { }

	public sealed record Ok(T ResultValue) : Result<T, TError>;

	public sealed record Error(TError ErrorValue) : Result<T, TError>;

	public static implicit operator Result<T, TError>(T resultValue) => new Ok(resultValue);

	public static implicit operator Result<T, TError>(TError errorValue) => new Error(errorValue);
}

public static class Result
{
	public static Result<T, TError> Ok<T, TError>(T resultValue) =>
		new Result<T, TError>.Ok(resultValue);

	public static Result<T, TError> Error<T, TError>(TError errorValue) =>
		new Result<T, TError>.Error(errorValue);
}
