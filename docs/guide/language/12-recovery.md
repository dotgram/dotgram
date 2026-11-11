# Recovery: `recover`

A grammar that reads a feed has a need a grammar that reads a source file does not: a million records,
one of them malformed, and the answer must be a word about that record and a parse that keeps going.
`recover` marks a repetition whose bad elements are stepped over rather than ending the parse. The
specification is [`syntax.md` §8.2–§8.3](../../syntax.md#82-recover--a-repetition-that-survives-a-bad-element).

## Marking a repetition

`X* recover S` says three things about the repetition:

1. At every boundary between elements, once its minimum is met, the **rest of the rule after the
   repetition is tried first**; where it succeeds, the repetition ends. Otherwise an element is read.
2. An element that fails while input remains is a bad element, even if it read nothing: the parser skips
   past the next match of `S` and starts the next element there.
3. A rejected element still takes its place in the numbering.

With a `=>`, a rejected element becomes an element of the sequence too:

```dotgram
Row  : @string   = v: { ['a'..'z']+ } & eol => v
Rows : @string[] = Row* recover eol => @("bad: " + parserText.TrimEnd())

parse Rows
```

```text examples
ParseRows("a\nb\n") → ["a", "b"]
ParseRows("a\n12\nb\n") → ["a", "bad: 12", "b"]
ParseRows("a\n\nb\n") → ["a", "bad: ", "b"]
```

Without a `=>` the bad element is dropped from the sequence and reported out of band:

```dotgram
Row  : @string   = v: { ['a'..'z']+ } & eol => v
Rows : @string[] = Row* recover eol

parse Rows
```

```text examples
ParseRows("a\n12\nb\n") → ["a", "b"]
```

The host learns of it through a partial method the generator declares in a grammar with a `recover` that
has no `=>`, `OnRecovered`, told the rule the element should have been, the text it covered, where it began,
which element it was and why it was rejected:

```dotgram
Row  : @string   = v: { ['a'..'z']+ } & eol => v
Rows : @string[] = Row* recover eol

parse Rows
```

```csharp
partial class Grammar
{
	public static readonly System.Collections.Generic.List<string> Log = new();

	static partial void OnRecovered(
		string rule, string text, long position, int line, int column, int ordinal, string message)
	{
		Log.Add($"{rule} #{ordinal} at {line}:{column}: {message}");
	}
}
```

```text examples
ParseRows("a\n12\nb\n") → ["a", "b"]
@Log → ["Row #1 at 2:1: Expected end of input or ['a'..'z'] at 2:1."]
```

The message names the end of input beside the row, because the rest of the rule — here nothing — was
tried first at that boundary (below). Implementing it is optional: it is a `partial void` with no access modifier, so where nobody implements it,
the compiler removes every call together with its arguments, and the parse counts no lines for a channel
nobody listens on.

Notes:

- `recover` belongs on a repetition: on anything else it is refused (`GRAM4010`).
- **Nothing is ever caught.** An exception out of a `=>` leaves the parse, inside a marked repetition as
  anywhere else: a bug in the author's C# is not "row 400 was malformed".
- A missing required element or trailer still fails the whole parse: recovery is about the elements, not
  the envelope around them.
- Recovery is opt-in because "read something, then failed, therefore malformed" is wrong for ordinary
  grammars: an element that reads four characters and then backtracks is usually healthy.

<!-- ru:begin -->
> **RU.** Грамматике ленты нужно то, чего не нужно грамматике исходника: миллион записей, одна из них
> кривая, и ответ — слово об этой записи и разбор, идущий дальше. `X* recover S` говорит о повторении
> три вещи: на каждой границе между элементами (после минимума) **сначала пробуется остаток правила
> после повторения**, и если он удался, повторение кончается; иначе читается элемент; элемент, который не
> удался при оставшемся входе, — плохой, даже если ничего не прочитал: разбор пропускает до следующего
> совпадения `S` и начинает там следующий элемент; отвергнутый элемент занимает своё место в нумерации.
> С `=>` отвергнутый элемент становится элементом последовательности (пример `bad: 12`); без `=>` он
> выбрасывается и сообщается в сторону — через частичный метод `OnRecovered`, который генератор
> объявляет в хосте: ему сообщаются правило, которым должен был быть элемент, его текст, начало, номер и
> причина (пример с журналом `Log`, проверенным прогоном `@Log`). Реализация необязательна: это
> `partial void` без модификатора доступа, и без реализации вызовы удаляются вместе с аргументами.
> `recover` ставится только на повторение (иначе `GRAM4010`). **Ничего не перехватывается**: исключение из
> `=>` выходит из разбора. Отсутствие обязательного элемента или окончания по-прежнему проваливает весь
> разбор. Восстановление включается явно, потому что «прочитал, потом упал — значит, кривой» неверно для
> обычных грамматик.
<!-- ru:end -->

## The rest of the rule goes first

At a boundary the parser tries the complete continuation after the repetition before another element. That
lets a broad row shape stand beside a specific trailer:

```dotgram
Line    : @string   = v: { [^ '\n']+ } & '\n' => v
Feed    : @string[] = "BEGIN\n" & Line* recover '\n' & "END\n" & eof

parse Feed
```

```text examples
ParseFeed("BEGIN\na\nb\nEND\n") → ["a", "b"]
ParseFeed("BEGIN\na\nEND\n") → ["a"]
ParseFeed("BEGIN\na\n") → refused
```

`"END\n"` gets first refusal at every boundary: it usually fails on its first character and costs almost
nothing, and at the real trailer it wins although `Line` could have read that line too. A missing trailer
fails the whole parse.

<!-- ru:begin -->
> **RU.** На границе разбор пробует полное продолжение после повторения раньше следующего элемента. Это
> позволяет широкой форме строки стоять рядом с конкретным окончанием (пример `Feed`): `"END\n"` получает
> право первого отказа на каждой границе (обычно отказывает на первом символе и почти ничего не стоит), а
> на настоящем окончании побеждает, хотя `Line` тоже мог бы прочитать эту строку. Отсутствие окончания
> проваливает весь разбор.
<!-- ru:end -->

## Where a bad element was, and why

A recovery's `=>` is handed names that a rule's `=>` is not:

| Name | Is |
| --- | --- |
| `parserOrdinal` | which element of the repetition, counting rejected ones, from 0 |
| `parserLine`, `parserColumn` | where it starts, for a person, from 1 |
| `parserPosition` | where it starts, an absolute offset (`long`) |
| `parserSpan`, `parserText` | its extent, and the text it covers up to where reading picked up again |
| `parserInput` | the whole input |
| `parserMessage` | why it was rejected: what was expected and where, `line:column` |
| `parserExpected` | what would have fit where reading stopped |
| `parserFailureLine`, `parserFailureColumn`, `parserFailurePosition` | where reading it stopped |

```dotgram
Amount : @string = v: { ['0'..'9']+ & '.' & ['0'..'9']{2} } & '\n' => v
Prices : @string[] = Amount* recover '\n'
                   => @($"#{parserOrdinal} at {parserLine}:{parserColumn}, stopped at {parserFailureColumn}: {parserExpected}")
Said   : @string[] = Amount* recover '\n' => @(parserMessage)

parse Prices
parse Said
```

```text examples
ParsePrices("1.50\n3,20\n7.00\n") → ["1.50", "#1 at 2:1, stopped at 2: '.'", "7.00"]
ParseSaid("1.50\n3,20\n") → ["1.50", "Expected '.' at 2:2."]
```

Notes:

- Where a bad element **began** and where reading it **stopped** are different places: `3,20` begins at
  column 1, and it is the comma at column 2 where a `.` would have fit.
- `parserMessage` puts them together, `Expected '.' at 2:2.`; a rule with `on fail` says its own sentence
  instead ([values](04-values.md#what-a-refusal-says-on-fail)).
- Lines count as `eol` counts them, however a stream's buffer cut a `\r\n`.
- A successful element may capture the same position names, which is how a record carries its own
  position.
- `parserMessage`, `parserExpected` and the `parserFailure…` names exist only in a recovery's `=>`; written in
  a rule's `=>` they are refused (`GRAM4053`). Counting lines costs a scan, done only where a name that
  needs it is written.

<!-- ru:begin -->
> **RU.** `=>` восстановления получает имена, которых нет у `=>` правила (таблица): `parserOrdinal` —
> номер элемента с отвергнутыми, с 0; `parserLine`, `parserColumn` — где начался, с 1; `parserPosition` —
> абсолютное смещение; `parserSpan`, `parserText` — протяжённость и текст до места, где чтение
> возобновилось; `parserInput` — весь вход; `parserMessage` — почему отвергнут (что ожидалось и где,
> `строка:столбец`); `parserExpected` — что подошло бы там, где чтение остановилось;
> `parserFailureLine/Column/Position` — где оно остановилось. Где плохой элемент **начался** и где его
> чтение **остановилось** — разные места (`3,20` начинается в столбце 1, а точка ожидалась вместо
> запятой в столбце 2). `parserMessage` соединяет их: `Expected '.' at 2:2.`; правило с `on fail` говорит
> своё. Строки считаются так, как их считает `eol`. Удачный элемент может захватывать те же имена мест.
> `parserMessage`, `parserExpected` и `parserFailure…` есть только в `=>` восстановления; в `=>` правила
> они отвергаются (`GRAM4053`). Подсчёт строк стоит прохода и делается только там, где он нужен.
<!-- ru:end -->

## The synchronization is one operand

`recover` binds tighter than `|`, like `&`, so a choice of synchronizations needs brackets:

```dotgram
Cell   : @string   = v: { ['a'..'z']+ } & ('|' | '\n') => v
Fields : @string[] = Cell* recover ('|' | '\n') => @("?")

parse Fields
```

```text examples
ParseFields("a|b\nc\n") → ["a", "b", "c"]
ParseFields("a|b\nc|\n") → ["a", "b", "c", "?"]
ParseFields("a|1|c\n") → ["a", "?", "c"]
ParseFields("a|1\nc\n") → ["a", "?", "c"]
```

In `c|` followed by a line end, the empty field before `\n` is itself a bad element: recovery reports what
the grammar says, and a grammar that allows an empty field says so in `Cell`. `X* recover '|' | eol` would be a choice between a recovering repetition and `eol`. Resuming at the next field
separator keeps the rest of a record; `eol` in the same choice is the backstop for a record so broken that no
separator is left in it.

**A repetition may recover inside another one's element.** It recovers as if it were a rule of its own in
that place:

```dotgram
Word  : @string   = v: { ['a'..'z']+ } => v
Lines : @string[] = ('[' & (Word & ';')* recover ';' & ']' & '\n')* recover '\n'

parse Lines
```

```text examples
ParseLines("[a;b;]\n[c;9;d;]\n[e;]\n") → ["a", "b", "c", "d", "e"]
```

The inner one steps over `9;` and the second line keeps `c` and `d`; the outer one steps over a line only
where the inner one cannot. A guard or capture inside it that a rule of its own could not be handed or hand
back is refused where the inner `recover` is written (`GRAM4045`).

<!-- ru:begin -->
> **RU.** `recover` связывает сильнее `|`, как `&`, поэтому выбор синхронизаций пишется в скобках (пример
> `Fields`): `X* recover '|' | eol` было бы выбором между восстанавливающимся повторением и `eol`.
> Возобновление на следующем разделителе поля сохраняет остаток записи; `eol` в том же выборе — страховка
> для записи, где разделителей не осталось. **Повторение может восстанавливаться внутри элемента
> другого** — как отдельное правило на этом месте (пример `Lines`: внутреннее перешагивает `9;`, и вторая
> строка сохраняет `c` и `d`; внешнее перешагивает строку, только если внутреннее не может). Охранник
> или захват внутри, которые нельзя передать отдельному правилу или получить от него, отвергаются там, где
> написан внутренний `recover` (`GRAM4045`).
<!-- ru:end -->

## Where `recover` cannot stand

```dotgram GRAM4010
Row = 'a' recover ';'

parse Row
```

```dotgram GRAM4010
Rows = ['a'..'z']* recover ';' => @("?")

parse Rows
```

```dotgram GRAM4038
Row  = ['a'..'z']+ & ';'
Look = ?=(Row* recover ';') & any*

parse Look
```

```dotgram Lexical GRAM4044
namespace Lex
{
	trivia = none

	Word = (['a'..'z'] & ';')* recover ';'
}

trivia = { ' '* }

Text = Lex.Word

parse Text
```

- `recover` on something that is not a repetition, or a `=>` on a repetition that collects text rather than
  values, has nowhere to put a rejected element (`GRAM4010`).
- A lookahead answers yes or no and plans no recovery: name the recovering repetition as a rule and call it
  from the lookahead (`GRAM4038`).
- A token is the longest match of a regular language and has no elements to step over: in a grammar split
  into tokens, `recover` belongs in the syntax (`GRAM4044`).
- A binding around or inside a recovering repetition is not supported yet (`GRAM4034`,
  [bindings](06-bindings.md)).

<!-- ru:begin -->
> **RU.** Где `recover` стоять не может: не на повторении или с `=>` на повторении, которое собирает текст,
> а не значения, — отвергнутому элементу некуда деться (`GRAM4010`); внутри просмотра — он отвечает да
> или нет и восстановления не планирует: вынесите повторение в правило и вызовите его из просмотра
> (`GRAM4038`); в лексическом правиле разделённой грамматики — токен есть самое длинное совпадение
> регулярного языка, перешагивать в нём нечего, `recover` место в синтаксисе (`GRAM4044`); привязка вокруг
> или внутри восстанавливающегося повторения пока не поддерживается (`GRAM4034`).
<!-- ru:end -->

## Recovery and streaming

The synchronization is also the bound on what a streamed parse holds: an element cannot reach back past the
previous synchronization point, so a parse from a `TextReader` holds one element, not the file. A rule that
marks two repetitions recovers in both when read whole, but gets no reader overload (`GRAM5001`), and a
`yield` publication hands a rejected element over in its place ([publication](11-publication.md#a-lazy-result-yield)):

```dotgram
Row  : @string   = v: { ['a'..'z']+ } & '\n' => v
Rows : @string[] = Row* recover '\n' => @("bad")

parse Rows as Each yield
parse Rows stream
```

```text examples
Each("a\n1\nb\n") → ["a", "bad", "b"]
ParseRows(reader("a\n1\nb\n")) → ["a", "bad", "b"]
```

<!-- ru:begin -->
> **RU.** Синхронизация — ещё и граница того, что держит потоковый разбор: элемент не может дотянуться
> назад дальше предыдущей точки синхронизации, поэтому разбор из `TextReader` держит один элемент, а не
> файл. Правило с двумя восстанавливающимися повторениями восстанавливается в обоих при чтении целиком,
> но перегрузки над читателем не получает (`GRAM5001`); публикация `yield` отдаёт отвергнутый элемент на
> его месте (пример: `Each` и `ParseRows(reader(…))` дают `["a", "bad", "b"]`).
<!-- ru:end -->
