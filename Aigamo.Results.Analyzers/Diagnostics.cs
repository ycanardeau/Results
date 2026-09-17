using Microsoft.CodeAnalysis;

namespace Aigamo.Results.Analyzers;

internal static class Diagnostics
{
	// .editorconfig key that selects the policy: `implicit` or `explicit`.
	// When unset (or any other value) both rules stay silent.
	public const string ConfigKey = "aigamo_results_conversion_style";

	private const string Category = "Style";

	// Reported when the policy is `implicit` but code uses an explicit Result.Ok/Result.Error factory.
	public static readonly DiagnosticDescriptor PreferImplicitConversion = new(
		id: "ARS001",
		title: "Use implicit conversion to Result",
		messageFormat: "Return the value directly instead of '{0}'",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "'"
			+ ConfigKey
			+ " = implicit' is set, so a value or error can be returned directly and converted to Result implicitly."
	);

	// Reported when the policy is `explicit` but code relies on the implicit conversion to Result.
	public static readonly DiagnosticDescriptor PreferExplicitConversion = new(
		id: "ARS002",
		title: "Use an explicit Result factory",
		messageFormat: "Wrap the value with '{0}' instead of relying on the implicit conversion",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "'"
			+ ConfigKey
			+ " = explicit' is set, so values and errors should be wrapped with Result.Ok or Result.Error."
	);
}
