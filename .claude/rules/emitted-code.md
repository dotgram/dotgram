---
paths:
  - "src/DotGram.Generator/Grammar/Emit/**"
---

# Emitted code lands in someone else's assembly

Text produced here is compiled in the consumer's project, under the consumer's
target framework, next to the consumer's own code. That changes the rules.

- **Only our own namespace.** Never emit a type into `System.*`. A consumer building
  for an older framework almost certainly uses PolySharp or Meziantou.Polyfill, and
  a second `internal struct System.Range` in one compilation is a compile error. This
  is also why `SourceSpan` is ours rather than `System.Range`.
- **Accessibility is explicit**, unlike in our own sources: `internal` by default,
  `public` when the assembly is marked `[assembly: GramRuntime]`. The two modes are
  **strictly additive** — opting in adds typed overloads and never changes an
  existing signature, so code written before opting in still compiles after.
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
