# The seam with C#

A grammar describes structure; C# describes meaning, and some recognition too. This page covers every
place C# enters a grammar, what shape each expects, and what the C# side must guarantee. The
specification is [`syntax.md` §7](../../syntax.md#7-the-bond-with-c).

**Position decides the role.** The generator never inspects a method's signature to choose what the author
meant; where `@M` is written says it:

| Written | Role | C# shape |
| --- | --- | --- |
| `[@M]` | element predicate | `bool M(char c)` |
| `@M` as an operand | external recognizer — the form to write | `bool M(ParserInput<char> input, ref int pos[, out T value][, Context context])` |
| `@M` as an operand | external recognizer over a string only, faster | `bool M(ReadOnlySpan<char> input, ref int pos[, out T value])` |
| `=> @M(a, b)`, `=> @(expr)` | construction | any C# value |
| `when @M(a)`, `when @(expr)` | guard | any C# `bool` |

<!-- ru:begin -->
> **RU.** Грамматика описывает структуру, C# — смысл и отчасти распознавание. **Роль определяет
> позиция**, а не сигнатура (таблица): `[@M]` — предикат элемента `bool M(char)`; `@M` как операнд —
> внешний распознаватель над `ReadOnlySpan<char>` с `ref int pos`, с `out T` — со значением, над
> `ParserInput<char>` — с буферизованным входом и контекстом; `=> @M(…)`/`@(…)` — конструирование (любое
> значение C#); `when @M(…)`/`@(…)` — охранник (любой `bool`).
<!-- ru:end -->

## Element predicates: `[@M]`

Inside an element set, `@M` is a method of the host asked about one character. It joins the set like any
item: unions and complements work as for characters.

```dotgram
Vowel     = [@IsVowel]
Consonant = [^ @IsVowel | ' ']
Word      = (Vowel | Consonant)+

parse Word
parse Consonant
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
ParseWord("banana") → "banana"
ParseConsonant("b") → "b"
ParseConsonant("a") → refused
ParseConsonant(" ") → refused
```

A predicate is never asked about the end of input: a set fails there before asking.

<!-- ru:begin -->
> **RU.** Внутри множества `@M` — метод хоста, которого спрашивают об одном символе; он входит в множество
> как любой элемент, объединение и дополнение работают как для символов (пример `Consonant = [^ @IsVowel |
> ' ']`). О конце входа предикат не спрашивают: множество отказывает там раньше.
<!-- ru:end -->

## External recognizers: `@M` as an operand

A bare `@M` hands the input and the position to the host, which moves the position past what it read and
answers whether it read anything. Write it over `ParserInput<char>`, a view of the input the generator writes
into the host: one method then serves every publication of the grammar — a string, a `TextReader`, a stream
(a `ParserInput<byte>` overload beside it serves `stream bytes`).
`TryAdvance(ref position, length)`, `Ensure(position, length)`, `Peek(position, out value)` and
`Slice(position, length)` read more of the input as they need it:

```dotgram
Record : @string = v: @Fixed & '\n' => v
Records : @string[] = { Record* }

parse Records stream
```

```csharp
partial class Grammar
{
	static bool Fixed(ParserInput<char> input, ref int position)
	{
		return input.TryAdvance(ref position, 4);
	}
}
```

```text examples
ParseRecords("abcd\nefgh\n") → ["abcd", "efgh"]
ParseRecords(reader("abcd\nefgh\n")) → ["abcd", "efgh"]
TryParseRecords(reader("abc\n")) → refused
```

**Over a string only, a span is faster.** Where the grammar reads nothing but strings, the recognizer may
take the input as a `ReadOnlySpan<char>` instead. It cannot tell the end of a buffer from the end of the
input, so a publication that reads a `TextReader` or a stream does not accept it:

```dotgram
Hex = "0x" & @HexRun

parse Hex
```

```csharp
partial class Grammar
{
	static bool HexRun(System.ReadOnlySpan<char> input, ref int pos)
	{
		var at = pos;

		while (at < input.Length && System.Uri.IsHexDigit(input[at]))
			at++;

		if (at == pos)
			return false;

		pos = at;

		return true;
	}
}
```

```text examples
ParseHex("0x1F") → "0x1F"
ParseHex("0x") → refused
ParseHex("0x1G") → refused
```

Its value is the text it covered, like any rule that captures nothing. A recognizer with an `out T` builds a
value, and a capture of it holds that value:

```dotgram
@using System;

Id : @Guid = '{' & g: @ReadGuid & '}' => g

parse Id
```

```csharp
using System;

partial class Grammar
{
	static bool ReadGuid(ReadOnlySpan<char> input, ref int pos, out Guid value)
	{
		if (input.Length - pos >= 36 && Guid.TryParse(input.Slice(pos, 36), out value))
		{
			pos += 36;

			return true;
		}

		value = default;

		return false;
	}
}
```

```text examples
ParseId("{0f8fad5b-d9cb-469f-a165-70867728950e}") → 0f8fad5b-d9cb-469f-a165-70867728950e
ParseId("{0f8fad5b}") → refused
```

The same name may be a predicate and a recognizer, by overloads: `[@Foo]` calls `bool Foo(char)`, `@Foo`
calls the recognizer overload.

**A recognizer is trusted absolutely.** The `ref` is the method saying it moves the position; nothing copies
it away or checks it afterwards. Move it backwards or past the end and the rest of the grammar reads
nonsense, undiagnosed. On failure it must leave `pos` where it was.

Notes:

- Where no method of the role's shape is in reach of the host — in it, around it, in what it derives from,
  in an included grammar's class or a `global using static` — that is said on the grammar (`GRAM4025`)
  rather than by the C# compiler in a generated file.
- Two `out T` overloads of one name with different `T` are a tie, reported rather than guessed (`GRAM4015`).
- A recognizer over the input view may take the grammar's `context` as its last parameter, after an `out T`:
  `bool M(ParserInput<char> input, ref int pos, out T value, Names context)`
  ([context and state](15-context-and-state.md#context--t)).
- In a grammar split into tokens, a terminal that is nothing but `@M` has no beginning the lexer can stop on,
  so the host is asked at every token (`GRAM5011`, a warning); write a regular beginning in front of it.

```dotgram GRAM4025
Number = @NoSuchMethod

parse Number
```

<!-- ru:begin -->
> **RU.** Голый `@M` передаёт хосту вход и позицию; хост сдвигает позицию за прочитанное и отвечает,
> прочитал ли что-нибудь. Пишите его над `ParserInput<char>` — представлением входа, которое генератор
> пишет в хост: один метод служит всем публикациям грамматики — строке, `TextReader`, потоку (пример
> `Fixed` проверен и над строкой, и над читателем; `TryAdvance`, `Ensure`, `Peek`, `Slice` дочитывают вход
> по мере надобности). **Только для строк быстрее span**: распознаватель над `ReadOnlySpan<char>` (пример
> `HexRun`) не отличает конец буфера от конца входа, поэтому публикация над читателем или потоком его не
> принимает. Значение — покрытый текст; распознаватель с `out T` строит
> значение, и захват держит его (пример `ReadGuid` → `Guid`). Одно имя может быть и предикатом, и
> распознавателем — перегрузками. **Распознавателю доверяют полностью**: позицию никто не копирует и не
> проверяет; сдвинуть назад или за конец — значит сломать разбор без диагностики; при неудаче `pos` нужно
> оставить на месте. Метод нужной формы вне досягаемости хоста — `GRAM4025` (на
> грамматике, а не ошибкой C#); две перегрузки `out T` с разными `T` — `GRAM4015`; распознаватель над
> `ParserInput` может последним параметром принять `context` грамматики (глава 15); терминал разделённой
> грамматики из одного `@M` — предупреждение `GRAM5011`.
<!-- ru:end -->

## C# in constructions and guards

After `=>` and `when`, everything under the `@` is the consumer's C#, handed to the compiler as written: a
name, a dotted name, a call, a generic name, or a parenthesized expression. The generator does not parse it;
for `@(…)` it finds the closing parenthesis with C#'s own tokenizer, so a `)` inside a string or a comment is
no end.

```dotgram
@using System;
@using System.Globalization;

Word    : @string = w: { ['a'..'z']+ } => @(w + ")")
Number  : int     = d: { ['0'..'9']+ } => @int.Parse(d, CultureInfo.InvariantCulture)
Color   : @ConsoleColor = c: { ['A'..'Z'] & ['a'..'z']+ } => @Enum.Parse<ConsoleColor>(c)

parse Word
parse Number
parse Color
```

```text examples
ParseWord("ab") → "ab)"
ParseNumber("042") → 42
ParseColor("Red") → Red
```

Notes:

- Captures and the [supplied names](07-values.md#names-the-parser-supplies) are local variables there.
  `=> @M(a, b)` and `=> @(M(a, b))` are one construction.
- A C# mistake is a C# error, reported against the grammar's line through `#line`: a misspelt method
  surfaces where the grammar wrote it.
- `@(` is written together; there is no block form `@{ … }`: code belongs in the host, where there is a
  debugger.

<!-- ru:begin -->
> **RU.** После `=>` и `when` всё под `@` — C# потребителя, передаваемый компилятору как написан: имя,
> имя через точку, вызов, обобщённое имя, выражение в скобках. Генератор его не разбирает; конец `@(…)`
> ищется токенизатором C#, поэтому `)` внутри строки или комментария — не конец (пример `w + ")"`).
> Захваты и даваемые имена — локальные переменные; `=> @M(a, b)` и `=> @(M(a, b))` — одно и то же. Ошибка
> C# — ошибка C#, привязанная к строке грамматики через `#line`. `@(` пишется слитно; блочной формы
> `@{ … }` нет — код место в хосте, где есть отладчик.
<!-- ru:end -->

## What the C# side must guarantee

- **Constructions, guards, selectors and recognizers may run more than once**, and for a reading the parse
  then gives up: ordered choice and lookahead try them on the path being read. Keep them free of effects that
  would need undoing ([values](07-values.md#when-a-construction-runs)).
- **Their answers depend on their arguments and the text only.** A reader over tokens remembers where a rule
  failed and answers the next entry there without reading again; a guard that reads a static field which
  changes during the parse may be answered from before the change. State that changes belongs in `context`,
  which the generator can see ([context and state](15-context-and-state.md)).
- **A recognizer restores the position on failure**, and may succeed without moving it: inside a repetition,
  past its fewest turns, a turn that does not move ends the repetition.
- **Nothing is caught.** An exception from the C# leaves the parse, from `TryParse…` too.

<!-- ru:begin -->
> **RU.** Что обязана гарантировать сторона C#: конструирование, охранники, селекторы и распознаватели могут
> выполняться несколько раз и для отброшенного прочтения — без эффектов, требующих отката; их ответ зависит
> только от аргументов и текста (читатель над токенами запоминает неудачи и отвечает повторно без чтения;
> охранник, читающий меняющееся статическое поле, может получить ответ «из прошлого»; меняющееся
> состояние — в `context`, который генератор видит); распознаватель при неудаче восстанавливает позицию и
> может успешно ничего не прочитать (в повторении сверх минимума такой шаг заканчивает повторение);
> ничего не перехватывается — исключение выходит из разбора, и из `TryParse…` тоже.
<!-- ru:end -->

## Names the generated code keeps

- The generated code gives every name of its own a `_DotGram` in it, so nothing the host declares can hide
  the author's method. A name the grammar writes may not contain it (`GRAM1012`), and the host's members whose
  names do are reserved.
- Every supplied name begins with `parser`: a capture or a parameter may not take one (`GRAM4012`), and using
  one in C# where it is not handed over — `parserInput` in a `when`, a method or a type of that name — is
  `GRAM4053`.

```dotgram GRAM1012
Row_DotGram = 'a'

parse Row_DotGram
```

<!-- ru:begin -->
> **RU.** Сгенерированный код даёт всем своим именам `_DotGram`, чтобы ничто в хосте не заслонило метод
> автора; имя в грамматике не может его содержать (`GRAM1012`), а члены хоста с ним зарезервированы. Все
> даваемые имена начинаются с `parser`: захват или параметр так называться не может (`GRAM4012`), а
> использование такого имени в C# там, где оно не передаётся, — `GRAM4053`.
<!-- ru:end -->
