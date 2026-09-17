# Aigamo.Results

**A fully-serializable, F#-like result pattern for C#.**

[![NuGet](https://img.shields.io/nuget/v/Aigamo.Results.svg)](https://www.nuget.org/packages/Aigamo.Results)

[Aigamo.Results](https://github.com/ycanardeau/Results) models success and failure as a single `Result<T, TError>` value — a **closed** record with exactly two cases, `Ok` and `Error` — and gives you a full set of functional operations (`Map`, `FlatMap`, `Fold`, `Combine`, …) to transform and compose results without ever throwing. Unlike many result types, it round-trips cleanly through `System.Text.Json`, so a `Result` can cross a serialization boundary — an API response, a message queue, a cache — and come back intact.

## Features

- `Result<T, TError>` with a strongly-typed **error** channel (not just a message or an exception)
- A **closed** hierarchy (`Ok` / `Error`) — the compiler knows there are no other cases
- **Fully serializable** with `System.Text.Json` (polymorphic, round-trips by value)
- Rich functional API: `Map`, `MapError`, `FlatMap`, `FlatMapError`, `Fold`, `Tap`, `GetOr`, `Combine`, `Merge`, `MapEach`, `Flatten`, `Validate`, `Contains`, and more
- **Async all the way**: every operation has `Task`-lifted overloads, so sync and async steps chain seamlessly
- `Unit` for value-less results (`Result<Unit, TError>`)
- **Implicit conversions**: return a value or an error directly and let it become a `Result<T, TError>`
- An optional **analyzer** to enforce implicit- or explicit-construction style across a project

## Getting Started

### 1. Install the package

```bash
dotnet add package Aigamo.Results
```

> **Tip:** To share the reference and drop the per-file `using`, add it to `Directory.Build.props` with a global `Using`:
>
> ```xml
> <Project>
>
>   <ItemGroup>
>     <PackageReference Include="Aigamo.Results" />
>     <Using Include="Aigamo.Results" />
>   </ItemGroup>
>
> </Project>
> ```

### 2. Create results

```csharp
using Aigamo.Results;

Result<int, string> ok = Result.Ok<int, string>(42);
Result<int, string> error = Result.Error<int, string>("something went wrong");
```

A value or an error also converts **implicitly**, so you can often drop the factory call:

```csharp
Result<int, string> ok = 42;                    // -> Ok(42)
Result<int, string> error = "something went wrong"; // -> Error("something went wrong")
```

A function that can fail returns its outcome instead of throwing:

```csharp
static Result<int, string> Parse(string s) =>
	int.TryParse(s, out var value)
		? value                                 // implicit -> Ok(value)
		: $"'{s}' is not a number";             // implicit -> Error(...)
```

> **Note:** implicit conversion is unavailable when `T` and `TError` are the **same type**
> (`Result<string, string>`) — the two conversions are ambiguous (CS0457). Use the explicit
> `Result.Ok` / `Result.Error` factories there.

### 3. Inspect the outcome

`Fold` collapses both cases into a single value:

```csharp
string message = Parse("42").Fold(
	onOk: value => $"Parsed {value}",
	onError: error => $"Failed: {error}"
);
```

`Match` gives you the case objects directly, and helpers like `GetOr`, `Contains`, and `Validate` cover common checks:

```csharp
int value = Parse("nope").GetOr(ifError: _ => 0); // 0
bool isFortyTwo = Parse("42").Contains(42);         // true
bool isPositive = Parse("42").Validate(x => x > 0); // true
```

## Transforming and composing

```csharp
// Map the success value; Error passes through untouched.
Result<string, string> label = Parse("42").Map(x => $"#{x}");

// FlatMap chains another fallible step, short-circuiting on the first Error.
Result<int, string> checked = Parse("42").FlatMap(x =>
	x >= 0 ? Result.Ok<int, string>(x) : Result.Error<int, string>("negative"));

// MapError transforms the error channel.
Result<int, MyError> typed = Parse("42").MapError(msg => new MyError(msg));

// Tap runs a side effect on success without changing the result.
Parse("42").Tap(x => Console.WriteLine(x));
```

Combine several results into a tuple (the first `Error` wins), or collapse a sequence:

```csharp
// Combine: accumulate success values into a growing tuple.
Result<(int, int, int), string> combined = Parse("1")
	.Combine(_ => Parse("2"))
	.Combine(_ => Parse("3"));

// Merge: IEnumerable<Result<T, E>> -> Result<T[], E>
Result<int[], string> merged = new[] { "1", "2", "3" }.Select(Parse).Merge();
```

## Async

Every operation ships `Task`-lifted overloads, so you can freely mix synchronous and asynchronous steps in one chain — the `Task` is threaded through for you:

```csharp
Result<User, string> result = await LoadUserAsync(id)   // Task<Result<User, string>>
	.FlatMap(user => ValidateAsync(user))               // async step
	.Map(user => user.Name)                             // sync step
	.TapError(error => logger.LogWarning(error));       // async source, sync action
```

For each operation you get the overloads for a `Task` source, an async selector, and both at once.

## Serialization

`Result<T, TError>` is annotated for `System.Text.Json` polymorphic serialization and round-trips by value:

```csharp
using System.Text.Json;

var ok = Result.Ok<int, string>(42);
string json = JsonSerializer.Serialize(ok);
// {"$type":"Ok","ResultValue":42}

var back = JsonSerializer.Deserialize<Result<int, string>>(json);
// back == ok  (true)
```

An `Error` serializes as `{"$type":"Error","ErrorValue":...}`. No custom converter or configuration required.

## Enforcing a conversion style

Whether to write `Result.Ok(value)` explicitly or lean on the implicit conversion is a matter of taste — so the package ships an analyzer that lets a project pick one and enforce it. Set the policy in `.editorconfig`:

```ini
[*.cs]
# choose one:
aigamo_results_conversion_style = implicit   # or: explicit
```

- `implicit` → **ARS001** flags an explicit `Result.Ok` / `Result.Error` call where the value could convert implicitly, and the code fix removes the wrapper.
- `explicit` → **ARS002** flags reliance on the implicit conversion, and the code fix wraps the value in `Result.Ok` / `Result.Error`.

When the key is unset, neither rule fires. Both default to **warning** severity and can be tuned per rule:

```ini
dotnet_diagnostic.ARS001.severity = suggestion
```

`ARS001` is never reported for `Result<T, T>`, where implicit conversion is ambiguous.

## Under the hood

`Result<T, TError>` is a `closed record` whose `Ok`/`Error` cases are nested with a private constructor, so the hierarchy cannot be extended elsewhere. The exhaustive `Match` method is generated by [Aigamo.MatchGenerator](https://github.com/ycanardeau/MatchGenerator), and the `Task`-lifted overloads are emitted by an internal `[GenerateAsyncOverloads]` source generator — the hand-written code only defines the synchronous core of each operation.

## Inspiration

Aigamo.Results is inspired by [Nut.Results](https://github.com/Archway-SharedLib/Nut.Results). The goal here is a result type that keeps a strongly-typed error channel _and_ serializes cleanly, with the functional surface generated rather than hand-maintained.

## License

MIT
