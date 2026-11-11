# Host options that change what a grammar means

Most options of `[Gram(…)]` say how a parser is generated, not what it reads. A few change what a grammar
means, and those are on this page: reading over tokens (`Lexical`), when constructions run (`Carrier`,
`ConstructionsMayThrow`), what captures are handed as, and grammars that are read twice or built on another.
The full list is [`syntax.md` §6.8](../../syntax.md#68-generation-options).

## Reading over tokens: `Lexical = true`

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
| a choice or a repetition | gives back when something later fails | stands once it matched; a rule marked `Name?` gives back inside itself ([rules](07-rules.md#rules-that-give-back)) |
| alternatives of literals | ordered | longest match decides |
| `~` | no seam is woven | the two tokens must touch ([trivia](08-trivia-and-words.md#adjacency-)) |
| `word` | a run of boundary characters | one test of the token kind |
| a binding `$q` | anywhere | only inside a terminal (`GRAM4035`, [bindings](06-bindings.md#bindings-in-a-grammar-split-into-tokens)) |
| `recover` | anywhere on a repetition | in the syntax only (`GRAM4044`) |

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

<!-- ru:begin -->
> **RU.** `Lexical = true` разрезает грамматику на лексер и синтаксическую половину: лексер превращает текст
> в токены по самому длинному совпадению, синтаксис читает токены и на каждом выборе решает по токену
> перед собой, не возвращаясь — как рукописный парсер. Разрез возможен при двух условиях: пространство с
> собственной `trivia = none`, правила которого — токены, и атомарная `trivia` снаружи, которую лексер
> пропускает между токенами (пример `Query`: `<>` — один токен по самому длинному совпадению, независимо
> от порядка альтернатив; `selecta` — одно имя благодаря `wordboundary`). Что меняется над токенами —
> таблица: выбор и повторение стоят (кроме правил `Name?`); альтернативы литералов решает самое длинное
> совпадение; `~` требует соприкосновения токенов; `word` — одна проверка вида токена; привязка — только
> внутри терминала (`GRAM4035`); `recover` — только в синтаксисе (`GRAM4044`). Грамматика, которую нельзя
> разрезать, компилируется над символами, как без опции, с предупреждением `GRAM5004`, называющим
> препятствие (второй пример: класс символов в синтаксическом правиле). С `eos` разрез невозможен
> (`GRAM4049`); терминал из одного `@M` — `GRAM5011`; `Scan(text, pos, out kind)` лексера — внутренний
> метод хоста.
<!-- ru:end -->

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

<!-- ru:begin -->
> **RU.** Конструирование выполняется там, куда его ставит носитель (таблица): `Auto` (по умолчанию) —
> по мере чтения, где это безопасно, и компилятор говорит, где это может иметь значение (`GRAM5012`,
> `GRAM5016`); `Immediate` — по мере чтения альтернативы, в том числе для отброшенных прочтений; `Deferred` —
> после принятия разбора, только для сохранённого прочтения (если охранник не попросил значение раньше).
> Разница видна только у конструирования с побочным действием (пример со счётчиком `Built`: по умолчанию
> `Item` строится дважды — первая альтернатива строит его и отбрасывает на `,`, вторая строит снова; с
> `Deferred` — один раз). `ConstructionsMayThrow = true` сохраняет немедленный носитель для конструирования,
> единственный эффект которого — исключение, и придерживает исключение до места, где его бросил бы отложенный носитель;
> это стоит каждому конструированию немного, поэтому включается явно.
<!-- ru:end -->

## Captures and buffered input

| Option | Default | Changes |
| --- | --- | --- |
| `UseSpanCaptures` | `false` | character captures and `parserText` reach C# as `ReadOnlySpan<char>`, not `string` |
| `BufferedText` | `false` | every `parse` and `find` also reads a `TextReader`, as if it said `stream` |
| `BufferedBytes` | `false` | every `parse` and `find` also reads bytes, as if it said `stream bytes` |
| `InitialBufferSize` | `4096` | where a buffered call does not say, the buffer's starting capacity |
| `MaxBufferedInputLength` | no limit | where a buffered call does not say, the most a buffered parse may hold |

A buffer size or a retention of zero or less is `GRAM0009`. What the buffered forms are and which rules they
take is in [publication](11-publication.md#input-from-a-reader-or-a-stream-stream-stream-bytes).

<!-- ru:begin -->
> **RU.** Захваты и буферизованный вход (таблица): `UseSpanCaptures` — символьные захваты и `parserText`
> приходят в C# как `ReadOnlySpan<char>`; `BufferedText`/`BufferedBytes` — все `parse` и `find` получают
> формы над `TextReader`/байтами, как с `stream`/`stream bytes`; `InitialBufferSize` и `MaxBufferedInputLength` — значения по
> умолчанию для буферизованных вызовов; ноль или меньше — `GRAM0009`. Подробно — в главе 11.
<!-- ru:end -->

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

<!-- ru:begin -->
> **RU.** `[Gram]` говорит, какая у класса грамматика; каждый `[GramParserVariant("Name", …)]` просит ещё
> одно её прочтение, скомпилированное во вложенный класс с этим именем (`Sql.Immediate.ParseQuery`). Всё,
> чего вариант не говорит, берётся из `[Gram]`. Имя — единственный обязательный аргумент; не идентификатор —
> `GRAM0007`. Два прочтения с одной областью (вариант с именем класса другого прочтения или того, что уже
> назвал `[Gram(…, Name = …)]`) — `GRAM0006`; публикация или тип правила с именем класса прочтения —
> `GRAM5018`.
<!-- ru:end -->

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

<!-- ru:begin -->
> **RU.** Включённая грамматика компилируется в эту внутри пространства с именем из `Alias` (или
> `DefaultAlias` её класса, или имени класса): её правила — `Sql92.Identifier`, либо `using Sql92;`. Включённая
> грамматика остаётся целым файлом: видит встроенные правила, `Std` и то, что включила сама, и никогда —
> правила, `trivia`, `wordboundary` и `context` включающей. Вызываемый её правилами C# приходит с ней через
> `using static` её класса. Два включения под одним именем при столкновении правил — `GRAM0008`; `DefaultAlias`
> не идентификатор — `GRAM0005`. Между проектами текст грамматики путешествует на её классе
> (`ExportGrammar`).
<!-- ru:end -->

## Everything else

`Name`, `TargetMethodPartSize`, `MaxExtraStacks`, `ExportGrammar`, `LocationType`, `UsePerCallLocations`,
`Trace` and the attributes for tooling
(`[GramLanguage]`, `[GramClassify]`, …) say how a parser is generated, traced or shown in an editor, and change
no answer. They are in [`syntax.md` §6.4–§6.9](../../syntax.md#64-targetmethodpartsize-the-one-thing-a-host-may-tune).

<!-- ru:begin -->
> **RU.** Остальные опции (`Name`, `TargetMethodPartSize`, `MaxExtraStacks`, `ExportGrammar`, `LocationType`,
> `UsePerCallLocations`, `Trace`) и
> атрибуты для инструментов (`[GramLanguage]`, `[GramClassify]`, …) говорят о том, как парсер
> генерируется, трассируется или показывается в редакторе, и ответов не меняют; они описаны в syntax.md
> §6.4–§6.9.
<!-- ru:end -->
