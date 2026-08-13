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

## Git

Commits go straight to `main`. No feature branches — this is a single-user
repository, and branching only adds a merge step.

## Repository layout

```text
docs/
	syntax.md             the language: notation and its bond with C#
	implementation.md     engine plan
src/
	DotGram/              the whole product: one analyzer package, no runtime
		Grammar/          pure: a function of grammar text, no Roslyn
		Generation/       the Roslyn shell
examples/
	DotGram.Examples/     whole parsers meant to be copied: a grammar, the class it
	                      attaches to, and the code written against it. No test
	                      framework and no scaffolding — DotGram.Tests runs them
tests/
	DotGram.Tests/        three levels: direct calls, in-memory generator driver,
	                      and the generator attached as an analyzer
	Snapshots/            a grammar and the file it must compile into, checked in so
	                      a change to code generation shows up as a diff
.work/                    scratch, ignored by git
```

Nothing under `examples/` may reference a test framework or be written for one. An
example that needs a fixture to make sense is not an example; assertions about it
belong in `tests/DotGram.Tests/ExampleTests.cs`.

No runtime assembly ships, deliberately: everything a generated parser needs is
emitted into the consumer's own compilation. See `docs/syntax.md` §6.1.

Two seams keep `Grammar/` free of Roslyn — `ISymbolResolver` for `@Name` and
`ICSharpScanner` for `@(...)`. Both are implemented over Roslyn in `Generation/`.
