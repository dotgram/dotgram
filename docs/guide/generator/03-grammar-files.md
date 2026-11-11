# Grammar files

A grammar can be written into its `[Gram]` attribute or kept in a `.gram` file of its own. This page
covers the files: how the build is given them, how a host finds its file, what a file holds and
what it does not, and when a file is the better place.

<!-- ru:begin -->
> **RU.** Грамматику можно написать прямо в атрибуте `[Gram]` или держать в отдельном файле `.gram`.
> Эта страница — о файлах: как передать их сборке, как хост находит свой файл, что файл содержит и
> чего не содержит, и когда файл — лучшее место.
<!-- ru:end -->

## Giving the build its grammar files

The generator sees only the files the compiler is given as **additional files**. A project lists
its grammars the way it lists any other:

```xml
<ItemGroup>
  <AdditionalFiles Include="Grammars/**/*.gram" />
</ItemGroup>
```

A file that is not listed is not there as far as the generator is concerned. The error names the
file it looked for and the line to add:

```csharp example=not-listed GRAM0003
using DotGram;

[Gram]
public static partial class Feed;
```

*No additional file 'Feed.gram' for 'Feed'; add `<AdditionalFiles Include="Feed.gram" />` to the
project.*

Listing a file that no class claims is harmless: it generates nothing. A parser is generated for a
class, never for a file.

<!-- ru:begin -->
> **RU.** Генератор видит только файлы, переданные компилятору как **additional files**. Проект
> перечисляет свои грамматики так же, как любые другие такие файлы (пример выше). Неперечисленного
> файла для генератора не существует; ошибка называет файл, который искался, и строку, которую нужно
> добавить (GRAM0003, второй пример). Перечисленный файл, который не забрал ни один класс, безвреден:
> он ничего не генерирует. Парсер генерируется для класса, а не для файла.
<!-- ru:end -->

## How a host finds its file

| The attribute | The file it reads |
| --- | --- |
| `[Gram]` on `Feed` | an additional file named `Feed.gram`, in any folder |
| `[Gram]` on `Outer.Parser<T>` | `Parser.gram`: the innermost class, without its type parameters |
| `[Gram("feeds/Feed.gram")]` | an additional file whose path ends with `feeds/Feed.gram` |

The path in the attribute is matched against the end of each additional file's full path, at a
folder boundary, so `[Gram("feeds/Feed.gram")]` finds `src/feeds/Feed.gram` and not
`src/oldfeeds/Feed.gram`. `/` and `\` are the same, and case is ignored on every operating system,
so a project finds the same file on Windows, macOS and Linux.

**Exactly one file has to match.** Where two do, the generator does not pick one by order; it says
which, and the cure is a path that names one of them:

```csharp example=ambiguous GRAM0004
using DotGram;

[Gram]
public static partial class Feed;
```

```dotgram file=current/Feed.gram example=ambiguous
Line = ['a'..'z']+

parse Line
```

```dotgram file=archive/Feed.gram example=ambiguous
Line = ['A'..'Z']+

parse Line
```

*'Feed.gram' matches several additional files (/project/current/Feed.gram,
/project/archive/Feed.gram); give [Gram] a path that names one of them.*

```csharp example=disambiguated
using DotGram;

namespace Demo;

[Gram("current/Feed.gram")]
public static partial class Feed;
```

```dotgram file=current/Feed.gram example=disambiguated
Line = ['a'..'z']+

parse Line
```

```dotgram file=archive/Feed.gram example=disambiguated
Line = ['A'..'Z']+

parse Line
```

```text run example=disambiguated
Feed.TryParseLine("abc").IsSuccess → true
Feed.TryParseLine("ABC").IsSuccess → false
```

The same holds for two files whose names differ only in case: they are both candidates, and the path
decides.

<!-- ru:begin -->
> **RU.** Как хост находит файл — в таблице выше: `[Gram]` на `Feed` — additional file с именем
> `Feed.gram` в любой папке; на `Outer.Parser<T>` — `Parser.gram` (самый внутренний класс без
> параметров типа); `[Gram("feeds/Feed.gram")]` — файл, чей путь оканчивается на `feeds/Feed.gram`.
> Путь из атрибута сравнивается с концом полного пути каждого файла по границе папки, так что
> `[Gram("feeds/Feed.gram")]` находит `src/feeds/Feed.gram`, но не `src/oldfeeds/Feed.gram`; `/` и `\`
> равнозначны, регистр не учитывается ни на одной ОС, поэтому проект находит один и тот же файл на
> Windows, macOS и Linux. **Подходить должен ровно один файл.** Если подходят два, генератор не
> выбирает по порядку, а называет их (GRAM0004), и лекарство — путь, называющий один из них (примеры
> выше). То же для двух файлов, чьи имена различаются только регистром: оба — кандидаты, решает путь.
<!-- ru:end -->

## What a file holds

A `.gram` file holds the grammar and nothing else: exactly the text that would otherwise be the
argument of `[Gram("""…""")]`. Its C# imports are the grammar's own `@using` lines; the C# file the
host is declared in lends it nothing.

```dotgram fragment
@using System.Globalization;

Amount : @decimal = text: Number => @(decimal.Parse(text, CultureInfo.InvariantCulture))
Number = ['0'..'9']+ & ('.' & ['0'..'9']+)?

parse Amount
```

What a file does **not** decide:

- **The namespace and the class.** The generated parser goes into the host, in the host's namespace;
  the file's name and folder play no part. `Grammars/Amount.gram` read by `Demo.Amount` generates
  `Demo.Amount.ParseAmount`.
- **The options.** How the grammar is read — `Lexical`, `Carrier`, everything of
  [the previous page](02-hosts-and-attributes.md#the-options-of-a-reading) — is said on the
  attribute, so one file can be read by two hosts in two ways, and each gets a parser of its own.

The standard library needs no file: `using Std;` in any grammar brings in the rules the generator
carries with it (`Std.Digits`, `Std.Spacing`, …), listed in [`syntax.md` §5.2](../../syntax.md#52-the-standard-library-std).

<!-- ru:begin -->
> **RU.** Файл `.gram` содержит грамматику и ничего больше — ровно тот текст, который иначе был бы
> аргументом `[Gram("""…""")]` (пример выше). Его импорты C# — собственные строки `@using` грамматики;
> C#-файл, где объявлен хост, ему ничего не даёт. Чего файл **не** решает: **пространство имён и
> класс** — парсер попадает в хост, в пространство имён хоста, имя и папка файла роли не играют
> (`Grammars/Amount.gram`, прочитанный `Demo.Amount`, даёт `Demo.Amount.ParseAmount`); **опции** — как
> читать грамматику, говорит атрибут, так что один файл могут читать два хоста двумя способами, и
> каждый получит свой парсер. Стандартной библиотеке файл не нужен: `using Std;` в любой грамматике
> приносит правила, которые генератор несёт с собой.
<!-- ru:end -->

## Where diagnostics point

A message about a grammar in a file points into the file, at the line and column the grammar's
own text has: the Error List opens the `.gram` file there. A message about a grammar written into
the attribute points into the string literal, at the same character. A grammar included from another
class is reported where it is written, in its own file.

A file's messages are filtered like any other: `NoWarn`, `WarningsAsErrors` and a global analyzer
configuration reach them. An `.editorconfig` section for `*.gram` does not, since the compiler applies
those sections only to C# files; a `#pragma` cannot stand in a grammar file either. The
[diagnostics](07-diagnostics.md) page has the details.

<!-- ru:begin -->
> **RU.** Сообщение о грамматике в файле указывает в файл, на строку и столбец текста самой
> грамматики: Error List открывает там файл `.gram`. Сообщение о грамматике, написанной в атрибуте,
> указывает внутрь строкового литерала, на тот же символ. Включённая из другого класса грамматика
> получает сообщения там, где написана, в своём файле. Сообщения файла фильтруются как любые другие:
> до них доходят `NoWarn`, `WarningsAsErrors` и глобальная конфигурация анализаторов. Секция
> `.editorconfig` для `*.gram` не доходит — компилятор применяет такие секции только к C#-файлам; и
> `#pragma` в файле грамматики не напишешь. Подробнее — на странице о диагностиках.
<!-- ru:end -->

## Files of other projects

A grammar included from a class in another project is not read from that project's `.gram` file,
which this project's build is not given: it is read from the class, which carries its grammar's text
when it is exported ([including](02-hosts-and-attributes.md#graminclude-one-grammar-built-on-another)).
A `.gram` file of this project that happens to have the same name changes nothing: the referenced
assembly's grammar is the one compiled and shipped with it, and the build says so where the two
differ (`GRAM0017`, information). A file is preferred only for a class compiled in this same
project, where it is the text being edited.

<!-- ru:begin -->
> **RU.** Грамматика, включённая из класса другого проекта, читается не из `.gram`-файла того проекта
> (этой сборке он не передан), а из самого класса, который несёт текст своей грамматики, если она
> экспортирована. Файл `.gram` этого проекта с тем же именем ничего не меняет: используется
> грамматика ссылочной сборки — та, что с ней скомпилирована и поставлена; если тексты различаются,
> сборка об этом говорит (GRAM0017, информация). Файл предпочитается только для класса, компилируемого
> в этом же проекте, где он и есть редактируемый текст.
<!-- ru:end -->

## A file or the attribute

| | In the attribute | In a `.gram` file |
| --- | --- | --- |
| C# version | 11 (raw string literal) | 8 |
| Listing in the project | none | `AdditionalFiles` |
| Grammar beside the C# it calls | yes, in one file | no, in two |
| Editor support | in the string, with the DotGram extension | in its own editor, with the extension |
| Size that stays readable | tens of rules | any |

The two are the same grammar: moving one from the attribute into a file changes nothing about the
parser. A file wants line endings that do not depend on the machine it was checked out on, since
the generator reads it as it is; `*.gram text eol=lf` in `.gitattributes` does that.

<!-- ru:begin -->
> **RU.** Сравнение в таблице выше: в атрибуте нужен C# 11 (raw string literal), в файле — C# 8; файл
> нужно перечислить в `AdditionalFiles`; в атрибуте грамматика лежит рядом с C#, который она
> вызывает, в одном файле; поддержка в редакторе есть в обоих случаях с расширением DotGram; в
> атрибуте удобно до десятков правил, в файле — сколько угодно. Это одна и та же грамматика: перенос
> из атрибута в файл ничего не меняет в парсере. Файлу нужны окончания строк, не зависящие от машины,
> на которую его выгрузили, потому что генератор читает его как есть; это делает
> `*.gram text eol=lf` в `.gitattributes`.
<!-- ru:end -->
