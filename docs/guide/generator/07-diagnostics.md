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

## Appendix: every number

Every number the generator, the build and the editor report, with the severity it has unless the
project says otherwise and the one line the reference opens with. Each links to its row in
[`diagnostics.md`](../../diagnostics.md), which says what to do. The table is written from the
reference, and a test fails where the two differ.

<!-- ru:begin -->
> **RU.** Все номера, которые сообщают генератор, сборка и редактор, с severity, которую номер имеет,
> если проект не скажет иначе, и строкой, с которой его описание начинается в справочнике. Каждый
> номер — ссылка на его строку в `diagnostics.md`, где сказано, что делать. Таблица пишется из
> справочника, и тест падает, если они расходятся.
<!-- ru:end -->

<!-- generated from docs/diagnostics.md: begin -->

| Number | Severity | What it says |
| --- | --- | --- |
| [`GRAM0001`](../../diagnostics.md#gram0001) | Error | The generator itself failed. |
| [`GRAM0002`](../../diagnostics.md#gram0002) | Error | A class hosting a grammar must be `partial`. |
| [`GRAM0003`](../../diagnostics.md#gram0003) | Error | No grammar file for a `[Gram]` class. |
| [`GRAM0004`](../../diagnostics.md#gram0004) | Error | More than one grammar file matches. |
| [`GRAM0005`](../../diagnostics.md#gram0005) | Error | The name a grammar is included under is not an identifier. |
| [`GRAM0006`](../../diagnostics.md#gram0006) | Error | Two readings of one grammar want the same scope. |
| [`GRAM0007`](../../diagnostics.md#gram0007) | Error | The name a grammar is compiled under is not an identifier. |
| [`GRAM0008`](../../diagnostics.md#gram0008) | Error | Two grammars are included under one name. |
| [`GRAM0009`](../../diagnostics.md#gram0009) | Error | A buffer option is not positive. |
| [`GRAM0010`](../../diagnostics.md#gram0010) | Error | The generated parser needs C# 8 or later. |
| [`GRAM0011`](../../diagnostics.md#gram0011) | Error | The location type declares no `Locate`. |
| [`GRAM0012`](../../diagnostics.md#gram0012) | Error | A name was renamed in 0.3.0. |
| [`GRAM0017`](../../diagnostics.md#gram0017) | Info | A local grammar file is not the grammar a referenced assembly carries. |
| [`GRAM1001`](../../diagnostics.md#gram1001) | Error | Unterminated character literal. |
| [`GRAM1002`](../../diagnostics.md#gram1002) | Error | Unterminated string literal. |
| [`GRAM1003`](../../diagnostics.md#gram1003) | Error | Unterminated comment. |
| [`GRAM1004`](../../diagnostics.md#gram1004) | Error | Unrecognized escape sequence. |
| [`GRAM1005`](../../diagnostics.md#gram1005) | Error | Unexpected character. |
| [`GRAM1006`](../../diagnostics.md#gram1006) | Error | Unterminated inline `@(…)` expression. |
| [`GRAM1007`](../../diagnostics.md#gram1007) | Error | An inline `@(…)` expression needs a C# scanner, and none was supplied. |
| [`GRAM1008`](../../diagnostics.md#gram1008) | Error | Expected a Unicode category in the form `\p{Lu}`. |
| [`GRAM1009`](../../diagnostics.md#gram1009) | Error | Not a Unicode category. |
| [`GRAM1010`](../../diagnostics.md#gram1010) | Error | A character literal holds one character. |
| [`GRAM1011`](../../diagnostics.md#gram1011) | Error | `$` or `#` needs a name right after it. |
| [`GRAM1012`](../../diagnostics.md#gram1012) | Error | A name contains `_DotGram`. |
| [`GRAM2001`](../../diagnostics.md#gram2001) | Error | Expected a particular token. |
| [`GRAM2002`](../../diagnostics.md#gram2002) | Error | Expected a rule, a namespace or a publication directive. |
| [`GRAM2003`](../../diagnostics.md#gram2003) | Error | Expected a literal, a reference, an element set, a group or an atomic group. |
| [`GRAM2004`](../../diagnostics.md#gram2004) | Error | Expected a name. |
| [`GRAM2005`](../../diagnostics.md#gram2005) | Error | A binding power is a whole number from 0 to 9999. |
| [`GRAM2006`](../../diagnostics.md#gram2006) | Error | A namespace header's rebindings need `with`. |
| [`GRAM2007`](../../diagnostics.md#gram2007) | Error | A publication of anything but a rule's name needs `as`. |
| [`GRAM2008`](../../diagnostics.md#gram2008) | Error | A named yield publication places a result type before `yield`. |
| [`GRAM2009`](../../diagnostics.md#gram2009) | Error | A condition after `when` whose operand is not followed by `is`. |
| [`GRAM2010`](../../diagnostics.md#gram2010) | Error | A length `#q` stands only inside count braces. |
| [`GRAM2011`](../../diagnostics.md#gram2011) | Error | Something stands more than 128 levels deep inside others: groups, braces, lookaheads, captures, bindings, a call's arguments, a condition's parentheses, a C# type's arguments, namespaces. |
| [`GRAM2012`](../../diagnostics.md#gram2012) | Error | `positional` on a `find` or on a `parse … yield`. |
| [`GRAM2013`](../../diagnostics.md#gram2013) | Error | The words before a directive's verb are out of order, one is written twice, or one stands after a directive on its line. |
| [`GRAM2014`](../../diagnostics.md#gram2014) | Error | A character range is malformed: nothing after its `..` (`['a'..]`), a string as its bound (`['a'.."z"]`), or written outside brackets (`'a'..'z'`). |
| [`GRAM2015`](../../diagnostics.md#gram2015) | Error | A case-insensitive character is an element of a set or a bound of a range: `['a'i]`, `['a'..'z'i]`. |
| [`GRAM2016`](../../diagnostics.md#gram2016) | Error | Type arguments after a name of the grammar: `B<int>`. |
| [`GRAM2017`](../../diagnostics.md#gram2017) | Error | A second quantifier on one operand: `'a'*?`, `'a'+*`, `'a'{2}?`. |
| [`GRAM2019`](../../diagnostics.md#gram2019) | Error | A `using` or `@using` after the first declaration of its file or namespace block. |
| [`GRAM2020`](../../diagnostics.md#gram2020) | Error | A `switch`'s braces hold something that is not a `case` label, a `default` or a body; or a label is not a whole number or a string, or is a number too large for a `long` (a `ulong` if it is not negative). |
| [`GRAM2021`](../../diagnostics.md#gram2021) | Error | A `switch` label, or its `default`, is written twice. |
| [`GRAM2022`](../../diagnostics.md#gram2022) | Error | A `switch`'s labels are numbers and strings both. |
| [`GRAM2023`](../../diagnostics.md#gram2023) | Error | A `switch` has no case, or a label has no body after its `:`. |
| [`GRAM3001`](../../diagnostics.md#gram3001) | Error | A rule of this name is already defined in this namespace. |
| [`GRAM3002`](../../diagnostics.md#gram3002) | Error | No rule of that name. |
| [`GRAM3003`](../../diagnostics.md#gram3003) | Error | No namespace of that name is in view. |
| [`GRAM3004`](../../diagnostics.md#gram3004) | Error | No C# type of that name is in view. |
| [`GRAM3005`](../../diagnostics.md#gram3005) | Error | Two publications would generate the same method. |
| [`GRAM3006`](../../diagnostics.md#gram3006) | Error | A rebinding's left side names no visible rule. |
| [`GRAM3007`](../../diagnostics.md#gram3007) | Error | A rebinding's right side names no visible rule. |
| [`GRAM3008`](../../diagnostics.md#gram3008) | Error | A rule is bound more than once in one `with`. |
| [`GRAM3009`](../../diagnostics.md#gram3009) | Error | A rebinding's two sides take different numbers of parameters. |
| [`GRAM3010`](../../diagnostics.md#gram3010) | Error | A rule bound by the active namespace's own header cannot be redeclared. |
| [`GRAM3011`](../../diagnostics.md#gram3011) | Error | A rebinding is circular. |
| [`GRAM3012`](../../diagnostics.md#gram3012) | Error | A rule shadows one from an enclosing namespace or an import. |
| [`GRAM3014`](../../diagnostics.md#gram3014) | Error | A grammar declares two `context` types. |
| [`GRAM3016`](../../diagnostics.md#gram3016) | Error | A grammar declares two `state` types. |
| [`GRAM3019`](../../diagnostics.md#gram3019) | Error | The context handed to a parse does not satisfy a contract it inherited. |
| [`GRAM3020`](../../diagnostics.md#gram3020) | Error | Two grammars in one composition declare a different `state`. |
| [`GRAM3021`](../../diagnostics.md#gram3021) | Error | `$q` or `#q` where the rule binds nothing called `q`. |
| [`GRAM3022`](../../diagnostics.md#gram3022) | Error | Two imports at one scope each declare a `trivia`, and the scope declares none. |
| [`GRAM4001`](../../diagnostics.md#gram4001) | Error | The body of a repetition can match without consuming input. |
| [`GRAM4002`](../../diagnostics.md#gram4002) | Error | Left recursion the rewrite cannot take. |
| [`GRAM4003`](../../diagnostics.md#gram4003) | Error | `trivia` must accept empty input. |
| [`GRAM4005`](../../diagnostics.md#gram4005) | Error | A name in an element set is not a rule. |
| [`GRAM4006`](../../diagnostics.md#gram4006) | Error | A capture inside a lookahead, or one read there. |
| [`GRAM4007`](../../diagnostics.md#gram4007) | Error | One name is captured twice with different types. |
| [`GRAM4008`](../../diagnostics.md#gram4008) | Error | A `=>` is not on an alternative, or a rule that builds does not say what type. |
| [`GRAM4009`](../../diagnostics.md#gram4009) | Error | An alternative is recursive and states no strength while its siblings do. |
| [`GRAM4010`](../../diagnostics.md#gram4010) | Error | A recovery's `=>` has no sequence to put the rejected element in. |
| [`GRAM4011`](../../diagnostics.md#gram4011) | Error | A rule's declared type is neither a C# type nor a rule in view. |
| [`GRAM4012`](../../diagnostics.md#gram4012) | Error | A capture or a rule's parameter takes one of the supplied names, or one of the names the generated parser gives parameters of its own: `parserSource`, `parserStarts`, `parserLengths`, `parserReading`, `parserLocating`, `parserWhole`. |
| [`GRAM4013`](../../diagnostics.md#gram4013) | Error | A call of a parameterized rule that cannot be built, or a value where a piece of grammar goes. |
| [`GRAM4014`](../../diagnostics.md#gram4014) | Error | A rebinding's replacement produces an incompatible result. |
| [`GRAM4015`](../../diagnostics.md#gram4015) | Error | An external recognizer has more than one value-returning overload with different `T`. |
| [`GRAM4016`](../../diagnostics.md#gram4016) | Warning | Two alternatives begin with the same operand, and that operand leads back to the rule. |
| [`GRAM4017`](../../diagnostics.md#gram4017) | Error | Two substitutions each need the other applied first: two rules each contain a `with` that reaches the other, a namespace's header reaches a `with` that calls back into the namespace from outside it, or two namespaces' headers each reach, from outside, a rule the other rebinds. |
| [`GRAM4018`](../../diagnostics.md#gram4018) | Warning | A rule nothing reaches. |
| [`GRAM4019`](../../diagnostics.md#gram4019) | Error | `word` in a grammar that never said what continues a word. |
| [`GRAM4020`](../../diagnostics.md#gram4020) | Error | A `when … is …` somewhere other than beside the operands of an alternative. |
| [`GRAM4021`](../../diagnostics.md#gram4021) | Error | A `when … is …` whose sides are not two listable sets of strings. |
| [`GRAM4022`](../../diagnostics.md#gram4022) | Info | Every alternative of a rule is ruled out by a condition in this parser. |
| [`GRAM4023`](../../diagnostics.md#gram4023) | Error | A grammar published in more than 64 readings. |
| [`GRAM4024`](../../diagnostics.md#gram4024) | Error | A `when` that leaves different C# behind in different readings. |
| [`GRAM4025`](../../diagnostics.md#gram4025) | Error | A C# method a grammar names cannot be called the way its position calls it. |
| [`GRAM4026`](../../diagnostics.md#gram4026) | Error | An explicitly requested buffered input form is unsupported. |
| [`GRAM4027`](../../diagnostics.md#gram4027) | Error | A `yield` publication cannot safely produce the requested element sequence. |
| [`GRAM4028`](../../diagnostics.md#gram4028) | Error | A publication result does not fit its explicit C# contract. |
| [`GRAM4029`](../../diagnostics.md#gram4029) | Error | A mark is placed, or `parserState`/`parserMarks` asked for, in a grammar that declares no `state`. |
| [`GRAM4030`](../../diagnostics.md#gram4030) | Error | A mark's value names a capture of its own alternative. |
| [`GRAM4031`](../../diagnostics.md#gram4031) | Error | A guard in a fold step names another step's name for the value so far, which this step has none of. |
| [`GRAM4032`](../../diagnostics.md#gram4032) | Error | A reference or a length may be read where nothing bound its text. |
| [`GRAM4033`](../../diagnostics.md#gram4033) | Error | A binding or a reference where none can stand. |
| [`GRAM4034`](../../diagnostics.md#gram4034) | Error | A binding, reference or length where it is not supported yet. |
| [`GRAM4035`](../../diagnostics.md#gram4035) | Error | A binding, reference or length in a rule read over token kinds. |
| [`GRAM4036`](../../diagnostics.md#gram4036) | Error | A `with` expression inside an instance of a parameterized rule made only while another rebinding was applied. |
| [`GRAM4037`](../../diagnostics.md#gram4037) | Error | `Std.Nested(open, close)` called with arguments a counter cannot tell apart. |
| [`GRAM4038`](../../diagnostics.md#gram4038) | Error | A `recover` inside a lookahead. |
| [`GRAM4039`](../../diagnostics.md#gram4039) | Error | A step of a left-recursive rule with no `=>` captures something: its leading call, or an operand after it. |
| [`GRAM4040`](../../diagnostics.md#gram4040) | Error | An argument of a call into a namespace with a header, written outside that namespace, names a rule the header rebinds. |
| [`GRAM4041`](../../diagnostics.md#gram4041) | Error | A `with` expression inside an argument of a parameterized rule. |
| [`GRAM4042`](../../diagnostics.md#gram4042) | Warning | A strength on an alternative of a climbing rule reaches no operand. |
| [`GRAM4043`](../../diagnostics.md#gram4043) | Warning | A rule is spelled from single characters where trivia is not empty. |
| [`GRAM4044`](../../diagnostics.md#gram4044) | Error | A `recover` in a rule the lexer reads. |
| [`GRAM4045`](../../diagnostics.md#gram4045) | Error | A `recover` inside another one's repetition, or a synchronization with a guard, shares what a rule of its own could not. |
| [`GRAM4046`](../../diagnostics.md#gram4046) | Error | A rule that is told where it was written builds a `Nullable` of a struct. |
| [`GRAM4047`](../../diagnostics.md#gram4047) | Error | A literal, or a class item, is U+0000 in a grammar that names `eos`. |
| [`GRAM4048`](../../diagnostics.md#gram4048) | Error | Text is captured, bound or published across an `eos`, or `parserText` or `parserInput` would hold one. |
| [`GRAM4049`](../../diagnostics.md#gram4049) | Error | `Lexical = true` over a grammar that names `eos`. |
| [`GRAM4050`](../../diagnostics.md#gram4050) | Error | A buffered or byte form is asked of a grammar that names `eos`. |
| [`GRAM4051`](../../diagnostics.md#gram4051) | Warning | `trivia` is not atomic, and a class beside one of its seams reads a character it reads. |
| [`GRAM4052`](../../diagnostics.md#gram4052) | Warning | A repetition can read one run of text in two ways. |
| [`GRAM4053`](../../diagnostics.md#gram4053) | Error | A supplied name is written where it is not handed over: `parserInput`, `parserState` or `parserMarks` in a `when`; one of the parser's own parameters (`parserSource`, `parserReading`, …, GRAM4012) anywhere; a rejected element's name (`parserMessage`, `parserLine`, …) in a `=>`; the marks in a `recover` factory; or a supplied name used as a method, a type or a variable, where it is reserved. |
| [`GRAM4054`](../../diagnostics.md#gram4054) | Info | Two alternatives of a rule read a nested instance of the rule's own recursion before either can be refused. |
| [`GRAM4055`](../../diagnostics.md#gram4055) | Error | `positional parse` in a grammar that names `eos`. |
| [`GRAM4056`](../../diagnostics.md#gram4056) | Error | A construction's value cannot be the type it is declared to give: a capture handed back whole (`=> x`, `=> @(x)`) that holds text where an array or a value type is declared, or a generated type where another type is; a grammar's type built with the wrong number of values, or from a capture that cannot be its member (`=> Pair(right)`); a rule's name built or taken as a type where the grammar generates none for it, because it declares its own type or captures nothing. |
| [`GRAM4057`](../../diagnostics.md#gram4057) | Error | What a rule can begin with does not settle. |
| [`GRAM5001`](../../diagnostics.md#gram5001) | Info | A publication gets no overload taking a reader, or, in a grammar that names `eos`, reads its segments whole. |
| [`GRAM5002`](../../diagnostics.md#gram5002) | Info | A repetition can begin with the same input as what follows it. |
| [`GRAM5003`](../../diagnostics.md#gram5003) | Warning | A generated method is left past the size at which the JIT stops optimizing it. |
| [`GRAM5004`](../../diagnostics.md#gram5004) | Warning | `Lexical = true` was asked for and the grammar cannot be cut in two. |
| [`GRAM5005`](../../diagnostics.md#gram5005) | Warning | A split grammar's syntactic half cannot be read by methods, so it runs on the shared engine. |
| [`GRAM5007`](../../diagnostics.md#gram5007) | Warning | The immediate carrier was asked for by name and is not what carries some of the grammar. |
| [`GRAM5009`](../../diagnostics.md#gram5009) | Info | An optional or a repetition can take what follows it, where nothing can give it back. |
| [`GRAM5011`](../../diagnostics.md#gram5011) | Warning | A terminal of a split grammar is nothing but `@M`. |
| [`GRAM5012`](../../diagnostics.md#gram5012) | Info | A grammar left to the generator is carried as `Immediate`, and `Deferred` would hold some of its constructions back. |
| [`GRAM5014`](../../diagnostics.md#gram5014) | Info | A buffered form is read by the engine where its string form is read by methods. |
| [`GRAM5013`](../../diagnostics.md#gram5013) | Info | Whether a refused input is read again over a context put back as it was. |
| [`GRAM5016`](../../diagnostics.md#gram5016) | Warning | A grammar left to the generator is carried as `Immediate`, and `Deferred` would be safer. |
| [`GRAM5017`](../../diagnostics.md#gram5017) | Info | A construction of a publication read from a reader or lines changes the context. |
| [`GRAM5018`](../../diagnostics.md#gram5018) | Error | A name would be written twice into the generated class. |
| [`GRAM5019`](../../diagnostics.md#gram5019) | Warning | A guard that names `context` is read again where a terminal's value is built. |
| [`GRAM6001`](../../diagnostics.md#gram6001) | Warning | A classification target is neither `Rule` nor `Rule.capture`. |
| [`GRAM6002`](../../diagnostics.md#gram6002) | Warning | The grammar rule a classification target names was not found. |
| [`GRAM6003`](../../diagnostics.md#gram6003) | Warning | The grammar rule a classification target names is ambiguous. |
| [`GRAM6004`](../../diagnostics.md#gram6004) | Warning | The capture a classification target names was not found in its rule. |
| [`GRAM6005`](../../diagnostics.md#gram6005) | Warning | A classification target is specified more than once. |
| [`GRAM6101`](../../diagnostics.md#gram6101) | Error | The text of an embedded DSL site does not match its language. |

Retired, and never given another meaning: `GRAM3013`, `GRAM3015`, `GRAM3017`, `GRAM3018`, `GRAM4004`, `GRAM5006`, `GRAM5008`, `GRAM5010`, `GRAM5015`.

<!-- generated from docs/diagnostics.md: end -->
