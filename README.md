# .Gram

A typed grammar notation for .NET, compiled to C# by a source generator.

> **Early work.** The pipeline runs end to end, the parsers it produces are real and
> typed, a grammar can compute, and a marked repetition survives a bad element — but
> streaming and a good deal of the diagnostics are not built. Nothing here is ready to
> depend on.
>
> The specification describes the target language. Not every specified feature is
> implemented — [`docs/status.md`](docs/status.md) says which are.

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

No `out` parameters: what a match has to say is a value, and the next thing it has to
say is a field on it rather than another parameter on every signature.

## The two ideas

**Notation means what it already means.** Quantifiers are postfix and spelled as in
regular expressions (`X?`, `X*`, `X+`, `X{4}`), lookahead is `?=` and `?!`, `|` is
ordered choice, and `@` is the one door into C#. Every piece of syntax was chosen
against a single test: it means exactly one thing, and that thing is what it means in
C# or in .NET regular expressions.

**No runtime assembly ships.** Everything a generated parser needs is emitted into the
consumer's own compilation. You take one analyzer package, acquire no dependency, and
there is nowhere for a "generator of one version, runtime of another" skew to come
from. When shared types are wanted — to expose a parser in a library's public API —
one assembly declares `[assembly: GramRuntime]` and publishes them for the others.
The two modes are strictly additive: opting in only adds overloads.

## Where it stands

Working end to end — a `.gram` file becomes a parser that runs:

- literals, element sets with ranges and Unicode categories, complements
- sequence, ordered choice, quantifiers, lookahead — backtracking fully inside a rule,
  so a greedy operand gives back what the rest of the sequence cannot use
  ([`docs/status.md`](docs/status.md) says where that stops: at a rule boundary)
- rules calling rules, scopes and shadowing, the standard library
  (`any`, `none`, `eol`, `eof`, `Trivia`)
- whitespace handling by shadowing `Trivia`, which needs no notation of its own
- both publication directives, and diagnostics that point into the `.gram` file — and
  a refusal names the furthest position the input could be followed to
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
  `where` guard asks a question of the values while matching:

  ```dotgram
  Sum   : @int = left: Sum & op: ['+' | '-'] & right: Product => @(op == "+" ? left + right : left - right)
               | value: Product                             => @(value)
  ```

- **left recursion**, which is what makes associativity expressible without notation
  for it: `1-2-3` is -4 because `Sum` takes its left operand at its own level, and
  `2^3^2` is 512 because `Power` takes its right one there instead. Two calculators in
  `examples/` — one with precedence, parentheses and unary minus, one with both
  groupings side by side

- **binding powers**, for the expression language that levels cannot say. An alternative
  states its own strength, and `-1-2` is -3 because unary minus is stronger than the
  binary one it shares a character with — which no stacking of rules can express:

  ```dotgram
  Expr : @decimal = left: Expr & '+' & right: Expr  << 1 => @(left + right)
                  | left: Expr & '^' & right: Expr  >> 3 => @(Raise(left, right))
                  | '-' & operand: Expr             >> 4 => @(-operand)
  ```

  `<<` reads the operand to the right one strength tighter, so it groups left; `>>`
  reads it at the same strength, so it groups right

- **`recover`**, which is how a feed survives a bad record. The mark says that inside
  this repetition an element that starts and then fails is an error rather than the end
  of the sequence; the parser skips to the next synchronization point and reads on, and
  the `=>` puts what it skipped into the same sequence as the records:

  ```dotgram
  lines: Row* recover eol => @(new RejectedLine(ordinal, line, text, message))
  ```

  A rejection arrives in its place, carrying which record it was, where a person would
  open the file, and why — so nothing has to be joined back up afterwards

Not built yet:

- `: T` naming another rule, and matching captures to a constructor by name
- parameterized rules: `R(n)` is in the specification and does not parse
- diagnostics beyond a position: the set of what was expected there is next
- `recover` without a `=>`, reporting broken elements out of band rather than in the
  sequence
- streaming input, incremental parsing

## Examples

Whole parsers, meant to be copied — a grammar, the class it attaches to, and the code
written against it, with no test framework anywhere near them.

| | |
| --- | --- |
| [`UrlExample.cs`](examples/DotGram.Examples/UrlExample.cs) | a URL, after RFC 3986 — captures, optional parts, `find` |
| [`FeedExample.cs`](examples/DotGram.Examples/FeedExample.cs) | a line-oriented feed — nested rule values, a sequence of records, an envelope checked as a whole |
| [`RecoveringFeedExample.cs`](examples/DotGram.Examples/RecoveringFeedExample.cs) | the same feed, read past a malformed record — `recover`, and rejections that arrive in the sequence with the records |
| [`CalculatorExample.cs`](examples/DotGram.Examples/CalculatorExample.cs) | arithmetic — precedence, associativity, `: @int` and `=>`, whitespace by shadowing `Trivia` |
| [`DecimalCalculatorExample.cs`](examples/DotGram.Examples/DecimalCalculatorExample.cs) | the same, with `^` — left and right recursion side by side, `: @decimal`, and a scope that shadows `Trivia` back off |
| [`StrengthCalculatorExample.cs`](examples/DotGram.Examples/StrengthCalculatorExample.cs) | the same language in one rule — `<< n` and `>> n`, and the unary operator no stacking of levels can make stronger |
| [`ExpressionTreeExample.cs`](examples/DotGram.Examples/ExpressionTreeExample.cs) | the same grammar building a tree instead of a number — records out, patterns back in, and the shape a small DSL wants |

[`examples/README.md`](examples/README.md) says what to add to a project to take one.

## Documentation

| | |
| --- | --- |
| [`docs/syntax.md`](docs/syntax.md) | the language: the notation and its bond with C# |
| [`docs/implementation.md`](docs/implementation.md) | the engine: what is being built, and in what order |
| [`docs/status.md`](docs/status.md) | what actually works, feature by pipeline stage |

Nothing decided in the second is a decision about the first. The third is the only one
that describes today.

## Building

```sh
dotnet build DotGram.slnx
dotnet test  DotGram.slnx
```

Tests run at three levels: direct calls into each stage, the generator driven in
memory, and the generator attached as an analyzer. `tests/Snapshots` holds a grammar
and the file it must compile into, so a change to code generation shows up as a diff,
and `examples/` is compiled and run by the same command.

## License

[MIT](LICENSE)
