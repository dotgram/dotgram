# Setting up the generator

.Gram is a source generator: it runs inside the C# compiler, reads the grammars a project
attaches to its classes, and adds the parsers to the same compilation. This page covers what the
`DotGram` package is, how a project takes it, and what older frameworks need beside it.

<!-- ru:begin -->
> **RU.** .Gram — генератор исходного кода: он работает внутри компилятора C#, читает грамматики,
> прикреплённые к классам проекта, и добавляет парсеры в ту же компиляцию. Эта страница — о том,
> что такое пакет `DotGram`, как проект его подключает и что нужно старым платформам.
<!-- ru:end -->

## What the package contains

| Path in the package | What it is |
| --- | --- |
| `analyzers/dotnet/cs/DotGram.dll` | the generator, loaded by the compiler as an analyzer |
| `build/DotGram.targets` | the MSBuild properties the generator reads ([MSBuild properties](04-msbuild.md)) |
| `README.md`, `SKILL.md` | the package's front page, and the guide to writing a grammar for a coding agent |

There is no `lib/` folder: nothing of the package is referenced by your code or deployed with
your application. Everything a parser needs is written into your own assembly as C#. The package
has no dependencies.

<!-- ru:begin -->
> **RU.** В пакете: сам генератор (`analyzers/dotnet/cs/DotGram.dll`), который компилятор загружает
> как анализатор; `build/DotGram.targets` со свойствами MSBuild, которые читает генератор; `README.md`
> и `SKILL.md` (руководство по написанию грамматики для агентов). Папки `lib/` нет: ничего из пакета
> не попадает в ссылки вашего кода и не разворачивается вместе с приложением — всё, что нужно
> парсеру, пишется в вашу сборку как C#. Зависимостей у пакета нет.
<!-- ru:end -->

## Adding it to a project

```xml
<ItemGroup>
  <PackageReference Include="DotGram" Version="0.3.0" PrivateAssets="all" />
</ItemGroup>
```

`PrivateAssets="all"` keeps the generator to this project: a project that references yours gets
your parsers as compiled code, and has no use for the generator that wrote them.

Reference the package in **every project that declares a grammar**. A project that only calls
parsers another project generated needs nothing.

<!-- ru:begin -->
> **RU.** Подключение — пример выше. `PrivateAssets="all"` оставляет генератор в этом проекте:
> проект, ссылающийся на ваш, получает парсеры уже скомпилированными, генератор ему не нужен.
> Ссылайтесь на пакет **в каждом проекте, где объявлена грамматика**; проекту, который только
> вызывает парсеры из другого проекта, ничего не нужно.
<!-- ru:end -->

## A first parser

A grammar is attached to a `partial` class with `[Gram]`, and every rule the grammar publishes
becomes a static method of that class:

```csharp example=first
using DotGram;

namespace Demo;

[Gram("""
	Digits = ['0'..'9']+

	parse Digits
	""")]
public static partial class Numbers;
```

```text run example=first
Numbers.ParseDigits("2026") → "2026"
Numbers.TryParseDigits("20x6").IsSuccess → false
```

`ParseDigits` throws `FormatException` on input it refuses; `TryParseDigits` answers a match that
says whether it succeeded, and why not. Nothing else is needed: the attribute, the parser and its
support types are all generated into the project.

Where the class can live, what `[Gram]` and its options say, and grammars kept in files of their
own are the next two pages: [the host and its attributes](02-hosts-and-attributes.md) and
[grammar files](03-grammar-files.md). The notation itself is the
[language guide](../language/README.md).

<!-- ru:begin -->
> **RU.** Грамматика прикрепляется к `partial`-классу атрибутом `[Gram]`, и каждое опубликованное ею
> правило становится статическим методом этого класса (пример выше). `ParseDigits` бросает
> `FormatException` на отвергнутом входе, `TryParseDigits` возвращает результат, который говорит,
> удалось ли и почему нет. Больше ничего не нужно: атрибут, парсер и его вспомогательные типы
> генерируются в проект. Где может находиться класс, что говорят `[Gram]` и его опции, грамматики в
> отдельных файлах — на следующих двух страницах; сама нотация — в руководстве по языку.
<!-- ru:end -->

## Language version and older frameworks

The generated parser is C# 8 and compiles for whatever the project targets: `net8.0` and later,
`netstandard2.0`, `net472`. Three things depend on the target.

**The language version.** A grammar written inside `[Gram("""…""")]` is a raw string literal,
which is C# 11. A grammar in a `.gram` file needs only what the generated code needs, C# 8.
`netstandard2.0` and .NET Framework projects default to C# 7.3, so they set `<LangVersion>`. Below
C# 8 the generator writes no parser and says why once, at the first `[Gram]`:

```csharp example=too-old LangVersion=7.3 GRAM0010
using DotGram;

[Gram]
public static partial class Feed { }
```

```dotgram file=Feed.gram example=too-old
Digits = ['0'..'9']+

parse Digits
```

The error reads *The generated parser needs C# 8 or later; this project compiles C# 7.3. Set
`<LangVersion>8.0</LangVersion>` or later.* A project with no `[Gram]` is not affected: the
attributes the generator adds to every compilation are C# 7.3 themselves.

**`System.Memory`.** The generated methods take `ReadOnlySpan<char>`. On .NET it is part of the
framework; on `netstandard2.0` and .NET Framework it comes from the `System.Memory` package, which
the project references itself:

```xml
<PropertyGroup>
  <TargetFramework>netstandard2.0</TargetFramework>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="DotGram" Version="0.3.0" PrivateAssets="all" />
  <PackageReference Include="System.Memory" Version="4.6.3" />
</ItemGroup>
```

The `DotGram` package cannot bring it along for you. It is private to the project, so a dependency
it declared would reach this project and stop there: a library built that way would not pass
`System.Memory` on to its own consumers, and they would fail at run time on .NET Framework. A
reference the project makes itself travels with the library like any other.

**`record` on old frameworks.** A grammar that builds `record` values needs
`System.Runtime.CompilerServices.IsExternalInit`, a type `netstandard2.0` and .NET Framework do
not have (CS0518). Declare it once in the project:

```csharp fragment
namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit { }
}
```

<!-- ru:begin -->
> **RU.** Сгенерированный парсер — C# 8 и компилируется под любую цель проекта: `net8.0` и новее,
> `netstandard2.0`, `net472`. От цели зависят три вещи.
>
> **Версия языка.** Грамматика внутри `[Gram("""…""")]` — raw string literal, это C# 11; грамматике в
> файле `.gram` нужен только C# 8. У проектов `netstandard2.0` и .NET Framework по умолчанию C# 7.3,
> поэтому они задают `<LangVersion>`. Ниже C# 8 генератор не пишет парсер и один раз, у первого
> `[Gram]`, говорит почему (GRAM0010, пример выше). Проекта без `[Gram]` это не касается: атрибуты,
> которые генератор добавляет в каждую компиляцию, сами написаны на C# 7.3.
>
> **`System.Memory`.** Сгенерированные методы принимают `ReadOnlySpan<char>`. В .NET он входит во
> фреймворк, в `netstandard2.0` и .NET Framework — в пакет `System.Memory`, на который проект
> ссылается сам (пример выше). Пакет `DotGram` не может притянуть его за вас: он приватен для
> проекта, и его зависимость дошла бы до этого проекта и остановилась — библиотека, собранная так, не
> передала бы `System.Memory` своим потребителям, и те упали бы в рантайме на .NET Framework. Ссылка,
> которую проект делает сам, передаётся дальше как любая другая.
>
> **`record` на старых платформах.** Грамматике, которая строит значения-`record`, нужен тип
> `System.Runtime.CompilerServices.IsExternalInit`, которого нет в `netstandard2.0` и .NET Framework
> (CS0518). Объявите его в проекте один раз (пример выше).
<!-- ru:end -->

## What the generator adds to every compilation

Before it looks at any grammar, the generator adds one file to every project that references it,
`DotGram.Attributes.g.cs`: `[Gram]` and the other attributes of the next page, all `internal`.
Being internal, two assemblies that both use .Gram never see each other's copies, and there is no
runtime library whose version could disagree with the generator's. The attributes are marked
`[Microsoft.CodeAnalysis.Embedded]`, which keeps them apart even where one assembly grants another
`InternalsVisibleTo`.

Then, for each class with a `[Gram]`, it adds that class's parser, named after the class:
`Demo.Numbers.g.cs` for the first example. Visual Studio lists every generated file under
**Dependencies → Analyzers → DotGram** of the project, and opens it like any other source. To have
them written to disk as well, set the compiler's own property:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

The files then land under `obj/`, in `generated/DotGram/DotGram.Generation.GramGenerator/`.

<!-- ru:begin -->
> **RU.** До того как смотреть на грамматики, генератор добавляет в каждый проект, который на него
> ссылается, файл `DotGram.Attributes.g.cs`: `[Gram]` и остальные атрибуты следующей страницы, все
> `internal`. Раз они внутренние, две сборки, обе использующие .Gram, не видят копий друг друга, и нет
> библиотеки времени выполнения, чья версия могла бы разойтись с генератором. Атрибуты помечены
> `[Microsoft.CodeAnalysis.Embedded]`, и это разделяет их даже там, где одна сборка даёт другой
> `InternalsVisibleTo`. Затем для каждого класса с `[Gram]` добавляется его парсер, названный по
> классу: `Demo.Numbers.g.cs` для первого примера. Visual Studio показывает все сгенерированные файлы
> в узле проекта **Dependencies → Analyzers → DotGram** и открывает их как обычный исходник. Чтобы они
> записывались и на диск, задайте собственное свойство компилятора (пример выше); файлы окажутся в
> `obj/`, в `generated/DotGram/DotGram.Generation.GramGenerator/`.
<!-- ru:end -->

## Generating outside the build

A large grammar takes seconds to generate, and every cold build pays for it. The same generator is
also a .NET tool, `dotnet dotgram`, which writes the files a build would generate into a folder
that the project then compiles without referencing the package. A project does one or the other,
never both: the build would generate every parser a second time. [The command-line tool](06-cli.md)
has the details.

<!-- ru:begin -->
> **RU.** Большая грамматика генерируется секунды, и каждая холодная сборка за это платит. Тот же
> генератор есть и как инструмент .NET, `dotnet dotgram`: он пишет файлы, которые сгенерировала бы
> сборка, в папку, и проект компилирует их, не ссылаясь на пакет. Проект делает либо одно, либо
> другое, но не оба: сборка сгенерировала бы каждый парсер второй раз. Подробности — на странице
> инструмента командной строки.
<!-- ru:end -->

## Visual Studio

Nothing in Visual Studio has to be set for the generator: a build inside it and a build from the
command line run the same generator from the same package. The optional DotGram extension adds
language support for grammars in the editor — colouring, diagnostics as you type, navigation —
and has no settings that change what is generated ([`visual-studio.md`](../../visual-studio.md)).

<!-- ru:begin -->
> **RU.** В Visual Studio для генератора ничего настраивать не нужно: сборка в IDE и сборка из
> командной строки запускают один и тот же генератор из одного пакета. Необязательное расширение
> DotGram добавляет поддержку грамматик в редакторе — подсветку, диагностики по мере ввода,
> навигацию — и не имеет настроек, влияющих на генерацию.
<!-- ru:end -->
