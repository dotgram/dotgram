# .Gram

A typed grammar notation for .NET, compiled to C# by a source generator.

> **Early work.** The pipeline runs end to end and the parsers it produces are real,
> but a rule's value is still the text it matched — typed results are not built yet.
> Nothing here is ready to depend on. See [Where it stands](#where-it-stands).

## What it looks like

A grammar lives in a `.gram` file beside a partial class:

```csharp
[Gram]                                  // looks for Feed.gram
public partial class Feed;
```

```dotgram
Feed    = Header & Row* & Trailer & eof
Header  = "H" & Sep & Date & eol
Row     = "R" & Sep & Name & Sep & Amount & eol
Trailer = "T" & Sep & Count & eol

Sep     = '|'
Digit   = ['0'..'9']
Count   = Digit+
Date    = Digit{4} & '-' & Digit{2} & '-' & Digit{2}
Amount  = '-'? & Digit+ & ('.' & Digit{2})?
Name    = [^ '|' | '\n' | '\r']+

parse Feed
find all Row as AllRows
```

A rule on its own creates no public API — a directive does, and it produces the pair
of methods a .NET developer already knows from `int.Parse` and `int.TryParse`:

```csharp
var feed = Feed.ParseFeed(text);                    // throws FormatException

if (Feed.TryAllRows(text, out var rows, out var error, out var position))
	…
```

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
- sequence, ordered choice with full backtracking, quantifiers, lookahead
- rules calling rules, scopes and shadowing, the standard library
  (`any`, `none`, `eol`, `eof`, `Trivia`)
- whitespace handling by shadowing `Trivia`, which needs no notation of its own
- all four publication directives, and diagnostics that point into the `.gram` file

Not built yet:

- **typed results** — captures and `=>` parse and normalize, but a rule's value is
  still the matched text, so every published method returns `string`
- `where` guards and `@(...)` C# interop at run time
- diagnostics beyond "it did not match": the furthest failure position and the set of
  what was expected are next
- the recovery engine, streaming input, incremental parsing

## Documentation

| | |
| --- | --- |
| [`docs/syntax.md`](docs/syntax.md) | the language: the notation and its bond with C# |
| [`docs/implementation.md`](docs/implementation.md) | the engine: what is being built, and in what order |

Nothing decided in the second is a decision about the first.

## Building

```sh
dotnet build DotGram.slnx
dotnet test  DotGram.slnx
```

Tests run at three levels: direct calls into each stage, the generator driven in
memory, and the generator attached as an analyzer. `tests/Snapshots` holds a grammar
and the file it must compile into, so a change to code generation shows up as a diff.

## License

[MIT](LICENSE)
