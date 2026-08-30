*[English](README.md)*

# .Gram

**.Gram — генератор исходного кода, компилирующий грамматики в строго типизированные
парсеры на C#.**

Грамматика известна во время компиляции. Порождённый парсер — обычный C# в вашей
собственной сборке: нет ни движка разбора, ни графа грамматики, ни библиотеки времени
выполнения, которая всё это истолковывала бы.

Из грамматики .Gram умеет породить:

* API `Parse`, `TryParse` и `Find`;
* строго типизированные результаты из именованных захватов;
* парсеры, специализированные под разные версии одной и той же грамматики;
* потоковые парсеры для `TextReader`;
* восстановление после ошибок для записеориентированного входа;
* диагностику времени компиляции, указывающую обратно в грамматику.

Годится всё, у чего есть грамматика: форматы данных и фиды, файлы конфигурации, протоколы
обмена, языки запросов и фильтров, шаблонные и разметочные синтаксисы, а также небольшие
собственные языки — в том числе такие, что компилируются прямо в
`System.Linq.Expressions` или в ваши собственные типы. Замена регулярному выражению,
ставшему трудным в поддержке, или рукописному парсеру, которому стало трудно доверять, —
одно применение из этих, а не граница возможностей.

## С чего начать

```xml
<PackageReference Include="DotGram" Version="0.1.0"
                  PrivateAssets="all" ExcludeAssets="runtime" />
```

Наименьший полезный парсер на .Gram выглядит почти как регулярное выражение:

```csharp
using DotGram;

[Gram("""
	Hex = ['0'..'9' | 'a'..'f' | 'A'..'F']

	Color = '#' & value: Hex{6}

	parse Color
	""")]
public static partial class CssColor;
```

Пользуются им как обычным C#:

```csharp
var color = CssColor.ParseColor("#12aBcF");

Console.WriteLine(color.Value);       // 12aBcF

var result = CssColor.TryParseColor("#xyz");

Console.WriteLine(result.IsSuccess);  // False
```

Равносильное регулярное выражение было бы примерно таким:

```text
^#(?<value>[0-9a-fA-F]{6})$
```

Знакомые части значат знакомое: диапазоны, альтернативы, `?`, `*`, `+` и `{n}`.

Но `value:` — не просто захват регулярки. Он становится свойством порождённого типа
результата.

Для небольших грамматик держать грамматику прямо в атрибуте `[Gram]` удобно: определение
парсера и его C#-API читаются вместе. Грамматики покрупнее могут жить и в файлах `.gram`,
перечисленных как `<AdditionalFiles Include="Name.gram" />`.

## Одна грамматика, два парсера

Грамматика не обязана описывать только один парсер. Арифметика ниже написана один раз и
опубликована дважды: над `int` и над `double`.

```csharp
using DotGram;

[Gram("""
	@using System.Globalization;

	trivia = [' ' | '\t']*

	Digits = ['0'..'9']+

	Value
		: @int
		= d: Digits
		=> @int.Parse(d)

	Sum
		: Value
		= left: Sum & op: ['+' | '-'] & right: Product
			=> @(op == "+" ? left + right : left - right)
		| value: Product
			=> @(value)

	Product
		: Value
		= left: Product & op: ['*' | '/'] & right: Unary
			=> @(op == "*" ? left * right : left / right)
		| value: Unary
			=> @(value)

	Unary
		: Value
		= '-' & operand: Unary
			=> @(-operand)
		| value: Primary
			=> @(value)

	Primary
		: Value
		= '(' & value: Sum & ')'
			=> @(value)
		| value: Value
			=> @(value)

	IntNumber
		: @int
		= d: Digits
		=> @int.Parse(d)

	DoubleNumber
		: @double
		= d: (Digits & ('.' & Digits)?)
		=> @double.Parse(d, CultureInfo.InvariantCulture)

	parse Sum with (Value = IntNumber)    as EvaluateInt
	parse Sum with (Value = DoubleNumber) as EvaluateDouble
	""")]
public static partial class Calculator;
```

Порождённый API содержит два независимо специализированных парсера:

```csharp
Calculator.EvaluateInt("7 / 2");       // 3

Calculator.EvaluateDouble("7 / 2");    // 3.5
Calculator.EvaluateDouble("1.5 * 4");  // 6

Calculator.TryEvaluateInt("1.5");      // no match
```

`Sum`, `Product`, `Unary` и `Primary` написаны ровно один раз. Разделяет два парсера
публикация:

```text
parse Sum with (Value = IntNumber)    as EvaluateInt
parse Sum with (Value = DoubleNumber) as EvaluateDouble
```

`with` подменяет правило по всей грамматике, достижимой из этой публикации.

Тип результата следует за подменой тоже. `Sum : Value` означает «тип, который производит
`Value`», поэтому первый порождённый парсер возвращает `int`, а второй — `double`.

Ни обобщённой диспетчеризации во время выполнения, ни объекта настройки парсера: оба
специализируются тогда, когда порождается C#.

## Типизированный разбор

Именованные захваты задают форму результата.

```csharp
using DotGram;

[Gram("""
	Feed
		= header: Header
		& rows: Row*
		& trailer: Trailer
		& eof

	Header
		= "H" & '|' & date: Date & eol

	Row
		= "R"
		& '|' & symbol: Text
		& '|' & quantity: Digit+
		& eol

	Trailer
		= "T" & '|' & count: Digit+ & eol

	Date
		= year: Digit{4} & '-' & month: Digit{2} & '-' & day: Digit{2}

	Text  = [^ '|' | '\r' | '\n']+
	Digit = ['0'..'9']

	parse Feed
	find Row as AllRows
	""")]
public static partial class FeedParser;
```

Парсер возвращает эту структуру напрямую:

```csharp
var feed = FeedParser.ParseFeed(text);

feed.Header.Date.Year;
feed.Rows[0].Symbol;
feed.Rows[0].Quantity;
feed.Trailer.Count;
```

Ни обобщённого дерева разбора, ни посетителя, чтобы превратить его в данные приложения.
Захваты можно сопоставить и напрямую с конструктором или со свойствами `required` уже
имеющегося у вас типа — тогда в грамматике не будет ни строчки кода построения.

`find` публикует правило как ленивый поиск по входу:

```csharp
foreach (var row in FeedParser.AllRows(text))
	Console.WriteLine(row.Value.Symbol);
```

## C# — часть грамматики там, где он нужен

`@` — граница между грамматикой и C#.

Правило может производить уже существующий тип C#:

```csharp
using DotGram;

[Gram("""
	@using System.Globalization;

	Number
		: @double
		= text: (['0'..'9']+ & ('.' & ['0'..'9']+)?)
		=> @double.Parse(text, CultureInfo.InvariantCulture)

	parse Number
	""")]
public static partial class Numbers;
```

Страж может проверять значения прямо во время разбора — то, чего грамматика сама сказать
не может:

```csharp
using DotGram;

[Gram("""
	Name = ['a'..'z' | 'A'..'Z']+

	Tag
		= '<' & open: Name & '>'
		& "</" & close: Name & '>'
		& when @(open == close)

	parse Tag
	""")]
public static partial class Tags;
```

Через ту же границу зовутся предикаты, внешние распознаватели, конструкторы и любой API
вообще. Грамматика описывает синтаксис; C# берёт на себя то, что на C# и выражается лучше.

## DotGram.Parsers

В репозитории есть также [`DotGram.Parsers`](src/DotGram.Parsers): настоящие парсеры,
построенные на .Gram, а не маленькие показательные грамматики.

### Парсер URI по RFC 3986

[`Rfc3986`](src/DotGram.Parsers/Rfc3986.cs) следует RFC 3986 близко к тексту: абсолютные
URI, относительные ссылки, IPv4, IPv6, `IPvFuture`, полномочие, пути, запросы, фрагменты и
процентное кодирование.

```csharp
using DotGram.Parsers;

var uri = Rfc3986.ParseUri("https://user@example.com:8080/a/b?q=1#top");

uri.Scheme;    // https
uri.UserInfo;  // user
uri.Host;      // example.com
uri.Port;      // 8080
uri.Path;      // /a/b
uri.Query;     // q=1
uri.Fragment;  // top
```

Ссылки могут быть относительными:

```csharp
var reference = Rfc3986.ParseReference("../images/logo.png?size=2");

reference.Scheme;  // null
reference.Path;    // ../images/logo.png
reference.Query;   // size=2
```

Процентное раскодирование намеренно отделено от разбора:

```csharp
Rfc3986.Decode("hello%20world"); // hello world
```

Это различие существенно. `%2F` внутри сегмента пути — при разборе закодированные данные;
раскодировав его рано, вы превратите его в разделитель пути, которым он не является.

### Язык выражений

[`ExpressionLanguage`](src/DotGram.Parsers/ExpressionLanguage.cs) — язык выражений в стиле
C#, производящий деревья `System.Linq.Expressions`.

```csharp
using DotGram.Parsers;

var square = ExpressionLanguage.Compile<Func<int, int>>("(int x) => x * x - 1");

square(3); // 8
```

Он поддерживает параметры, локальные переменные, блоки и `return`:

```csharp
var calculate = ExpressionLanguage.Compile<Func<int, int, int>>(
	"""
	(int x, int y) =>
	{
		int sum = x + y;
		return sum * sum;
	}
	""");

calculate(2, 3); // 25
```

Или можно оставить дерево выражения, не компилируя его:

```csharp
var expression = ExpressionLanguage.Parse("(double x) => x / 2.0");

Console.WriteLine(expression);   // x => (x / 2)
```

Грамматика зовёт фабрики `System.Linq.Expressions` напрямую. Никакого промежуточного AST,
свойственного .Gram, который потом пришлось бы переводить в дерево выражений, — а значит,
фабрика, которой не существует, или фабрика, которой подали не тот тип, оказывается
ошибкой C# на той строке грамматики, которая её попросила, а не исключением во время
выполнения.

`DotGram.Parsers` полезен двояко: как библиотека настоящих парсеров и как пример того, во
что выливаются грамматики .Gram покрупнее, применённые к настоящим спецификациям и API.

## Производительность

.Gram порождает C#, свойственный конкретному парсеру. Он не истолковывает грамматику во
время выполнения.

Бенчмарк URL сравнивает грамматику URL на .Gram с **тем же языком, переписанным правило в
правило в регулярное выражение**. До начала замеров бенчмарк убеждается, что обе
реализации сходятся на каждом проверяемом входе и на каждой разобранной части.

| Вход | .Gram | `RegexOptions.Compiled` | Преимущество .Gram |
| --- | ---: | ---: | ---: |
| короткий URL | 133.8 нс | 298.9 нс | 2.23× |
| хост как IPv4 | 146.9 нс | 285.4 нс | 1.94× |
| неверный URL | 80.2 нс | 113.5 нс | 1.42× |
| путь из 84 символов | 191.0 нс | 453.0 нс | 2.37× |

Против интерпретируемого `Regex` — примерно **в 2.2–6.5 раза**. Сам бенчмарк и его
методика — в [`benchmarks`](benchmarks/).

Сравнение намеренно берёт грамматику URL, достаточно маленькую, чтобы её можно было
переписать равносильным регулярным выражением. Это **не** бенчмарк полной реализации
[`Rfc3986`](src/DotGram.Parsers/Rfc3986.cs): сравнивать два разных языка и называть
результат бенчмарком парсеров — значит обессмыслить числа.

Бенчмарк к тому же спрашивает у обеих сторон разобранные значения, а не только то, совпал
ли вход. Распознавание и разбор — разные виды работы.

## Потоковое чтение и восстановление

Там, где генератор может доказать, что вход можно отпускать по мере разбора, он выпускает
перегрузки для `TextReader` рядом с обычными.

```csharp
using DotGram;

[Gram("""
	Text  = [^ '|' | '\r' | '\n']+
	Digit = ['0'..'9']

	Header          = "H" & '|' & Text & eol
	Row   : @string = "R" & '|' & t: Text & eol => @(t)
	Trailer         = "T" & '|' & Digit+ & eol

	Feed : @string[] = Header & Row* & Trailer & eof

	parse Feed
	""")]
public static partial class StreamingFeed;
```

`Feed` собирает то, что производят его операнды: `Row` строит `string`, а заголовок и
трейлер не строят ничего и потому ни к чему не присоединяются. Порождается четыре метода —
`ParseFeed` и `TryParseFeed` над `string`, и `ParseFeed` над `TextReader` и над
`IEnumerable<string>`:

```csharp
using var reader = File.OpenText("large.feed");

foreach (var row in StreamingFeed.ParseFeed(reader))
	Handle(row);
```

Буфер входа переиспользуется вместо удержания входа целиком.

Записеориентированные форматы умеют и подниматься после испорченного входа:

```csharp
using DotGram;

[Gram("""
	Text = [^ '|' | '\r' | '\n']+

	Row : @string = "R" & '|' & t: Text & eol => @(t)

	Feed : @string[] = Row* recover eol => @(parserText)

	parse Feed
	""")]
public static partial class RecoveringFeed;
```

`recover eol` говорит, где повторение вправе подняться, а `=>` — что делать с тем, что оно
отвергло: здесь текст испорченной строки, который приходит в последовательности рядом с
хорошими. Отказ с тем же успехом может стать записью со своими `parserLine` и
`parserMessage` или уйти в хук `partial void` и вовсе не попасть в результат.

Испорченная запись, таким образом, становится данными об отказе, а не концом фида.

## Что .Gram поддерживает

* литералы и множества элементов;
* диапазоны и категории Unicode;
* последовательность и упорядоченный выбор;
* `?`, `*`, `+` и ограниченное повторение;
* предпросмотр и атомарные группы;
* именованные захваты и порождённые типы результата;
* существующие типы C# как результат — через конструктор или свойства `required`;
* семантические действия и стражи;
* внешние предикаты и распознаватели на C#;
* параметризованные правила;
* перепривязку правил и специализацию парсеров;
* левую рекурсию и степени связывания для грамматик выражений;
* пространства имён грамматик и переиспользуемые библиотеки грамматик;
* `Parse`, `TryParse` и `Find`;
* потоковое чтение из `TextReader`;
* восстановление внутри повторений;
* контекст парсера и состояние разбора.

[`docs/status.md`](docs/status.md) — достоверный источник о состоянии возможность за
возможностью, включая текущие ограничения.

## Никакой библиотеки парсера времени выполнения

`DotGram` — пакет-генератор исходного кода. Всё, что нужно для исполнения порождённого
парсера, выпускается в потребляющую сборку как внутренний C#.

```text
ваша сборка
 ├── ваш код
 ├── порождённый парсер
 └── поддержка порождённого парсера
```

Нет ни сборки DotGram времени выполнения, которую надо разворачивать, ни пары
«генератор — среда», способной разойтись версиями. Генератор делает работу, свойственную
грамматике, во время компиляции; приложение исполняет порождённый парсер.

## Примеры

Целые примеры — в [`examples/DotGram.Examples`](examples/DotGram.Examples/).

| Пример | Что показывает |
| --- | --- |
| [`UrlExample.cs`](examples/DotGram.Examples/UrlExample.cs) | разбор URL, типизированные захваты, `find` |
| [`FeedExample.cs`](examples/DotGram.Examples/FeedExample.cs) | записеориентированный вход и вложенные порождённые типы |
| [`RecoveringFeedExample.cs`](examples/DotGram.Examples/RecoveringFeedExample.cs) | восстановление после испорченных записей |
| [`StreamingFeedExample.cs`](examples/DotGram.Examples/StreamingFeedExample.cs) | потоковое чтение большого входа |
| [`TwoCalculatorsExample.cs`](examples/DotGram.Examples/TwoCalculatorsExample.cs) | одна грамматика, специализированная в несколько парсеров |
| [`JsonExample.cs`](examples/DotGram.Examples/JsonExample.cs) | рекурсивные структурированные данные |
| [`XmlExample.cs`](examples/DotGram.Examples/XmlExample.cs) | закрывающий тег, сверенный со своим открывающим |
| [`FixExample.cs`](examples/DotGram.Examples/FixExample.cs) | сообщения FIX и проверка на C# |
| [`FilterExample.cs`](examples/DotGram.Examples/FilterExample.cs) | небольшой язык запросов |
| [`TypedCsvExample.cs`](examples/DotGram.Examples/TypedCsvExample.cs) | построение существующих типов C# |
| [`GramExample.cs`](examples/DotGram.Examples/GramExample.cs) | нотация .Gram, разбираемая самой .Gram |

Полный список — в [`examples/README.md`](examples/README.md).

## Документация

| Документ | Содержание |
| --- | --- |
| [`docs/syntax.md`](docs/syntax.md) | нотация грамматик и порождаемый API |
| [`docs/implementation.md`](docs/implementation.md) | как работает порождённый парсер |
| [`docs/diagnostics.md`](docs/diagnostics.md) | диагностика компилятора |
| [`docs/status.md`](docs/status.md) | реализованные возможности, ограничения и измерения |

## Сборка

```sh
dotnet build DotGram.slnx
dotnet test  DotGram.slnx
```

## Лицензия

[MIT](LICENSE)
