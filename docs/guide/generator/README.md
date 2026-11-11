# The generator guide

How to use the DotGram generator: the package, the classes a grammar is attached to, the files it is
kept in, the build properties, the command-line tool, what to do when something is wrong, and the
diagnostics. The notation a grammar is written in is the [language guide](../language/01-grammar-and-host.md);
the specification of both is [`syntax.md`](../../syntax.md).

| Page | What it covers |
| --- | --- |
| [1. Setting up](01-setup.md) | the package, adding it to a project, a first parser, older frameworks |
| [2. The host class and its attributes](02-hosts-and-attributes.md) | `[Gram]`, its options, `[GramParserVariant]`, `[GramInclude]`, the attributes for editors |
| [3. Grammar files](03-grammar-files.md) | `.gram` files: listing them, how a host finds its file, what a file decides |
| [4. MSBuild properties](04-msbuild.md) | the generation report, trace builds, the compiler server's cache |
| [5. The command-line tool](05-cli.md) | `dotnet dotgram`: generating outside the build, keeping the files current |
| [6. When something is wrong](06-troubleshooting.md) | symptoms, causes and what to do |
| [7. Diagnostics](07-diagnostics.md) | reading `GRAM` messages and deciding what each does to a build |

Every example on these pages is compiled through the generator, and run where the page shows what it
answers, by `tests/DotGram.Tests/GeneratorGuideTests.cs`.

<!-- ru:begin -->
> **RU.** Как пользоваться генератором DotGram: пакет, классы, к которым прикрепляется грамматика,
> файлы, в которых она хранится, свойства сборки, инструмент командной строки, что делать, если что-то
> не так, и диагностики. Нотация грамматик — в руководстве по языку, спецификация того и другого — в
> `syntax.md`. Страницы перечислены в таблице выше. Каждый пример на этих страницах компилируется
> через генератор и выполняется там, где страница показывает ответ, тестом
> `tests/DotGram.Tests/GeneratorGuideTests.cs`.
<!-- ru:end -->
