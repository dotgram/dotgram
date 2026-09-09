# .Gram

**.Gram is a source generator that compiles grammars into strongly typed C# parsers.**

The grammar is known at compile time. The generated parser is ordinary C# in your own
assembly — there is no parser engine, grammar graph, or runtime library to interpret.

From a grammar, .Gram can generate:

* `Parse`, `TryParse`, and `Find` APIs;
* strongly typed results from named captures;
* parsers specialized for different versions of the same grammar;
* streaming parsers for `TextReader`;
* error recovery for record-oriented input;
* compile-time diagnostics that point back into the grammar.

Anything with a grammar is in scope: data formats and feeds, configuration files, wire
protocols, query and filter languages, template and markup syntaxes, and small languages of
your own — including ones that compile straight to `System.Linq.Expressions` or to your own
types. Replacing a regular expression that has become hard to maintain, or a hand-written
parser that has become hard to trust, is one use among those rather than the boundary.

## Getting started

```xml
<PackageReference Include="DotGram" Version="0.1.0"
                  PrivateAssets="all" ExcludeAssets="runtime" />
```

The smallest useful .Gram parser looks much like a regular expression:

```csharp
using DotGram;

[Gram("""
	Hex = ['0'..'9' | 'a'..'f' | 'A'..'F']

	Color = '#' & value: Hex{6}

	parse Color
	""")]
public static partial class CssColor;
```

Use it as ordinary C#:

```csharp
var color = CssColor.ParseColor("#12aBcF");

Console.WriteLine(color.Value);       // 12aBcF

var result = CssColor.TryParseColor("#xyz");

Console.WriteLine(result.IsSuccess);  // False
```

The equivalent regular expression would be roughly:

```text
^#(?<value>[0-9a-fA-F]{6})$
```

The familiar pieces mean familiar things: ranges, alternatives, `?`, `*`, `+`, and `{n}`.

But `value:` is not just a regex capture. It becomes a property of the generated result
type.

For small grammars, keeping the grammar in the `[Gram]` attribute makes the parser
definition and its C# API easy to read together. Larger grammars can also live in `.gram`
files, listed as `<AdditionalFiles Include="Name.gram" />`.

## One grammar, two parsers

A grammar does not have to describe only one parser. The arithmetic below is written once
and published twice: once over `int`, and once over `double`.

```csharp
using DotGram;

[Gram("""
	@using System.Globalization;

	trivia = [' ' | '\t']*

	Digits = ['0'..'9']+

	Value
		: @int
		= d: Digits
		=> @int.Parse(d)

	Sum
		: Value
		= left: Sum & op: ['+' | '-'] & right: Product
			=> @(op == "+" ? left + right : left - right)
		| value: Product
			=> @(value)

	Product
		: Value
		= left: Product & op: ['*' | '/'] & right: Unary
			=> @(op == "*" ? left * right : left / right)
		| value: Unary
			=> @(value)

	Unary
		: Value
		= '-' & operand: Unary
			=> @(-operand)
		| value: Primary
			=> @(value)

	Primary
		: Value
		= '(' & value: Sum & ')'
			=> @(value)
		| value: Value
			=> @(value)

	IntNumber
		: @int
		= d: Digits
		=> @int.Parse(d)

	DoubleNumber
		: @double
		= d: (Digits & ('.' & Digits)?)
		=> @double.Parse(d, CultureInfo.InvariantCulture)

	parse Sum with (Value = IntNumber)    as EvaluateInt
	parse Sum with (Value = DoubleNumber) as EvaluateDouble
	""")]
public static partial class Calculator;
```

The generated API contains two independently specialized parsers:

```csharp
Calculator.EvaluateInt("7 / 2");       // 3

Calculator.EvaluateDouble("7 / 2");    // 3.5
Calculator.EvaluateDouble("1.5 * 4");  // 6

Calculator.TryEvaluateInt("1.5");      // no match
```

`Sum`, `Product`, `Unary`, and `Primary` are written only once. What separates the two
parsers is the publication:

```text
parse Sum with (Value = IntNumber)    as EvaluateInt
parse Sum with (Value = DoubleNumber) as EvaluateDouble
```

`with` substitutes a rule through the grammar reachable from that publication.

The result type follows the substitution too. `Sum : Value` means "the type produced by
`Value`", so the first generated parser returns `int` and the second returns `double`.

There is no runtime generic dispatch and no parser configuration object. Both parsers are
specialized when the C# is generated.

## Typed parsing

Named captures define the shape of the result.

```csharp
using DotGram;

[Gram("""
	Feed
		= header: Header
		& rows: Row*
		& trailer: Trailer
		& eof

	Header
		= "H" & '|' & date: Date & eol

	Row
		= "R"
		& '|' & symbol: Text
		& '|' & quantity: Digit+
		& eol

	Trailer
		= "T" & '|' & count: Digit+ & eol

	Date
		= year: Digit{4} & '-' & month: Digit{2} & '-' & day: Digit{2}

	Text  = [^ '|' | '\r' | '\n']+
	Digit = ['0'..'9']

	parse Feed
	find Row as AllRows
	""")]
public static partial class FeedParser;
```

The parser returns that structure directly:

```csharp
var feed = FeedParser.ParseFeed(text);

feed.Header.Date.Year;
feed.Rows[0].Symbol;
feed.Rows[0].Quantity;
feed.Trailer.Count;
```

There is no generic parse tree and no visitor required to turn it into application data.
Captures can also be matched straight to a constructor or to the `required` properties of
a type you already have, in which case the grammar contains no construction code at all.

`find` publishes a rule as a lazy search through the input:

```csharp
foreach (var row in FeedParser.AllRows(text))
	Console.WriteLine(row.Value.Symbol);
```

## C# is part of the grammar when you need it

`@` is the boundary between grammar and C#.

A rule can produce an existing C# type:

```csharp
using DotGram;

[Gram("""
	@using System.Globalization;

	Number
		: @double
		= text: (['0'..'9']+ & ('.' & ['0'..'9']+)?)
		=> @double.Parse(text, CultureInfo.InvariantCulture)

	parse Number
	""")]
public static partial class Numbers;
```

A guard can check values while parsing — the thing a grammar cannot say on its own:

```csharp
using DotGram;

[Gram("""
	Name = ['a'..'z' | 'A'..'Z']+

	Tag
		= '<' & open: Name & '>'
		& "</" & close: Name & '>'
		& when @(open == close)

	parse Tag
	""")]
public static partial class Tags;
```

The same boundary calls predicates, external recognizers, constructors, or any API at all.
Grammar describes the syntax; C# handles the parts that are already better expressed as C#.

## DotGram.Parsers

[`DotGram.Parsers`](src/DotGram.Parsers) is a library of parsers written in .Gram against
real specifications rather than demonstration grammars, and a package of its own.

| Parser | What it reads |
| --- | --- |
| [`Rfc3986`](src/DotGram.Parsers/Rfc3986.cs) | URIs and relative references after RFC 3986 — authority, IPv4, IPv6, `IPvFuture`, paths, queries, fragments, percent encoding |
| [`ExpressionLanguage`](src/DotGram.Parsers/ExpressionLanguage.cs) | a C#-style expression language that builds `System.Linq.Expressions` trees directly, with parameters, locals, blocks and `return` |
| [`SqlStandard92`](src/DotGram.Parsers/SqlStandard92.gram) | SQL-92, read through a lexical split |
| [`TransactSql`](src/DotGram.Parsers/TransactSql.gram) | T-SQL, written as a dialect over SQL-92 rather than as a copy of it |

[`src/DotGram.Parsers/README.md`](src/DotGram.Parsers/README.md) has what each one parses
and what it hands back.

## Streaming and recovery

Where the generator can prove that input may be released as parsing progresses, it emits
`TextReader` overloads beside the ordinary ones.

```csharp
using DotGram;

[Gram("""
	Text  = [^ '|' | '\r' | '\n']+
	Digit = ['0'..'9']

	Header          = "H" & '|' & Text & eol
	Row   : @string = "R" & '|' & t: Text & eol => @(t)
	Trailer         = "T" & '|' & Digit+ & eol

	Feed : @string[] = Header & Row* & Trailer & eof

	parse Feed
	""")]
public static partial class StreamingFeed;
```

`Feed` collects what its operands produce: `Row` builds a `string`, while the header and
trailer build nothing and so join nothing. Four methods are generated — `ParseFeed` and
`TryParseFeed` over a `string`, and `ParseFeed` over a `TextReader` and over an
`IEnumerable<string>`:

```csharp
using var reader = File.OpenText("large.feed");

foreach (var row in StreamingFeed.ParseFeed(reader))
	Handle(row);
```

The input buffer is reused instead of the complete input being held.

Record-oriented formats can also recover after malformed input:

```csharp
using DotGram;

[Gram("""
	Text = [^ '|' | '\r' | '\n']+

	Row : @string = "R" & '|' & t: Text & eol => @(t)

	Feed : @string[] = Row* recover eol => @(parserText)

	parse Feed
	""")]
public static partial class RecoveringFeed;
```

`recover eol` says where the repetition may pick itself up, and the `=>` says what to make
of what it rejected — here the text of the bad line, which arrives in the sequence beside
the good ones. A rejection can just as well become a record of its own carrying
`parserLine` and `parserMessage`, or go to a `partial void` hook and stay out of the
result entirely.

A bad record therefore becomes data describing the rejection, instead of ending the feed.

## What .Gram supports

* literals and element sets;
* ranges and Unicode categories;
* sequence and ordered choice;
* `?`, `*`, `+`, and bounded repetition;
* lookahead and atomic groups;
* named captures and generated result types;
* existing C# result types, filled by constructor or by `required` properties;
* semantic actions and guards;
* external C# predicates and recognizers;
* parameterized rules;
* rule rebinding and parser specialization;
* left recursion, and binding powers for expression grammars;
* grammar namespaces and reusable grammar libraries;
* `Parse`, `TryParse`, and `Find`;
* streaming from `TextReader`;
* recovery inside repetitions;
* parser context and parsing state.

[`docs/status.md`](docs/status.md) is the authoritative feature-by-feature status,
including current limitations.

## No runtime parser library

`DotGram` is a source-generator package. Everything needed to execute a generated parser
is emitted into the consuming assembly as internal C#.

```text
your assembly
 ├── your code
 ├── generated parser
 └── generated parser support
```

There is no DotGram runtime assembly to deploy, and no generator/runtime version pair that
can drift apart. The generator does the grammar-specific work during compilation; the
application executes the generated parser.

## Visual Studio

`DotGram.VisualStudio` is an extension for Visual Studio: classification, diagnostics,
Quick Info, navigation, completion, brace matching and folding, both for `.gram` files and
for a grammar written inside a `[Gram]` string.

It does the same for the language you generate. A parser host that names its language
lends that name to `StringSyntax`, and a literal passed to such a parameter is then edited
as that language rather than as text:

```csharp
[Gram("Filter.gram")]
[GramLanguage("filter")]
public static partial class FilterLanguage;

void Execute([StringSyntax("filter")] string query);

Execute("status = active");   // classified, checked, and completed as Filter
```

The annotation works on a parameter, on the receiver of an extension method, and on a
field or property initializer. [`docs/visual-studio.md`](docs/visual-studio.md) covers the
rest, along with building and installing; the extension targets Visual Studio 18.

## Examples

Complete examples are under [`examples/DotGram.Examples`](examples/DotGram.Examples/).

| Example | What it demonstrates |
| --- | --- |
| [`UrlExample.cs`](examples/DotGram.Examples/UrlExample.cs) | URL parsing, typed captures, `find` |
| [`FeedExample.cs`](examples/DotGram.Examples/FeedExample.cs) | record-oriented input and nested generated types |
| [`RecoveringFeedExample.cs`](examples/DotGram.Examples/RecoveringFeedExample.cs) | recovery after malformed records |
| [`StreamingFeedExample.cs`](examples/DotGram.Examples/StreamingFeedExample.cs) | streaming large input |
| [`TwoCalculatorsExample.cs`](examples/DotGram.Examples/TwoCalculatorsExample.cs) | one grammar specialized into multiple parsers |
| [`JsonExample.cs`](examples/DotGram.Examples/JsonExample.cs) | recursive structured data |
| [`XmlExample.cs`](examples/DotGram.Examples/XmlExample.cs) | a closing tag checked against its opening tag |
| [`FixExample.cs`](examples/DotGram.Examples/FixExample.cs) | FIX messages and C# validation |
| [`FilterExample.cs`](examples/DotGram.Examples/FilterExample.cs) | a small query language |
| [`TypedCsvExample.cs`](examples/DotGram.Examples/TypedCsvExample.cs) | construction of existing C# types |
| [`GramExample.cs`](examples/DotGram.Examples/GramExample.cs) | the .Gram notation parsed by .Gram itself |

See [`examples/README.md`](examples/README.md) for the complete list.

## Documentation

| Document | Contents |
| --- | --- |
| [`docs/syntax.md`](docs/syntax.md) | grammar notation and generated API |
| [`docs/implementation.md`](docs/implementation.md) | how the generated parser works |
| [`docs/diagnostics.md`](docs/diagnostics.md) | compiler diagnostics |
| [`docs/visual-studio.md`](docs/visual-studio.md) | the Visual Studio extension, and the `StringSyntax` annotations |
| [`docs/status.md`](docs/status.md) | implemented features, limitations, and measurements |

## Building

```sh
dotnet build DotGram.slnx
dotnet test  DotGram.slnx
```

## License

[MIT](LICENSE)
