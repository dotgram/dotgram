# .Gram

.Gram is a source generator that compiles grammars into strongly typed C# parsers, from
single-character rules and hand-written feeds to the SQL standard.

The grammar is known at compile time. The generated parser is ordinary C# in your own
assembly: there is no parser engine, grammar graph or runtime library to interpret.

## Getting started

```xml
<PackageReference Include="DotGram" Version="0.3.0"
                  PrivateAssets="all" ExcludeAssets="runtime" />
```

The generator runs in the C# compiler of .NET SDK 9.0.300 or newer (Roslyn 4.14; Visual Studio 17.14).
The project may still target `net8.0` or older, but an SDK 8 build skips the generator with warning
CS9057 and generates no parser, so the code that calls it does not compile.

A grammar lives in the `[Gram]` attribute of the class the parser is generated into, or in a
`.gram` file listed as `<AdditionalFiles Include="Name.gram" />`:

```csharp
[Gram("""
	Word   = ['a'..'z']+
	Filter = field: Word & '=' & value: Word

	parse Filter
	""")]
public static partial class FilterParser;
```

The generator turns it into `Parse`, `TryParse` and `Find` over text, streams, bytes and
segments, strongly typed results from the named captures, error recovery for record-oriented
input, and compile-time diagnostics that point back into the grammar.

## Where to go next

| | |
| --- | --- |
| [Language reference](syntax.md) | The notation and its bond with C#: the specification. |
| [Diagnostics](diagnostics.md) | Every message the generator reports, by identifier. |
| [Command line](cli.md) | `dotnet dotgram`, the generator run outside the build. |
| [Feeds](feeds.md) | A worked guide: trading files in pipes, CSV and fixed width, bulk-loaded into a database. |
| [Visual Studio](visual-studio.md) | The extension, and the `StringSyntax` annotations. |
| [Status](status.md) | What this version does, held against the specification. |
| [API reference](api/index.md) | The public types of the runtime libraries. |

The source, the examples and the issue tracker are on [GitHub](https://github.com/dotgram/dotgram).
