*[Русский](README.ru.md)*

# .Gram

A typed grammar notation for .NET, compiled to C# by a source generator.

**The grammar is the product; the parser is a generated artifact.** A `.gram` file is
the one place the language's meaning is written down; everything else — the generated
code, its speed, static analysis, diagnostics, streaming, tooling, a description handed
to an agent — exists to agree with that text, not to replace it. That holds even where
a hand-written parser turns out faster: what a grammar buys is meaning read
declaratively rather than reconstructed from someone's control flow, and speed does not
touch that. When the compiler also gets speed right — predictive dispatch, possessive
repetition, flat lowering, deferred construction — that is a bonus worth having, not the
reason to reach for this over a parser written by hand.

## Getting started

Two things, and nothing else:

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
in it — nothing to wire up and nothing to name twice. The class must be `partial`, and
every class around it too; `static` is fine.

A grammar long enough to want its own place goes in a `.gram` file instead, named after
the class or given to the attribute, and listed so the generator can see it:

```xml
<AdditionalFiles Include="Numbers.gram" />
```

No runtime assembly is referenced, because there is none: everything the parser needs is
generated into your own compilation.

## Where you would have written a regular expression

A URL, after RFC 3986. This is the shape that usually ends up as a regular expression
nobody can read a year later:

```dotgram
Url        = scheme: Scheme & "://" & authority: Authority & path: Path
           & ('?' & query: Rest)? & ('#' & fragment: Rest)?

Scheme     = "https"i | "http"i | "ftp"i

Authority  = (user: UserInfo & '@')? & host: Host & (':' & port: Digit+)?
Host       = IPv4 | RegName

IPv4       = Octet & '.' & Octet & '.' & Octet & '.' & Octet
Octet      = Digit{1,3}

UserInfo   = (Unreserved | SubDelim | PctEncoded | ':')+
RegName    = (Unreserved | SubDelim | PctEncoded)+
// Path, Rest, Digit, Unreserved, SubDelim and PctEncoded are rules too — see the example

parse Url
find  Url as FindUrls
```

Quantifiers, character classes and alternation are spelled as in regular expressions
because they mean the same things. What a regular expression has no way to say is the
rest: `scheme:` and `host:` are captures, and they come back as named properties rather
than as numbered groups.

It is also faster. `benchmarks/` runs this grammar against the same language written as
a regular expression and refuses to time anything until both agree on every part of every
input; generated parsing beats `RegexOptions.Compiled` on all five benchmarked inputs —
133.8 ns against 298.9 for a short URL, 191.0 against 453.0 for an 84-character path —
and interpreted `Regex` by 2.2× to 6.5×. [`docs/status.md`](docs/status.md) has the table
and what it does not prove.

## Captures are the result

```dotgram
Feed    = header: Header & rows: Row* & trailer: Trailer & eof

Header  = "H" & '|' & date: Date & '|' & source: Text & eol
Row     = "R" & '|' & symbol: Text & '|' & qty: Digit+ & '|' & date: Date & eol
Trailer = "T" & '|' & count: Digit+ & eol

Date    = year: Digit{4} & '-' & month: Digit{2} & '-' & day: Digit{2}

Text    = [^ '|' | '\r' | '\n']+
Digit   = ['0'..'9']

parse Feed
find Row as AllRows
```

A rule on its own creates no public API — a directive does. There are two, and the whole
of the difference is whether input that does not match may sit between the matches:

```csharp
var feed = ParseFeed(text);              // the whole input is a Feed, or it throws

feed.Rows[0].Symbol;                     // every capture is a property
feed.Trailer.Count;
feed.Header.Date.Year;                   // and a rule's own captures are its own type

if (TryParseFeed(text) is { IsSuccess: true } match)
	…                                    // or ask, and get Value, Error, Position

foreach (var found in AllRows(text))     // occurrences, found as they are asked for
	…                                    // found.Value is a Row
```

Everything else — one value against a lazy sequence, throwing against asking — follows
from that; none of it is a second decision. No `out` parameters: what a match has to say
is a value, and the next thing it has to say is a field on it rather than another
parameter on every signature.

## One grammar, two parsers

A rebinding substitutes a rule across everything a publication reaches. Put it on the
directive and one grammar publishes twice:

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
TwoCalculators.EvaluateInt("7/2");        // 3        — an int
TwoCalculators.EvaluateDecimal("7/2");    // 3.5      — a decimal
TwoCalculators.TryEvaluateInt("1.5");     // no match — that calculator has no decimal point
```

The arithmetic is written once and names no type. `Sum : Value` says "whatever `Value`
produces", so the type follows the substitution out to the published method — one returns
`int`, the other `decimal`, from the same four rules. `left + right` is C#'s `+` on
whichever number arrived.

There is no generic rule here and no type parameter. A rebinding is a substitution, and a
substitution changes what the rules around it produce as readily as what they read.

## Real formats

[`src/DotGram.Parsers`](src/DotGram.Parsers) is not teaching material. An example shows
one feature; a parser there answers whether the notation is enough for a whole
specification — and, being an ordinary project the generator runs over, it is where the
seam between grammar and C# is exercised against a real compilation.

**[`Rfc3986`](src/DotGram.Parsers/Rfc3986.cs)** — URI references as the RFC divides them,
with every part left as it was written. When to decode a percent-escape is the
application's question and not the parser's, so `Decode` sits beside the parts rather
than inside them.

**[`ExpressionLanguage`](src/DotGram.Parsers/ExpressionLanguage.cs)** — a small language
that compiles to a .NET expression tree:

```csharp
ExpressionLanguage.Compile<Func<int, int>>("(int x) => x * x - 1")(3);   // 8

ExpressionLanguage.Compile<Func<int, int, int>>(
	"(int x, int y) => { int sum = x + y; return sum * sum; }")(2, 3);  // 25
```

What it reads is C#'s expression syntax — the same precedence ladder in the same order,
the same operators at each level, the same literal forms down to the digit separator and
the verbatim string. And **every `=>` in it is a call into `System.Linq.Expressions` by
name**: one alternative per operator, each naming the factory that builds it. There is no
model of this project's own in between, and no dispatch on an operator's text. A factory
that does not exist, or one handed the wrong type, is a C# error reported on the line of
the grammar that asked for it — which the same choice made by a `switch` over `op` would
have turned into a run-time exception in a library instead.

It carries what a language of that size needs: a `context` the parse works out and hands
to its own semantic code, `with state` for what holds while something is being read, and
recovery for reading past what is broken.

## The two ideas

**Notation means what it already means.** Quantifiers are postfix and spelled as in
regular expressions (`X?`, `X*`, `X+`, `X{4}`), lookahead is `?=` and `?!`, `|` is
ordered choice, and `@` is the one door into C#: a predicate, a recognizer, a guard, a
construction all cross through it, and it means the same one thing at every one of
them — what follows is C#. Every piece of syntax was chosen against a single test: it
means exactly one thing, and that thing is what it means in C# or in .NET regular
expressions.

**No runtime assembly ships.** Everything a generated parser needs is emitted into the
consumer's own compilation, and all of it `internal`. You take one analyzer package,
acquire no dependency, and there is nowhere for a "generator of one version, runtime of
another" skew to come from — an internal type is invisible across an assembly boundary,
so two assemblies that both emit one never have to agree about it.

## Where it stands

The notation is built and the pipeline runs end to end: elements, sequence and ordered
choice, quantifiers, lookahead and atomic groups, rules and namespaces and rebinding,
precedence and associativity, captures and construction, guards, parameterized rules,
external recognizers, publication as a value or a lazy sequence, reading from a
`TextReader`, and recovery inside a repetition.

What is not built is written down rather than left to be discovered.
[`docs/status.md`](docs/status.md) is that document, feature by pipeline stage, with the
measurements each claim rests on.

## Examples

Whole parsers, meant to be copied — a grammar, the class it attaches to, and the code
written against it, with no test framework anywhere near them.

| | |
| --- | --- |
| [`UrlExample.cs`](examples/DotGram.Examples/UrlExample.cs) | a URL, after RFC 3986 — captures, optional parts, `find` |
| [`FeedExample.cs`](examples/DotGram.Examples/FeedExample.cs) | a line-oriented feed — nested rule values, a sequence of records, an envelope checked as a whole |
| [`RecoveringFeedExample.cs`](examples/DotGram.Examples/RecoveringFeedExample.cs) | the same feed, read past a malformed record — `recover`, and rejections that arrive in the sequence with the records |
| [`LoggingFeedExample.cs`](examples/DotGram.Examples/LoggingFeedExample.cs) | the same again with the rejections sent elsewhere — `recover` with no `=>`, and the `partial void` that vanishes when nobody implements it |
| [`StreamingFeedExample.cs`](examples/DotGram.Examples/StreamingFeedExample.cs) | the same feed out of a `TextReader` — a result that comes in parts, a window that is reused, and a trailer checked against records nobody held |
| [`TwoCalculatorsExample.cs`](examples/DotGram.Examples/TwoCalculatorsExample.cs) | one grammar published twice — `parse Sum with (Value = …)`, an `int` calculator and a `decimal` one from the same arithmetic |
| [`CalculatorExample.cs`](examples/DotGram.Examples/CalculatorExample.cs) | arithmetic — precedence, associativity, `: @int` and `=>`, whitespace by shadowing `trivia` |
| [`DecimalCalculatorExample.cs`](examples/DotGram.Examples/DecimalCalculatorExample.cs) | the same, with `^` — left and right recursion side by side, `: @decimal`, and a namespace that shadows `trivia` back off |
| [`StrengthCalculatorExample.cs`](examples/DotGram.Examples/StrengthCalculatorExample.cs) | the one before it written the other way — `<< n` and `>> n` in one rule instead of five, checked against it expression by expression |
| [`LocaleNumberExample.cs`](examples/DotGram.Examples/LocaleNumberExample.cs) | one decimal-number rule, published under two decimal points — `namespace Name with (A = B) { ... }` reusing a rule rather than a namespace shadowing one locally |
| [`ExpressionTreeExample.cs`](examples/DotGram.Examples/ExpressionTreeExample.cs) | the same grammar building a tree instead of a number — one record per operation, patterns back in, and the shape a small DSL wants |
| [`OneRuleTreeExample.cs`](examples/DotGram.Examples/OneRuleTreeExample.cs) | that tree from one rule of eight lines — the whole of a small DSL in one place, and the same nodes its five-rule twin builds |
| [`Expression.cs`](examples/DotGram.Examples/Expression.cs) | the tree those two build, and everything it can do. No grammar in it, deliberately |
| [`JsonExample.cs`](examples/DotGram.Examples/JsonExample.cs) | JSON — a value that is any of six things nested inside itself, and one parameterized list written once for members and elements |
| [`XmlExample.cs`](examples/DotGram.Examples/XmlExample.cs) | XML — a closing tag checked against the tag it closes with a `when`, which is the thing no grammar can say on its own |
| [`MarkdownExample.cs`](examples/DotGram.Examples/MarkdownExample.cs) | Markdown blocks — a format where the line is the unit, ordered choice carrying a definition, and every newline written down |
| [`FixExample.cs`](examples/DotGram.Examples/FixExample.cs) | FIX messages — fields in order because a tag may repeat, and a checksum done in C# because arithmetic over the matched bytes is not a shape |
| [`FilterExample.cs`](examples/DotGram.Examples/FilterExample.cs) | `Price > 10 AND Country IN ('UK','DE')` — heterogeneous literals, an operator whose right side is a list, and a tree a caller evaluates against their own data |
| [`NetstringExample.cs`](examples/DotGram.Examples/NetstringExample.cs) | a frame that says how long it is — the one shape a grammar cannot express, handed to a C# recognizer (§7.1) one step at a time |
| [`FixedWidthExample.cs`](examples/DotGram.Examples/FixedWidthExample.cs) | records with no delimiters at all — widths in the grammar rather than substring arithmetic, and one rule per field kind parameterized by width |
| [`HttpHeadersExample.cs`](examples/DotGram.Examples/HttpHeadersExample.cs) | header fields, where a value may continue on the next line — the one format here whose value spans lines, and a lookup that ignores case |
| [`IniExample.cs`](examples/DotGram.Examples/IniExample.cs) | an INI file read into a dictionary of dictionaries — a sequence result folded into the lookup a caller actually wants |
| [`SqlReadOnlyExample.cs`](examples/DotGram.Examples/SqlReadOnlyExample.cs) | a guard that answers whether a statement can write — exact SQL lexis, because every bypass lives in the strings and comments |
| [`TypedCsvExample.cs`](examples/DotGram.Examples/TypedCsvExample.cs) | a CSV read into records with no `=>` anywhere — captures matched to a constructor and to `required` properties, and the same feed out of a reader |
| [`GramExample.cs`](examples/DotGram.Examples/GramExample.cs) | the notation's own grammar, written in itself |

[`examples/README.md`](examples/README.md) says what to add to a project to take one.

## Documentation

| | |
| --- | --- |
| [`docs/syntax.md`](docs/syntax.md) | the language: the notation and its bond with C# |
| [`docs/implementation.md`](docs/implementation.md) | the engine: how it executes the language |
| [`docs/diagnostics.md`](docs/diagnostics.md) | every message it can report, and what to do about it |
| [`docs/status.md`](docs/status.md) | what actually works, feature by pipeline stage |

Nothing decided in the second is a decision about the first. The second describes how
the first is executed, and may be replaced entirely without the language changing.

## Building

```sh
dotnet build DotGram.slnx
dotnet test  DotGram.slnx
```

[`docs/development.md`](docs/development.md) has the rest — the snapshot baseline, how
the benchmarks are run and why not by CI, and what a change owes.

## License

[MIT](LICENSE)
