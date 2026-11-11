# The .Gram language, a guide

A guide to writing grammars, one topic a page, each feature with what it means, a small grammar that uses it,
the cases worth knowing, and the form the generator refuses with the diagnostic it gives. The specification is
[`syntax.md`](../../syntax.md); every message is in [`diagnostics.md`](../../diagnostics.md); the tool that
generates outside a build is [`cli.md`](../../cli.md).

| Page | What it covers |
| --- | --- |
| [The grammar and its host](01-grammar-and-host.md) | `[Gram]`, rules and declarations, `@` and C#, imports; a first look at trivia, `Std`, the built-in rules and the supplied names |
| [Elements](02-elements.md) | literals and escapes, case-insensitive literals, element sets, `any`, `none`, `eol`, `eof`, `eos` |
| [Composition](03-composition.md) | `&`, `\|`, groups, atomic groups, quantifiers, lookahead, the order of operators |
| [Captures and values](04-values.md) | captures, what a rule gives back, `=>`, the supplied names, `on fail` |
| [Guards and `switch`](05-guards-and-switch.md) | `when @(…)`, conditions `when A is B`, `switch` |
| [Text bindings](06-bindings.md) | `$q: X`, `$q`, `X{#q}` |
| [Rules](07-rules.md) | declarations, parameters, rules that give back, levels, left recursion, binding powers |
| [Trivia and words](08-trivia-and-words.md) | `trivia`, `~`, `wordboundary`, `word` |
| [Namespaces](09-namespaces.md) | namespaces, imports, rebinding with `with` |
| [The standard library](10-std.md) | every rule of `Std` |
| [Publication](11-publication.md) | `parse`, `find`, `as`, access, `positional`, `stream`, `yield` |
| [Recovery](12-recovery.md) | `recover`, the names a recovery is handed, `OnRecovered` |
| [Context and state](13-context-and-state.md) | `context`, `state`, `with state` |
| [The seam with C#](14-csharp-seam.md) | predicates, external recognizers, C# in constructions and guards |
| [Host options](15-host-options.md) | `Lexical`, `Carrier`, buffered input, `[GramParserVariant]`, `[GramInclude]` |
| [Quick reference](16-reference.md) | every sign and word on one page |

## Every example runs

The guide is held to the generator by `LanguageGuideTests` (in `tests/DotGram.Tests`), so a page that
disagrees with the code is a failing test rather than a stale page. For whoever edits it:

- A ` ```dotgram ` fence is a whole grammar, compiled. Diagnostic identifiers after the language say what
  it reports (` ```dotgram GRAM4006 `), and nothing else at the level of a warning or above; with none, it
  reports nothing. `Lexical` compiles it with `Lexical = true`, `Deferred` with `Carrier = GramCarrier.Deferred`.
  `pending` marks one that shows the behaviour of a fix not made yet; it is skipped until the word goes.
- A ` ```csharp ` fence right after it that declares a `partial class` is its host.
- A ` ```text examples ` fence after that holds runs, a line each: `ParseX("input") → value`,
  `→ refused`, or `→ refused: message`. The input may be `reader("…")`, `stream("…")` or `bytes("…")`;
  whole numbers after it are a position and a length, `new` hands a fresh context, a last `out` calls the
  form that answers only whether. `@Name → value` reads what the host kept after the calls above it.
- Russian review blocks between `<!-- ru:begin -->` and `<!-- ru:end -->` are temporary, hold no fences, and
  are skipped.
