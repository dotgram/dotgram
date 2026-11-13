# Literals, element sets and the built-in rules

The smallest recognizers: a literal, a set of input items, and the eight rules every grammar has
without declaring them. Each tests the input at the current position and moves past what it
matched. The specification is [`syntax.md` §3.1](../../syntax.md#31-elementary).

## Character literals

`'x'` matches one character. A character literal holds exactly one character after its escapes are
resolved.

```dotgram
Pipe = '|'
Tab  = '\t'
Acute = 'é'

parse Pipe
parse Tab
parse Acute
```

```text examples
ParsePipe("|") → "|"
ParsePipe("||") → refused
ParseTab("\t") → "\t"
ParseAcute("é") → "é"
```

The escapes are C#'s:

| Escape | Character | Escape | Character |
| --- | --- | --- | --- |
| `\'` | `'` | `\a` | U+0007 alert |
| `\"` | `"` | `\b` | U+0008 backspace |
| `\\` | `\` | `\f` | U+000C form feed |
| `\0` | U+0000 | `\v` | U+000B vertical tab |
| `\n` | U+000A line feed | `\r` | U+000D carriage return |
| `\t` | U+0009 tab | `\uXXXX` | the UTF-16 code unit `XXXX`, four hex digits |

Notes:

- A literal is compared one UTF-16 code unit at a time, like every element: a character outside
  the Basic Multilingual Plane is written as a string of its two surrogates, `"😀"`.
- What a rule made only of a literal gives back is the text it matched, a `string`
  ([values](07-values.md)).

Refused:

```dotgram GRAM1010
TwoChars = 'ab'
```

```dotgram GRAM1010
Empty = ''
```

```dotgram GRAM1004 GRAM1010
Hex = '\x41'
```

`'ab'` is two characters (write `"ab"`), `''` none, and `\x` is not an escape (write `'A'`).
An unterminated literal is `GRAM1001`.

## String literals

`"text"` matches its characters in order. The escapes are the same.

```dotgram
Keyword = "select"
Crlf    = "\r\n"

parse Keyword
parse Crlf
```

```text examples
ParseKeyword("select") → "select"
ParseKeyword("Select") → refused
ParseKeyword("selected") → refused
ParseCrlf("\r\n") → "\r\n"
```

Notes:

- `parse` asks for the whole input, so `"selected"` is refused: `"select"` matched, and `ed` was
  left over. A `find` would have found it ([publication](08-publication.md)).
- `"select"` does not stop at a word's end unless the grammar says what a word is made of: see
  [`wordboundary`](05-trivia-and-words.md#wordboundary).
- An empty string `""` matches nothing and always succeeds, like `none`. An unterminated string is
  `GRAM1002`.

## Without regard to case: `'x'i`, `"text"i`

A trailing `i`, written right after the closing quote, matches each character of the literal and
its simple upper and lower case.

```dotgram
Select = "select"i
Yes    = 'y'i

parse Select
parse Yes
```

```text examples
ParseSelect("SeLeCt") → "SeLeCt"
ParseSelect("SELECT") → "SELECT"
ParseYes("Y") → "Y"
```

The folding is a character at a time, by Unicode 16.0's simple case mappings, with no culture,
and a partner on the other side of ASCII is not taken. It is not Unicode case folding:

```dotgram
S      = 's'i
K      = 'k'i
Sigma  = 'σ'i
Final  = 'ς'i

parse S
parse K
parse Sigma
parse Final
```

```text examples
ParseS("S") → "S"
ParseS("ſ") → refused
ParseK("K") → refused
ParseSigma("Σ") → "Σ"
ParseSigma("ς") → refused
ParseFinal("Σ") → "Σ"
```

`ſ` (U+017F) upper-cases to `S`, and the Kelvin sign (U+212A) lower-cases to `k`, but neither is
an ASCII letter's partner. The relation belongs to the literal and is not symmetric: `'ς'i` reads
`Σ`, `'σ'i` does not read `ς`. The full table is in
[`syntax.md` §3.1](../../syntax.md#31-elementary).

Notes:

- The `i` must touch the quote and must not begin a longer name: `"text"id` is the string `"text"`
  followed by the name `id` (and then needs a connector between them).
- It attaches to the literal only: `"in"i & Name` folds `in`, not whatever `Name` reads.
- A back-reference `$q` never folds, whatever the literal that bound it did
  ([bindings](12-bindings.md)).
- There is no case-insensitive element set: `['a'..'z']i` is not a form, and `'a'i` is refused
  inside brackets, as an element or a range's bound (`GRAM2015`). Write both cases,
  `['a'..'z' | 'A'..'Z']`, or a choice of literals, `'a'i | 'b'i`:

```dotgram GRAM2015
Letter = ['a'i | 'b']
```

## Element sets: `[ … ]`

Square brackets test **one** input item against a set. Inside them `|` is set union, not ordered
choice.

| Item | Example | Matches |
| --- | --- | --- |
| a character | `'_'` | that character |
| a range | `'a'..'z'` | every code unit from the first to the last, inclusive |
| a Unicode category | `\p{Lu}`, `\p{L}`, `\p{Nd}` | a code unit of that general category |
| a rule that is itself one item | `Letter` | what that rule matches |
| a C# predicate | `@IsVowel` | a character for which `bool IsVowel(char)` answers true |

`[^ … ]` is the complement: any one item that is none of those.

```dotgram
Letter = ['a'..'z' | 'A'..'Z']
Digit  = ['0'..'9']
Name   = [Letter | '_'] & [Letter | Digit | '_']*
Field  = [^ '|' | '\r' | '\n']*
Upper  = [\p{Lu}]

parse Name
parse Field
parse Upper
```

```text examples
ParseName("_tmp1") → "_tmp1"
ParseName("1tmp") → refused
ParseField("a b,c") → "a b,c"
ParseField("a|b") → refused
ParseUpper("Ж") → "Ж"
ParseUpper("ж") → refused
```

A predicate is a static method of the host, called with the character:

```dotgram
Vowels = [@IsVowel]+

parse Vowels
```

```csharp
partial class Grammar
{
	static bool IsVowel(char c)
	{
		return "aeiou".IndexOf(c) >= 0;
	}
}
```

```text examples
ParseVowels("aei") → "aei"
ParseVowels("abc") → refused
```

Notes:

- The brackets are what make it a set: `Letter | Digit` outside them is an ordered choice of two
  rules, which reads the same here but is a different construct ([composition](03-composition.md)).
- A category is the .NET regular-expression spelling, `\p{…}` with a one- or two-letter name;
  the table is Unicode 16.0's, carried by the generator, not the runtime's. A character outside
  the BMP is two `Cs` code units, so `\p{L}` does not match it. There is no `\P{…}`: write
  `[^ \p{L}]`.
- A rule named in a set must match exactly one item: a character, a range, a set, or a rule made
  of those.
- `[^ …]` is one item too: at the end of input it fails, like `any`.
- Ranges are by code unit: `'A'..'z'` also holds `[`, `\`, `]`, `^`, `_` and `` ` ``.

Refused:

```dotgram GRAM2003
Nothing = []
```

```dotgram GRAM1009 GRAM2003
Unknown = [\p{Xx}]
```

```dotgram GRAM4005
Pair   = "ab"
Either = [Pair | 'c']
```

```dotgram GRAM2014
Open = ['a'..]
```

```dotgram GRAM2014
Bare = 'a'..'z'
```

An empty set is `GRAM2003`, an unknown category `GRAM1009` (a malformed one, `\p` without braces,
`GRAM1008`), and a rule that reads more than one item cannot stand in a set (`GRAM4005`). A range
has a character at both ends and stands inside brackets: `['a'..]`, a string as a bound and a range
written outside brackets are `GRAM2014`.

## `any`, `none`, `eol`, `eof`

Four of the built-in rules, available in every grammar:

| Rule | Matches |
| --- | --- |
| `any` | one input item, whatever it is; fails only at the end of input |
| `none` | nothing; always succeeds |
| `eol` | one line ending: `"\r\n"`, `"\n"` or `"\r"`, CRLF tried first |
| `eof` | the end of input, consuming nothing |

```dotgram
Line   = [^ '\r' | '\n']* & { eol }
Lines  = Line*
Last   = Line* & [^ '\r' | '\n']+ & eof
Opt    = 'a' & ('b' | none)

parse Lines
parse Last
parse Opt
```

```text examples
ParseLines("a\r\nb\n") → "a\r\nb\n"
ParseLines("a\nb") → refused
ParseLast("a\nb") → "a\nb"
ParseOpt("a") → "a"
ParseOpt("ab") → "ab"
```

The until idiom reads anything up to a closing delimiter: a negative lookahead in front of `any`
([lookahead](03-composition.md#lookahead-x-and-x)).

```dotgram
Comment = "/*" & (?!"*/" & any)* & "*/"

parse Comment
```

```text examples
ParseComment("/* a * b */") → "/* a * b */"
ParseComment("/* open") → refused
```

Notes:

- `{ eol }` is an atomic group ([composition](03-composition.md#atomic-groups---)). Without it, `Line*`
  could read `"\r\n"` as one line or as two — an empty line ended by `\r`, then one ended by `\n` —
  and a refused input would be read again for every way of cutting it. The compiler says so
  (`GRAM4052`, a warning).
- These are rules, not keywords, and a grammar may declare its own `any` or `eol`; the others keep
  their own meaning: `eof` is still the true end of input in a grammar whose `any` reads less.
- `eof` matters inside a rule (`Header & Row* & eof`) and in a `find`; a `parse` already asks for
  the whole input.
- The remaining four built-ins are `eos` (below), `trivia`, `wordboundary` and `word`
  ([trivia and words](05-trivia-and-words.md)).

## `eos` and input in segments

A grammar that names `eos` reads a **sequence of segments** — the arguments of a command line, the
lines of a file — instead of one text. `eos` matches the boundary at the end of each segment; at
a boundary there is no character, so `any`, literals and sets fail there and an until idiom stops
at the end of its segment by itself.

```dotgram
Args  : @string[] = { items: Arg* } => @(items)
Arg   : @string   = ("--output" | "-o") & eos & v: Value & eos => @("out=" + v)
                  | v: Value & eos                              => @(v)
Value = any*

parse Args
```

The braces around `items: Arg*` keep each argument as it was first read: without them `-o x.txt` could also
be read as two plain arguments, and the compiler warns of the two readings (`GRAM4052`).
`["-o", "x.txt", "a"]` reads as `-o eos x.txt eos a eos eof` and gives `["out=x.txt", "a"]`.

Notes:

- Such a grammar is published over `IReadOnlyList<string>`, `IEnumerable<string>` and a
  `TextReader` (one line a segment), and over nothing else ([publication](08-publication.md)).
  A position counts one for each character and one for each boundary.
- U+0000 is how a boundary is read: a segment holding it is refused before it is read, and a
  literal or set item that is U+0000 alone is an error (`GRAM4047`).
- Not supported yet, and refused: text captured, bound or published across an `eos` (`GRAM4048`),
  `Lexical = true` (`GRAM4049`, [reading over tokens](06-lexical.md)), buffered or byte forms (`GRAM4050`), `positional parse`
  (`GRAM4055`).

```dotgram GRAM4048
Both : @string = text: (any* & eos & any*) & eos => @(text)

parse Both
```
