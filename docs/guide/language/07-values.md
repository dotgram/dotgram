# Captures and values

What a rule gives back: the text it matched, a value built from what it captured, a sequence, or
whatever C# makes of the parts. The specification is
[`syntax.md` §3.5, §3.7, §4.1 and §7.3](../../syntax.md#41-a-rules-result).

## Captures: `name: X`

`name: X` reads `X` and keeps what it gave under `name`. A capture is visible to the `=>` and the
`when` of its alternative, and a rule that captures and says nothing else gets a type with one
member per capture.

```dotgram
Url  = scheme: ("https" | "http") & "://" & host: Host
Host = ['a'..'z' | '.']+

parse Url
```

```text examples
ParseUrl("https://dotgram.dev") → Url { Scheme = "https", Host = "dotgram.dev" }
```

The type `Url` is generated inside the host, a class with an `internal` constructor and a get-only
property per capture, named by capitalizing the capture: `scheme` becomes `Scheme`.

What a capture holds follows from what it captures:

| Captured | Type |
| --- | --- |
| a rule | that rule's result type |
| a literal, an element set, or a group of those | `string`, the text matched |
| a repetition of something that gives text | `string`, the text of every turn joined |
| a repetition of a rule that builds a value | `T[]` |
| `name: X?` or `(name: X)?` that did not match | `null` |
| any other repetition that took no turn | `""` or an empty array |

```dotgram
Number : int    = { ['0'..'9']+ } => @int.Parse(parserText)
Signed          = sign: '-'? & digits: ['0'..'9']*
List            = items: Number* & tail: ['a'..'z']*

parse Signed
parse List
```

```text examples
ParseSigned("7") → Signed { Sign = null, Digits = "7" }
ParseSigned("-") → Signed { Sign = "-", Digits = "" }
ParseList("1") → List { Items = [1], Tail = "" }
```

`?` is the one quantifier that is an option rather than a run: `sign: '-'?`, like `(sign: '-')?`, is
`null` where it did not match. Every other quantifier is a run, whatever its bounds, and one that took
no turn is `""` — `digits: ['0'..'9']*` above, and `['0'..'9']{0,3}` as well. The braces in `Number` keep `12` one number:
without them `Number*` could read it as one turn or two ([composition](03-composition.md#a-run-that-can-be-cut-two-ways)).

Notes:

- A capture may be called anything the grammar can spell, C#'s keywords included. Inside `@(…)` it is
  written as C# writes such a name: `using: Word => @(@using)`.
- One name may be captured in several alternatives, or several times in one, as long as it has one
  type everywhere.
- A binding `$q: X` is not a capture: it adds no member and no name ([bindings](12-bindings.md)).
- Captures inside a lookahead are refused (`GRAM4006`, [composition](03-composition.md#lookahead-x-and-x)).

```dotgram
Import : @string = using: ['a'..'z']+ => @(@using)

parse Import
```

```text examples
ParseImport("linq") → "linq"
```

Refused:

```dotgram GRAM4007
Number : int = ['0'..'9']+ => @int.Parse(parserText)
Value          = v: Number | v: ['a'..'z']+

parse Value
```

```dotgram GRAM4012
Word = parserText: ['a'..'z']+

parse Word
```

```dotgram GRAM2003 GRAM2002
Guarded : @string = when: ['a'..'z']+ => @(@when)
```

`v` is an `int` in one alternative and a `string` in the other (`GRAM4007`). Every name beginning
with `parser` is the generator's ([below](#names-the-parser-supplies), `GRAM4012`). `when` cannot
name a capture, since it begins a guard (`GRAM2003`, and the rest of the line is then read as a
declaration, `GRAM2002`).

## What a rule gives back

A rule's value is decided by the first of four cases that applies.

**1. There is a `=>`.** It builds the value ([below](#construction-)).

**2. The type is a sequence, `T[]`.** Every operand whose rule declares a type assignable to `T` joins
it, in order, a repetition with all its turns. An operand of another type, or of a rule that declares
none — separators, whitespace — contributes nothing.

```dotgram
Item  : @string = v: ['a'..'z']+ => @(v.ToUpperInvariant())
Comma = ','
Items : @string[] = Item & (Comma & Item)*

parse Items
```

```text examples
ParseItems("a,bc,d") → ["A", "BC", "D"]
```

**3. There are captures.** They are matched by name to the type: to the parameters of a constructor
whose every parameter is covered, or to `init` properties. With no type of the host's, the type is
generated, as for `Url` above.

```dotgram
Number : int    = ['0'..'9']+ => @int.Parse(parserText)
Point  : @Point = x: Number & ',' & y: Number

parse Point
```

```csharp
partial class Grammar
{
	public sealed record Point(int X, int Y);
}
```

```text examples
ParsePoint("3,4") → Point { X = 3, Y = 4 }
```

**4. None of these.** The value is what the rule matched: its text as a `string`, or, declared
`: @SourceSpan`, where it is.

```dotgram
Word  = ['a'..'z']+
Where : @SourceSpan = 'a' & 'b'

parse Word
parse Where
```

```text examples
ParseWord("abc") → "abc"
ParseWhere("ab") → [0..2)
```

Notes:

- `string`, `int` and the other C# keywords name C# types without `@`; any other C# type is written
  `@Name`. A type without `@` names a rule and means that rule's type.
- A rule whose body is only a call is case 4 like any other: `Name = Word` is the text of the
  `Word`, not its value. Capture it to hand the value on: `Name : int = v: Number => v`.
- Any type but `string` and `@SourceSpan` needs a `=>`, captures or a sequence: a rule declared
  `: int` with nothing to build the `int` from is refused (`GRAM4008`).
- The text of a whole `parse` leaves out the trivia read before and after the rule
  ([trivia](05-trivia-and-words.md)).

Refused:

```dotgram GRAM4008
Count : int = ['0'..'9']+

parse Count
```

```dotgram GRAM3002
Row : Missing = 'a'

parse Row
```

```dotgram GRAM4056
Item  : @string   = 'a' => @("A")
Items : @string[] = items: { Item }* => @(items)

parse Items
```

`GRAM4008`: nothing says how to make an `int` of the text. `GRAM3002`: `Missing` is neither a rule in
view nor a C# type (which would be written `@Missing`). A capture whose type does not fit what the
`=>` makes of it — here a repetition of a braced group, which is its text and not an array — is said
against the grammar's line (`GRAM4056`), rather than by the C# compiler in the generated file.

## Construction: `=>`

`=> value` gives its alternative a value. It never reads input, and it belongs to **one
alternative**: every branch of a `|` builds its own.

```dotgram
Number : int = ['0'..'9']+ => @int.Parse(parserText)
Term   : int = n: Number                => n
             | '(' & inner: Term & ')'  => inner
             | '-' & t: Term            => @(-t)
             | "max" & '(' & a: Term & ',' & b: Term & ')' => @Max(a, b)

parse Term
```

```csharp
partial class Grammar
{
	static int Max(int a, int b)
	{
		return a > b ? a : b;
	}
}
```

```text examples
ParseTerm("(42)") → 42
ParseTerm("-(7)") → -7
ParseTerm("max(3,-4)") → 3
```

What may stand after `=>`:

| Written | Is |
| --- | --- |
| `name` | a capture, or a parameter of the rule |
| `@M(a, b)`, `@int.Parse(t)` | a C# call |
| `@(expression)` | any C# expression |
| `Row(a, b)` | the type the grammar generates for the rule `Row`, built from those values |

Captures are ordinary local variables there, and so are the [names the parser
supplies](#names-the-parser-supplies). `@M(a, b)` and `@(M(a, b))` are the same construction.

A name without `@` builds the type the grammar generates for a rule, its arguments in the order the
rule writes its captures. A rule declared with another rule's name as its type gives back a value of
that rule's type:

```dotgram
Number  : int  = ['0'..'9']+ => @int.Parse(parserText)
Pair           = left: Number & ',' & right: Number
Swapped : Pair = left: Number & ';' & right: Number => Pair(right, left)

parse Pair
parse Swapped
```

```text examples
ParsePair("1,2") → Pair { Left = 1, Right = 2 }
ParseSwapped("1;2") → Pair { Left = 2, Right = 1 }
```

Notes:

- What follows `@` is the consumer's C#, handed to the compiler as written: a misspelt method is a
  C# error, reported against the grammar's line ([C# seam](14-csharp-seam.md)).
- An exception a construction throws is not a refusal: it leaves the parse, from `TryParse…` as from
  `Parse…` (§8.2). Check what may not hold with a guard ([guards](11-guards-and-switch.md)).
- `=>` binds looser than `&` and tighter than `|` ([composition](03-composition.md#operator-order-in-an-expression)).

## Names the parser supplies

Beside its captures, every `=>` and `when` may name what the parser knows about the alternative:

| Name | In | Is |
| --- | --- | --- |
| `parserText` | `=>`, `when` | the text matched so far, a `string` (`ReadOnlySpan<char>` with `UseSpanCaptures`) |
| `parserSpan` | `=>`, `when` | where it is, a `SourceSpan` with `Start` and `Length` |
| `parserInput` | `=>` | the whole input, a `string` |

```dotgram
Word  : @string = ['a'..'z']+ & ' '* => @(parserText.TrimEnd())
Found : @string = ' '* & ['a'..'z']+  => @(parserSpan.Start + ":" + parserSpan.Length)
Slice : @string = ['a'..'z']+         => @(parserInput.Substring(parserSpan.Start))

parse Word
parse Found
find  Slice as Slices
```

```text examples
ParseWord("abc  ") → "abc"
ParseFound("  ab") → "0:4"
Slices("ab cd") → ["ab cd", "cd"]
```

Notes:

- A `when` runs before its rule is finished, so there `parserText` and `parserSpan` say what the rule
  has matched **so far** ([guards](11-guards-and-switch.md)).
- `parserInput` is what a value meant to cut its own string later is built from; holding it keeps the
  whole input alive, and a publication that reads from a stream does not have it.
- The names of [recovery](13-recovery.md) (`parserLine`, `parserMessage`, …) and of
  [state](15-context-and-state.md) (`parserState`, `parserMarks`) are supplied the same way. A name
  written where it is not handed over is refused, not left to the C# compiler (`GRAM4053`).
- Every supplied name begins with `parser`, so no capture may (`GRAM4012`).

```dotgram GRAM4053
Word = ['a'..'z']+ & when @(parserInput.Length > 0)

parse Word
```

## Constructing groups

A group may build a value of its own with a `=>` inside it. Repeated in a rule of type `T[]`, it
collects one value a turn; its type is the element type of the rule.

```dotgram
Key   = ['a'..'z']+
Pairs : @string[] = (k: Key & '=' & v: Key & ';' => @(k + ":" + v))*

parse Pairs
```

```text examples
ParsePairs("a=b;c=d;") → ["a:b", "c:d"]
ParsePairs("") → []
```

Notes:

- The group owns its captures, as a rule does: `k` and `v` are not visible outside it, and its `=>`
  sees nothing captured outside it. `parserText` and `parserSpan` there are the group's own.
- A group that needs a type other than the rule's (or its element type) is better written as a named
  rule with a declared type.
- A plain group without `=>` keeps captures in the rule's scope, as before.

## A capture that may be absent

A capture that one route to a `=>` or a `when` skips reaches it **nullable**; one every route writes
reaches it as the value itself. What decides is the alternative, not the rule.

```dotgram
Digits : @string = ['0'..'9']+
Signed : int = (s: '-')? & d: Digits => @(s is null ? int.Parse(d) : -int.Parse(d))

parse Signed
```

```text examples
ParseSigned("5") → 5
ParseSigned("-5") → -5
```

The construction owes the rule a value of its declared type, so it has to say what absence means:
`s is null`, `s ?? ""`, `s?.Length`.

## When a construction runs

By default a construction runs **as its alternative is read** (the immediate carrier): its value is
there for whatever reads it next. Over characters an alternative may be read and then given up, so a
construction may run for a reading the parse does not keep. One that only builds cannot tell; one that
counts, logs or throws can. The host chooses otherwise:

| `[Gram(…)]` | Constructions run |
| --- | --- |
| nothing (`Carrier = Auto`) | as read, wherever that is safe; the compiler says where it may not be (`GRAM5012`, `GRAM5016`) |
| `Carrier = GramCarrier.Deferred` | once the parse has accepted, for the reading kept only |
| `ConstructionsMayThrow = true` | as read, with an exception held and thrown only where the deferred carrier would throw it |

The details are in [`syntax.md` §3.7](../../syntax.md#37-construction) and
[host options](16-host-options.md).

## What a refusal says: `on fail`

A refusal names what could have stood where the parse stopped. A rule that knows what it is can say
so instead, between its type and its body:

```dotgram
Name on fail "Expected a name." = ['a'..'z']+
Pair = Name & '=' & Name

parse Pair
```

```text examples
TryParsePair("a=1") → refused: Expected a name.
TryParsePair("a=b") → "a=b"
```

Notes:

- The text is handed back as written, in every reading of the grammar. Where the place matters it is
  in `Match<T>.Position`.
- Without it, the refusal lists what was expected — exact, and for a rule an expression may begin with,
  a list nobody reads.
