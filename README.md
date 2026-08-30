*[Русский](README.ru.md)*

# .Gram

A source generator that compiles a grammar into a C# parser.

You write the format once, as rules. From that one file the generator produces:

- the parser, as ordinary C# in your own assembly — no runtime library is referenced;
- the result types the captures imply, so a match comes back as `row.Symbol` rather than
  as `match.Groups[3].Value`;
- `Parse`, `TryParse` and `Find` methods, with failures that carry a position and say what
  was expected there;
- overloads that read from a `TextReader` without holding the input, where the grammar
  permits it;
- compile-time errors in your build for what the grammar gets wrong, at the character in
  the grammar that is wrong.

It is aimed at the formats a program actually has to read — feeds, configuration,
protocols, query and expression languages — and at the point where a regular expression
has stopped being readable or a hand-written reader has stopped being trustworthy.

```dotgram
Row = "R" & '|' & symbol: Text & '|' & qty: Digit+ & eol
```

```csharp
row.Symbol      // a property, because `symbol:` is a capture
row.Qty
```

## Getting started

```xml
<PackageReference Include="DotGram" Version="0.1.0"
                  PrivateAssets="all" ExcludeAssets="runtime" />
```

```csharp
[Gram("""
	Digits = ['0'..'9']+
	parse Digits
	""")]
public static partial class Numbers
{
	public static int Length(string text) => ParseDigits(text).Length;   // ParseDigits is generated here
}
```

The attribute goes on the class you want the parser in, and the methods and types appear
in it. The class must be `partial`, and every class around it too; `static` is fine. A
longer grammar goes in a `.gram` file listed as
`<AdditionalFiles Include="Numbers.gram" />`.

Nothing is referenced at run time, because there is nothing to reference: everything the
parser needs is generated into your own compilation.

## Instead of a regular expression

Quantifiers, character classes and alternation are spelled as in regular expressions,
because they mean the same things. What a regular expression cannot do is name the parts:

```dotgram
Url        = scheme: Scheme & "://" & authority: Authority & path: Path
           & ('?' & query: Rest)? & ('#' & fragment: Rest)?

Scheme     = "https"i | "http"i | "ftp"i
Authority  = (user: UserInfo & '@')? & host: Host & (':' & port: Digit+)?
Host       = IPv4 | RegName

parse Url
find  Url as FindUrls
```

`scheme:` and `host:` come back as named properties of a generated type, not as numbered
groups whose order a caller has to remember. A rule that grows keeps its name; a regular
expression that grows loses its reader.

It is also faster. The benchmark runs this grammar against the same language written as a
regular expression, and refuses to time either until both agree on every part of every
input:

| Input | .Gram | `RegexOptions.Compiled` |
| --- | ---: | ---: |
| short URL | 133.8 ns | 298.9 ns |
| host as an IP address | 146.9 ns | 285.4 ns |
| a refusal | 80.2 ns | 113.5 ns |
| 84-character path | 191.0 ns | 453.0 ns |

Against interpreted `Regex`, 2.2× to 6.5×. [`docs/status.md`](docs/status.md) has the
conditions and what the numbers do not prove.

## Instead of a hand-written reader

A line-oriented feed, which is where most of the parsing work in a business actually is:

```dotgram
Feed    = header: Header & rows: Row* & trailer: Trailer & eof

Header  = "H" & '|' & date: Date & '|' & source: Text & eol
Row     = "R" & '|' & symbol: Text & '|' & qty: Digit+ & '|' & date: Date & eol
Trailer = "T" & '|' & count: Digit+ & eol

Date    = year: Digit{4} & '-' & month: Digit{2} & '-' & day: Digit{2}

parse Feed
find Row as AllRows
```

```csharp
var feed = ParseFeed(text);              // the whole input is a Feed, or it throws

feed.Rows[0].Symbol;                     // captures are properties
feed.Header.Date.Year;                   // a rule's own captures are its own type

if (TryParseFeed(text) is { IsSuccess: true } match)
	…                                    // or ask, and get Value, Error, Position

foreach (var found in AllRows(text))     // occurrences, found as they are asked for
	…
```

There is no visitor and no parse tree to walk: the types are generated from the captures,
so the shape you read in the grammar is the shape you get in C#. Captures can also be
matched straight to a constructor or to the `required` properties of a type you already
have, in which case the grammar contains no construction code at all.

## Feeds that do not fit in memory

The same grammar reads from a `TextReader`. The overload is emitted where the generator
can prove the grammar works against a window that is reused rather than a string that is
held:

```csharp
foreach (var part in ParseFeed(reader))       // parts arrive as they are read
	…

foreach (var part in ParseFeed(File.ReadLines(path)))
	…
```

Ten thousand records, the same feed and the same parts built, given three ways:

| Input | Time | Allocated | Gen2 collections |
| --- | ---: | ---: | ---: |
| `string` | 719 µs | 2653 KB | 249 |
| `TextReader` | 433 µs | 1415 KB | 0 |
| `IEnumerable<string>` | 518 µs | 1884 KB | 0 |

Streaming is not a slower mode paid for with robustness: it holds one part at a time and
never reaches the large object heap.

**And a bad record does not end the run.** `recover` says where a repetition may pick
itself up, and what to make of what it rejected:

```dotgram
Feed = header: Header
     & lines:  Row* recover eol => @(new RejectedLine(parserOrdinal, parserLine, parserText, parserMessage))
     & trailer: Trailer & eof
```

Rejections arrive in the sequence beside the records, carrying their line number and the
message the parser would otherwise have thrown — or, with no `=>`, go to a `partial void`
hook that disappears entirely when nobody implements it.

## One grammar, many parsers

A rebinding substitutes a rule across everything a publication reaches. Put it on the
directive, and the same grammar publishes more than once:

```dotgram
IntNumber     : @int     = d: Digits                     => @int.Parse(d)
DecimalNumber : @decimal = d: (Digits & ('.' & Digits)?) => @(Decimal(d))

Value : @int = d: Digits => @int.Parse(d)

Sum     : Value = left: Sum     & op: ['+' | '-'] & right: Product => @(op == "+" ? left + right : left - right)
                | value: Product                                   => @(value)

Product : Value = left: Product & op: ['*' | '/'] & right: Unary   => @(op == "*" ? left * right : left / right)
                | value: Unary                                     => @(value)

Unary   : Value = '-' & operand: Unary                             => @(-operand)
                | value: Primary                                   => @(value)

Primary : Value = '(' & inner: Sum & ')'                           => @(inner)
                | value: Value                                     => @(value)

parse Sum with (Value = IntNumber)     as EvaluateInt
parse Sum with (Value = DecimalNumber) as EvaluateDecimal
```

```csharp
EvaluateInt("7/2");          // 3        — an int
EvaluateDecimal("7/2");      // 3.5      — a decimal
TryEvaluateInt("1.5");       // no match — that parser has no decimal point in it
```

The arithmetic is written once and names no type. `Sum : Value` says "whatever `Value`
produces", so the type follows the substitution out to the published method. There is no
generic rule and no type parameter: a rebinding is a substitution, and a substitution
changes what the rules around it produce as readily as what they read.

The same mechanism gives one number grammar two decimal points, one list grammar two
separators, or one protocol two dialects.

## Grammars as libraries

A grammar is not confined to one class. Give a class a grammar, inherit from it, and its
rules are in scope:

```csharp
[Gram("Word = ['a'..'z']+\nName = Word & ('.' & Word)*")]
public partial class Lexemes { }

[Gram("using Lexemes;\nStart : @string = w: Name => @(w)\nparse Start")]
public partial class Reader : Lexemes { }
```

`using Lexemes;` brings the base's rules in under a namespace of their own, so nothing
collides and nothing is copied. Shared lexis — identifiers, numbers, string literals,
comment syntax — is written once and included by everything that needs it, across projects
as readily as within one.

## One door into C#

`@` is the only way through, and it means the same thing everywhere: what follows is C#.
A predicate, an external recognizer, a guard over what has been captured, a construction —
all cross at the same door.

[`src/DotGram.Parsers`](src/DotGram.Parsers) is where that is put to a whole specification
rather than to an example.
**[`ExpressionLanguage`](src/DotGram.Parsers/ExpressionLanguage.cs)** reads C#'s expression
syntax and compiles it to a .NET expression tree:

```csharp
ExpressionLanguage.Compile<Func<int, int>>("(int x) => x * x - 1")(3);   // 8

ExpressionLanguage.Compile<Func<int, int, int>>(
	"(int x, int y) => { int sum = x + y; return sum * sum; }")(2, 3);  // 25
```

Every `=>` in it names a factory of `System.Linq.Expressions` directly — one alternative
per operator, no model of this project's own in between, and no dispatch on an operator's
text. A factory that does not exist, or one handed the wrong type, is a C# error reported
on the line of the grammar that asked for it, rather than an exception at run time.

**[`Rfc3986`](src/DotGram.Parsers/Rfc3986.cs)** is the other one: URI references as the RFC
divides them, with every part left exactly as it was written, because when to decode a
percent-escape is the application's question and not the parser's.

## No runtime assembly

Everything a generated parser needs is emitted into the consumer's own compilation, and
all of it `internal`. You take one analyzer package and acquire no dependency. There is
nowhere for a "generator of one version, runtime of another" skew to come from: an
internal type is invisible across an assembly boundary, so two assemblies that both emit
one never have to agree about it.

## Where it stands

The notation is built and the pipeline runs end to end: elements, sequence and ordered
choice, quantifiers, lookahead and atomic groups, rules, namespaces and rebinding,
precedence and associativity, captures and construction, guards, parameterized rules,
external recognizers, publication as a value or a lazy sequence, reading from a
`TextReader`, recovery inside a repetition, and grammars included from a base class.

What is not built is written down rather than left to be discovered.
[`docs/status.md`](docs/status.md) is that document, feature by feature, with the
measurement each claim rests on.

## Examples

Whole parsers, meant to be copied — a grammar, the class it attaches to, and the code
written against it.

| | |
| --- | --- |
| [`UrlExample.cs`](examples/DotGram.Examples/UrlExample.cs) | a URL, after RFC 3986 — captures, optional parts, `find` |
| [`FeedExample.cs`](examples/DotGram.Examples/FeedExample.cs) | a line-oriented feed — nested rule values, a sequence of records, an envelope checked as a whole |
| [`RecoveringFeedExample.cs`](examples/DotGram.Examples/RecoveringFeedExample.cs) | the same feed, read past a malformed record — `recover`, and rejections that arrive in the sequence with the records |
| [`LoggingFeedExample.cs`](examples/DotGram.Examples/LoggingFeedExample.cs) | the same again with the rejections sent elsewhere — `recover` with no `=>`, and a `partial void` that vanishes when nobody implements it |
| [`StreamingFeedExample.cs`](examples/DotGram.Examples/StreamingFeedExample.cs) | the same feed out of a `TextReader` — a result that comes in parts, a window that is reused, and a trailer checked against records nobody held |
| [`TwoCalculatorsExample.cs`](examples/DotGram.Examples/TwoCalculatorsExample.cs) | one grammar published twice — an `int` calculator and a `decimal` one from the same arithmetic |
| [`CalculatorExample.cs`](examples/DotGram.Examples/CalculatorExample.cs) | arithmetic — precedence, associativity, `: @int` and `=>`, whitespace by shadowing `trivia` |
| [`DecimalCalculatorExample.cs`](examples/DotGram.Examples/DecimalCalculatorExample.cs) | the same, with `^` — left and right recursion side by side |
| [`StrengthCalculatorExample.cs`](examples/DotGram.Examples/StrengthCalculatorExample.cs) | the one before it written the other way — `<< n` and `>> n` in one rule instead of five |
| [`LocaleNumberExample.cs`](examples/DotGram.Examples/LocaleNumberExample.cs) | one decimal-number rule published under two decimal points |
| [`ExpressionTreeExample.cs`](examples/DotGram.Examples/ExpressionTreeExample.cs) | the same grammar building a tree instead of a number — the shape a small DSL wants |
| [`OneRuleTreeExample.cs`](examples/DotGram.Examples/OneRuleTreeExample.cs) | that tree from one rule of eight lines, building the same nodes |
| [`Expression.cs`](examples/DotGram.Examples/Expression.cs) | the tree those two build, and everything it can do. No grammar in it, deliberately |
| [`JsonExample.cs`](examples/DotGram.Examples/JsonExample.cs) | JSON — a value that is any of six things nested inside itself, and one parameterized list written once |
| [`XmlExample.cs`](examples/DotGram.Examples/XmlExample.cs) | XML — a closing tag checked against the tag it closes with a `when` |
| [`MarkdownExample.cs`](examples/DotGram.Examples/MarkdownExample.cs) | Markdown blocks — a format where the line is the unit |
| [`FixExample.cs`](examples/DotGram.Examples/FixExample.cs) | FIX messages — fields in order because a tag may repeat, and a checksum done in C# |
| [`FilterExample.cs`](examples/DotGram.Examples/FilterExample.cs) | `Price > 10 AND Country IN ('UK','DE')` — a tree a caller evaluates against their own data |
| [`NetstringExample.cs`](examples/DotGram.Examples/NetstringExample.cs) | a frame that says how long it is — handed to a C# recognizer, the one shape a grammar cannot express |
| [`FixedWidthExample.cs`](examples/DotGram.Examples/FixedWidthExample.cs) | records with no delimiters — widths in the grammar rather than substring arithmetic |
| [`HttpHeadersExample.cs`](examples/DotGram.Examples/HttpHeadersExample.cs) | header fields, where a value may continue on the next line |
| [`IniExample.cs`](examples/DotGram.Examples/IniExample.cs) | an INI file read into a dictionary of dictionaries |
| [`SqlReadOnlyExample.cs`](examples/DotGram.Examples/SqlReadOnlyExample.cs) | a guard that answers whether a statement can write — exact SQL lexis |
| [`TypedCsvExample.cs`](examples/DotGram.Examples/TypedCsvExample.cs) | a CSV read into records with no `=>` anywhere — captures matched to a constructor and to `required` properties |
| [`GramExample.cs`](examples/DotGram.Examples/GramExample.cs) | the notation's own grammar, written in itself |

[`examples/README.md`](examples/README.md) says what to add to a project to take one.

## Documentation

| | |
| --- | --- |
| [`docs/syntax.md`](docs/syntax.md) | the language: the notation and its bond with C# |
| [`docs/implementation.md`](docs/implementation.md) | the engine: how it executes the language |
| [`docs/diagnostics.md`](docs/diagnostics.md) | every message it can report, and what to do about it |
| [`docs/status.md`](docs/status.md) | what works, feature by feature, with the measurements |

Nothing decided in the second is a decision about the first. The second describes how the
first is executed, and may be replaced entirely without the language changing.

## Building

```sh
dotnet build DotGram.slnx
dotnet test  DotGram.slnx
```

[`docs/development.md`](docs/development.md) has the rest.

## License

[MIT](LICENSE)
