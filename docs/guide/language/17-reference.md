# Quick reference

Every word and sign of the notation on one page, each with the chapter that tells it. The specification is
[`syntax.md`](../../syntax.md); every message the generator reports is in [`diagnostics.md`](../../diagnostics.md).

## Signs

| Sign | Means | Chapter |
| --- | --- | --- |
| `'x'`, `"text"` | a character, a string | [elements](02-elements.md) |
| `'x'i`, `"text"i` | the same without regard to case | [elements](02-elements.md#without-regard-to-case-xi-texti) |
| `\'`, `\"`, `\\`, `\0`, `\a`, `\b`, `\f`, `\n`, `\r`, `\t`, `\v`, `\uXXXX` | escapes in a literal | [elements](02-elements.md#character-literals) |
| `[ … ]`, `[^ … ]`, `..`, `\p{Lu}` | an element set, its complement, a range, a Unicode category | [elements](02-elements.md#element-sets---) |
| `&` | sequence | [composition](03-composition.md#sequence-) |
| `\|` | ordered choice; set union inside `[ … ]` | [composition](03-composition.md#ordered-choice-) |
| `~` | sequence with no trivia between | [trivia](05-trivia-and-words.md#adjacency-) |
| `( … )` | a group; a rule's parameters; a call's arguments; a rebinding list | [composition](03-composition.md#groups---) |
| `{ … }` | an atomic group; a namespace block; a `switch` body | [composition](03-composition.md#atomic-groups---) |
| `?`, `*`, `+` | zero or one, zero or more, one or more | [composition](03-composition.md#quantifiers----n-nm-n) |
| `{n}`, `{n,m}`, `{n,}` | a count | [composition](03-composition.md#quantifiers----n-nm-n) |
| `?=X`, `?!X` | positive and negative lookahead | [composition](03-composition.md#lookahead-x-and-x) |
| `name: X` | a capture | [values](07-values.md#captures-name-x) |
| `$q: X`, `$q` | bind the text `X` read; match it again | [bindings](12-bindings.md) |
| `#q` | the length of the bound text, inside count braces | [bindings](12-bindings.md#counting-by-a-length-xq) |
| `=` | a rule's body; one side of a rebinding | [rules](04-rules.md) |
| `:` | a type in a rule's head, a parameter or a directive; a capture; a `switch` label | [rules](04-rules.md#declaring-a-rule) |
| `T[]` | a sequence of `T` | [values](07-values.md#what-a-rule-gives-back) |
| `Name?` | a rule that gives back over tokens | [reading over tokens](06-lexical.md#choices-stand-and-rules-that-give-back) |
| `=>` | a construction; a recovery's factory | [values](07-values.md#construction-) |
| `<< n`, `>> n` | a binding power: left- or right-associative at level `n` | [rules](04-rules.md#binding-powers) |
| `@Name`, `@A.B`, `@M(…)`, `@T<U>`, `@(…)` | C#: a name, a call, a generic name, an expression | [the grammar and its host](01-grammar-and-host.md#two-vocabularies--and-its-absence), [C# seam](14-csharp-seam.md) |
| `[@M]` | a C# predicate in an element set | [C# seam](14-csharp-seam.md#element-predicates-m) |
| `.` | a qualified name, `Ns.Rule`, `Std.Integer` | [namespaces](10-namespaces.md) |
| `,` | between parameters, arguments, rebindings and count bounds | |
| `;` | ends a `using`; optional after a `switch` body | [the grammar and its host](01-grammar-and-host.md#imports-using-and-using) |
| `-` | a negative `switch` label | [guards](11-guards-and-switch.md#computed-choice-switch) |
| `//`, `/* … */` | comments in a grammar | [the grammar and its host](01-grammar-and-host.md#layout-and-comments) |

## Words

All are words only where the notation reads them; elsewhere they are ordinary names.

| Word | Where | Chapter |
| --- | --- | --- |
| `using`, `@using` | an import of a grammar namespace, of a C# namespace | [the grammar and its host](01-grammar-and-host.md#imports-using-and-using) |
| `namespace` | a namespace block | [namespaces](10-namespaces.md) |
| `with (…)` | a rebinding: of a namespace, an expression, a publication | [namespaces](10-namespaces.md) |
| `with state @(…)` | a state mark | [context and state](15-context-and-state.md) |
| `parse`, `find` | publication | [publication](08-publication.md) |
| `public`, `internal`, `private`, `positional` | before `parse` or `find` | [publication](08-publication.md#access-and-reading-from-a-position) |
| `as`, `stream`, `bytes`, `yield` | after a directive's operand | [publication](08-publication.md) |
| `context`, `state` | `context : @T`, `state : @T` | [context and state](15-context-and-state.md) |
| `on fail` | `on fail "…"` in a rule's head | [values](07-values.md#what-a-refusal-says-on-fail) |
| `when` | a guard or a condition | [guards](11-guards-and-switch.md) |
| `is`, `not`, `and`, `or` | in a condition after `when` | [guards](11-guards-and-switch.md#conditions-when-a-is-b) |
| `switch`, `case`, `default` | computed choice | [guards](11-guards-and-switch.md#computed-choice-switch) |
| `recover` | a recovering repetition | [recovery](13-recovery.md) |

## Built-in rules

| Rule | Matches | Chapter |
| --- | --- | --- |
| `any` | one input item | [elements](02-elements.md#any-none-eol-eof) |
| `none` | nothing | [elements](02-elements.md#any-none-eol-eof) |
| `eol` | a line ending | [elements](02-elements.md#any-none-eol-eof) |
| `eof` | the end of input | [elements](02-elements.md#any-none-eol-eof) |
| `eos` | the end of a segment | [elements](02-elements.md#eos-and-input-in-segments) |
| `trivia` | what may stand between operands | [trivia](05-trivia-and-words.md) |
| `wordboundary` | what a word is made of | [words](05-trivia-and-words.md#wordboundary) |
| `word` | one whole word | [words](05-trivia-and-words.md#word) |

The standard library, `Std`, is in [its own chapter](09-std.md); the names the parser supplies to C# are listed
in [the grammar and its host](01-grammar-and-host.md#built-in-rules-and-supplied-names-at-a-glance).

## All of it at once

```dotgram
@using System;
using Std;

trivia       = { (Spacing | LineComment("//"))* }
wordboundary = [\p{L} | \p{Nd} | '_']

Value : int = left: Value & '+' & right: Value << 1 => @(left + right)
            | left: Value & '*' & right: Value << 2 => @(left * right)
            | '(' & inner: Value & ')'             => inner
            | n: Integer                           => n

Let    : @string = "let"i & name: Identifier & '=' & v: Value & ';' => @($"{name}={v}")
Script : @string[] = Let* recover ';'

parse Script
```

```text examples
ParseScript("let a = 1 + 2 * 3; LET b = (1 + 2) * 3; // done") → ["a=7", "b=9"]
ParseScript("let a = 1; let = 2; let c = 3;") → ["a=1", "c=3"]
```
