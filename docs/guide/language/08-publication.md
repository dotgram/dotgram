# Publication: `parse` and `find`

A rule on its own creates no public API. A directive does: `parse` and `find` turn a rule into methods of
the host class. This page covers both directives and every part a directive may have. The specification
is [`syntax.md` §6](../../syntax.md#6-publication).

## `parse` and `find`

The two differ in one thing: whether input that does not match may sit between the matches.

| Directive | Says | Generates |
| --- | --- | --- |
| `parse R` | the whole input is an `R` | `R ParseR(input)`, which throws `FormatException`; `Match<R> TryParseR(input)`; `bool TryParseR(input, out R value)` |
| `find R` | there are `R`s inside something else | `IEnumerable<Match<R>> FindR(input)`, lazy |

```dotgram
Number : int = { ['0'..'9']+ } => @int.Parse(parserText)

parse Number
find  Number
```

```text examples
ParseNumber("42") → 42
ParseNumber("4 2") → refused
TryParseNumber("42x") → refused: Expected end of input.
FindNumber("a1 b22, 333") → [1, 22, 333]
TryParseNumber("7", out) → 7
TryParseNumber("x", out) → refused
```

`ParseNumber` asserts and throws; `TryParseNumber` asks and answers a `Match<int>`; the form with an `out`
answers only whether, `if (Grammar.TryParseNumber(text, out var n))`.

Notes:

- `Match<T>` has `Outcome`, `Value`, `Error`, `Position`, `Length` and `IsSuccess`. On a match, `Position`
  is where it began; on a failure, the furthest the input could be followed.
- The `out` form reads once and says nothing about why; the `Match` form may read a refused input twice to
  say where and what was expected. An accepted input is one reading either way.
- `find` yields occurrences as it finds them, not overlapping, and skips what does not match. `First()`,
  `Where()` and `Take()` are LINQ's.
- A rule whose value is a sequence is published with `parse` like any other; what comes back is the
  sequence.

<!-- ru:begin -->
> **RU.** Правило само по себе публичного API не создаёт — его создаёт директива. `parse R` говорит «весь
> вход — это `R`» и генерирует `ParseR` (бросает `FormatException`), `TryParseR` → `Match<R>` и
> `bool TryParseR(input, out R)` (прогоны с `out`); `find R` говорит «внутри есть `R`» и генерирует ленивый
> `FindR` → `IEnumerable<Match<R>>`. `Match<T>`: `Outcome`, `Value`, `Error`, `Position`, `Length`,
> `IsSuccess`; при совпадении `Position` — начало, при неудаче — дальше всего, куда удалось дочитать.
> Форма с `out` читает один раз и ничего не объясняет; форма с `Match` может прочитать отвергнутый вход
> дважды, чтобы сказать где и что ожидалось. `find` отдаёт вхождения по мере нахождения, без перекрытий,
> пропуская несовпадающее; `First()`, `Where()`, `Take()` — это LINQ. Правило-последовательность
> публикуется обычным `parse`.
<!-- ru:end -->

## Naming the methods: `as`

A directive names its methods after the rule: `ParseNumber`, `TryParseNumber`, `FindNumber`. `as` names
them instead — `parse Number as ReadNumber` makes `ReadNumber` and `TryReadNumber`.

```dotgram
Number : int = { ['0'..'9']+ } => @int.Parse(parserText)

parse Number as ReadNumber
find  Number as Numbers
```

```text examples
ReadNumber("7") → 7
TryReadNumber("x") → refused
Numbers("1,2") → [1, 2]
```

A publication is members of the generated class, so its names may not be ones the class declares for its
callers (`Match`, `Outcome`, `SourceSpan`, `ParserInput`, `OnRecovered`, …) or for itself, nor the class's
own name (`GRAM5018`), and two publications may not make one method (`GRAM3005`). Names containing
`_DotGram` are the generator's (`GRAM1012`).

```dotgram GRAM5018
Word = ['a'..'z']+

parse Word as Match
```

```dotgram GRAM3005
Word = ['a'..'z']+
Name = ['A'..'Z']+

parse Word as Read
parse Name as Read
```

<!-- ru:begin -->
> **RU.** Директива называет методы по правилу (`ParseNumber`, `TryParseNumber`, `FindNumber`); `as` даёт
> своё имя: `parse Number as ReadNumber` создаёт `ReadNumber` и `TryReadNumber`. Публикация — это члены
> сгенерированного класса, поэтому её имя не может совпадать с тем, что класс объявляет для вызывающих
> (`Match`, `Outcome`, `SourceSpan`, `ParserInput`, `OnRecovered`, …) или для себя, ни с именем самого
> класса (`GRAM5018`); две публикации не могут дать один метод (`GRAM3005`); имена с `_DotGram`
> принадлежат генератору (`GRAM1012`).
<!-- ru:end -->

## Publishing an expression

Wherever a directive names a rule, any operand may stand: a call, a group, a set. An expression has no name
to make a method name from, so `as` is required.

```dotgram
Word = { ['a'..'z']+ }
Padded(item, pad) = pad* & item & pad*

parse Padded(Word, ' ') as Spaced
parse ('a' | 'b')       as Ab
find  ['0'..'9']+       as Numbers
```

```text examples
Spaced("  ab ") → "  ab "
Ab("b") → "b"
Numbers("a12b3") → ["12", "3"]
```

The expression becomes an ordinary rule of that name, declared where the directive is written, so it reads
the `trivia`, the imports and the rebindings around it, and its value is the extent it matched. Given a
type, it may build a value too:

```dotgram
Word = { ['a'..'z']+ }

parse (w: Word & '!' => @(w.ToUpperInvariant())) as Shout : @string
```

```text examples
Shout("hey!") → "HEY"
```

Refused:

```dotgram GRAM2007
parse ('a' | 'b')
```

```dotgram GRAM4008
Word = { ['a'..'z']+ }

parse (w: Word & '!' => @(w.ToUpperInvariant())) as Shout
```

A publication of an expression needs `as` (`GRAM2007`), and one that constructs needs its type
(`GRAM4008`). A choice needs brackets: the operand is one operand, as `recover`'s synchronization is.

<!-- ru:begin -->
> **RU.** Там, где директива называет правило, может стоять любой операнд: вызов, группа, множество. У
> выражения нет имени для метода, поэтому `as` обязателен (иначе `GRAM2007`). Выражение становится
> обычным правилом с этим именем, объявленным там, где написана директива: оно читает окружающие
> `trivia`, импорты и перепривязки, а его значение — прочитанный текст. С объявленным типом оно может и
> строить значение (пример `Shout`); конструирование без типа — `GRAM4008`. Выбор пишется в скобках:
> операнд один, как синхронизация `recover`.
<!-- ru:end -->

## The result contract: `: @T`

On a named rule, `: @T` after the directive sets the type its methods return, without changing how the
rule builds its value: `parse` returns `T`, `TryParse` a `Match<T>`, `find` `IEnumerable<Match<T>>`.

```dotgram
Number : int = { ['0'..'9']+ } => @int.Parse(parserText)

parse Number as Boxed : @object
parse Number as Plain
```

```text examples
Boxed("5") → 5
Plain("5") → 5
```

The rule's value must convert to `T` by identity, a reference conversion or boxing; no numeric or
user-defined conversion is added (`GRAM4028`). For another type, declare the rule with it (`Number : @long = …`) or
convert in the construction (`=> @((long)n)`).

```dotgram GRAM4028
Number : int = { ['0'..'9']+ } => @int.Parse(parserText)

parse Number as Wide : @long
```

<!-- ru:begin -->
> **RU.** На именованном правиле `: @T` после директивы задаёт тип, который возвращают методы, не меняя
> того, как правило строит значение: `parse` возвращает `T`, `TryParse` — `Match<T>`, `find` —
> `IEnumerable<Match<T>>` (пример `Boxed : @object`). Значение правила должно приводиться к `T`
> тождественно, ссылочным преобразованием или упаковкой; числовых и пользовательских преобразований нет
> (`GRAM4028`: `int` в `long` не публикуется). Для другого типа объявите правило с ним (`Number : @long = …`) или
> преобразуйте в конструировании (`=> @((long)n)`).
<!-- ru:end -->

## Access, and reading from a position

A directive publishes `public` methods unless it says otherwise, as C# does; and a `parse` reads the whole
input unless it says `positional`.

```dotgram
Word = { ['a'..'z']+ }

parse Word                       as ParseWord
internal parse Word              as ParseInternal
private parse Word               as ParsePrivate
positional parse Word            as ParseHere
```

```text examples
ParseWord("ab") → "ab"
TryParseHere("ab cd", 3) → "cd"
TryParseHere("ab cd", 0) → "ab"
TryParseHere("ab cd", 2) → refused
TryParseHere("ab cd", 0, 1) → "a"
TryParseHere("ab cd", 3, out) → "cd"
TryParseWord("ab cd") → refused
```

`TryParseHere("ab cd", 0)` stops after `ab`, where the whole-input `TryParseWord` refuses what follows;
`(input, 0, 1)` reads the window of one character from 0. The forms that answer only whether take the
position by `ref` and move it past what they read — `TryParseHere(text, ref at, out var word)` — so a caller
can read on from there.

| Written | Methods |
| --- | --- |
| `parse Word` | public `ParseWord`, `TryParseWord` |
| `internal parse …` | the same, `internal` |
| `private parse …` | the same, `private`: for the host's own code and constructions |
| `positional parse …` | also `TryParseWord(input, at)`, `TryParseWord(input, at, length)` and their `bool` forms: read from where the caller says, and stop short of the end |

The access comes first, then `positional`, each once; the words are words only in front of `parse` or
`find`, so a rule may still be called `internal`. A `find` reads every position already, and a `yield` has
no `Try` form, so neither takes `positional`.

```dotgram GRAM2013
Word = { ['a'..'z']+ }

positional internal parse Word
```

```dotgram GRAM2012
Word = { ['a'..'z']+ }

positional find Word
```

<!-- ru:begin -->
> **RU.** Директива публикует `public`-методы, если не сказано иное (как в C#), а `parse` читает весь вход,
> если не сказано `positional` (таблица): `internal`/`private` — доступ всех методов директивы;
> `positional` добавляет `TryParseWord(input, at)`, `TryParseWord(input, at, length)` и их `bool`-формы —
> чтение с указанной позиции с остановкой до конца (прогоны: с позиции 3 — `cd`, с 0 — `ab`, окно
> `(0, 1)` — `a`). Сначала доступ, затем `positional`, каждое один раз
> (иначе `GRAM2013`); эти слова — ключевые только перед `parse`/`find`. `find` и так читает с каждой
> позиции, у `yield` нет формы `Try` — `positional` им не положен (`GRAM2012`).
<!-- ru:end -->

## Input from a reader or a stream: `stream`, `stream bytes`

A string is read in memory. `stream` adds overloads over a `TextReader`, and `stream bytes` over a byte
`Stream`, `byte[]` and `ReadOnlyMemory<byte>`; both keep the string overload.

```dotgram
Row  : @string   = v: { ['a'..'z']+ } & eol => v
Rows : @string[] = { Row* }

parse Rows stream
```

```text examples
ParseRows("a\nb\n") → ["a", "b"]
ParseRows(reader("a\nb\n")) → ["a", "b"]
TryParseRows(reader("a\n1\n")) → refused
```

`ParseRows(reader)` reads through a buffer; `bufferSize` and `maxRetained` may be passed per call.

A grammar read over bytes compares byte values, and what it captures is a span of bytes:

```dotgram
Line = ['a'..'z']+ & '\n'
Size : @int = text: Line* => @(text.Length)

parse Size stream bytes
```

```text examples
ParseSize("ab\ncd\n") → 6
ParseSize(bytes("ab\ncd\n")) → 6
ParseSize(stream("ab\n")) → 3
TryParseSize(stream("ab\n1")) → refused
```

`text` is a `string` in the string form and a `ReadOnlySpan<byte>` in the byte forms; `Length` is a member of
both, so one construction serves all of them.

Over bytes, captured text is a `ReadOnlySpan<byte>` that nothing decodes, so a rule that builds a `string`
from it is refused for the byte form, saying what to write — a host method for each input:

```dotgram GRAM4026
Row  : @string   = v: { ['a'..'z']+ } & eol => v
Rows : @string[] = { Row* }

parse Rows as RowBytes stream bytes
```

Notes:

- The caller owns the reader or the stream; the parse does not dispose it, and may read ahead.
- `bufferSize` and `maxRetained` are optional per call; the defaults are the host's `[Gram(InitialBufferSize = …,
  MaxBufferedInputLength = …)]`.
- Over bytes, literals compare byte values; Unicode categories, case folding and predicates over
  characters are not available, and captured text reaches C# as `ReadOnlySpan<byte>`.
- A form the grammar cannot be read in is refused, naming why (`GRAM4026`); where the reader overload is
  not offered for a plain `parse`, the reason is reported (`GRAM5001`). What decides which rules stream is
  in [`syntax.md` §6.3](../../syntax.md#63-the-input-type-picks-the-execution-mode).

<!-- ru:begin -->
> **RU.** Строка читается в памяти. `stream` добавляет перегрузки над `TextReader`, `stream bytes` — над
> байтовым `Stream`, `byte[]` и `ReadOnlyMemory<byte>`; строковая перегрузка остаётся. Над байтами
> захваченный текст — `ReadOnlySpan<byte>`, который никто не декодирует, поэтому правило, строящее из
> него `string`, для байтовой формы отвергается с подсказкой — метод хоста для каждого вида входа
> (`GRAM4026`). Пример `Size`: одна конструкция `text.Length` работает и над строкой, и над байтами
> (`ParseSize(bytes("ab\ncd\n")) → 6`, `stream(…)`). Читатель или поток
> принадлежит вызывающему: разбор его не закрывает и может читать вперёд. `bufferSize` и `maxRetained` —
> необязательные параметры вызова, по умолчанию — из `[Gram(InitialBufferSize, MaxBufferedInputLength)]`. Над байтами
> литералы сравнивают значения байтов; категорий Unicode, свёртки регистра и предикатов над символами нет,
> захваченный текст приходит в C# как `ReadOnlySpan<byte>`. Неподдерживаемая форма отвергается с причиной
> (`GRAM4026`); если для обычного `parse` перегрузки над читателем нет, причина сообщается (`GRAM5001`).
<!-- ru:end -->

## A lazy result: `yield`

`yield` makes a `parse` of a sequence rule return `IEnumerable<T>` that reads one element at a time,
whatever the input — a string included.

```dotgram
Row  : @string   = v: { ['a'..'z']+ } & ';' => v
Rows : @string[] = { Row* }

parse Rows as Each yield
parse Rows as AsObjects yield : @object
```

```text examples
Each("a;bc;") → ["a", "bc"]
AsObjects("a;") → ["a"]
```

Notes:

- `yield : @T` states the element type; bare `yield` takes it from the rule's `@T[]`.
- The collection must be a whole `Rule*` or `Rule+`, possibly braced, with no factory, outer choice,
  prefix, suffix or implicit trivia; other shapes are refused rather than silently materialized
  (`GRAM4027`). A recovering repetition is supported ([recovery](13-recovery.md)).
- A malformed element throws `FormatException` from `MoveNext`, after the elements before it have been
  returned. There is no `Try` form: publish the ordinary `parse` as well for an all-or-nothing answer.
- On a named rule the element type goes after `yield`, not before it (`GRAM2008`).

```dotgram GRAM4027
Row  : @string   = v: { ['a'..'z']+ } & ';' => v
Rows : @string[] = '[' & Row* & ']'

parse Rows as Each yield
```

```dotgram GRAM2008
Row  : @string   = v: { ['a'..'z']+ } & ';' => v
Rows : @string[] = { Row* }

parse Rows as Each : @object yield
```

<!-- ru:begin -->
> **RU.** `yield` делает `parse` правила-последовательности ленивым `IEnumerable<T>`, читающим по одному
> элементу, даже над строкой. `yield : @T` задаёт тип элемента, голый `yield` берёт его из `@T[]`
> правила. Коллекция должна быть целым `Rule*` или `Rule+` (можно в скобках) без фабрики, внешнего выбора,
> префикса, суффикса и неявной trivia; иначе — отказ, а не молчаливая материализация (`GRAM4027`, пример
> со скобками `[ … ]`). Восстанавливающееся повторение поддерживается. Плохой элемент бросает
> `FormatException` из `MoveNext` уже после выданных до него; формы `Try` нет — для ответа «всё или
> ничего» опубликуйте и обычный `parse`. На именованном правиле тип элемента пишется после `yield`
> (`GRAM2008`).
<!-- ru:end -->

## A whole directive

Every part, in order, every bracketed one optional:

```text
[access] [positional] parse|find <operand> [with (…)] [as Name] [: @T] [stream [bytes]] [yield [: @T]]
```

- `with (…)` reads the operand with rules replaced, which is how one grammar publishes several parsers
  ([namespaces](10-namespaces.md#rebinding-a-publication)).
- A whole `parse` reads the rule's `trivia` before and after it, and the value leaves it out
  ([trivia](05-trivia-and-words.md#trivia-what-may-stand-between-operands)).
- A grammar that names `eos` publishes over lists of strings and readers of lines only
  ([elements](02-elements.md#eos-and-input-in-segments)).

<!-- ru:begin -->
> **RU.** Полная директива — все части по порядку, каждая в скобках необязательна (схема выше). `with (…)`
> читает операнд с заменёнными правилами — так одна грамматика публикует несколько парсеров (глава 10).
> Целый `parse` читает `trivia` правила до и после него, а значение её не включает (глава 05).
> Грамматика с `eos` публикуется только над списками строк и читателями строк (глава 02).
<!-- ru:end -->
