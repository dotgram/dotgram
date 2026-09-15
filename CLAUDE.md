# Project instructions

## File format — strict

These rules are mandatory and are followed without being asked. Write new files in
this format from the start: converting spaces to tabs afterwards mishandles XML
(two-space base indent) and alignment continuations.

- **Leading indentation is tabs only.** Spaces are allowed solely for alignment
  *after* the first non-whitespace character: a column of `=`, a wrapped XML
  attribute, columns inside a comment.
- **CRLF**, final newline present, no trailing whitespace.
- **UTF-8 with BOM** for `.cs`, `.csproj`, `.props`, `.targets` — what Visual Studio
  itself writes. Every other file is UTF-8 without BOM, `.slnx` included: Visual
  Studio writes that one without, and matching it keeps solution edits from showing
  up as encoding churn.

Markdown is the one exception and keeps spaces: leading whitespace inside fenced
blocks is often column alignment, which tabs destroy at any width but one.

Enforced by `.editorconfig` and `.gitattributes`.

## using directives

- **Every `.cs` file opens with `using System;`** — even when it is not needed.
- **`System` namespaces come first.**
- **Groups are separated by a blank line.** A group is the top level of the name
  (`System`, `DotGram`, `Microsoft`, `Xunit`).
- **Alphabetical within and between groups**, except `System`, which is always first.

```csharp
using System;
using System.Collections.Immutable;
using System.Linq;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
```

## Access modifiers

A modifier that merely restates the language default is noise and is removed:

- `private` on a member or a nested type;
- `internal` on a top-level type;
- `public` on an interface member.

Only what actually changes accessibility is written.

```csharp
static class Diagnostics                                    // not internal static class
{
	const string Category = "DotGram";                      // not private const

	public static readonly DiagnosticDescriptor Rule = …;   // public is meaningful — kept
}
```

Exception: code the generator emits into a foreign assembly stays explicit. That is
the convention for generated code, and accessibility there has to be chosen
deliberately anyway.

Enforced by `.editorconfig`: `dotnet_style_require_accessibility_modifiers = omit_if_default`.

## Profiling

How the generator is measured and profiled — the harness, the byte-for-byte check against a
baseline, dotTrace with its Reporter, dotMemory — is in `.claude/rules/profiling.md`. Read it
before measuring anything rather than working it out again.

## Git

Commits go straight to `main`. No feature branches — this is a single-user
repository, and branching only adds a merge step.

## Repository layout

```text
docs/
	README.md             the index: what authority each document carries, and when it
	                      is expected to be out of date. Read it before believing one
	syntax.md             the language: notation and its bond with C#
	status.md             what this version does, held against that specification
	diagnostics.md        every message the generator reports, by identifier
	implementation.md     engine plan
	development.md        standing process: build, test, the snapshot baseline, measuring,
	                      and the Linux container
	visual-studio.md      the extension, and the StringSyntax annotations
	ast.md                the tree the SQL parsers build, and where each node comes from
	coverage.md           how much of Microsoft's T-SQL reference the T-SQL grammar reads;
	                      written by --coverage, never edited
	next.md               the engineering diary, authoritative about nothing present
	design/               proposals and plans; no statement here is about the compiler
src/
	DotGram/              the generator: one analyzer package, no runtime
		Grammar/          pure: a function of grammar text, no Roslyn
		Generation/       the Roslyn shell
		Language/         the editor-neutral analysis of a .gram document — classification,
		                  symbols, diagnostics — that the Visual Studio extension reads
		README.md         the package's own front page, not the repository's: what to
		                  write once it is installed, and links back for the rest
		SKILL.md          how to write a grammar, for an agent. Ships in the package
		                  beside that README, which points at it in a comment nothing
		                  renders
	DotGram.ExpressionLanguage/ the C#-style expression language, a package of its own:
	                      ExpressionParser, in the namespace the project is named for
	DotGram.Sql/          SQL, a package of its own, with room for other databases: the one
	                      tree the SQL grammars meet in, its writer and walker, in the
	                      DotGram.Sql namespace, and a directory and namespace per dialect
		Standard/         ISO SQL: SqlStandardParser (SQL:2023) and, for now, Sql92Parser;
		                  Specification/ holds the ISO BNF
		TransactSql/      T-SQL, TransactSqlParser; Specification/ holds Microsoft's
		                  published syntax. Each dialect is its own grammar, meeting the
		                  others only in the tree (docs/design/sql-parsers.md)
	DotGram.VisualStudio/ the Visual Studio extension: .Gram language support in the
	                      editor, over DotGram/Language (docs/visual-studio.md)
	DotGram.Web/          the formats of the web, a package of its own, in the DotGram.Web
	                      namespace: Rfc3339, Rfc3986, Rfc5646, Rfc6570, Rfc6901, Rfc6902, Rfc8259, Rfc8288, Rfc9110, Rfc9651. Written against whole specifications, not as
	                      teaching material — and, being an ordinary project the generator
	                      runs over, it is where a real symbol resolver is exercised
examples/
	DotGram.Examples/     whole parsers meant to be copied: a grammar, the class it
	                      attaches to, and the code written against it. No test
	                      framework and no scaffolding — DotGram.Tests runs them.
	                      Grouped by what they read: Formats/, Feeds/, Expressions/,
	                      Languages/, each its own namespace under DotGram.Examples
tests/
	DotGram.Tests/        three levels: direct calls, in-memory generator driver,
	                      and the generator attached as an analyzer
	  Calculators/        one language spelled several ways, held against itself: the
	                      spellings an example would only repeat
	DotGram.Sql.Tests/    the SQL parsers held to what SQL Server answers, and the tree, its
	                      writer and walker
	DotGram.Compatibility/ the generated code built for netstandard2.0, net472 and
	                      net8.0 at the C# 8 floor. Building it is the assertion
	DotGram.PackageSmoke/ the packed package asked what it promises, under the oldest
	                      Roslyn it supports. Not in the solution; CI runs it after packing
	DotGram.VisualStudio.Tests/
	  Playground/         grammars to open in the experimental instance, each with the
	                      manual check beside it (docs/visual-studio.md)
	Snapshots/            a grammar and the file it must compile into, checked in so
	                      a change to code generation shows up as a diff
	Corpus/               somebody else's SQL: SqlScriptDOM's test corpus, copied byte for
	                      byte, that the SQL grammars are read against
benchmarks/
	DotGram.Benchmarks/   BenchmarkDotNet, run by hand and not by CI. Built by the
	                      solution so that it has to keep compiling
	DotGram.HandDeferred/ one small grammar (Deferred.gram) read by hand every safe way,
	                      side by side, to measure deferred construction; no generator
	                      reference, and the grammar is there to be read, not compiled
.work/                    scratch, ignored by git
```

Nothing under `examples/` may reference a test framework or be written for one. An
example that needs a fixture to make sense is not an example; assertions about it
belong in `tests/DotGram.Tests/ExampleTests.cs`.

No runtime assembly ships, deliberately: everything a generated parser needs is
emitted into the consumer's own compilation. See `docs/syntax.md` §6.1.

Two seams keep `Grammar/` free of Roslyn — `ISymbolResolver` for `@Name` and
`ICSharpScanner` for `@(...)`. Both are implemented over Roslyn in `Generation/`.
