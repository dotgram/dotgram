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
- A binding `$q: X` is not a capture: it adds no member and no name ([bindings](06-bindings.md)).
- Captures inside a lookahead are refused (`GRAM4006`, [composition](03-composition.md#lookahead)).

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

```dotgram GRAM2003 GRAM2002 recheck
Guarded : @string = when: ['a'..'z']+ => @(@when)
```

`v` is an `int` in one alternative and a `string` in the other (`GRAM4007`). Every name beginning
with `parser` is the generator's ([below](#names-the-parser-supplies), `GRAM4012`). `when` cannot
name a capture, since it begins a guard (`GRAM2003`, and the rest of the line is then read as a
declaration, `GRAM2002`).

<!-- ru:begin -->
> **RU.** `name: X` читает `X` и сохраняет полученное под именем `name`. Захват виден `=>` и `when`
> своей альтернативы; правило, которое захватывает и больше ничего не говорит, получает тип с
> членом на каждый захват (пример `Url`: класс внутри хоста с `internal`-конструктором и
> get-свойствами, имя свойства — захват с заглавной буквы). Тип захвата — по таблице: правило → его
> тип; литерал, множество или их группа → `string`; повторение текста → склеенный текст; повторение
> правила со значением → `T[]`; несработавший необязательный захват `(name: X)?` → `null`;
> несработавший `name: X?` или `(name: X)?` → `null`; любое другое повторение без шагов → `""` или
> пустой массив. `?` — единственный квантор-«вариант», а не повторение: `sign: '-'?` так же, как
> `(sign: '-')?`, даёт `null`, если не совпал; остальные кванторы — повторения при любых границах, и
> без шагов дают `""` (`digits: ['0'..'9']*`, и `{0,3}` тоже). Скобки в `Number` не дают `Number*` прочитать `12` и как одно, и как два числа. Захват может называться как угодно, включая ключевые слова C# (внутри `@(…)` — как в C#:
> `@(@using)`); одно имя может захватываться в нескольких альтернативах, если тип везде один.
> Привязка `$q:` — не захват. Отвергаются: одно имя с разными типами — `GRAM4007`; имя на `parser` —
> `GRAM4012`; захват `when:` — `GRAM2003` (сейчас; помечено `recheck`, см. вопрос Q04.1).
<!-- ru:end -->

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
  ([trivia](08-trivia-and-words.md)).

Refused:

```dotgram GRAM4008
Count : int = ['0'..'9']+

parse Count
```

```dotgram GRAM3002 recheck
Row : Missing = 'a'

parse Row
```

`GRAM4008`: nothing says how to make an `int` of the text. `GRAM3002`: `Missing` is neither a rule in
view nor a C# type (which would be written `@Missing`).

<!-- ru:begin -->
> **RU.** Значение правила определяется первым подходящим из четырёх случаев. **1.** Есть `=>` — он
> строит значение. **2.** Тип — последовательность `T[]`: в неё по порядку входит каждый операнд, чьё
> правило объявляет тип, совместимый с `T` (повторение — всеми шагами); операнды другого типа или
> правил без типа (разделители, пробелы) ничего не вносят (пример `Items`). **3.** Есть захваты — они
> сопоставляются по имени с параметрами конструктора, покрытого целиком, или с `init`-свойствами;
> без типа хоста тип генерируется (пример `Point` с `record` хоста). **4.** Иначе значение —
> прочитанное: текст (`string`) или, при объявлении `: @SourceSpan`, место. Ключевые слова C#
> (`string`, `int`, …) — типы C# без `@`; другой тип C# пишется `@Name`; тип без `@` — это правило
> и его тип. Правило, тело которого — один вызов, тоже случай 4: `Name = Word` — текст, а не значение
> `Word`; чтобы передать значение, захватите его. Тип, кроме `string` и `@SourceSpan`, требует `=>`,
> захватов или последовательности: `: int` без способа построить `int` — `GRAM4008`. Тип, который не
> правило и не тип C#, — `GRAM3002` (diagnostics.md описывает этот случай как `GRAM4011`, см. вопрос). Текст целого `parse` не включает trivia вокруг правила.
<!-- ru:end -->

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

Captures are ordinary local variables there, and so are the [names the parser
supplies](#names-the-parser-supplies). `@M(a, b)` and `@(M(a, b))` are the same construction.

Notes:

- What follows `@` is the consumer's C#, handed to the compiler as written: a misspelt method is a
  C# error, reported against the grammar's line ([C# seam](14-csharp-seam.md)).
- An exception a construction throws is not a refusal: it leaves the parse, from `TryParse…` as from
  `Parse…` (§8.2). Check what may not hold with a guard ([guards](05-guards-and-switch.md)).
- `=>` binds looser than `&` and tighter than `|` ([composition](03-composition.md#operator-order-in-an-expression)).

<!-- ru:begin -->
> **RU.** `=> value` задаёт значение своей альтернативы; вход оно не читает и относится к **одной
> альтернативе** — каждая ветка `|` строит своё. После `=>` может стоять: имя захвата или параметра;
> вызов C# `@M(a, b)`; любое выражение C# `@(…)`. Захваты там — обычные локальные переменные, как и даваемые имена;
> `@M(a, b)` и `@(M(a, b))` — одно и то же. То, что после `@`, — C# потребителя: опечатка в методе —
> ошибка C# на строке грамматики. Исключение из конструирования — не отказ: оно уходит из
> `TryParse…` так же, как из `Parse…`; то, что может не выполниться, проверяйте охранником. `=>`
> связывает слабее `&` и сильнее `|`.
<!-- ru:end -->

## Names the parser supplies

Beside its captures, every `=>` and `when` may name what the parser knows about the alternative:

| Name | In | Is |
| --- | --- | --- |
| `parserText` | `=>`, `when` | the text matched so far, a `string` (`ReadOnlySpan<char>` with `SpanCaptures`) |
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
  has matched **so far** ([guards](05-guards-and-switch.md)).
- `parserInput` is what a value meant to cut its own string later is built from; holding it keeps the
  whole input alive, and a publication that reads from a stream does not have it.
- The names of [recovery](12-recovery.md) (`parserLine`, `parserMessage`, …) and of
  [state](13-context-and-state.md) (`parserState`, `parserMarks`) are supplied the same way. A name
  written where it is not handed over is refused, not left to the C# compiler (`GRAM4053`).
- Every supplied name begins with `parser`, so no capture may (`GRAM4012`).

```dotgram GRAM4053
Word = ['a'..'z']+ & when @(parserInput.Length > 0)

parse Word
```

<!-- ru:begin -->
> **RU.** Кроме захватов, каждый `=>` и `when` может назвать то, что парсер знает об альтернативе:
> `parserText` — прочитанный текст (`string`, или `ReadOnlySpan<char>` при `SpanCaptures`);
> `parserSpan` — место (`SourceSpan` с `Start` и `Length`); `parserInput` — весь вход (только в
> `=>`). В `when` правило ещё не закончено, поэтому там `parserText` и `parserSpan` — прочитанное
> **до сих пор**. `parserInput` нужен значению, которое потом само вырежет свою строку; он держит
> весь вход в памяти, а у публикации из потока его нет. Имена восстановления (`parserLine`,
> `parserMessage`, …) и состояния (`parserState`, `parserMarks`) даются так же. Имя там, где оно не
> передаётся, отвергается самим генератором (`GRAM4053`, пример выше: `parserInput` в `when`). Все
> даваемые имена начинаются с `parser`, поэтому захват так называться не может (`GRAM4012`).
<!-- ru:end -->

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

<!-- ru:begin -->
> **RU.** Группа может строить своё значение `=>` внутри. Повторённая в правиле типа `T[]`, она
> собирает по значению за шаг; её тип — тип элемента правила (пример `Pairs`). Группа владеет своими
> захватами, как правило: `k` и `v` не видны снаружи, а её `=>` не видит внешних захватов;
> `parserText`/`parserSpan` там — свои. Группе с другим типом лучше стать именованным правилом с
> объявленным типом. Обычная группа без `=>` оставляет захваты в области видимости правила.
<!-- ru:end -->

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

<!-- ru:begin -->
> **RU.** Захват, который какой-то путь к `=>` или `when` пропускает, приходит туда **nullable**;
> захват, который пишет каждый путь, — самим значением. Решает альтернатива, а не правило.
> Конструирование обязано вернуть значение объявленного типа, поэтому должно сказать, что значит
> отсутствие: `s is null`, `s ?? ""`, `s?.Length` (пример `Signed`).
<!-- ru:end -->

## When a construction runs

By default a construction runs **as its alternative is read** (the immediate carrier): its value is
there for whatever reads it next. Over characters an alternative may be read and then given up, so a
construction may run for a reading the parse does not keep. One that only builds cannot tell; one that
counts, logs or throws can. The host chooses otherwise:

| `[Gram(…)]` | Constructions run |
| --- | --- |
| nothing (`Carrier = Auto`) | as read, wherever that is safe; the compiler says where it may not be (`GRAM5012`, `GRAM5016`) |
| `Carrier = GramCarrier.Tape` | once the parse has accepted, for the reading kept only |
| `ConstructionsMayThrow = true` | as read, with an exception held and thrown only where the tape would throw it |

The details are in [`syntax.md` §3.7](../../syntax.md#37-construction) and
[host options](15-host-options.md).

<!-- ru:begin -->
> **RU.** По умолчанию конструирование выполняется **по мере чтения** альтернативы (немедленный
> носитель): значение сразу доступно. Над символами альтернатива может быть прочитана и затем
> отброшена, поэтому конструирование может выполниться для прочтения, которое разбор не сохранит;
> чистой сборке значения это безразлично, а счётчику, логу или исключению — нет. Хост может выбрать
> иначе (таблица): `Carrier = GramCarrier.Tape` — только после принятия, для оставленного прочтения;
> `ConstructionsMayThrow = true` — по мере чтения, но исключение задерживается и бросается только там,
> где бросила бы лента. Компилятор говорит, где это может иметь значение (`GRAM5012`, `GRAM5016`).
<!-- ru:end -->

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

<!-- ru:begin -->
> **RU.** Отказ называет то, что могло стоять там, где разбор остановился. Правило, которое знает,
> что оно такое, может сказать это само — `on fail "…"` между типом и телом (пример `Pair`). Текст
> возвращается как написан, во всех вариантах чтения; место — в `Match<T>.Position`. Без `on fail`
> отказ перечисляет ожидавшееся — точно, но для правила, с которого может начинаться выражение, это
> список, который никто не читает.
<!-- ru:end -->
