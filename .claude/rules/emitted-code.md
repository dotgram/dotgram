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
- **Assume nothing about the consumer's language version or TFM.** No file-scoped
  namespaces, no collection expressions, no `record` unless the emitted code carries
  what makes it work. Prefer plainly compilable C#.
- **Fully qualify with `global::`** — the consumer's usings are unknown and their
  type names may collide with ours.
- **Public API of a generated parser uses BCL types only** in the default mode:
  `internal` support types cannot appear in a `public` signature (CS0051). See
  `docs/syntax.md` §6.1.

Changing anything emitted here changes what every consumer compiles. Prefer adding
over reshaping.
