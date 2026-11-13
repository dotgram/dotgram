# Text bindings: `$q` and `#q`

Some constructs close with the very text that opened them: a Lua long bracket `[==[ … ]==]`, a C# raw
string, a PostgreSQL `$tag$ … $tag$`, a heredoc, a Markdown fence. A grammar cannot list the
openings, because there are as many as there are texts. It binds the opening to a name and matches that
text again. The specification is [`syntax.md` §3.5.1](../../syntax.md#351-binding-and-back-reference).

## Binding and reference: `$q: X` and `$q`

`$q: X` records the text `X` matched under the name `q`. `$q`, later in the same alternative, matches
exactly that text again.

```dotgram
Twice = $w: ['a'..'z']+ & '-' & $w

parse Twice
```

```text examples
ParseTwice("ab-ab") → "ab-ab"
ParseTwice("ab-ac") → refused
ParseTwice("ab-abc") → refused
```

A reference compares item for item, with no regard to how the binding was read: a binding of a
case-insensitive literal that read `End` matches `End` and nothing else.

```dotgram
Block = $k: ("begin"i | "do"i) & ' ' & ['a'..'z']+ & ' ' & $k

parse Block
```

```text examples
ParseBlock("Begin x Begin") → "Begin x Begin"
ParseBlock("Begin x BEGIN") → refused
```

A binding is **not a capture**: it adds no member to the rule's type, and `=>` and `when` do not see it.
To have the text in C# too, capture it as well — the two names live apart:

```dotgram
Tagged : @string = '<' & tag: ($tag: ['a'..'z']+) & '>' & body: ['a'..'z']* & "</" & $tag & '>'
                 => @(tag + ":" + body)

parse Tagged
```

```text examples
ParseTagged("<b>bold</b>") → "b:bold"
ParseTagged("<b>bold</i>") → refused
```

Notes:

- `$q` is an ordinary operand and composes as one: `?!$q`, `$q*`, `r: $q`, `($q & X)`. `$q*` is refused
  where `q` may be empty, as `('a'?)*` is (`GRAM4001`).
- A binding stands where a capture stands, so a quantifier after it repeats the binding: `$q: '"'{3,}`
  is `($q: '"'){3,}`, which binds the whole run, joined.
- The sigil and the name are one token: `$ q` is `GRAM1011`.

```dotgram GRAM1011
Bad = $ q: 'a'
```

## Counting by a length: `X{#q}`

`#q` is the length of the text bound to `q`, in input items, and stands only inside count braces.

| Written | Repeats `X` |
| --- | --- |
| `X{#q}` | exactly as many times as `q` is long |
| `X{#q,}` | at least that many |
| `X{n,#q}` | from `n` to that many |
| `X{#q,m}` | from that many to `m` |
| `X{#q,#r}` | from the length of `q` to the length of `r` |

In reStructuredText the underline of a title is at least as long as the title:

```dotgram
Heading = $t: [^ '\r' | '\n']+ & eol & '='{#t,} & eol

parse Heading
```

```text examples
ParseHeading("Intro\n=====\n") → "Intro\n=====\n"
ParseHeading("Intro\n=======\n") → "Intro\n=======\n"
ParseHeading("Intro\n===\n") → refused
```

Notes:

- The count is read when it is reached. There is no arithmetic: `{#q-1}` is not a count.
- A count whose lower bound comes out larger than its upper one simply fails when it is read; the
  lengths are known only then, so it is not a build error.
- A quantifier binds tighter than a lookahead: `?!'}'{#d}` is `(?!'}'){#d}`. To look ahead for the whole
  count, write `?!('}'{#d})`.

```dotgram GRAM2010
Bad = $q: 'a'+ & #q
```

`#q` outside count braces is `GRAM2010`: to match the text itself, write `$q`.

## Where a binding can be read

A reference sees a binding to its left in the same alternative of the same rule — through groups,
atomic groups and repetitions, and from inside a lookahead. It does not see one in a sibling of `|`, in a
rule it calls or that calls it, or across a constructing group. **Every route** to a reference must have
bound its name; two alternatives that both bind it share it:

```dotgram
Quoted = ('\'' & $t: Word & '\'' | $t: Word) & ' ' & $t
Word   = ['a'..'z']+

parse Quoted
```

```text examples
ParseQuoted("'ab' ab") → "'ab' ab"
ParseQuoted("ab ab") → "ab ab"
ParseQuoted("ab ac") → refused
```

What a binding holds when what it bound matched nothing follows the line a text capture draws:

| Written | After zero turns |
| --- | --- |
| `$q: (X?)`, `$q: X*`, `$q: (X{m,n})` | `""`, the text of no items |
| `($q: X)*`, `($q: X){m,n}` | `""` too: a repetition of a binding is a binding of the repetition |
| `($q: X)?` | nothing: the route that skipped it never bound `q` |

```dotgram
Dollar = '$' & $tag: (['a'..'z']*) & '$' & ['a'..'z' | ' ']* & '$' & $tag & '$'

parse Dollar
```

```text examples
ParseDollar("$$ a b $$") → "$$ a b $$"
ParseDollar("$x$ a b $x$") → "$x$ a b $x$"
```

Refused:

```dotgram GRAM4032
Opt = ($q: 'a')? & $q

parse Opt
```

```dotgram GRAM4032
Turns = ($q: 'a' & ',')* & $q

parse Turns
```

```dotgram GRAM3021
NoBinding = 'a' & $q

parse NoBinding
```

```dotgram GRAM4060
Inside = ?=($q: 'a') & $q

parse Inside
```

```dotgram GRAM4061
Twice = $q: 'a' & $q: 'b' & $q

parse Twice
```

```dotgram GRAM4062
Self = $q: ('a' & $q)

parse Self
```

```dotgram GRAM4063
Split : @string = v: ($q: 'a' & 'b' => @("ab")) & $q => v

parse Split
```

```dotgram GRAM4064
Fold : @string = l: Fold & $q: 'a' & $q => @(l + "a")
               | 'x'                     => @("x")

parse Fold
```

```dotgram GRAM4033
Value = $q: 'a' & when $q is "a"

parse Value
```

- After `($q: X)?` the skipping route bound nothing (`GRAM4032`): write `$q: (X?)`, which binds `""`
  there.
- A repeated binding whose turns hold more than it, `($q: X & ',')*`, is read only within its turn; after
  the repetition bind the whole run, `$q: (X & ',')*` (`GRAM4032`).
- A name no binding of the rule gives is `GRAM3021`. A rule that needs another rule's text receives it as
  a capture and compares with a guard.
- A binding inside a lookahead would bind nothing, since the lookahead gives back what it read
  (`GRAM4060`); a reference after it is not told again.
- One name bound twice on one route is `GRAM4061`; a reference or a length inside its own binding,
  `GRAM4062`.
- A binding and its reference on two sides of a constructing group, which is built as a rule of its own,
  are `GRAM4063`.
- A binding in a step of a left-recursive rule, or around its leading call to itself, is `GRAM4064`: put the
  binding and its read in a rule the step calls.
- `$q` where a value goes — in a condition, as an argument — is `GRAM4033`: a binding is text to match
  again, not a value.

## Backtracking, and making a binding whole

Backtracking is as in a regular expression: when a greedy binding gives an item back, the references
after it are tried again with the shorter text. Where the opening is meant as a whole, write it atomic.

A C# raw string opens with three quotes or more and closes with the same number:

```dotgram
RawString = { $q: '"'{3,} } & { (?!$q & any)* } & $q & ?!'"'

parse RawString
```

```text examples
ParseRawString("\"\"\"a\"b\"\"\"") → "\"\"\"a\"b\"\"\""
ParseRawString("\"\"\"\"a\"\"\"\"") → "\"\"\"\"a\"\"\"\""
ParseRawString("\"\"\"\"a\"\"\"") → refused
```

The braces around the opening say that every opening quote belongs to it: `""""a"""` is not a
three-quote string that begins with a quote. The last `?!'"'` refuses a closing run longer than the
opening.

## Recipes

Each recognizes the token; what is made of it — unindenting, unescaping — is the rule's `=>` or C#'s.

A **Lua long bracket**:

```dotgram
LongString = '[' & $eq: '='* & '[' & { (?!(']' & $eq & ']') & any)* } & ']' & $eq & ']'

parse LongString
```

```text examples
ParseLongString("[[a]]") → "[[a]]"
ParseLongString("[==[a]]b]==]") → "[==[a]]b]==]"
ParseLongString("[==[a]=]") → refused
```

**Rust raw strings**, `#` for the quotes:

```dotgram
RustRaw = 'r' & $h: '#'* & '"' & { (?!('"' & $h) & any)* } & '"' & $h

parse RustRaw
```

```text examples
ParseRustRaw("r#\"a\"b\"#") → "r#\"a\"b\"#"
ParseRustRaw("r\"ab\"") → "r\"ab\""
```

A **heredoc** whose body starts on the next line:

```dotgram
Heredoc = "<<" & $tag: Word & eol & { (?!($tag & EndLine) & Line)* } & $tag & ?=EndLine
Word    = ['A'..'Z' | 'a'..'z' | '_']+
Line    = [^ '\r' | '\n']+ & EndLine | eol
EndLine = eol | eof

find Heredoc as Heredocs
```

```text examples
Heredocs("x = <<END\nline 1\nline 2\nEND\n") → ["<<END\nline 1\nline 2\nEND"]
```

The closing tag is followed by `?=EndLine`, a lookahead, so the line break after it is not part of the
match: that is what a `find` wants, and why the whole-input `parse` of this rule refuses a text that ends in
that line break. Where the break belongs to the token, say `& EndLine` instead of `& ?=EndLine`:

```dotgram
Heredoc     = "<<" & $tag: Word & eol & { (?!($tag & EndLine) & Line)* } & $tag & ?=EndLine
HeredocLine = "<<" & $tag: Word & eol & { (?!($tag & EndLine) & Line)* } & $tag & EndLine
Word        = ['A'..'Z' | 'a'..'z' | '_']+
Line        = [^ '\r' | '\n']+ & EndLine | eol
EndLine     = eol | eof

parse Heredoc
parse HeredocLine
```

```text examples
ParseHeredoc("<<END\nx\nEND") → "<<END\nx\nEND"
ParseHeredoc("<<END\nx\nEND\n") → refused
ParseHeredocLine("<<END\nx\nEND\n") → "<<END\nx\nEND\n"
ParseHeredocLine("<<END\nx\nEND") → "<<END\nx\nEND"
```

A **Markdown fence**: the closing fence is the same character and at least as long, which is `$f`
followed by more of the same — no comparison needed:

```dotgram
Fenced  = ' '{0,3} & $f: '`'{3,} & [^ '`' | '\r' | '\n']* & eol
          & { (?!(' '{0,3} & $f & '`'* & ' '* & EndLine) & Line)* }
          & (' '{0,3} & $f & '`'* & ' '* & EndLine | eof)
Line    = [^ '\r' | '\n']+ & EndLine | eol
EndLine = eol | eof

parse Fenced
```

```text examples
ParseFenced("```cs\nx\n```\n") → "```cs\nx\n```\n"
ParseFenced("````\nx\n```\n````") → "````\nx\n```\n````"
ParseFenced("```\nnever closed\n") → "```\nnever closed\n"
```

Notes:

- Where the closing begins with a delimiter character of its own (Lua, heredoc), a position that does not
  begin a closing fails at its first item and the scan is linear. For `(?!$q & any)*` every quote compares
  the binding with the text there: the worst case is the text's length times the binding's, which a crafted
  input can make as slow as its opening is long.
- Read from a `TextReader` or a stream, a binding holds the input from where it began until its rule is
  done with it; over bytes, `#q` counts bytes.

## Bindings in a grammar split into tokens

A binding compares text, so in a grammar read over tokens it stands only inside a terminal of the lexical
half; in the syntactic half it is refused (`GRAM4035`). Both are shown with the reading over tokens itself
([`Lexical`](06-lexical.md#bindings-and-terminals-that-measure-themselves)).
