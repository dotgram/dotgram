---
paths:
  - "src/DotGram/Grammar/**"
---

# `Grammar/` must not know about Roslyn

Everything under `Grammar/` is a pure function of grammar text. It is callable
without a host compilation — that is what lets generated code be obtained as a
string and inspected as code, and what lets each pipeline stage be tested on its
own rather than only through a build.

**Do not add `using Microsoft.CodeAnalysis…` here.** If a stage needs something only
the host knows, it goes behind a seam:

| Need | Seam | Roslyn implementation |
| --- | --- | --- |
| what a grammar's `@Name` refers to in C# | `ISymbolResolver` | `Generation/RoslynSymbolResolver.cs` |
| where an inline `@(...)` expression ends | `ICSharpScanner` | `Language/RoslynCSharpScanner.cs` |

A third kind of host knowledge means a third seam — an interface here, its
implementation in `Generation/` (or in `Language/`, where the editor needs it too, as it
needs the scanner), and a default or fake so callers that do not care still work.

Diagnostics are reported as `GramDiagnostic` (id, message, position, length). The
shell converts them; this half never constructs a Roslyn `Diagnostic`.

The split is inside one assembly in the generator, and the build holds it there:
`src/DotGram.Grammar.Guard` compiles `Grammar/` with no Roslyn and nothing of
`Language/` or `Generation/` in reach, so a `using` of any of them does not build
(`src/DotGram/Language/README.md`).
