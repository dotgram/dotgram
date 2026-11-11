# Diagnostics

Everything the generator has to say about a grammar or a host is a compiler diagnostic with a number
`GRAMnnnn`: in the Error List, in the build output, in `dotnet dotgram`. This page covers how to read
them and how a project decides what each one does to its build. Every number, with what it means and
what to do about it, is in the reference, [`diagnostics.md`](../../diagnostics.md).

<!-- ru:begin -->
> **RU.** Всё, что генератор хочет сказать о грамматике или хосте, — диагностика компилятора с номером
> `GRAMnnnn`: в Error List, в выводе сборки, в `dotnet dotgram`. Здесь — как их читать и как проект
> решает, что каждая делает с его сборкой. Каждый номер с его смыслом и тем, что делать, — в
> справочнике `diagnostics.md`.
<!-- ru:end -->

## Reading a message

```text
Hosts.cs(5,2): warning GRAM4018: Nothing reaches 'Unused': no publication names it, no rule the grammar reads calls it, and no rebinding puts it in place of one. Publish it, call it, or delete it. (https://github.com/dotgram/dotgram/blob/v0.3.0/docs/diagnostics.md#gram4018)
```

- **Where.** A grammar in a `.gram` file is pointed at in that file, at its line and column. A grammar
  written into `[Gram("…")]` is pointed at inside the string literal, at the same character. A
  message about the host itself — not `partial`, an option out of range — points at the attribute.
- **The number** says which stage found it, and stays the same from release to release; a number that
  is retired is never given a new meaning.
- **The link** at the end is the number's row in the reference, as it stood in the version of the
  generator that reported it. In Visual Studio the number in the Error List is the same link.

| Numbers | Raised by | Usually |
| --- | --- | --- |
| `GRAM0xxx` | finding the grammar: the host, its attributes, its files | errors |
| `GRAM1xxx` | reading the characters of the grammar | errors |
| `GRAM2xxx` | reading its shape | errors |
| `GRAM3xxx` | resolving its names, and the C# types it names | errors |
| `GRAM4xxx` | what the grammar means: recursion, captures, rebindings | errors; a few warnings about a grammar that is right but probably not what was meant |
| `GRAM5xxx` | what a grammar gets: the reader, the carrier, the methods published | information and warnings: the grammar is correct, and this is what it costs |
| `GRAM6xxx` | the attributes for editors (`[GramClassify]`) | warnings |

<!-- ru:begin -->
> **RU.** **Где:** грамматика в файле `.gram` указывается в этом файле, на строке и столбце;
> грамматика в `[Gram("…")]` — внутри строкового литерала, на том же символе; сообщение о самом
> хосте (не `partial`, опция вне диапазона) указывает на атрибут. **Номер** говорит, какой этап
> нашёл проблему, и не меняется от релиза к релизу; выведенный из употребления номер никогда не
> получает новый смысл. **Ссылка** в конце ведёт на строку номера в справочнике в той версии, которая
> это сообщила; в Visual Studio номер в Error List — та же ссылка. Диапазоны номеров — в таблице
> выше: 0xxx — поиск грамматики (хост, атрибуты, файлы), 1xxx — символы, 2xxx — структура, 3xxx —
> имена и C#-типы, 4xxx — смысл (в основном ошибки, несколько предупреждений о правильной, но,
> вероятно, не задуманной грамматике), 5xxx — что грамматика получает (информация и предупреждения:
> грамматика верна, и вот цена), 6xxx — атрибуты для редакторов.
<!-- ru:end -->

## Errors, warnings and information

- **An error** means no parser for that host: the grammar is not one, or the host cannot hold it. The
  first error is the one to fix — the rest of the project's errors about missing methods follow from
  it.
- **A warning** means the parser is generated and does what the grammar says, but the grammar
  probably does not say what was meant, or will cost more than it reads like: a rule nothing reaches,
  a repetition that can read one text in two ways, a carrier that would be safer as the other.
- **Information** is what the generator decided where it had a choice — a reader overload it could
  not give, a carrier it picked. A build does not show it; the Error List does, under Messages.

A warning is meant to be acted on: most say what to write instead. A project builds with warnings as
errors without fear of the generator, as long as it handles each warning it meets — in the grammar,
or by deciding about that number below.

```csharp example=warned GRAM4018
using DotGram;

[Gram("""
	Digits = ['0'..'9']+
	Unused = 'x'

	parse Digits
	""")]
public static partial class Numbers;
```

<!-- ru:begin -->
> **RU.** **Ошибка** — парсера для этого хоста нет: это не грамматика, или хост не может её принять;
> исправлять надо первую ошибку, остальные ошибки проекта о несуществующих методах — её следствие.
> **Предупреждение** — парсер сгенерирован и делает то, что сказано в грамматике, но грамматика,
> вероятно, говорит не то, что задумано, или обойдётся дороже, чем выглядит: правило, до которого
> ничто не доходит, повторение, читающее один текст двумя способами, носитель, который безопаснее
> поменять. **Информация** — что генератор решил там, где у него был выбор (перегрузка, которую он не
> смог дать, выбранный носитель); сборка её не показывает, Error List — показывает в Messages.
> Предупреждение рассчитано на то, чтобы по нему действовать: большинство говорит, что написать
> вместо. Проект может собираться с warnings as errors, не опасаясь генератора, если разбирается с
> каждым встреченным предупреждением — в грамматике или решением об этом номере (ниже). Пример выше —
> предупреждение GRAM4018.
<!-- ru:end -->

## Deciding what a number does

A `GRAM` number is configured the way any compiler diagnostic is.

**Around one grammar**, where it is written in the attribute — with the reason beside it:

```csharp fragment
#pragma warning disable GRAM4018 // Unused is kept for the next version of the format.
[Gram("""
	Digits = ['0'..'9']+
	Unused = 'x'

	parse Digits
	""")]
public static partial class Numbers;
#pragma warning restore GRAM4018
```

**For a project**, in its file:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);GRAM4018</NoWarn>                         <!-- not reported -->
  <WarningsAsErrors>$(WarningsAsErrors);GRAM4052</WarningsAsErrors>  <!-- an error -->
  <WarningsNotAsErrors>$(WarningsNotAsErrors);GRAM5003</WarningsNotAsErrors>  <!-- stays a warning under TreatWarningsAsErrors -->
</PropertyGroup>
```

**For the C# files of a folder**, in an `.editorconfig` section, which reaches the grammars written into
those files' attributes:

```ini
[*.cs]
dotnet_diagnostic.GRAM4018.severity = error     # error, warning, suggestion, silent or none
```

**For everything the compiler sees**, `.gram` files included, in a global analyzer configuration:

```ini
# dotgram.globalconfig, listed as <GlobalAnalyzerConfigFiles Include="dotgram.globalconfig" />
is_global = true

dotnet_diagnostic.GRAM4018.severity = none
```

A grammar in a `.gram` file has no C# around it: no `#pragma` stands in it, and an `.editorconfig`
section for `*.gram` does not reach its messages, since the compiler applies sections to C# files only.
For one of those, use `NoWarn`, `WarningsAsErrors` or the global configuration.

`dotnet dotgram generate` applies a project's settings the same way the build does, and stops on an
error among them as the build would.

<!-- ru:begin -->
> **RU.** Номер `GRAM` настраивается так же, как любая диагностика компилятора. **Вокруг одной
> грамматики**, написанной в атрибуте, — `#pragma` с причиной рядом (первый пример). **Для проекта** —
> в его файле: `NoWarn` (не сообщать), `WarningsAsErrors` (сделать ошибкой), `WarningsNotAsErrors`
> (оставить предупреждением при `TreatWarningsAsErrors`). **Для C#-файлов папки** — секция
> `.editorconfig`, которая доходит до грамматик в атрибутах этих файлов. **Для всего, что видит
> компилятор**, включая файлы `.gram`, — глобальная конфигурация анализаторов. У грамматики в файле
> `.gram` нет C# вокруг: `#pragma` в ней не напишешь, а секция `.editorconfig` для `*.gram` до её
> сообщений не доходит, потому что компилятор применяет секции только к C#-файлам; для таких —
> `NoWarn`, `WarningsAsErrors` или глобальная конфигурация. `dotnet dotgram generate` применяет
> настройки проекта так же, как сборка, и останавливается на ошибке среди них так же, как она.
<!-- ru:end -->

## What to silence, and what not

Silence a number where the grammar is right and the warning is wrong about it, at the narrowest place
that covers it — a `#pragma` around one attribute before a project-wide `NoWarn` — and say why next to
it. A number silenced project-wide is silenced for the next grammar too, about which it may be right.

Two families are worth reading rather than silencing:

- **`GRAM5016`** — a grammar left to choose its carrier runs its constructions as it reads, and the
  deferred carrier would be safer. Naming a carrier, `Carrier = GramCarrier.Deferred` or `Immediate`,
  answers it and silences it.
- **`GRAM4052`** — a repetition can read one run of text in two ways, so a refused input is read again
  for every way of cutting it. Fixing the grammar removes a cost that grows exponentially with the
  input.

<!-- ru:begin -->
> **RU.** Глушите номер там, где грамматика правильна, а предупреждение о ней ошибается, — в самом
> узком месте, которое это покрывает (`#pragma` вокруг одного атрибута раньше, чем `NoWarn` на весь
> проект), и пишите рядом почему. Номер, заглушённый на весь проект, заглушён и для следующей
> грамматики, о которой он может быть прав. Два семейства стоит читать, а не глушить: **GRAM5016** —
> грамматика, оставившая выбор носителя генератору, выполняет конструкции по мере чтения, а
> отложенный носитель был бы безопаснее; назвать носитель (`Carrier = GramCarrier.Deferred` или
> `Immediate`) — значит ответить и заглушить. **GRAM4052** — повторение может прочитать один отрезок
> текста двумя способами, поэтому отвергнутый вход перечитывается для каждого способа разрезать его;
> исправление грамматики убирает цену, растущую экспоненциально с входом.
<!-- ru:end -->
