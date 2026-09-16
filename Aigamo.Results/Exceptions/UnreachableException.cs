namespace System.Diagnostics;

// Compile-time polyfill for the netstandard2.0 target only. Kept internal so it is not
// exported to consumers, where on modern TFMs it would collide with the BCL's
// System.Diagnostics.UnreachableException (CS0433).
internal sealed class UnreachableException : Exception;
