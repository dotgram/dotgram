# The .Gram language, a guide

A guide to writing grammars, one topic a page, in the order a grammar needs them: what every grammar uses
first, what is used often next, and the advanced material last; each feature with what it means, a small grammar that uses it,
the cases worth knowing, and the form the generator refuses with the diagnostic it gives. The specification is
[`syntax.md`](../../syntax.md); every message is in [`diagnostics.md`](../../diagnostics.md); the tool that
generates outside a build is [`cli.md`](../../cli.md).

| | Page | What it covers |
| --- | --- | --- |
| 01 | [The grammar and its host](01-grammar-and-host.md) | `[Gram]`, rules and declarations, `@` and C#, imports; a first look at trivia, `Std`, the built-in rules and the supplied names |
| 02 | [Elements](02-elements.md) | literals and escapes, case-insensitive literals, element sets, `any`, `none`, `eol`, `eof`, `eos` |
| 03 | [Composition](03-composition.md) | `&`, `\|`, groups, atomic groups, quantifiers, lookahead, the order of operators |
| 04 | [Rules](04-rules.md) | declarations, parameters, levels, left recursion, binding powers |
| 05 | [Trivia and words](05-trivia-and-words.md) | `trivia`, `~`, `wordboundary`, `word` |
| 06 | [Reading over tokens](06-lexical.md) | `Lexical = true`: how a grammar is cut, what changes, rules that give back, terminals, what cannot be cut |
| 07 | [Captures and values](07-values.md) | captures, what a rule gives back, `=>`, the supplied names, `on fail` |
| 08 | [Publication](08-publication.md) | `parse`, `find`, `as`, access, `positional`, `stream`, `yield` |
| 09 | [The standard library](09-std.md) | every rule of `Std` |
| 10 | [Namespaces](10-namespaces.md) | namespaces, imports, rebinding with `with` |
| 11 | [Guards and `switch`](11-guards-and-switch.md) | `when @(…)`, conditions `when A is B`, `switch` |
| 12 | [Text bindings](12-bindings.md) | `$q: X`, `$q`, `X{#q}` |
| 13 | [Recovery](13-recovery.md) | `recover`, the names a recovery is handed, `OnRecovered` |
| 14 | [The seam with C#](14-csharp-seam.md) | predicates, external recognizers, C# in constructions and guards |
| 15 | [Context and state](15-context-and-state.md) | `context`, `state`, `with state` |
| 16 | [Host options](16-host-options.md) | `Carrier`, buffered input, `[GramParserVariant]`, `[GramInclude]` |
| 17 | [Quick reference](17-reference.md) | every sign and word on one page |

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
