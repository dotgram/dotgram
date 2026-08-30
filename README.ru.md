*[English](README.md)*

# .Gram

Генератор исходного кода, компилирующий грамматику в парсер на C#.

Формат пишется один раз, правилами. Из этого одного файла генератор производит:

- парсер — обычный C# в вашей собственной сборке, никакой библиотеки времени выполнения не
  подключается;
- типы результата, следующие из захватов, так что совпадение возвращается как
  `row.Symbol`, а не как `match.Groups[3].Value`;
- методы `Parse`, `TryParse` и `Find`, чьи отказы несут позицию и говорят, чего в ней
  ждали;
- перегрузки, читающие из `TextReader` без удержания входа, — там, где грамматика это
  позволяет;
- ошибки времени компиляции в вашей сборке на то, что в грамматике неверно, и указывают
  они на тот символ грамматики, который неверен.

Он нацелен на форматы, которые программе действительно приходится читать, — фиды,
конфигурацию, протоколы, языки запросов и выражений — и на тот момент, когда регулярное
выражение перестало читаться, а рукописный ридер перестал внушать доверие.

```dotgram
Row = "R" & '|' & symbol: Text & '|' & qty: Digit+ & eol
```

```csharp
row.Symbol      // a property, because `symbol:` is a capture
row.Qty
```

## С чего начать

```xml
<PackageReference Include="DotGram" Version="0.1.0"
                  PrivateAssets="all" ExcludeAssets="runtime" />
```

```csharp
[Gram("""
	Digits = ['0'..'9']+
	parse Digits
	""")]
public static partial class Numbers
{
	public static int Length(string text) => ParseDigits(text).Length;   // ParseDigits is generated here
}
```

Атрибут ставится на класс, в котором нужен парсер, и методы с типами появляются в нём.
Класс должен быть `partial`, и всякий класс вокруг него тоже; `static` не мешает. Грамматика
подлиннее кладётся в файл `.gram`, перечисленный как
`<AdditionalFiles Include="Numbers.gram" />`.

Во время выполнения не подключается ничего, потому что подключать нечего: всё, что нужно
парсеру, порождается в вашей собственной компиляции.

## Вместо регулярного выражения

Кванторы, символьные классы и альтернация пишутся как в регулярных выражениях, потому что
значат то же самое. Чего регулярное выражение не умеет — называть части:

```dotgram
Url        = scheme: Scheme & "://" & authority: Authority & path: Path
           & ('?' & query: Rest)? & ('#' & fragment: Rest)?

Scheme     = "https"i | "http"i | "ftp"i
Authority  = (user: UserInfo & '@')? & host: Host & (':' & port: Digit+)?
Host       = IPv4 | RegName

parse Url
find  Url as FindUrls
```

`scheme:` и `host:` возвращаются именованными свойствами порождённого типа, а не
пронумерованными группами, чей порядок вызывающий обязан помнить. Растущее правило
сохраняет имя; растущее регулярное выражение теряет читателя.

И это ещё быстрее. Бенчмарк гоняет эту грамматику против того же языка, записанного
регулярным выражением, и отказывается мерить, пока обе стороны не сойдутся на каждой части
каждого входа:

| Вход | .Gram | `RegexOptions.Compiled` |
| --- | ---: | ---: |
| короткий URL | 133.8 нс | 298.9 нс |
| хост как IP-адрес | 146.9 нс | 285.4 нс |
| отказ | 80.2 нс | 113.5 нс |
| путь из 84 символов | 191.0 нс | 453.0 нс |

Против интерпретируемого `Regex` — в 2.2–6.5 раза. [`docs/status.md`](docs/status.md)
содержит условия замера и то, чего эти числа не доказывают.

## Вместо рукописного ридера

Построчный фид — то, на что в деловой работе и уходит большая часть разбора:

```dotgram
Feed    = header: Header & rows: Row* & trailer: Trailer & eof

Header  = "H" & '|' & date: Date & '|' & source: Text & eol
Row     = "R" & '|' & symbol: Text & '|' & qty: Digit+ & '|' & date: Date & eol
Trailer = "T" & '|' & count: Digit+ & eol

Date    = year: Digit{4} & '-' & month: Digit{2} & '-' & day: Digit{2}

parse Feed
find Row as AllRows
```

```csharp
var feed = ParseFeed(text);              // the whole input is a Feed, or it throws

feed.Rows[0].Symbol;                     // captures are properties
feed.Header.Date.Year;                   // a rule's own captures are its own type

if (TryParseFeed(text) is { IsSuccess: true } match)
	…                                    // or ask, and get Value, Error, Position

foreach (var found in AllRows(text))     // occurrences, found as they are asked for
	…
```

Ни посетителя, ни дерева разбора, которое надо обходить: типы порождаются из захватов, так
что форма, которую вы читаете в грамматике, и есть форма, которую вы получаете в C#.
Захваты можно сопоставить и напрямую с конструктором или со свойствами `required` уже
имеющегося у вас типа — тогда в грамматике не будет ни строчки кода построения.

## Фиды, не помещающиеся в память

Та же грамматика читает из `TextReader`. Перегрузка выпускается там, где генератор может
доказать, что грамматика работает с переиспользуемым окном, а не с удерживаемой строкой:

```csharp
foreach (var part in ParseFeed(reader))       // parts arrive as they are read
	…

foreach (var part in ParseFeed(File.ReadLines(path)))
	…
```

Десять тысяч записей, тот же фид, те же построенные части, поданные тремя способами:

| Вход | Время | Выделено | Сборок Gen2 |
| --- | ---: | ---: | ---: |
| `string` | 719 мкс | 2653 КБ | 249 |
| `TextReader` | 433 мкс | 1415 КБ | 0 |
| `IEnumerable<string>` | 518 мкс | 1884 КБ | 0 |

Потоковое чтение — не медленный режим, которым платят за надёжность: оно держит одну часть
за раз и вовсе не доходит до кучи больших объектов.

**И испорченная запись не обрывает прогон.** `recover` говорит, где повторение вправе
подняться и что делать с тем, что оно отвергло:

```dotgram
Feed = header: Header
     & lines:  Row* recover eol => @(new RejectedLine(parserOrdinal, parserLine, parserText, parserMessage))
     & trailer: Trailer & eof
```

Отказы приходят в последовательности рядом с записями, неся номер строки и сообщение,
которое парсер иначе бросил бы, — либо, без `=>`, уходят в хук `partial void`, целиком
исчезающий, когда его никто не реализует.

## Одна грамматика, много парсеров

Перепривязка подменяет правило во всём, до чего дотягивается публикация. Поставьте её на
директиву — и та же грамматика опубликуется не один раз:

```dotgram
IntNumber     : @int     = d: Digits                     => @int.Parse(d)
DecimalNumber : @decimal = d: (Digits & ('.' & Digits)?) => @(Decimal(d))

Value : @int = d: Digits => @int.Parse(d)

Sum     : Value = left: Sum     & op: ['+' | '-'] & right: Product => @(op == "+" ? left + right : left - right)
                | value: Product                                   => @(value)

Product : Value = left: Product & op: ['*' | '/'] & right: Unary   => @(op == "*" ? left * right : left / right)
                | value: Unary                                     => @(value)

Unary   : Value = '-' & operand: Unary                             => @(-operand)
                | value: Primary                                   => @(value)

Primary : Value = '(' & inner: Sum & ')'                           => @(inner)
                | value: Value                                     => @(value)

parse Sum with (Value = IntNumber)     as EvaluateInt
parse Sum with (Value = DecimalNumber) as EvaluateDecimal
```

```csharp
EvaluateInt("7/2");          // 3        — an int
EvaluateDecimal("7/2");      // 3.5      — a decimal
TryEvaluateInt("1.5");       // no match — that parser has no decimal point in it
```

Арифметика написана один раз и не называет ни одного типа. `Sum : Value` говорит «то, что
производит `Value`», поэтому тип следует за подменой до самого опубликованного метода. Ни
обобщённого правила, ни типового параметра: перепривязка есть подстановка, а подстановка
меняет то, что окружающие правила производят, так же охотно, как то, что они читают.

Тот же механизм даёт одной грамматике чисел два десятичных разделителя, одной грамматике
списка — два разделителя элементов, одному протоколу — два диалекта.

## Грамматики как библиотеки

Грамматика не заперта в одном классе. Дайте грамматику классу, унаследуйтесь от него — и её
правила в области видимости:

```csharp
[Gram("Word = ['a'..'z']+\nName = Word & ('.' & Word)*")]
public partial class Lexemes { }

[Gram("using Lexemes;\nStart : @string = w: Name => @(w)\nparse Start")]
public partial class Reader : Lexemes { }
```

`using Lexemes;` вносит правила базы под собственным пространством имён, поэтому ничто не
сталкивается и ничего не копируется. Общая лексика — идентификаторы, числа, строковые
литералы, синтаксис комментариев — пишется однажды и включается всем, кому нужна, между
проектами так же легко, как внутри одного.

## Одна дверь в C#

`@` — единственный проход, и он значит одно и то же везде: дальше идёт C#. Предикат, внешний
распознаватель, страж над захваченным, построение — все проходят в одну и ту же дверь.

[`src/DotGram.Parsers`](src/DotGram.Parsers) — место, где это приложено к целой
спецификации, а не к примеру.
**[`ExpressionLanguage`](src/DotGram.Parsers/ExpressionLanguage.cs)** читает синтаксис
выражений C# и компилирует его в дерево выражений .NET:

```csharp
ExpressionLanguage.Compile<Func<int, int>>("(int x) => x * x - 1")(3);   // 8

ExpressionLanguage.Compile<Func<int, int, int>>(
	"(int x, int y) => { int sum = x + y; return sum * sum; }")(2, 3);  // 25
```

Каждый `=>` в нём напрямую называет фабрику `System.Linq.Expressions` — одна альтернатива на
оператор, никакой собственной модели этого проекта посередине и никакой диспетчеризации по
тексту оператора. Фабрика, которой не существует, или фабрика, которой подали не тот тип, —
это ошибка C# на той строке грамматики, которая её попросила, а не исключение во время
выполнения.

**[`Rfc3986`](src/DotGram.Parsers/Rfc3986.cs)** — второй: ссылки URI так, как их делит RFC, и
каждая часть оставлена ровно такой, какой была написана, потому что когда раскодировать
процентную последовательность — вопрос приложения, а не парсера.

## Никакой сборки времени выполнения

Всё, что нужно порождённому парсеру, выпускается в компиляцию самого потребителя, и всё это
`internal`. Вы берёте один пакет-анализатор и не приобретаете ни одной зависимости.
Расхождению «генератор одной версии, среда другой» просто неоткуда взяться: внутренний тип
невидим за границей сборки, поэтому двум сборкам, каждая из которых его выпускает, никогда
не надо о нём договариваться.

## Где это сейчас

Нотация построена, и конвейер работает от начала до конца: элементы, последовательность и
упорядоченный выбор, кванторы, предпросмотр и атомарные группы, правила, пространства имён и
перепривязка, приоритет и ассоциативность, захваты и построение, стражи, параметризованные
правила, внешние распознаватели, публикация как значения или ленивой последовательности,
чтение из `TextReader`, восстановление внутри повторения и грамматики, включённые из
базового класса.

Что не построено — записано, а не оставлено на обнаружение.
[`docs/status.md`](docs/status.md) и есть тот документ: возможность за возможностью, с
измерением, на котором стоит каждое утверждение.

## Примеры

Целые парсеры, предназначенные для копирования, — грамматика, класс, к которому она
прикреплена, и код, написанный против неё.

| | |
| --- | --- |
| [`UrlExample.cs`](examples/DotGram.Examples/UrlExample.cs) | URL по RFC 3986 — захваты, необязательные части, `find` |
| [`FeedExample.cs`](examples/DotGram.Examples/FeedExample.cs) | построчный фид — вложенные значения правил, последовательность записей, конверт, проверяемый целиком |
| [`RecoveringFeedExample.cs`](examples/DotGram.Examples/RecoveringFeedExample.cs) | тот же фид, прочитанный мимо испорченной записи — `recover`, и отказы, приходящие в последовательности вместе с записями |
| [`LoggingFeedExample.cs`](examples/DotGram.Examples/LoggingFeedExample.cs) | он же, но отказы уходят в сторону — `recover` без `=>` и `partial void`, исчезающий, когда его никто не реализует |
| [`StreamingFeedExample.cs`](examples/DotGram.Examples/StreamingFeedExample.cs) | тот же фид из `TextReader` — результат частями, переиспользуемое окно и трейлер, сверенный с записями, которых никто не держал |
| [`TwoCalculatorsExample.cs`](examples/DotGram.Examples/TwoCalculatorsExample.cs) | одна грамматика, опубликованная дважды, — целочисленный калькулятор и десятичный из той же арифметики |
| [`CalculatorExample.cs`](examples/DotGram.Examples/CalculatorExample.cs) | арифметика — приоритет, ассоциативность, `: @int` и `=>`, пробелы через затенение `trivia` |
| [`DecimalCalculatorExample.cs`](examples/DotGram.Examples/DecimalCalculatorExample.cs) | она же со степенью — левая и правая рекурсия бок о бок |
| [`StrengthCalculatorExample.cs`](examples/DotGram.Examples/StrengthCalculatorExample.cs) | предыдущий, написанный иначе — `<< n` и `>> n` в одном правиле вместо пяти |
| [`LocaleNumberExample.cs`](examples/DotGram.Examples/LocaleNumberExample.cs) | одно правило десятичного числа, опубликованное под двумя разделителями |
| [`ExpressionTreeExample.cs`](examples/DotGram.Examples/ExpressionTreeExample.cs) | та же грамматика, строящая дерево вместо числа, — форма, которую хочет небольшой DSL |
| [`OneRuleTreeExample.cs`](examples/DotGram.Examples/OneRuleTreeExample.cs) | то же дерево из одного правила в восемь строк, строящее те же узлы |
| [`Expression.cs`](examples/DotGram.Examples/Expression.cs) | дерево, которое строят эти двое, и всё, что оно умеет. Грамматики в нём нет намеренно |
| [`JsonExample.cs`](examples/DotGram.Examples/JsonExample.cs) | JSON — значение, которое есть любая из шести вещей, вложенных в него самого, и один параметризованный список, написанный однажды |
| [`XmlExample.cs`](examples/DotGram.Examples/XmlExample.cs) | XML — закрывающий тег, сверенный со своим открывающим через `when` |
| [`MarkdownExample.cs`](examples/DotGram.Examples/MarkdownExample.cs) | блоки Markdown — формат, где единица есть строка |
| [`FixExample.cs`](examples/DotGram.Examples/FixExample.cs) | сообщения FIX — поля по порядку, потому что тег может повториться, и контрольная сумма на C# |
| [`FilterExample.cs`](examples/DotGram.Examples/FilterExample.cs) | `Price > 10 AND Country IN ('UK','DE')` — дерево, которое вызывающий применяет к своим данным |
| [`NetstringExample.cs`](examples/DotGram.Examples/NetstringExample.cs) | кадр, который сам говорит свою длину, отданный распознавателю на C#, — единственная форма, невыразимая грамматикой |
| [`FixedWidthExample.cs`](examples/DotGram.Examples/FixedWidthExample.cs) | записи вовсе без разделителей — ширины в грамматике вместо арифметики над подстроками |
| [`HttpHeadersExample.cs`](examples/DotGram.Examples/HttpHeadersExample.cs) | поля заголовков, где значение может продолжиться на следующей строке |
| [`IniExample.cs`](examples/DotGram.Examples/IniExample.cs) | файл INI, прочитанный в словарь словарей |
| [`SqlReadOnlyExample.cs`](examples/DotGram.Examples/SqlReadOnlyExample.cs) | страж, отвечающий, может ли инструкция писать, — точная лексика SQL |
| [`TypedCsvExample.cs`](examples/DotGram.Examples/TypedCsvExample.cs) | CSV, прочитанный в записи вовсе без `=>`, — захваты, сопоставленные с конструктором и со свойствами `required` |
| [`GramExample.cs`](examples/DotGram.Examples/GramExample.cs) | грамматика самой нотации, написанная на ней же |

[`examples/README.md`](examples/README.md) говорит, что добавить в проект, чтобы взять один
из них.

## Документация

| | |
| --- | --- |
| [`docs/syntax.md`](docs/syntax.md) | язык: нотация и её связь с C# |
| [`docs/implementation.md`](docs/implementation.md) | движок: как язык исполняется |
| [`docs/diagnostics.md`](docs/diagnostics.md) | каждое сообщение, которое он может выдать, и что с ним делать |
| [`docs/status.md`](docs/status.md) | что работает, возможность за возможностью, с измерениями |

Ничто, решённое во втором, не есть решение о первом. Второй описывает, как исполняется
первый, и может быть заменён целиком без изменения языка.

## Сборка

```sh
dotnet build DotGram.slnx
dotnet test  DotGram.slnx
```

[`docs/development.md`](docs/development.md) содержит остальное.

## Лицензия

[MIT](LICENSE)
