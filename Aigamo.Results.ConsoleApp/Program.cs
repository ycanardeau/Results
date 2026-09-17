using Aigamo.Results;

Console.WriteLine(ConversionStyleDemo.FromValue());
Console.WriteLine(ConversionStyleDemo.FromError());

static class ConversionStyleDemo
{
	// aigamo_results_conversion_style = implicit (see .editorconfig), so ARS001 flags these
	// explicit factory calls — the code fix rewrites them to `=> 42` / `=> "boom"`.
	public static Result<int, string> FromValue() => 42;

	public static Result<int, string> FromError() => "boom";
}
