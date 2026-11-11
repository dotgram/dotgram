# MSBuild properties

A few things are said to the generator by the build rather than by a grammar's attribute: whether it
reports what it did, whether every grammar of a project is a trace build, whether it may reuse a
parser it compiled before. They are MSBuild properties, set in the project, in a
`Directory.Build.props` for many projects at once, or on the command line with `-p:`.

<!-- ru:begin -->
> **RU.** Кое-что генератору говорит сборка, а не атрибут грамматики: сообщать ли, что он сделал,
> делать ли trace-сборку из всех грамматик проекта, можно ли повторно использовать уже
> скомпилированный парсер. Это свойства MSBuild: их задают в проекте, в `Directory.Build.props` сразу
> для многих проектов или в командной строке через `-p:`.
<!-- ru:end -->

## The properties

| Property | Values | Default | What it does |
| --- | --- | --- | --- |
| `DotGramReportGeneration` | `none`, `summary`, `full` | `none` | prints a line per parser in the build output, and with `full` what the generator decided for it |
| `DotGramTrace` | `true` | off | compiles every grammar of the project as a trace build, as if each said `Trace = true` |
| `DotGramNoCache` | `true` | off | compiles every grammar afresh, never taking a parser kept from an earlier build |
| `DotGramPositionalFollow` | `true`, `split` | off | experimental: compiles every `parse` knowing it is also read from a position |

Values are compared without regard to case. A value the table does not list means off: a typo in a
property is not reported, and a build with the property unset is the build it would have been.

They reach the generator through `build/DotGram.targets`, which the package brings into every project
that references it. Nothing has to be imported by hand.

<!-- ru:begin -->
> **RU.** Свойства перечислены в таблице выше: `DotGramReportGeneration` (`none`, `summary`, `full`,
> по умолчанию `none`) печатает строку на каждый парсер, а с `full` — ещё и что генератор для него
> решил; `DotGramTrace=true` делает trace-сборку из всех грамматик проекта, будто каждая сказала
> `Trace = true`; `DotGramNoCache=true` компилирует каждую грамматику заново, не беря парсер из
> прошлой сборки; `DotGramPositionalFollow` (`true` или `split`, экспериментальное) компилирует каждый
> `parse` с учётом чтения с позиции. Значения сравниваются без учёта регистра; значение не из таблицы
> означает «выключено» — опечатка в свойстве не сообщается, и сборка с незаданным свойством остаётся
> той же. Генератору они доходят через `build/DotGram.targets`, который пакет приносит в каждый
> проект, ссылающийся на него; ничего импортировать вручную не нужно.
<!-- ru:end -->

## What the generator did: `DotGramReportGeneration`

A build says nothing about the generator unless asked. `summary` prints one line per parser — the
host and the reading, how many rules the grammar has once normalized, how large the generated C# is,
and how long generating it took. For a host with a second reading:

```csharp example=report
using DotGram;

namespace Demo;

[Gram("""
	trivia = { ' '* }

	Sum  : @int = left: Sum & '+' & right: Term << 1 => @(left + right)
	            | value: Term                       => @(value)
	Term : @int = digits: { ['0'..'9']+ } => @(int.Parse(digits))

	parse Sum
	""")]
[GramParserVariant("Deferred", Carrier = GramCarrier.Deferred)]
public static partial class Adder;
```

```shell
dotnet build -p:DotGramReportGeneration=summary
```

```text report example=report
DotGram: Demo.Adder.Deferred, 3 normalized rules, 33442 bytes UTF-8 C#, 4.30 ms generation
DotGram: Demo.Adder, 3 normalized rules, 24076 bytes UTF-8 C#, 3.39 ms generation
```

The lines are at the importance a minimal build (the default of `dotnet build`) shows; `-v:quiet`
shows none. The time is the generator's own — reading, analysing and writing the grammar — and not
the C# compiler's. Where it says `cached`, the parser was taken from an earlier build
(`DotGramNoCache`, below). A build that compiles nothing — nothing changed — prints nothing, and an
editor's background builds never report.

`full` adds, at the importance `-v:normal` and above show, what the generator decided: the options of
the reading as it compiled them, and how each machine of the parser carries what it read
([`Carrier`](02-hosts-and-attributes.md#the-options-of-a-reading)):

```shell
dotnet build -v:n -p:DotGramReportGeneration=full
```

```text
DotGram: Demo.Adder, mode=characters, options: Lexical=False, Carrier=Auto, ConstructionsMayThrow=False, BufferedText=False, BufferedBytes=False, UseSpanCaptures=False, InitialBufferSize=4096, MaxBufferedInputLength=2147483647, MaxExtraStacks=0, TargetMethodPartSize=0, LocationType=none, ExportGrammar=True, Trace=False
carrier: immediate; gate: none; reasons: none; told: none; machines: 1; deferred: 0; points: 5/5
machine ParseSum [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 5/5
```

`full` costs the analysis that decides those carriers, which is worked out for the report alone.

**What any report also does.** The report is written by the generator as comment-only files beside
the parsers, so any value other than `none` turns on `EmitCompilerGeneratedFiles`: the generated
files are written to disk, under `obj/<configuration>/<framework>/DotGramGenerated/` unless
`CompilerGeneratedFilesOutputPath` says elsewhere. No value changes a parser or turns a message into a
warning. `true` and `false` are older spellings of `full` and `none`.

<!-- ru:begin -->
> **RU.** Сборка ничего не говорит о генераторе, если не попросить. `summary` печатает по строке на
> парсер: хост и чтение, сколько правил у грамматики после нормализации, размер сгенерированного C# и
> время генерации (пример выше — для хоста со вторым чтением). Строки идут с важностью, которую показывает минимальная сборка
> (умолчание `dotnet build`); при `-v:quiet` их нет. Время — собственное время генератора (чтение,
> анализ и запись грамматики), а не компилятора C#. `cached` значит, что парсер взят из прошлой
> сборки (`DotGramNoCache`, ниже). Сборка, которая ничего не компилирует, ничего и не печатает, а
> фоновые сборки редактора не сообщают никогда.
>
> `full` добавляет — с важностью, которую показывает `-v:normal` и выше, — что генератор решил:
> опции чтения, как он их скомпилировал, и как каждая машина парсера несёт прочитанное (второй пример).
> `full` стоит анализа, который выбирает носители и делается только ради отчёта.
>
> **Что ещё делает любой отчёт.** Генератор пишет отчёт файлами из одних комментариев рядом с
> парсерами, поэтому любое значение кроме `none` включает `EmitCompilerGeneratedFiles`:
> сгенерированные файлы пишутся на диск, в `obj/<конфигурация>/<платформа>/DotGramGenerated/`, если
> `CompilerGeneratedFilesOutputPath` не говорит другого. Ни одно значение не меняет парсер и не
> превращает сообщение в предупреждение. `true` и `false` — старые написания `full` и `none`.
<!-- ru:end -->

## Every grammar traced: `DotGramTrace`

`Trace = true` on one `[Gram]` makes that reading a trace build
([the host's attributes](02-hosts-and-attributes.md#why-an-input-was-refused-trace)).
`DotGramTrace` does the same for every grammar of the project, which is what a Debug build usually
wants:

```xml
<PropertyGroup>
  <DotGramTrace Condition="'$(Configuration)' == 'Debug'">true</DotGramTrace>
</PropertyGroup>
```

With it set, a host that says nothing about tracing has the sinks all the same:

```csharp example=trace-property DotGramTrace=true
using DotGram;

namespace Demo;

[Gram("""
	trivia = { ' '* }

	Sum  : @int = left: Sum & '+' & right: Term << 1 => @(left + right)
	            | value: Term                       => @(value)
	Term : @int = digits: { ['0'..'9']+ } => @(int.Parse(digits))

	parse Sum
	""")]
public static partial class Adder;

public static class Explain
{
	public static string Why(string text)
	{
		var why = new Adder.GramWhy();

		using (Adder.Tracing(why))
			Adder.TryParseSum(text);

		return why.ToString().Split('\n')[0].TrimEnd();
	}
}
```

```text run example=trace-property
Adder.ParseSum("1 + 2") → 3
Explain.Why("1 +") → "Expected ['0'..'9']. (line 1, column 4)"
```

A trace build answers exactly what the plain parser answers. It adds the sinks (`GramWhy`,
`GramTraceLog`, `GramProfile`) and `Tracing(sink)` to every host, and costs the parser the calls that
report to them. The names a trace build declares are reserved in every build, traced or not, so code
that names them compiles in Release too if it stands behind `#if DEBUG`:

```csharp fragment
var sum = Adder.ParseSum("1 + 2");          // 3, in every configuration

#if DEBUG
var why = new Adder.GramWhy();

using (Adder.Tracing(why))
	Adder.TryParseSum("1 +");

Console.WriteLine(why);                     // Expected ['0'..'9']. (line 1, column 4) …
#endif
```

<!-- ru:begin -->
> **RU.** `Trace = true` на одном `[Gram]` делает trace-сборкой одно чтение; `DotGramTrace` делает то
> же для всех грамматик проекта — обычно это нужно Debug-сборке (первый пример). С этим свойством
> приёмники есть и у хоста, который о трассировке ничего не говорит (второй пример). Trace-сборка отвечает
> ровно то же, что обычный парсер; она добавляет в каждый хост приёмники (`GramWhy`, `GramTraceLog`,
> `GramProfile`) и `Tracing(sink)` и стоит парсеру вызовов, которые им сообщают. Имена, которые
> объявляет trace-сборка, зарезервированы в любой сборке, поэтому код, который их называет, за
> `#if DEBUG` компилируется и в Release (последний пример).
<!-- ru:end -->

## Reusing a parser: `DotGramNoCache`

A command-line build runs the compiler in a server that outlives it by some minutes, and the
generator keeps there the parsers it compiled. The next build after an edit to C# alone — one that
leaves the grammar, its options and every answer about the host's C# types as they were — takes the
parser from there instead of generating it again: for a grammar of a thousand rules, seconds a build.
A parser is found by everything its compile read, so any edit that matters compiles afresh. What is
kept is bounded (about 80 MB and 64 parsers for each version of the generator), and builds inside an
editor keep nothing.

```xml
<PropertyGroup>
  <DotGramNoCache>true</DotGramNoCache>
</PropertyGroup>
```

Set, every grammar is generated afresh. `dotnet build-server shutdown` empties what is kept, together
with the server it lives in. Either is the thing to try where a parser looks older than the grammar it
was compiled from — which would be a defect worth reporting.

<!-- ru:begin -->
> **RU.** Сборка из командной строки запускает компилятор в сервере, который живёт ещё несколько минут
> после неё, и генератор держит там скомпилированные парсеры. Следующая сборка после правки только
> C# — грамматика, её опции и все ответы о C#-типах хоста те же — берёт парсер оттуда, а не генерирует
> заново: для грамматики в тысячу правил это секунды на сборку. Парсер находится по всему, что читала
> его компиляция, поэтому любая существенная правка компилирует заново. Хранимое ограничено (около
> 80 МБ и 64 парсера на каждую версию генератора), а сборки внутри редактора ничего не хранят.
> С `DotGramNoCache=true` каждая грамматика генерируется заново (пример выше); `dotnet build-server
> shutdown` опустошает хранимое вместе с сервером. И то и другое стоит попробовать, если парсер
> выглядит старше грамматики, из которой скомпилирован, — и это был бы дефект, о котором стоит
> сообщить.
<!-- ru:end -->

## Reading from a position: `DotGramPositionalFollow` (experimental)

A `positional parse` has whole forms, which read all of the input, and forms that begin where the
caller says and read as much as the rule takes. The rule is compiled once for both, against the end of
the input, and in a few shapes a decision that holds for the whole forms makes a positional form refuse,
or stop short of, the rule's first reading ([`syntax.md` §6.8](../../syntax.md#dotgrampositionalfollow-experimental)).
`DotGramPositionalFollow` compiles every `parse` knowing it is also read from a position:

```xml
<PropertyGroup>
  <DotGramPositionalFollow>split</DotGramPositionalFollow>
</PropertyGroup>
```

`true` makes the positional forms right and costs the whole forms some of the proofs their speed
rests on. `split` gives the same answers and keeps those proofs where the two kinds of entry can be told
apart, so it costs less. Both are experimental, and both are for a build that needs the positional
forms right now; the whole forms are right without either.

<!-- ru:begin -->
> **RU.** У `positional parse` есть целые формы, которые читают весь вход, и формы, которые начинают с
> указанной позиции и читают столько, сколько возьмёт правило. Правило компилируется один раз для
> обоих видов, в расчёте на конец входа, и в нескольких формах грамматики решение, верное для целых
> форм, заставляет позиционную форму отвергнуть или не дочитать первое прочтение правила.
> `DotGramPositionalFollow` компилирует каждый `parse` с учётом чтения с позиции (пример выше). `true`
> делает позиционные формы правильными и лишает целые формы части доказательств, на которых держится
> их скорость; `split` даёт те же ответы и сохраняет эти доказательства там, где два вида входа можно
> различить, поэтому обходится дешевле. Оба значения экспериментальные и нужны сборке, которой
> позиционные формы нужны правильными уже сейчас; целые формы правильны и без них.
<!-- ru:end -->

## Seeing the generated code

Visual Studio lists every generated file under **Dependencies → Analyzers → DotGram** of the project.
To have them on disk without a report, set the compiler's own `EmitCompilerGeneratedFiles`
([setup](01-setup.md#what-the-generator-adds-to-every-compilation)). Nothing about the parser depends
on either.

<!-- ru:begin -->
> **RU.** Visual Studio показывает все сгенерированные файлы в узле проекта **Dependencies → Analyzers
> → DotGram**. Чтобы получить их на диске без отчёта, задайте собственное свойство компилятора
> `EmitCompilerGeneratedFiles`. Парсер ни от того, ни от другого не зависит.
<!-- ru:end -->
