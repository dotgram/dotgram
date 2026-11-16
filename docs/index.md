# .Gram

Fast, strongly typed C# parsers, generated from a grammar at compile time, with no runtime library.
The generated parser is ordinary C# in your own assembly: there is no parser engine or grammar graph
to interpret, and nothing to deploy beside your application. One notation covers a
six-character colour code, a calculator, feeds written by people and the SQL standard.

A grammar is a small language of its own. Every package ships a skill for coding agents, so an
agent can write the grammar with you: [Writing grammars with a coding agent](agents.md).

## Start here

Five steps of growing scale, each a parser that builds and runs in the repository:

1. **A pattern instead of a regular expression**: a colour code, and a capture that becomes a property
   ([`CssColorExample.cs`](https://github.com/dotgram/dotgram/blob/main/examples/DotGram.Examples/Formats/CssColorExample.cs)).
2. **A calculator**: precedence written on the operators, one grammar published over `int`, `decimal`
   and a tree ([One grammar, three parsers](tour.md#one-grammar-three-parsers)).
3. **A CSV feed with recovery**: quoted fields, and a bad line reported where it is while reading goes on
   ([Feeds: CSV with quoted fields](feeds.md#corporate-actions-csv-with-quoted-fields)).
4. **A SQL fragment**: a guard that decides whether a statement can write anything
   ([`SqlReadOnlyExample.cs`](https://github.com/dotgram/dotgram/blob/main/examples/DotGram.Examples/Languages/SqlReadOnlyExample.cs)).
5. **A stream of records**: a feed read from a `TextReader`, in parts, with a trailer checked against
   records nobody held
   ([`StreamingFeedExample.cs`](https://github.com/dotgram/dotgram/blob/main/examples/DotGram.Examples/Feeds/StreamingFeedExample.cs)).

The [Language guide](guide/language/README.md) then takes the notation a topic at a time.

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
| [Writing grammars with a coding agent](agents.md) | Where each package's `SKILL.md` is, and a loop that works. |
| [Language guide](guide/language/README.md) | The notation, a page a topic. |
| [Language reference](syntax.md) | The notation and its bond with C#: the specification. |
| [Diagnostics](diagnostics.md) | Every message the generator reports, by identifier. |
| [Command line](cli.md) | `dotnet dotgram`, the generator run outside the build. |
| [Feeds](feeds.md) | A worked guide: trading files in pipes, CSV and fixed width, bulk-loaded into a database. |
| [Visual Studio](visual-studio.md) | The extension, and the `StringSyntax` annotations. |
| [Status](status.md) | What this version does, held against the specification. |
| [API reference](api/index.md) | The public types of the runtime libraries. |

The source, the examples and the issue tracker are on [GitHub](https://github.com/dotgram/dotgram).
