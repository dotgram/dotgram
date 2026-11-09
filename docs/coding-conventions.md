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

- Use block bodies for methods and constructors, including single-return methods
  and forwarding methods. Do not use expression-bodied (`=>`) notation for them.
  Short property accessors may remain expression-bodied. This does not prohibit
  lambda expressions or grammar semantic actions.
  Preserve unrelated existing code rather than reformatting it opportunistically.
- Put each statement on its own line. Expand loop bodies and nested control flow;
  do not compress a constructor or a multi-statement loop into `{ ...; ...; }`.
  A simple `if`, `foreach` or `while` may omit braces, with its body on the next
  indented line. Short `switch` cases may keep their action on the case line when
  this forms a readable decision table.
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

More of the same, as the maintainer applies it:

- **Lines are up to 100 characters, and what fits is joined.** A signature, a field
  initializer, a call or a `with` that fits in 100 characters goes on one line. A longer
  one is wrapped — unless wrapping would carry only some 20 to 30 characters onto the next
  line: then it stays on one line. A statement that does wrap indents its continuation
  lines one tab deeper than the line it starts on — a `return` over several `&&` or `or`
  lines, a concatenation of strings, a property pattern opened after `is`.
- **A table is aligned to its body, not to its longest row.** Within a run of similar lines
  — a record's or constructor's parameters (the type column, then the names), named
  arguments (`Key:        key`), an object initializer's `=`, a tuple's element names,
  property accessors (`{ get; init; }`), switch arms, a chain of `?:` conditions, a list
  of `or` patterns laid out as a grid — the column is set by the rows around it. A row
  much longer than the rest stays unpadded and runs past the column rather than pushing
  every other row out.
- **What sits in the padding lines up the operands.** A prefix such as `!` takes one
  column of the padding (`Memoises:  !input…` under `Counts:     input…`); a call in a
  table of calls on different receivers may be padded before `(` (`Sets  (` under
  `Builds(`) or after the `.` (`rules.       UnionWith(other);` under
  `publications.AddRange(…);`) so that the arguments start in one column.
- **A tuple or argument list written over several lines** puts `(` and `)` on lines of
  their own, one element a line, aligned.
- **A lone short `if` takes two lines**: the condition, then the statement indented on the
  next. The one-line form is for a table of them (below).
- **Braces on one branch mean braces on both**: where the `else` of an `if` needs a block,
  the `if` gets one too.
- **Short switch arms form one table**, with no blank line between them, even where one of
  them was once written over two lines.

#### Blank lines between statements

- A run of local declarations is one block. Separate it from the statements around it by
  a blank line.
- `if`, `for`, `foreach`, `while`, `do`, `switch`, `using`, `lock` and `try` stand apart:
  a blank line before and after each, so that a control statement is never squeezed
  between calls. The blank line is left out only directly after `{` or before `}`.
  `else`, `catch` and `finally` continue their statement and take no blank line. A
  statement after the `}` of any block takes a blank line too, and a one-line `foreach`
  is no exception.
- Exception: short `if`s that belong together may form a group with no blank line
  inside — one-line ones (condition and statement on one line, no braces), which then
  make a table whose statements start in one column, and guards whose single statement
  is on the next line (`if (a is null)` / `return;` twice in a row). The group as a whole
  is separated by blank lines — except from a `break` or `return` that ends a block
  holding nothing but the group and it, such as a case's body (`case 1:` / `if (a) b();`
  / `break;`). Anything else in the block, and the blank line is back.
- One declaration of one variable may stand directly above the `return` after it
  (`var value = Read();` / `return value;`). Two or more declarations, or one declaring two
  variables, take the blank line.
- Conditional compilation stands apart: a blank line above each `#if` and below each
  `#endif`, except directly after `{` or before `}`. An `#endif` directly above a statement
  is no separation.
- A comment directly above a statement belongs to it: the blank line goes above the
  comment.

The aim is the one it always was: neither a solid wall of statements nor a blank line
after every statement.

```csharp
Prepare();
Load();

if (a is null)  return;
if (b is null)  Fail();
if (n < 0)      n = 0;

Write(a);
Write(b);
```

`BlankLinesTests` in `tests/DotGram.Tests` holds every handwritten file to this and to
`BlankLinesBaseline.txt`, the count per file of what the sources did otherwise when the
rule was written. A new break fails, and so does a file that improved until its count is
lowered: `DOTGRAM_RECORD_BLANK_LINES_BASELINE=1` and run the test again rewrites the file.
Do not reformat untouched code to bring the counts down; lower them as code is edited.

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
- Prefer the shorter modern spelling where it says the same thing: `record with { … }`
  rather than a new record built from the old one's members field by field; `x[^1]` rather
  than `x[x.Length - 1]`; a property pattern (`is { IsImplicit: true, IsNumeric: false }`)
  rather than a chain of `&&` over one value's members; a primary constructor's parameter
  used directly rather than copied into a private field of the same value; one
  interpolated string rather than two concatenated.
- Do not qualify a name a `using` already brings in. Remove `using` directives and aliases
  nothing uses, and those the project's implicit global usings already cover —
  `using System;` excepted, which every file keeps.
- In handwritten project code, rely on the shared `<Nullable>enable</Nullable>`;
  do not repeat `#nullable enable` without a file-specific reason.

### Code locality and scope

- Match feature namespaces to their folders, such as `DotGram.Finance.Fix.Fix44` for
  `DotGram.Finance/Fix/Fix44`. Use a role-bearing public type name such as `FixParser` rather than
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

## Grammar layout

How the hand-written `.gram` files are laid out — the SQL grammars are the model. The
grammar language itself is `docs/syntax.md`; this is only how it is spaced.

- **A run of one-line rules is a table.** The `:`, the result type and the `=` each start
  in one column, one space past the longest name or type of the run; a run's padding
  never grows wider than that. A rule standing alone takes no padding (`trivia = …`).
- **The alternatives of a rule are a table too.** `=>` starts in one column across them;
  labels of different lengths are padded after the `:` so the rules they name line up
  (`| s:  SchemaDefinition`, `| sr: SchemaRoutine`); where every alternative begins with a
  key word of its own, the `&` after it lines up (`"ADD"i   & …`, `"ALTER"i & …`). Inside
  a table of nearly identical rules even the C# of the actions is padded so that the
  differing part lines up.
- **A row much longer than the rest stays unpadded** and does not set the column.
- **Two short alternatives share the rule's line**, and in a table of such rules the second
  `|` lines up as well:

  ```
  PeriodContained : @PeriodRight = d: DatetimeValueExpression => @(new PeriodRight.Point(d.Node)) | p: PeriodPredicand => @(new PeriodRight.Period(p))
  PeriodPredicand : @PeriodValue = c: PeriodConstructor => @(c)                                   | n: IdentifierChain => @(new PeriodValue.Reference(n))
  ```
- **One alternative, one line.** A `=>` wrapped onto the next line is joined back to its
  alternative; only an alternative far too long to read on one line breaks before its `=>`,
  which is indented one level under the rule.
- **Indentation is tabs**, as in C#; alignment after it is spaces. Grammars still indented
  with spaces are converted when they are next edited, not all at once.
- **Continuation lines are indented one level**, the `=` and every `|` of a rule's
  alternatives alike; a choice of a rule written on its line continues with its `|` under
  the `=`.
- **A long choice of key words is a grid**: one column a word, as many columns as the lines
  carry, so that the list can be read down as well as across.
