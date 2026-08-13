# .Gram

A typed grammar notation for .NET, compiled to C# by a source generator.

> **Early work.** The pipeline runs end to end and the parsers it produces are real and
> typed, but the seam with C# — a rule declaring its own type, `=>`, `where` — is not
> built. Nothing here is ready to depend on.
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

Not built yet:

- rule types `: @T`, and `=>` construction — a rule's type is generated, never taken
  from C#
- `where` guards and `@(...)` C# interop at run time
- parameterized rules: `R(n)` is in the specification and does not parse
- C# name resolution beyond "the name exists"
- diagnostics beyond a position: the set of what was expected there is next
- `recover`, the recovery engine, streaming input, incremental parsing

## Examples

Whole parsers, meant to be copied — a grammar, the class it attaches to, and the code
written against it, with no test framework anywhere near them.

| | |
| --- | --- |
| [`UrlExample.cs`](examples/DotGram.Examples/UrlExample.cs) | a URL, after RFC 3986 — captures, optional parts, `find` |
| [`FeedExample.cs`](examples/DotGram.Examples/FeedExample.cs) | a line-oriented feed — nested rule values, a sequence of records, an envelope checked as a whole |

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
