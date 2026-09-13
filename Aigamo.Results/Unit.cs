namespace Aigamo.Results;

/// <summary>
/// A type with a single value, used as the success value of a result that carries
/// no meaningful data — the counterpart of F#'s <c>unit</c>. <c>Result&lt;Unit, TError&gt;</c>
/// is the value-less result.
/// </summary>
public readonly record struct Unit
{
	/// <summary>The single <see cref="Unit"/> value.</summary>
	public static Unit Default => default;
}
