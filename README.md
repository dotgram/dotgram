# .Gram

A typed grammar notation for .NET, compiled to C# by a source generator.

**The grammar is the product; the parser is a generated artifact.** A `.gram` file is
the one place the language's meaning is written down; everything else — the generated
code, its speed, static analysis, diagnostics, streaming, tooling, a description handed
to an agent — exists to agree with that text, not to replace it. That holds even where
a hand-written parser turns out faster: what a grammar buys is meaning read
declaratively rather than reconstructed from someone's control flow, and speed does not
touch that. When the compiler also gets speed right — predictive dispatch, possessive
repetition, flat lowering, deferred construction — that is a bonus worth
having, not the reason to reach for this over a parser written by hand.

## What it looks like

A grammar lives in a `.gram` file beside a partial class:

```csharp
[Gram]                                  // looks for Feed.gram
public partial class Feed;
```

```dotgram
Feed    = header: Header & rows: Row* & trailer: Trailer & eof
Header  = "H" & Sep & date: Date & eol
Row     = "R" & Sep & name: Name & Sep & amount: Amount & eol
Trailer = "T" & Sep & count: Count & eol

Sep     = '|'
Digit   = ['0'..'9']
Count   = Digit+
Date    = Digit{4} & '-' & Digit{2} & '-' & Digit{2}
Amount  = '-'? & Digit+ & ('.' & Digit{2})?
Name    = [^ '|' | '\n' | '\r']+

parse Feed
find Row as AllRows
```

A rule on its own creates no public API — a directive does. There are two, and the
whole of the difference is whether input that does not match may sit between the
matches:

```csharp
var feed = Feed.ParseFeed(text);            // the whole input is a Feed, or it throws

feed.Rows[0].Name;                          // every capture is a property
feed.Trailer.Count;

if (Feed.TryParseFeed(text) is { IsSuccess: true } match)
	…                                       // or ask, and get Value, Error, Position

foreach (var row in Feed.AllRows(text))     // occurrences, found as they are asked for
	…
```

Everything else — one value against a lazy sequence, throwing against asking — follows
from that; none of it is a second decision.

No `out` parameters: what a match has to say is a value, and the next thing it has to
say is a field on it rather than another parameter on every signature.

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

Working end to end — a `.gram` file becomes a parser that runs:

- literals, element sets with ranges and Unicode categories, complements
- sequence, ordered choice, quantifiers, lookahead and explicit atomic groups —
  backtracking crosses ordinary rule calls, so extracting an expression into a rule
  does not change its meaning
- rules calling rules, namespaces and shadowing, the standard library
  (`any`, `none`, `eol`, `eof`, `trivia`)
- whitespace handling by shadowing `trivia`, which needs no notation of its own
- **rebinding** — a `namespace` header substitutes a rule across everything it
  reaches, not just what is written inside the block:

  ```dotgram
  B = 'b'
  A = B

  namespace Ns with (B = D)
  {
      E = A                    // E -> A, with B substituted -> D
  }

  D = 'd'
  ```

  `A` itself is untouched — nothing outside the namespace depends on it existing. The
  same substitution reuses an already-written rule under a different `trivia`, without a
  second copy of it
- both publication directives, and diagnostics that point into the `.gram` file — and
  a refusal names the furthest position the input could be followed to, and what would
  have fit there: `"Expected ')'."`, built live at the position rather than guessed
  afterward
- **typed results** — a rule with captures gets a type of its own, generated beside the
  parser, and every published method hands it back:

  ```dotgram
  Url       = scheme: Scheme & "://" & authority: Authority & path: Path
  Authority = (user: UserInfo & '@')? & host: Host & (':' & port: Digit+)?
  ```

  ```csharp
  var url = UrlGrammar.ParseUrl("https://user@example.com:8080/a");

  url.Authority.Host;   // "example.com"
  url.Authority.Port;   // "8080"
  url.Authority.User;   // "user", and null when there is none
  ```

  A capture the parser gave back on the way to a match is not in the result — a
  member's slot is cleared wherever an abandoned attempt is resumed from, which the
  generator works out while generating rather than the parser tracking as it runs.

- **sequences** — `rows: Row*` is a `Row[]`, so a whole feed comes back from one pass:

  ```dotgram
  Feed = header: Header & rows: Row* & trailer: Trailer & eof
  ```

  ```csharp
  var feed = FeedReader.ParseFeed(text);   // one header, a trailer, nothing after it

  feed.Rows[0].Symbol;
  feed.Trailer.Count;
  ```

- **the seam with C#** — a rule names its own type and says how to build it, and a
  `when` guard asks a question of the values while matching:

	```dotgram
	Sum   : @int = left: Sum & op: ['+' | '-'] & right: Product => @(op == "+" ? left + right : left - right)
	             | value: Product                             => @(value)
	Tag          = '<' & open: Name & '>' & "</" & close: Name & '>' & when @(open == close)
	```

- **left recursion**, which is what makes associativity expressible without notation
  for it: `1-2-3` is -4 because `Sum` takes its left operand at its own level, and
  `2^3^2` is 512 because `Power` takes its right one there instead. Two calculators in
  `examples/` — one with precedence, parentheses and unary minus, one with both
  groupings side by side

- **binding powers**, for when a whole expression language should be one rule instead of
  a stack of them. The alternative states its own strength rather than having it implied
  by which rule calls which:

  ```dotgram
  Expr : @decimal = left: Expr & '+' & right: Expr  << 1 => @(left + right)
                  | left: Expr & '*' & right: Expr  << 2 => @(left * right)
                  | left: Expr & '^' & right: Expr  >> 3 => @(Raise(left, right))
                  | '-' & operand: Expr             >> 3 => @(-operand)
  ```

  `<<` reads the operand to the right one strength tighter, so it groups left; `>>`
  reads it at the same strength, so it groups right. `examples/` has this calculator
  and its five-rule twin, tested against each other expression by expression

- **`recover`**, which is how a feed survives a bad record. The mark says that inside
  this repetition an element that starts and then fails is an error rather than the end
  of the sequence; the parser skips to the next synchronization point and reads on, and
  the `=>` puts what it skipped into the same sequence as the records:

  ```dotgram
  lines: Row* recover eol => @(new RejectedLine(parserOrdinal, parserLine, parserText, parserMessage))
  ```

  A rejection arrives in its place, carrying which record it was, where a person would
  open the file, and why — so nothing has to be joined back up afterwards. At each row
  boundary the complete continuation after the repetition is tried first, so a trailer
  wins there even when the row rule itself is broad

- **a C# predicate inside an element set** — `bool M(char c)` asks the same question
  about one input item that a range does. The brackets establish that contract and let
  the predicate merge with ranges:

  ```dotgram
  Start = ([@IsVowel] | ['0'..'9'])+ & [@IsStop]
  ```

- **and a C# method that reads the input itself**, for what a grammar spells badly — a
  length-prefixed run, a date in ten formats, anything the BCL already knows:

  ```csharp
  static bool Blob(ReadOnlySpan<char> input, ref int pos)
  ```

  A bare `@Blob` is the corresponding grammar operand. Its position, rather than the
  method's signature, tells the generator which call to emit.

  The `ref` is the method saying that it moves the position, and it is taken at its word:
  it is handed the parser's own, and nothing checks what came back. Reaching into the
  parse means taking the parse's invariants on with it, which §7.1 says in as many words

- **a rule that takes another rule** — written once, used with whatever it is given:

  ```dotgram
  List(item, sep) = item & (sep & item)*
  Digits(n)       = ['0'..'9']{n}

  Start = List(Word, Comma) & ' ' & Digits(4)
  ```

  By substitution: each call becomes a rule of its own with the parameters replaced, so
  nothing is dispatched at run time and a parameter can be a recognizer. An argument is
  a piece of grammar or a number, and a repetition count may name one

- **a result built without saying how** — a rule that declares its type and writes no
  `=>` is filled from its captures, matched by name:

  ```dotgram
  Row : @Trade = symbol: Symbol & ',' & size: Amount & ',' & on: Day & eol
  ```

  A constructor those captures cover is called; a type with none and `required`
  properties is made and written into. What that removes is the line that repeats in
  the grammar what the C# type already says and goes stale when a parameter is added

- **a rule that is a sequence of what it is made of** — the envelope and the records in
  one result, in the order they were read, with no `=>` anywhere:

  ```dotgram
  Feed : @FeedItem[] = Header & Row* & Trailer & eof
  ```

  Every operand whose value fits `FeedItem` joins; `Row*` contributes all of its
  elements; `eof` contributes nothing. It is also the shape a streamed parse needs,
  since a sequence is the only result that can be handed over one element at a time

- **streaming** — both directives read from a `TextReader`, through a buffer that is
  reused, so what is held is the part being read and not the file:

  ```csharp
  using var file = File.OpenText("huge.feed");

  foreach (var item in FeedGrammar.ParseFeed(file))   // header, records, trailer
      Handle(item);                                   // one at a time
  ```

  A sequence of lines is the same door — `ParseFeed(File.ReadLines(path))` — with the
  terminators put back, since lines have had them taken off.

  A `parse` gets the overload when its result is a sequence and every repetition in it
  ends where the grammar says rather than where backtracking finds — handing an element
  to the caller cannot be undone. A repetition that cannot tell its own end from what
  follows it may still be marked `recover`, which commits it (§8.2). A grammar that asks
  and does not qualify is told why

- **`find` over a `TextReader`**, the same thing for occurrences. The input is read
  through a buffer that is reused, so what is held is the occurrence being read and not
  the file:

  ```csharp
  using var file = File.OpenText("huge.log");

  foreach (var match in LogGrammar.AllUrls(file))   // the same occurrences, one at a time
      Handle(match.Value, match.Position);          // Position is a long, into the input
  ```

  The overload appears only where the grammar provably works with a reused buffer — a
  rule that could give back any of the file gets none — and an occurrence straddling a
  buffer boundary is still one occurrence, which is the part that has to be got right

Not built yet:

Each of these is refused with the reason where it can be — a construct that parses and
then quietly means nothing is the failure this project is most careful about:

- a value parameter that is not a number — `Padded(item, pad: char)` handed a literal is
  refused rather than quietly taken as a recognizer
- indirect left recursion, except through rules that only forward — `Call` reaching itself
  through a `Primary` that does nothing but hand its alternatives on is rewritten and works;
  a chain of rules that recurse through each other, or one where a rule between does
  something of its own, is refused
- the allocation-free `Read()`/`Current` and generated-outcome surfaces from §8.3;
  typed streamed results and the `OnRecovered` sink already work
- incremental parsing

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

[`examples/README.md`](examples/README.md) says what to add to a project to take one.

## Documentation

| | |
| --- | --- |
| [`docs/syntax.md`](docs/syntax.md) | the language: the notation and its bond with C# |
| [`docs/implementation.md`](docs/implementation.md) | the engine: how it executes the language |
| [`docs/diagnostics.md`](docs/diagnostics.md) | every message it can report, and what to do about it |
| [`docs/status.md`](docs/status.md) | what actually works, feature by pipeline stage |

Nothing decided in the second is a decision about the first. The second describes how
the current engine works; the third says how much of the language it covers so far.

## Building

```sh
dotnet build DotGram.slnx
dotnet test  DotGram.slnx
```

Tests run at three levels: direct calls into each stage, the generator driven in
memory, and the generator attached as an analyzer. `tests/Snapshots` holds a grammar
and the file it must compile into, so a change to code generation shows up as a diff,
and `examples/` is compiled and run by the same command.

Benchmarks are run by hand and not by CI — a number from a shared runner is a number
about the runner:

```sh
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --filter "*"
```

They compare the `Url` grammar against the same language as a regular expression, and a
feed read three ways — all in memory, from a `TextReader`, and from `File.ReadLines`.
[`docs/status.md`](docs/status.md) records what they said.

## License

[MIT](LICENSE)
