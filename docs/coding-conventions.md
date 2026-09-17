# Coding conventions

Required conventions for handwritten repository code, project files and their
formatting. This is the canonical document for coding style; keep agent instructions
and the documentation index linked here rather than duplicating these rules.

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

Mechanically supported by [`.editorconfig`](../.editorconfig) and
[`.gitattributes`](../.gitattributes); apply layout rules during editing and review.

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
  into consumer assemblies must still follow [the emitted-code rules](../.claude/rules/emitted-code.md),
  including the supported language-version floor and explicit nullable context.
