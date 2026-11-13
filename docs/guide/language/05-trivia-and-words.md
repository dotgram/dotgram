# Trivia, adjacency and words

Whitespace, comments and keyword boundaries are not notation of their own. They are three built-in
rules — `trivia`, `wordboundary` and `word` — that a grammar redefines, and the compiler does
something automatic around them once they are not empty. The specification is
[`syntax.md` §4.5–§4.6](../../syntax.md#45-trivia--insignificant-whitespace-and-comments).

The examples call `Std` and declare namespaces as [the grammar and its host](01-grammar-and-host.md)
introduced them; both have chapters of their own later, [the standard library](09-std.md) and
[namespaces](10-namespaces.md).

## `trivia`: what may stand between operands

`trivia` is inserted at every seam between the operands of a sequence. It is `none` by default, so
by default nothing is inserted and nothing may stand between operands. A grammar to which
whitespace is insignificant redefines it:

```dotgram
trivia = { ' '* }

Word = { ['a'..'z']+ }
Pair = Word & '=' & Word

parse Pair
```

```text examples
ParsePair("a=b") → "a=b"
ParsePair("a = b") → "a = b"
ParsePair("  a = b  ") → "a = b"
ParsePair("a b = c") → refused
```

Notes:

- A whole `parse` also reads trivia before and after the rule, and the value leaves it out: the
  text of `"  a = b  "` is `"a = b"`.
- `trivia` must accept empty input (`GRAM4003`): it is inserted everywhere, and a second insertion
  reads nothing.
- When `trivia` is empty the insertions are dropped before anything is generated: a grammar that
  does not use it pays nothing.

```dotgram GRAM4003
trivia = ' '+
Pair   = 'a' & 'b'

parse Pair
```

## Where the seams are

Trivia goes where an author wrote `&` — and between the turns of a repetition whose element is a
sequence or a value:

```dotgram
trivia = { ' '* }

Word    = { ['a'..'z']+ }
Entry   : @string = w: Word => w
List    = Word & (',' & Word)*
Several = Word*
Entries : @string[] = Entry*

parse List
parse Several
parse Entries
```

```text examples
ParseList("a , b ,c") → "a , b ,c"
ParseSeveral("abcd") → "abcd"
ParseSeveral("ab cd") → refused
ParseEntries("ab cd") → ["ab", "cd"]
```

| Shape | Seams |
| --- | --- |
| `a & b` | between `a` and `b` |
| `A & (S & A)*` | between every operand, and between the turns: the repeated part is a sequence |
| `Entry*` where `Entry` builds a value | between the turns: a list of things |
| `Word*`, `['0'..'9']+` — a repetition of something valueless | none: a lexeme is written this way |
| `X?`, a repetition of a choice | none |
| before a `when` | none: a guard reads nothing |

A spaced run of one valueless thing is the one case nothing can infer — `Word*` and `Digit*` have the
same shape. The author says which by naming `trivia`, an ordinary rule:

```dotgram
trivia = { ' '* }

Word  = { ['a'..'z']+ }
Words = Word & (trivia & Word)*

parse Words
```

```text examples
ParseWords("ab cd  ef") → "ab cd  ef"
```

## Comments, and why trivia is braced

Comments are trivia too. The standard library has the usual ones:

```dotgram
trivia = { (Std.Spacing | Std.LineComment("//") | Std.Nested("/*", "*/"))* }

Word = { ['a'..'z']+ }
Pair = Word & '=' & Word

parse Pair
```

```text examples
ParsePair("a // note\n = /* x /* y */ */ b") → "a // note\n = /* x /* y */ */ b"
```

**Brace the trivia.** Without braces a failing parse may give back what trivia read, one character at
a time, and re-read a comment's interior as syntax. Where what stands beside a seam reads the same
characters as trivia — a field that may contain spaces — every run of spaces then has as many readings
as it is long, and a refused input is tried in every combination: a few malformed rows take seconds.
The compiler warns of that shape (`GRAM4051`):

```dotgram GRAM4051
trivia = ' '*

Field = [^ ',' | '\n']+
Row   = Field & (',' & Field)*

parse Row
```

`trivia = { ' '* }` reads each run once. Braced trivia is read by a scanner, which cannot follow a rule
that reaches itself; a comment that nests is written `Std.Nested(open, close)`, which a scanner reads
with a counter ([standard library](09-std.md)).

## Lexemes under trivia

A rule spelled out of single characters — a number, a name — is a lexeme, and under non-empty trivia
its seams would let `1 . 5` be a number. Write such a rule where `trivia` is empty, or join its parts
with `~` (below). The compiler warns of the shape almost never meant: every operand tests one item, at
least one is a class or `any`, and a seam is woven between two of them (`GRAM4043`).

```dotgram GRAM4043
trivia = { ' '* }

Number = Std.Digit+ & ('.' & Std.Digit+)?

parse Number
```

```dotgram
trivia = { ' '* }

Number = Std.Digit+ ~ ('.' ~ Std.Digit+)?

parse Number
```

```text examples
ParseNumber("1.5") → "1.5"
ParseNumber("1 . 5") → refused
```

Notes:

- A run is not one item, so `Std.Digits & ('.' & Std.Digits)?` is not told — it reads `1 .5` all the
  same. Write numbers and strings built from runs with `~` by hand.
- Braces do not silence `GRAM4043`: they commit a reading and keep every seam.

## Trivia by namespace

`trivia` is an ordinary rule, so it is shadowed as any rule is: a namespace that declares its own reads
with it. A rule uses the `trivia` visible where it is **declared**, not where it is called.

```dotgram
using Lexical;

trivia = { ' '* }

namespace Lexical
{
	trivia = none

	Name   = ['a'..'z'] & ['a'..'z' | '0'..'9']*
	Number = ['0'..'9']+
}

Assign = Name & '=' & Number

parse Assign
```

```text examples
ParseAssign("x1 = 42") → "x1 = 42"
ParseAssign("x 1 = 42") → refused
```

Notes:

- The `trivia` visible in a namespace is looked up along the namespace's own chain first — the
  namespace, then each one around it — and only then among the namespaces it imports. Where neither
  finds one, it is the built-in `none`. So `Assign` above reads spaces: the `trivia = none` that
  `using Lexical;` brings in is shadowed by the `trivia` around it.
- Two imports at one scope that each declare a `trivia`, in a scope that declares none, are refused
  (`GRAM3022`): the order of the `using`s never decides. Declare the one meant, `trivia = A.trivia`.
- A substitution replaces `trivia` like any rule: `namespace N with (trivia = none) { … }` reuses rules
  under another whitespace ([namespaces](10-namespaces.md)).

```dotgram GRAM3022
using A;
using B;

namespace A { trivia = { ' '* } }
namespace B { trivia = { '\t'* } }

Pair = 'x' & 'y'

parse Pair
```

## Adjacency: `~`

`a ~ b` is `a & b` with no seam: what `b` matches begins exactly where `a` ended. It binds tighter than
`&`, so `a & b ~ c` is `a & (b ~ c)`.

```dotgram
trivia = { ' '* }

Name  = { ['a'..'z']+ }
Shift = Name & '>' ~ '>' & Name
Type  = Name & ('<' & Type & (',' & Type)* & '>')?

parse Shift
parse Type
```

```text examples
ParseShift("x >> y") → "x >> y"
ParseShift("x > > y") → refused
ParseType("list<list<int> >") → "list<list<int> >"
ParseType("list<list<int>>") → "list<list<int>>"
```

It exists for operators that share their characters with something else: a shift is two `>` with
nothing between them, and a nested type argument list closes with two that belong to different levels.
Writing the shift as `">>"` cannot do both: a literal is one token, and with `>>` among the tokens the
inner list would be handed a shift.

Notes:

- Over tokens (`Lexical = true`) `~` asks the tokens themselves whether they touch, so a grammar reads
  the same language either way ([reading over tokens](06-lexical.md#adjacency-and-word-over-tokens)).
- Inside a namespace whose `trivia` is `none`, `~` says nothing and is allowed for that reason.

## `wordboundary`

`wordboundary` names the characters that continue a word. It is `none` by default; once it is not
empty, every string literal **whose characters all fall in that class** is guarded on both sides: it
may not be followed by a word character, nor preceded by one.

```dotgram
trivia       = { ' '* }
wordboundary = ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']

Name    = { ['a'..'z']+ }
If      = "if" & Name
Keyword = "if" | "else"
Ident   = ?!Keyword & Name

parse If
parse Ident
```

```text examples
ParseIf("if x") → "if x"
ParseIf("ifx") → refused
ParseIdent("iffy") → "iffy"
ParseIdent("if") → refused
```

With the boundary declared, `"if"` no longer matches the start of `iffy`, so `?!Keyword` lets `iffy`
through (compare [lookahead](03-composition.md#lookahead-x-and-x)). Whether a literal qualifies is
decided when the grammar is built: `"if"` gets the checks, `"("` does not. The checks come before the
trivia insertion, so what is asked is whether a letter follows the keyword, not the whitespace.

Notes:

- The boundary may be named through another rule, `wordboundary = WordChar`, and Unicode categories
  count as classes.
- A namespace whose own `trivia` is empty does not inherit a `wordboundary` declared outside: its
  literals are parts of one lexeme. One that declares its own beside its empty trivia keeps it.
- Over tokens, the lexer asks the first question of the character after a keyword's token; `1MB` is two
  tokens.

## `word`

`word` is one whole word made of what `wordboundary` says continues one, whatever the word says: a
maximal run, which cannot read half of a word however the parse backtracks.

```dotgram
trivia       = { ' '* }
wordboundary = ['A'..'Z' | 'a'..'z' | '_']

Option         = word & '=' & ['0'..'9']+
PermissionEnd  = "ON"i | "TO"i
PermissionWord = ?!PermissionEnd & word
Permission     = PermissionWord & PermissionWord* & "ON"i & word

parse Option
parse Permission
```

```text examples
ParseOption("FILLFACTOR = 80") → "FILLFACTOR = 80"
ParsePermission("VIEW DEFINITION ON t") → "VIEW DEFINITION ON t"
ParsePermission("ON t") → refused
```

It is for places where a language admits a word and does not care which: an option's name, a
permission spelled out of words, a name belonging to another catalogue. Where only some words will do,
the shape is a lookahead in front of `word`, not a list of every word the specification happened to
have.

Notes:

- `word` in a grammar with no `wordboundary` would be a run of nothing (`GRAM4019`).
- A boundary that can match nothing, `['a'..'z']?`, would make `word` a run that never ends
  (`GRAM4001`, at the boundary).
- Over tokens `word` is one test of the token kind: a keyword, or a class every character of which
  continues a word.

```dotgram GRAM4019
Name = word

parse Name
```
