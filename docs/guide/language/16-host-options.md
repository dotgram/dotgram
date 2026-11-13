# Host options that change what a grammar means

Most options of `[Gram(…)]` say how a parser is generated, not what it reads. A few change what a grammar
means, and those are on this page: when constructions run (`Carrier`, `ConstructionsMayThrow`), what
captures are handed as, and grammars that are read twice or built on another. Reading over tokens,
`Lexical = true`, has [a chapter of its own](06-lexical.md).
The full list is [`syntax.md` §6.8](../../syntax.md#68-generation-options).

## When constructions run: `Carrier`

A construction runs where its carrier puts it:

| `Carrier` | Constructions run |
| --- | --- |
| `GramCarrier.Auto` (default) | as read (`Immediate`) wherever that is safe; the compiler says where it may matter (`GRAM5012`, `GRAM5016`) |
| `GramCarrier.Immediate` | as their alternative is read, also for readings the parse then gives up |
| `GramCarrier.Deferred` | once the parse has accepted, for the reading kept only — unless a guard asks for a value earlier |

The difference shows only in a construction that does something besides building:

```dotgram
Item  : @string = v: { ['a'..'z']+ } => @(Count(v))
Comma : @string = i: Item & ',' => i
Pair  : @string = i: Item & ';' => i
                | c: Comma      => c

parse Pair
```

```csharp
partial class Grammar
{
	public static int Built;

	static string Count(string value)
	{
		Built++;

		return value;
	}
}
```

```text examples
ParsePair("a,") → "a"
@Built → 2
```

```dotgram Deferred
Item  : @string = v: { ['a'..'z']+ } => @(Count(v))
Comma : @string = i: Item & ',' => i
Pair  : @string = i: Item & ';' => i
                | c: Comma      => c

parse Pair
```

```csharp
partial class Grammar
{
	public static int Built;

	static string Count(string value)
	{
		Built++;

		return value;
	}
}
```

```text examples
ParsePair("a,") → "a"
@Built → 1
```

The first alternative reads `Item`, builds it, and gives it up at `,`; the second, through `Comma`, reads and
builds it again. Deferred, only the accepted reading is built.

`ConstructionsMayThrow = true` keeps the immediate carrier for a construction whose only effect is to throw —
`int.Parse`, a division, a lookup — and holds what it throws until the deferred carrier would have thrown it, so
the answer is the deferred carrier's. The hold costs every construction a little, which is why it is asked for.

## Captures and buffered input

| Option | Default | Changes |
| --- | --- | --- |
| `UseSpanCaptures` | `false` | character captures and `parserText` reach C# as `ReadOnlySpan<char>`, not `string` |
| `BufferedText` | `false` | every `parse` and `find` also reads a `TextReader`, as if it said `stream` |
| `BufferedBytes` | `false` | every `parse` and `find` also reads bytes, as if it said `stream bytes` |
| `InitialBufferSize` | `4096` | where a buffered call does not say, the buffer's starting capacity |
| `MaxBufferedInputLength` | no limit | where a buffered call does not say, the most a buffered parse may hold |

A buffer size or a retention of zero or less is `GRAM0009`. What the buffered forms are and which rules they
take is in [publication](08-publication.md#input-from-a-reader-or-a-stream-stream-stream-bytes).

## Reading a grammar twice: `[GramParserVariant]`

`[Gram]` says which grammar a class has; each `[GramParserVariant("Name", …)]` asks for one more reading of it,
compiled into a nested class of that name:

```csharp
[Gram("Sql.gram", Lexical = true)]
[GramParserVariant("Immediate", Carrier = GramCarrier.Immediate)]
public static partial class Sql;

var first     = Sql.ParseQuery(text);              // the [Gram] reading
var immediate = Sql.Immediate.ParseQuery(text);    // the variant
```

Everything a `[GramParserVariant]` does not say it takes from the `[Gram]`. The name is its one argument and
cannot be left out; one that is not an identifier is `GRAM0007`. Two readings wanting one scope — a variant
named like the class another reading is compiled into, or the one `[Gram(…, Name = …)]` already names — are
refused (`GRAM0006`), and no publication or rule type may take the name of a reading's class (`GRAM5018`).

## A grammar built on another: `[GramInclude]`

```csharp
[GramInclude(typeof(Sql92Parser), Alias = "Sql92")]
[Gram("TransactSql.gram", Lexical = true)]
public abstract partial class TransactSqlParser;
```

The included grammar is compiled into this one inside a namespace called what `Alias` says (or what its
class's `DefaultAlias` says, or the class's name): its rules are reached as `Sql92.Identifier`, or opened with
`using Sql92;`. An included grammar stays a whole file: it sees the built-in rules, `Std` and what it included
itself, never the including grammar's rules, `trivia`, `wordboundary` or `context`. The C# its rules call comes
with it, through a `using static` of its class. Two includes under one name are refused where their rules
collide (`GRAM0008`); a `DefaultAlias` that is not an identifier is `GRAM0005`. Across projects the
grammar's text travels on its class (`ExportGrammar`, [`syntax.md` §6.7](../../syntax.md#67-graminclude-a-grammar-built-on-another)).

## Everything else

`Name`, `TargetMethodPartSize`, `MaxExtraStacks`, `ExportGrammar`, `LocationType`, `UsePerCallLocations`,
`Trace` and the attributes for tooling
(`[GramLanguage]`, `[GramClassify]`, …) say how a parser is generated, traced or shown in an editor, and change
no answer. They are in [`syntax.md` §6.4–§6.9](../../syntax.md#64-targetmethodpartsize-the-one-thing-a-host-may-tune).
