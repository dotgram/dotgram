# Project instructions

## File format — strict

These rules are mandatory and are followed without being asked. Write new files in
this format from the start: converting spaces to tabs afterwards mishandles XML
(two-space base indent) and alignment continuations.

- **Leading block indentation is tabs only.** Use spaces for horizontal alignment
  within that indentation level: names, `=`, `=>`, arguments, wrapped XML
  attributes and comments. Numeric keys in a table may have space padding after
  the indentation tabs to align the first column.
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
- Keep `using System;` once. Rely on the project's implicit/global usings for other
  namespaces already available; do not add redundant imports.

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

## Handwritten C# style

These conventions reflect the user's reviewed Finance changes in `8e54c999`.
Apply them when writing or editing code; do not reformat unrelated files or treat
untouched older code as a reason to undo the user's style.

### Layout and alignment

- Use a block body for ordinary methods and constructors, including a single
  return when it benefits from the same layout as neighboring methods. Keep short,
  obvious accessors and compact tables of forwarding members expression-bodied.
  Do not mechanically convert every member in either direction.
- Put each statement on its own line. Expand loop bodies and nested control flow;
  do not compress a constructor or a multi-statement loop into `{ ...; ...; }`.
  A simple `if`, `foreach` or `while` may omit braces, with its body on the next
  indented line. Short `switch` cases may keep their action on the case line when
  this forms a readable decision table.
- Separate logical phases with blank lines: guards, related local declarations,
  mutations, loops and the final return. Keep closely related declarations together;
  avoid both a solid wall of statements and a blank line after every statement.
- Align adjacent, related declarations and assignments in columns: member names,
  property accessors, `=`, `=>`, corresponding constructor arguments and comments.
  Align switch keys and arms, grammar definitions/publications and attribute options
  the same way. Padding before a call's `(` is allowed in a table of similar calls.
  Keep alignment local to a logical group; do not pad an entire file to one width.
- Put generic `where` constraints on their own indented lines before the body.
- Write new XML summaries with `<summary>`, the text and `</summary>` on separate
  lines. Do not expand existing documentation merely as formatting churn.

```csharp
readonly struct Entry(int tag, bool required)
{
	public readonly int  Tag      = tag;
	public readonly bool Required = required;
}

static Entry ReadEntry(int tag, bool required)
{
	if (tag <= 0)
		throw new ArgumentOutOfRangeException(nameof(tag));

	return new Entry(tag, required);
}
```

### C# constructs and naming

- Prefer primary constructors for simple data holders whose constructors only
  assign parameters. Keep an ordinary constructor when it performs validation or
  meaningful initialization; do not change storage or public behavior just for style.
- Prefer collection expressions (`[]`, `[a, b]`) where the target type is clear and
  the replacement preserves behavior. This is a syntax preference, not a claim that
  an allocation disappears or that a `readonly struct` becomes static RVA data.
- Use explicit construction types where they make a returned value or a table row
  immediately recognizable, for example `return new FixNode(...)`. Target-typed
  `new()` remains appropriate when a nearby declaration already states the type.
- Prefer relational patterns for bounds on one value (`tag is > 0 and < 957`)
  and a pattern `switch` for a related set of cases when this makes the decisions
  easier to read. A relational pattern requires a constant bound. Keep ordinary
  conditions when they express the logic more directly.
- Name private instance fields `_camelCase`, parameters and locals `camelCase`.
  Keep public members in `PascalCase`; follow the surrounding convention for static
  fields rather than inferring a new static-field rule from the instance-field edits.
- In handwritten project code, rely on the shared `<Nullable>enable</Nullable>`;
  do not repeat `#nullable enable` without a file-specific reason.

### Code locality and scope

- Match feature namespaces to their folders, such as `DotGram.Finance.Fix` for
  `Finance/Fix`. Use a role-bearing public type name such as `FixParser` rather than
  a name that collides with the feature namespace. Coordinate API changes with
  callers, package smoke tests and documentation; do not rename existing APIs merely
  to make an unrelated change conform to this guideline.
- Keep tightly coupled code together. A small grammar and its C# hooks can live in
  one host file using a raw-string `[Gram]` attribute; a context used only by that
  grammar can be nested in the host. Keep larger, independently useful grammars in
  `.gram` files. Do not extract helpers solely to create more layers or files.
- User edits are intentional current context. Preserve them and follow their local
  conventions; do not restore an earlier assistant version or copy accidental
  duplicate imports and incomplete alignments as new rules.
- These modern C# preferences apply to handwritten repository sources. Code emitted
  into consumer assemblies must still follow `.claude/rules/emitted-code.md`,
  including the supported language-version floor and explicit nullable context.

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
	                      namespace. A file and an internal grammar class per specification
	                      (Rfc3339 … Rfc9651); what is public is the values, each with its own
	                      Parse and TryParse (JsonValue.Parse, MediaType.Parse, …). README.md
	                      ships on NuGet. Written against whole specifications, not as
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
