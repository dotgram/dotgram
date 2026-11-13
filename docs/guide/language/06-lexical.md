# Reading over tokens: `Lexical`

A grammar read over characters tries, gives back and tries again; a grammar read over tokens is cut in two,
a lexer that turns the text into tokens and a syntactic half that reads the tokens the way a parser written
by hand does. The second is what most language grammars want: it is faster, its refusals name tokens, and a
choice is decided by the token in front of it. This page shows how a grammar is cut, what changes, and what
cannot be cut. Captures and `=>` are used as [the grammar and its host](01-grammar-and-host.md) introduced
them and [values](07-values.md) tells in full; namespaces as [namespaces](10-namespaces.md) does. The
specification is [`syntax.md` §4 and §7.1](../../syntax.md#4-rules).

## Cutting a grammar in two

`[Gram("…", Lexical = true)]` cuts a grammar into a lexer and a syntactic half. The lexer turns the text into
tokens by longest match; the syntactic half reads the tokens, deciding at each choice by the token in front of
it and never going back — as a parser written by hand does.

What makes the cut possible is two things: a namespace whose own `trivia` is `none`, whose rules are the
tokens, and a braced `trivia` outside it, which the lexer skips between tokens.

```dotgram Lexical
namespace Lex
{
	trivia = none

	Name   = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*
	Number = ['0'..'9']+
}

trivia       = { [' ' | '\t' | '\r' | '\n']* }
wordboundary = [\p{L} | \p{Nd} | '_']

Query    : @string = "select"i & c: Lex.Name & "where"i & w: Compare => @(c + " " + w)
Compare  : @string = l: Lex.Name & op: Operator & r: Lex.Number => @(l + op + r)
Operator : @string = t: ("<" | "<>" | "=") => t

parse Query
```

```text examples
ParseQuery("SELECT a WHERE b <> 1") → "a b<>1"
ParseQuery("select a where b<1") → "a b<1"
ParseQuery("selecta where b=1") → refused
```

The lexer settles `<>` as one token because it is the longer match, whatever the order of the alternatives;
over characters, `"<" | "<>"` would need backtracking to find it. And `selecta` is one name, not `select`
followed by `a`, because `wordboundary` is declared.

What changes over tokens:

| | Over characters | Over tokens |
| --- | --- | --- |
| a choice or a repetition | gives back when something later fails | stands once it matched; a rule marked `Name?` gives back inside itself ([below](#choices-stand-and-rules-that-give-back)) |
| alternatives of literals | ordered | longest match decides |
| `~` | no seam is woven | the two tokens must touch ([trivia](05-trivia-and-words.md#adjacency-)) |
| `word` | a run of boundary characters | one test of the token kind |
| a binding `$q` | anywhere | only inside a terminal (`GRAM4035`, [below](#bindings-and-terminals-that-measure-themselves)) |
| `recover` | anywhere on a repetition | in the syntax only (`GRAM4044`) |

## Choices stand, and rules that give back

Over characters a rule call is transparent: when something after a rule fails, the parse may go back into
the rule and take another of its choices ([composition](03-composition.md#ordered-choice-)). Over tokens
**a rule's answer stands**: once it has matched, nothing that fails after it sends the parse back into it —
and inside a rule, a choice or a repetition is not revisited either.

```dotgram Lexical
namespace Lex
{
	trivia = none
	Word   = ['a'..'z']+
}

trivia = { ' '* }
Member = Lex.Word & ('.' & Lex.Word)* & '.' & Lex.Word

parse Member
```

```text examples
ParseMember("a.b.c") → refused
```

The repetition reads `.b.c`, and the
`'.'` after it then finds nothing: over tokens a repetition, like a choice, is not revisited. A rule
that needs to give back says so with `?` after its name:

```dotgram Lexical
namespace Lex
{
	trivia = none
	Word   = ['a'..'z']+
}

trivia = { ' '* }
Member? = Lex.Word & ('.' & Lex.Word)* & '.' & Lex.Word

parse Member
```

```text examples
ParseMember("a.b.c") → "a.b.c"
```

Notes:

- Inside a rule marked `?`, a choice or a repetition may be revisited when something later *in the
  same rule* fails — here the repetition gives `.c` back to the `'.' & Lex.Word` after it. The answer
  of the rule still stands at its boundary: a caller that fails after `Member` is not sent back into
  it. What a caller needs given back it reads itself, as a parser written by hand does.
- Over characters `?` is accepted and changes nothing: every rule there gives back already. Without
  `Lexical = true` both grammars above read `a.b.c`.
- `{ … }` over tokens says what is already so.

## Literals: the longest match, and keywords

The lexer decides how far a token reaches by the longest match, so the order of alternatives of literals
says nothing (`"<" | "<>"` above). A keyword, once `wordboundary` is declared, is guarded against running on
into a word; the question whether a word character stands *before* it is not asked over tokens, where two
tokens may touch:

```dotgram Lexical
namespace Lex
{
	trivia = none

	Number = ['0'..'9']+
}

trivia       = { ' '* }
wordboundary = ['A'..'Z' | 'a'..'z' | '0'..'9']

Size = Lex.Number & ("MB"i | "KB"i)

parse Size
```

```text examples
ParseSize("1MB") → "1MB"
ParseSize("16 kb") → "16 kb"
ParseSize("1MBX") → refused
```

Over characters, the same grammar refuses `1MB`: the `M` stands right after a word character.

```dotgram
namespace Lex
{
	trivia = none

	Number = ['0'..'9']+
}

trivia       = { ' '* }
wordboundary = ['A'..'Z' | 'a'..'z' | '0'..'9']

Size = Lex.Number & ("MB"i | "KB"i)

parse Size
```

```text examples
ParseSize("1MB") → refused
ParseSize("16 kb") → "16 kb"
```

## Adjacency and `word` over tokens

Over tokens, trivia was skipped before the tokens were made, so `~` asks the tokens themselves whether they
touch, and the grammar reads the same language as over characters
([trivia](05-trivia-and-words.md#adjacency-)):

```dotgram Lexical
namespace Lex
{
	trivia = none

	Name = ['a'..'z']+
}

trivia = { ' '* }

Shift = Lex.Name & '>' ~ '>' & Lex.Name

parse Shift
```

```text examples
ParseShift("x >> y") → "x >> y"
ParseShift("x > > y") → refused
```

`word` is one test of the token kind there: a keyword, or a token every character of which continues a word
— `Lex.Name` below, read elsewhere in the grammar; without such a token the grammar has no kind for `word`
and is not cut (`GRAM5004`)
([words](05-trivia-and-words.md#word)).

```dotgram Lexical
namespace Lex
{
	trivia = none

	Number = ['0'..'9']+
	Name   = ['A'..'Z' | '_']+
}

trivia       = { ' '* }
wordboundary = ['A'..'Z' | '_']

Option = word & '=' & (Lex.Number | Lex.Name)

parse Option
```

```text examples
ParseOption("FILL_FACTOR = 80") → "FILL_FACTOR = 80"
ParseOption("ONLINE = OFF") → "ONLINE = OFF"
ParseOption("= 80") → refused
```

## Bindings, and terminals that measure themselves

An automaton reads only regular languages: comments that nest, strings whose closing depends on their
opening, are not. A terminal written as a regular beginning and a rest is begun by the lexer with every other
token, and measured from where the beginning ended by its own rule:

```dotgram Lexical
namespace Lex
{
	trivia = none

	Remark = "(*" & Rest
	Rest   = ([^ '*' | '('] | '*' & ?!')' | '(' & ?!'*' | "(*" & Rest)* & "*)"
	Name   = ['a'..'z']+
}

trivia = { ' '* }

Item = Lex.Name | Lex.Remark
List = Item & (',' & Item)*

parse List
```

```text examples
ParseList("a, (* x (* y *) *), b") → "a, (* x (* y *) *), b"
ParseList("a, (* x (* y *), b") → refused
```

Trivia is skipped by a scanner, which cannot follow a rule that reaches itself, so a comment that nests in
trivia is written `Std.Nested(open, close)`: its `open` is the beginning, and a counter reads the rest
([standard library](09-std.md#comments)).

```dotgram Lexical
namespace Lex
{
	trivia = none

	Name = ['a'..'z']+
}

trivia = { (' ' | Std.Nested("(*", "*)"))* }

Pair = Lex.Name & '=' & Lex.Name

parse Pair
```

```text examples
ParsePair("a (* x (* y *) *) = b") → "a (* x (* y *) *) = b"
ParsePair("a (* x (* y *) = b") → refused
```

A binding compares text, so it is read over characters only. In a grammar cut into a lexer and a parser
(`Lexical = true`) a terminal of the lexical half may bind: when its body is braced and no other token
can match a longer text that begins like it, the lexer starts it on its regular beginning and measures it
with its own rule.

```dotgram Lexical
namespace Lex
{
	trivia = none

	LongString = { '[' & $eq: '='* & '[' & (?!(']' & $eq & ']') & any)* & ']' & $eq & ']' }
	Name       = ['a'..'z']+
}

trivia = { ' '* }

Assign = Lex.Name & '=' & Lex.LongString

parse Assign
```

```text examples
ParseAssign("s = [=[a]]b]=]") → "s = [=[a]]b]=]"
```

In the syntactic half a binding would bind token kinds, not text, and is refused: compare two captures
with a guard instead, `close: Name & when @(close == open)` (`GRAM4035`).

```dotgram Lexical GRAM4035
namespace Lex
{
	trivia = none

	Name = ['a'..'z']+
}

trivia = { ' '* }

Pair = $n: Lex.Name & '=' & $n

parse Pair
```

Notes:

- Where the lexer cannot read such a terminal as one token, that part of the grammar is read over
  characters and `GRAM5004` says which token stood in the way and what to write.
- Not yet supported, and refused with a message saying so (`GRAM4034`): a binding inside a repetition
  that recovers, a binding in what `trivia` is made of once a grammar is split, and a binding terminal that
  begins with nothing an automaton reads.

## A grammar that cannot be cut

A grammar that cannot be cut is compiled over characters, as if the option were not there, and warned of
(`GRAM5004`), which names what stood in the way — here a class of characters in a syntactic rule:

```dotgram Lexical GRAM5004
trivia = { ' '* }

Word = { ['a'..'z']+ }
Pair = Word & '=' & Word

parse Pair
```

```text examples
ParsePair("a = b") → "a = b"
```

Notes:

- A grammar that names `eos` cannot be cut (`GRAM4049`).
- A terminal that is nothing but a host recognizer `@M` has no beginning the lexer can stop on, so the host
  is asked at every token (`GRAM5011`).
- The lexer's `Scan(text, pos, out kind)` is internal to the host, for the host's own code.
- A `recover` in a rule the lexer reads is refused: a token has no elements to step over (`GRAM4044`,
  [recovery](13-recovery.md#where-recover-cannot-stand)).
