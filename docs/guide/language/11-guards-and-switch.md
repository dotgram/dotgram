# Guards, conditions and `switch`

Three ways to let something other than the next character decide: a C# test on what has been read
(`when @(…)`), a question about the grammar answered when the parser is generated (`when A is B`),
and a C# value that picks one of several bodies (`switch`). The specification is
[`syntax.md` §3.6](../../syntax.md#36-guard).

## Guards: `when @(…)`

`when` followed by a C# value is an operand that reads nothing: where the value is `false`, the
alternative does not match here. It sees the captures made before it, as local variables.

```dotgram
Number : int = { ['0'..'9']+ } => @int.Parse(parserText)
Byte   : int = n: Number & when @(n <= 255) => n

parse Byte
```

```text examples
ParseByte("255") → 255
ParseByte("256") → refused
```

A guard may call the host, `when @IsKnown(symbol)`, or be any C# expression, `when @(qty > 0 &&
symbol.Length == 4)`. The two forms are one: everything under the `@` is the consumer's C#.

```dotgram
Symbol = { ['A'..'Z']+ }
Order  = side: ('B' | 'S') & ' ' & symbol: Symbol & when @IsKnown(symbol)

parse Order
```

```csharp
partial class Grammar
{
	static bool IsKnown(string symbol)
	{
		return symbol is "MSFT" or "AAPL";
	}
}
```

```text examples
ParseOrder("B MSFT") → Order { Side = "B", Symbol = "MSFT" }
ParseOrder("S IBM") → refused
```

**Where a guard stands is part of what it means.** A failing guard is a failing operand, and ordered
choice then tries the next alternative. Put it before anything is read, and the alternative simply
does not begin; a lookahead lets a guard look at what comes without reading it:

```dotgram
Number : int = { ['0'..'9']+ } => @int.Parse(parserText)
Size   : @string = n: ?=Number & when @(n < 100) & v: Number => @("small " + v)
                 | v: Number                                => @("large " + v)

parse Size
```

```text examples
ParseSize("42") → "small 42"
ParseSize("420") → "large 420"
```

A guard runs **while** the text is read, so `parserText` and `parserSpan` there say what the rule has
matched so far:

```dotgram
Short = ['a'..'z']+ & when @(parserText.Length <= 3)

parse Short
find  Short as Shorts
```

```text examples
ParseShort("abc") → "abc"
ParseShort("abcd") → refused
Shorts("abcd ef") → ["abc", "d", "ef"]
```

Over characters a repetition gives back what a guard after it refuses: `find` reads `abc`, then `d`.

Notes:

- A guard may run more than once, and for a reading the parse then gives up: it runs on the path
  being tried. Keep it free of effects, or make them safe to repeat ([context](15-context-and-state.md)).
- A capture that a route to the guard skips reaches it nullable: `('(' & a: X)? & when @(a is { … })`
  ([values](07-values.md#a-capture-that-may-be-absent)).
- A guard that names a capture whose value a `=>` builds asks for that value now: the construction runs
  during recognition, its value is kept, and acceptance reuses it.
- A guard reads nothing, so it needs no lookahead around it: `?=(when @(g))` is `when @(g)`, and
  `?!(when @(g))` is `when @(!(g))`. A guard inside a lookahead may not read a capture made outside it
  (`GRAM4006`).
- `when` begins a guard wherever an operand begins, so a capture cannot be called `when` (`GRAM2003`).
- A guard is handed `parserText` and `parserSpan` and no other supplied name: `parserInput` in a `when`
  is `GRAM4053`.

## Conditions: `when A is B`

The same word asks a second kind of question, about the grammar rather than the input: whether two
recognizers have a string in common. It is answered when the parser is generated. Where it holds,
nothing is emitted for it; where it does not, the alternative it stands in is not in that parser at
all. What gives it more than one answer is a rebinding: a publication's `with` substitutes a rule, so
one grammar publishes parsers that answer it differently ([namespaces](10-namespaces.md)).

```dotgram
trivia  = { ' '* }
Version = "2000" | "2008" | "2022"
Name    = { ['a'..'z']+ }

Test = l: Name & "*=" & r: Name                            & when Version is "2000"
     | l: Name & "is" & "distinct" & "from" & r: Name      & when Version is "2022"
     | l: Name & "=" & r: Name

parse Test with (Version = "2000") as ParseOld
parse Test with (Version = "2022") as ParseNew
parse Test                         as ParseAny
```

```text examples
ParseOld("a *= b") → Test { L = "a", R = "b" }
ParseNew("a *= b") → refused
ParseNew("a is distinct from b") → Test { L = "a", R = "b" }
ParseOld("a is distinct from b") → refused
ParseAny("a *= b") → Test { L = "a", R = "b" }
ParseNew("a = b") → Test { L = "a", R = "b" }
```

`ParseAny` names no version, so it keeps the grammar's own `Version` — the choice of every one — and
has every alternative.

Conditions combine as C#'s patterns do — `is not`, `and`, `or`, brackets, `and` binding tighter than
`or` — and a C# guard may stand among them:

```dotgram
Version = "2008" | "2022"
Product = "Box" | "Azure"
Name    = { ['a'..'z']+ }

Row = l: Name & "=>" & r: Name & when (Version is "2008" or Version is "2022") and Product is not "Azure"
    | l: Name & "->" & r: Name & when Version is "2022" and @(r.Length > 1)

parse Row with (Version = "2022", Product = "Box")   as ParseBox
parse Row with (Version = "2022", Product = "Azure") as ParseAzure
```

```text examples
ParseBox("a=>b") → Row { L = "a", R = "b" }
ParseAzure("a=>b") → refused
ParseAzure("a->bc") → Row { L = "a", R = "bc" }
ParseAzure("a->b") → refused
```

What is generated is what is left once the conditions are answered:

| Written | Generated |
| --- | --- |
| a false condition `and @(g)` | no alternative, and `g` compiled into nothing |
| a true condition `and @(g)` | `when @(g)` |
| a true condition `or @(g)` | nothing — no answer of `g` could change the reading |

Notes:

- A side is a recognizer: a name, a literal, an element set, a bracketed choice; anything with a
  quantifier is named as a rule first (`GRAM2024` otherwise). There is no combinator inside a side:
  `Version is ("2008" | "2022")` already says what `Version is "2008" or "2022"` would.
- A range is a rule, not an operator: `when Version is Since2008` with `Since2008 = "2008" | "2022"`.
- `is`, `not`, `and` and `or` are words only after `when`.

Refused, and told:

```dotgram GRAM2009
Version = "2000" | "2022"

Test = 'a' & when Version and Version is "2000"

parse Test with (Version = "2000") as ParseOld
```

```dotgram GRAM2009
Version = "2000" | "2022"

Test = 'a' & when Version

parse Test
```

```dotgram GRAM4021
Version = "2000" | "2022"

Digits  = ['0'..'9']+

Test = 'a' & when Version is Digits

parse Test with (Version = "2000") as ParseOld
```

```dotgram GRAM2024
Version = "2000" | "2022"

Test = 'a' & when Version is ['0'..'9']+

parse Test with (Version = "2000") as ParseOld
```

```dotgram GRAM4020
Version = "2000" | "2022"

Test = 'a' & ('b' & when Version is "2000")*

parse Test
```

```dotgram GRAM4022
Version = "2000" | "2022"

Old  = 'a' & when Version is "2000"
Test = Old | 'b'

parse Test with (Version = "2022") as ParseNew
```

- An operand after `when` that is not C# and not followed by `is` is `GRAM2009`, a bare name as well as
  one joined to a condition by `and` or `or`.
- A condition is answered only where each side accepts a listable set of strings — a literal, a
  choice of them, a rule that is one; elsewhere it is reported and the alternative kept (`GRAM4021`):
  deciding either way would silently change what the parser reads.
- A condition stands only among the operands of an alternative, since removing an alternative is what
  it is for (`GRAM4020`), whether or not a publication answers it.
- A rule every alternative of which a parser rules out is not an error: a construct removed from a
  dialect has to be removable, and its callers fail where it is written. It is said, as information
  (`GRAM4022`).
- One grammar published in more than 64 readings is refused (`GRAM4023`), and so is a `when` that leaves
  different C# behind in different readings (`GRAM4024`).

## Computed choice: `switch`

`switch @(…)` evaluates a C# value once, each time recognition reaches it, and that value selects one
body directly, without trying the others.

```dotgram
Digits = { ['0'..'9']+ }

Field = tag: Digits & '=' & switch @(Kind(tag)) {
	case 0:  text: ['a'..'z']+
	case 1:  number: Digits
	case 2:
	case 3:  none
	case -1: none
	default: rest: any*
}

parse Field
```

```csharp
partial class Grammar
{
	static int Kind(string tag)
	{
		return tag == "99" ? -1 : int.Parse(tag) % 5;
	}
}
```

```text examples
ParseField("10=abc") → Field { Tag = "10", Text = "abc", Number = null, Rest = null }
ParseField("11=42") → Field { Tag = "11", Text = null, Number = "42", Rest = null }
ParseField("12=") → Field { Tag = "12", Text = null, Number = null, Rest = null }
ParseField("14=x!") → Field { Tag = "14", Text = null, Number = null, Rest = "x!" }
ParseField("11=abc") → refused
```

The selector reads captures made before it, as a guard does: `Kind(tag)` decides what follows `=`. Each
body captures for the rule as any alternative would, so a member a case did not write is `null`.

Labels are decimal integers, negative ones included, or strings; one `switch` uses one kind. String
labels compare ordinally, and a `null` string selects `default`:

```dotgram
Typed = name: { ['a'..'z']+ } & ':' & switch @(name) {
	case "int":  digits: ['0'..'9']+
	case "bool": flag: ("true" | "false")
}

parse Typed
```

```text examples
ParseTyped("int:42") → Typed { Name = "int", Digits = "42", Flag = null }
ParseTyped("bool:true") → Typed { Name = "bool", Digits = null, Flag = "true" }
ParseTyped("char:x") → refused
ParseTyped("int:true") → refused
```

`int:true` is refused although `"true"` is a `bool`: the selected body failed, and no other case is
tried.

Notes:

- The selector is C# and must be of an integral type or `string`; C# checks that the labels fit it.
- Each body is a grammar expression: it may capture, construct, or hold alternatives. Several labels
  may share a body; a `;` after a body is optional.
- No label and no `default` is a failure. A failure inside the selected body fails the `switch` and
  never tries another case, `default` included; an enclosing alternative may still backtrack.
- The selector sees earlier captures, `context`, `parserText` and `parserSpan` as a guard does, and
  like a guard may run again when the parse backtracks to it.
- A `switch` enters one case and records nothing for the others, so a refusal names only what that case
  expected. That makes it the cheap way to classify what was read once ([rules](04-rules.md#alternatives-that-decide-after-a-nested-construct)).

Refused:

```dotgram GRAM2021
A = switch @(1) {
	case 1: 'a'
	case 1: 'b'
}
```

```dotgram GRAM2022
A = switch @(1) {
	case 1:   'a'
	case "1": 'b'
}
```

```dotgram GRAM2023
A = switch @(1) {
	case 1:
}
```

```dotgram GRAM2020
A = switch @(1) {
	when: 'a'
}
```

A label written twice, or two `default`s, is `GRAM2021`; numbers and strings together, `GRAM2022`; a
`switch` with no label or a label with no body, `GRAM2023` (`none` is a body that reads nothing);
anything else in the braces, or a label that is neither a whole number nor a string, `GRAM2020`.
