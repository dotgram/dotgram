---
paths:
  - "src/DotGram/Grammar/Emit/**"
---

# Emitted code lands in someone else's assembly

Text produced here is compiled in the consumer's project, under the consumer's
target framework, next to the consumer's own code. That changes the rules.

- **Only our own namespace.** Never emit a type into `System.*`. A consumer building
  for an older framework almost certainly uses PolySharp or Meziantou.Polyfill, and
  a second `internal struct System.Range` in one compilation is a compile error. This
  is also why `SourceSpan` is ours rather than `System.Range`.
- **Accessibility is explicit**, unlike in our own sources. Anything emitted into a
  namespace is `internal`, always — that is what makes two assemblies emitting the same
  type unable to see, collide with or disagree about each other's, and it is the whole
  of why no runtime assembly ships. A generated parser's own types are `public` and
  nested in the host class, so their names are the host's and cannot clash either.
- **C# 8 is the floor, and it is tested.** Every emitted file opens with `#nullable
  enable`, which needs C# 8, and nothing emitted needs more — so that is where the floor
  sits rather than anywhere chosen. No file-scoped namespaces, no collection expressions,
  no `record`, no list patterns, no `is not null`. `DotGram.Compatibility` compiles the
  generated code at `LangVersion 8.0` for `netstandard2.0`, so a feature the emitter
  starts using fails there rather than in somebody else's build.

  Emitting *above* the floor is allowed where the consumer's own compiler is known to
  accept it: a generator runs on every compilation, so nothing generated outlives the
  compilation that produced it, and `context.ParseOptionsProvider` gives the effective
  `LanguageVersion` — the one the consumer's `<LangVersion>` actually resolves to, not the
  one their TFM would suggest. Never infer it from a TFM or a preprocessor symbol:
  `net8.0` with an explicit `<LangVersion>8</LangVersion>` is legal, and `#if
  NET8_0_OR_GREATER` would be wrong about exactly that project. Anything written this way
  needs the floor form beside it, which is what the floor build then checks.

  **Method bodies only.** A body is a code-generation choice in the same sense that Debug
  and Release, or one JIT and the next, are: nothing a consumer can observe changes, so
  writing it two ways is two spellings and not two parsers. What must not vary is anything
  they do observe — the shape of an emitted type (a `record` and a class differ in equality
  and `ToString`), a signature, a diagnostic's text. Vary one of those by language version
  and the same grammar really does mean two different things.
- **The consumer's build configuration changes nothing that runs.** Diagnostics and
  the like may differ between Debug and Release; algorithms and behaviour may not.
  A generated parser is one parser, and the one stepped through in a debugger has to
  be the one that ships — an optimization that pays only under an optimizing compiler
  is still emitted where there is none, and costs what it costs there.
- **Fully qualify with `global::`** — the consumer's usings are unknown and their
  type names may collide with ours.
- **Public API of a generated parser uses BCL types only** in the default mode:
  `internal` support types cannot appear in a `public` signature (CS0051). See
  `docs/syntax.md` §6.1.

Changing anything emitted here changes what every consumer compiles. Prefer adding
over reshaping.
