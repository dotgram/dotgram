<!--
  Agents: every package ships a skill for you. For writing grammars read src/DotGram/SKILL.md
  (in a restored project: ~/.nuget/packages/dotgram/<version>/SKILL.md). The other packages'
  skills are listed under "Documentation and agent skills".
-->

# .Gram

[![build](https://github.com/dotgram/dotgram/actions/workflows/build.yml/badge.svg)](https://github.com/dotgram/dotgram/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/DotGram?logo=nuget)](https://www.nuget.org/packages/DotGram)
[![License: MIT](https://img.shields.io/github/license/dotgram/dotgram)](LICENSE)

**Grammars in, typed C# parsers out — at compile time.**

.Gram is a source generator: write a grammar in an attribute or a `.gram` file, and the build writes a strongly
typed parser into your own assembly. No parser engine, no runtime package, nothing to deploy. One notation
covers a six-character colour code, a calculator, and the SQL standard.

## From a regex to the SQL standard

**Small.** A grammar the size of a regular expression stays that size:

```csharp
using DotGram;

[Gram("""
	Hex   = ['0'..'9' | 'a'..'f' | 'A'..'F']
	Color = '#' & value: Hex{6}

	parse Color
	""")]
public static partial class CssColor;
```

```csharp
var color = CssColor.ParseColor("#12aBcF");

Console.WriteLine(color.Value);                               // 12aBcF
Console.WriteLine(CssColor.TryParseColor("#xyz").IsSuccess);  // False
```

`value:` became a property of the generated result type; `TryParseColor` says what was expected, and where.

**A language.** A calculator is one rule. Precedence and associativity are written on the operators, and the
same grammar is published three times — over `int`, over `decimal`, and as a syntax tree:

```csharp
[Gram("""
	@using System.Globalization;

	trivia = Std.Spacing?

	Value : @int = d: Std.Digits => @(int.Parse(d))
	Point        = { Std.Digits ~ ('.' ~ Std.Digits)? }

	Expr : Value = left: Expr & '+' & right: Expr  << 1 => @(left + right)
	             | left: Expr & '-' & right: Expr  << 1 => @(left - right)
	             | left: Expr & '*' & right: Expr  << 2 => @(left * right)
	             | left: Expr & '/' & right: Expr  << 2 => @(left / right)
	             | left: Expr & '^' & right: Expr  >> 3 => @(Raise(left, right))
	             | '-' & operand: Expr             >> 3 => @(-operand)
	             | '(' & inner: Expr & ')'              => @(inner)
	             | value: Value                         => @(value)

	IntNumber     : @int     = d: Std.Digits => @(int.Parse(d))
	DecimalNumber : @decimal = d: Point      => @(decimal.Parse(d, CultureInfo.InvariantCulture))
	NodeNumber    : @Node    = d: Point      => @(new Node.Number(decimal.Parse(d, CultureInfo.InvariantCulture)))

	parse Expr with (Value = IntNumber)     as EvaluateInt
	parse Expr with (Value = DecimalNumber) as EvaluateDecimal
	parse Expr with (Value = NodeNumber)    as BuildTree
	""")]
public static partial class Calculator { /* Node, Raise and two options: CalculatorExample.cs */ }
```

```csharp
Calculator.EvaluateInt("7 / 2");        // 3
Calculator.EvaluateDecimal("7 / 2");    // 3.5
Calculator.EvaluateInt("2 ^ 3 ^ 2");    // 512: ^ groups to the right
Calculator.BuildTree("2 ^ 3 ^ 2");      // a Node tree for 2 ^ (3 ^ 2)
```

The whole file, tested: [`CalculatorExample.cs`](examples/DotGram.Examples/Expressions/CalculatorExample.cs).
How it works, operator by operator: [One grammar, three parsers](docs/tour.md#one-grammar-three-parsers).

**The SQL standard.** The same notation reads [T-SQL in over 1,000 rules](src/DotGram.Sql/TransactSql/TransactSql.gram),
one parser per SQL Server version from a single grammar, and [ISO SQL:2023](src/DotGram.Sql/Standard/SqlStandard.gram),
written from the standard's BNF. They ship ready to use in [`DotGram.Sql`](src/DotGram.Sql/README.md).
More complete examples, from formats and feeds to languages: [`examples/`](examples/README.md).

## Install

```xml
<PackageReference Include="DotGram" Version="0.3.0" PrivateAssets="all" ExcludeAssets="runtime" />
```

Builds with .NET SDK 9.0.300 or newer (Visual Studio 17.14+); the project may target anything from
`netstandard2.0` up ([Compatibility](#compatibility)). A grammar lives in the `[Gram]` attribute of the class the
parser is generated into, as above, or in a `.gram` file listed as `<AdditionalFiles Include="Name.gram" />`.

## In Visual Studio

<!-- SCREENSHOT: docs/images/vs-calculator-dark.png and vs-calculator-light.png, CalculatorExample.cs lines 48-73
     in VS 2026, dark and light themes; being shot with the VS UI harness. -->
<picture>
  <source media="(prefers-color-scheme: dark)" srcset="docs/images/vs-calculator-dark.png">
  <img alt="The calculator grammar in Visual Studio: rules, operators, C# actions and publications each highlighted in their own colour" src="docs/images/vs-calculator-light.png" width="820">
</picture>

The [Visual Studio extension](docs/visual-studio.md) reads grammars in `.gram` files and inside `[Gram]` strings:
highlighting, errors as you type, Quick Info, completion, Go To Definition, Find All References, Rename, folding.
And the languages you generate get the same: a string marked `[StringSyntax("yourlanguage")]` is highlighted
and checked as your language.

## Documentation and agent skills

| To | Read |
| --- | --- |
| see what a grammar can do, by example | [`docs/tour.md`](docs/tour.md) |
| look up the notation and the generated API | [`docs/syntax.md`](docs/syntax.md) |
| understand a diagnostic (GRAM*nnnn*) | [`docs/diagnostics.md`](docs/diagnostics.md) |
| run the generator outside the build | [`docs/cli.md`](docs/cli.md) |
| set up the editor | [`docs/visual-studio.md`](docs/visual-studio.md) |
| check what is implemented, and what is not | [`docs/status.md`](docs/status.md) |
| see how the generated parser works | [`docs/implementation.md`](docs/implementation.md) |
| build, check and measure .Gram itself | [`docs/development.md`](docs/development.md) |
| find everything else, and what authority each page carries | [`docs/README.md`](docs/README.md) |

**Your coding agent already has the manual.** Every package ships a `SKILL.md` written for coding agents: the
notation, the generated API, what each diagnostic means, and the mistakes to avoid. After a restore it sits in the
package folder (`~/.nuget/packages/dotgram/<version>/SKILL.md`); `dotnet dotgram --skill` prints the CLI's. Point
your agent at it and let it write the grammar.

[DotGram](src/DotGram/SKILL.md) · [DotGram.Cli](src/DotGram.Cli/SKILL.md) · [DotGram.Sql](src/DotGram.Sql/SKILL.md) ·
[DotGram.Web](src/DotGram.Web/SKILL.md) · [DotGram.Finance](src/DotGram.Finance/SKILL.md) ·
[DotGram.ExpressionLanguage](src/DotGram.ExpressionLanguage/SKILL.md)

## Performance

"× faster" is the other side's time divided by .Gram's.

<!-- STAND: every number in this section is a placeholder from the night-1 release-candidate stand (c831e00b,
     2026-11-04, exclusive window, 5 runs). Re-read all of them from the release stand (`--stand`, S1) on the
     tagged commit: rows web/url.{plain,ipv4,full,long-path}, web/date-time.utc, fix/Order.text (readings
     generated, regex-compiled(-lesser), regex(-lesser)); sql/select20, feeds/stock-count.good.text,
     fix/Order.text, fix/Orders128.text, web/url.full (generated against hand). A row that is not a win on the
     tag leaves the table. -->

**Faster than `Regex`**, with both sides returning the same parts of every input, checked before timing:

| Input | .Gram | `Regex`, compiled | | `Regex` | |
| --- | ---: | ---: | ---: | ---: | ---: |
| `http://example.com` | 121 ns | 594 ns | **4.9×** | 1,506 ns | **12×** |
| `https://192.168.0.1/` | 159 ns | 583 ns | **3.7×** | 1,361 ns | **8.5×** |
| a URL with every part, 47 characters | 230 ns | 595 ns | **2.6×** | 1,434 ns | **6.2×** |
| a URL path of eight segments, 84 characters | 338 ns | 855 ns | **2.5×** | 2,986 ns | **8.8×** |
| an RFC 3339 timestamp | 59 ns | 447 ns | **7.6×** | 646 ns | **11×** |
| a FIX order message | 489 ns | 1,406 ns | **2.9×** | 1,978 ns | **4.0×** |

The timestamp pattern does not check the calendar, and the FIX pattern only splits `tag=value`: .Gram does more
work on those rows and is still faster.

**Faster than hand-written parsers** of the same language, held to the same answers on every input:

| What is parsed | .Gram | hand-written | |
| --- | ---: | ---: | ---: |
| a SQL `SELECT` of 20 columns | 6.3 µs | 12.5 µs | **2.0×** |
| a hand-kept stock count, 1,000 lines | 35 µs | 61 µs | **1.7×** |
| a FIX order message | 489 ns | 761 ns | **1.6×** |
| 128 FIX messages | 57 µs | 88 µs | **1.5×** |
| a URL with every part | 230 ns | 264 ns | **1.1×** |

<!-- STAND: commit sha and results file of the release stand; commit the stand's stand.md and stand.json as
     benchmarks/results/stand-<date>-0.3.0.{md,json} and link the .md below. -->
Linux, AMD Ryzen 9 5950X, .NET 10; median of 5 runs, each in a fresh process, on an idle machine; commit `<sha>`.
Every row we measure — every format, refusals, allocations, and the rows where a hand-written parser is still
ahead — is in [the full results](benchmarks/README.md).

## Big grammars, fast builds, fast starts

<!-- STAND: generation time from the generator gate (G1) on the tag, DotGram.Sql per target framework
     (night 1, c831e00b: net10.0 13,368 ms, netstandard2.0 12,780 ms). -->
**Build time.** Generating the three SQL grammars of `DotGram.Sql` takes about 13 s per target framework, and a cold
build pays it every time. Keep a large grammar in a project of its own, so only the projects that change it pay;
or generate it once, commit the files, and the build compiles plain C#:

```shell
dotnet new tool-manifest
dotnet tool install DotGram.Cli
dotnet dotgram generate src/Grammars/Grammars.csproj -o src/Grammars/Generated
```

<!-- STAND: cold `dotnet build` of src/DotGram.Sql with the DotGram package against the same project compiling
     committed generated files (exclusive window, both target frameworks); not measured yet. Fill in: -->
<!-- A cold build of `DotGram.Sql` takes N s with the package and M s with the files committed. -->

`generate` writes the same files the build would, byte for byte. Use one or the other: a project that references
the package and also compiles the generated files defines every type twice (CS0102). Write the grammar with the
package, where the editor shows its diagnostics, and move to the tool when it is done.
[`docs/cli.md`](docs/cli.md) has the CI check that keeps the files current, what exactly the tool promises, and
every option.

<!-- STAND: first call JIT and ReadyToRun, exclusive run on the tag (directional so far: science perf-parsers-1110
     round 1, rows web/url.full, sql/select20, tsql/select-long; 5.9/36.5/70.7 ms JIT, 0.5/6.9/16.8 ms R2R). -->
**First call.** The first parse in a process waits for the JIT: 6 ms for a URL, 37 ms for a 20-column SQL `SELECT`,
71 ms for a long T-SQL `SELECT`; the calls after it cost what the tables above show. Publish the application with
`<PublishReadyToRun>true</PublishReadyToRun>` and the first call drops to 0.5 ms, 6.9 ms and 17 ms. The price is
size on disk (`DotGram.Sql.dll` grows from 11.4 to 22.6 MB); the packages cannot ship precompiled code, because
such images are specific to a platform and a runtime version.

## What a grammar can do

* literals and element sets; ranges and Unicode categories;
* sequence and ordered choice; `?`, `*`, `+`, and bounded repetition; lookahead and atomic groups;
* named captures and generated result types, or [existing C# types](docs/tour.md#typed-parsing), filled by
  constructor or by `required` properties;
* semantic actions and guards; external C# predicates and recognizers ([Grammar and C#](docs/tour.md#grammar-and-c));
* parameterized rules; rule rebinding and parser specialization ([one grammar, three parsers](docs/tour.md#one-grammar-three-parsers));
* conditions about the grammar — `when Version is "2022"` — decided when a parser is generated, which is how one
  grammar is [several versions of a language](docs/tour.md#versions-of-one-language);
* left recursion — direct, and indirect through rules that only forward — with binding powers for expression grammars;
* grammar namespaces, and [grammar libraries](docs/tour.md#grammar-libraries) that cross a project reference;
* a lexical split: the same notation read over tokens instead of characters;
* `Parse`, `TryParse`, and `Find`, over strings, slices, bytes, streams and lines
  ([what a grammar reads](docs/tour.md#what-a-grammar-reads-and-what-comes-out));
* streaming from `TextReader`, and recovery inside repetitions, so a bad record in a
  <a name="feeds-written-by-people"></a>[feed written by people](docs/tour.md#feeds-written-by-people) is reported
  where it is and reading goes on ([streaming, tokens and recovery](docs/tour.md#streaming-tokens-and-recovery));
* parser context and parsing state;
* [why an input was refused](docs/tour.md#why-an-input-was-refused): a trace build names the rules and the guard.

[`docs/status.md`](docs/status.md) lists the features one by one, with what is not implemented and what is only
partly.

## Ready-made parsers

| Package | What it reads | |
| --- | --- | --- |
| [`DotGram.Sql`](src/DotGram.Sql/README.md) | T-SQL as SQL Server reads it, ISO SQL:2023 and SQL-92, into one tree, and back to text | [![NuGet](https://img.shields.io/nuget/v/DotGram.Sql?label=)](https://www.nuget.org/packages/DotGram.Sql) |
| [`DotGram.Web`](src/DotGram.Web/README.md) | JSON, JSON Pointer and Patch, URIs and URI templates, media types, cookies, structured fields, e-mail addresses, timestamps, language tags — each after its RFC | [![NuGet](https://img.shields.io/nuget/v/DotGram.Web?label=)](https://www.nuget.org/packages/DotGram.Web) |
| [`DotGram.Finance`](src/DotGram.Finance/README.md) | FIX 4.2, 4.4 and 5.0 SP2 tag-value messages, from logs, archives, files and buses | [![NuGet](https://img.shields.io/nuget/v/DotGram.Finance?label=)](https://www.nuget.org/packages/DotGram.Finance) |
| [`DotGram.ExpressionLanguage`](src/DotGram.ExpressionLanguage/README.md) | a C#-style expression language, straight into `System.Linq.Expressions` | [![NuGet](https://img.shields.io/nuget/v/DotGram.ExpressionLanguage?label=)](https://www.nuget.org/packages/DotGram.ExpressionLanguage) |

A tour of each, with a line of code: [`docs/tour.md`](docs/tour.md#dotgramfinance).

## Compatibility

<a name="no-runtime-parser-library"></a>

| | |
| --- | --- |
| Building | .NET SDK 9.0.300+ (Roslyn 4.14; Visual Studio 17.14+); an SDK 8 build skips the generator (CS9057) |
| Generated code | C# 8, for whatever the project targets: `netstandard2.0`, `net472`, `net8.0` and later are built on every commit |
| Inline grammar (`[Gram("""…""")]`) | C# 11, for the raw string literal; a `.gram` file needs C# 8 |
| `netstandard2.0`, `net472` | `<LangVersion>` set, a `System.Memory` reference, and `IsExternalInit` for records |
| Runtime dependencies | none: everything a parser needs is generated into your assembly as internal C#, so there is no runtime assembly to deploy and no version pair to drift apart |

[`docs/compatibility.md`](docs/compatibility.md) has the project file for an older target, and the one thing that
costs more below C# 11.

## Building

```sh
dotnet build DotGram.slnx
dotnet test  DotGram.slnx
```

On Linux or macOS, build and test the `Linux` configuration, which leaves out the Visual Studio
extension and its tests (they build only on Windows): `dotnet build DotGram.slnx -c Linux`, then
`dotnet test DotGram.slnx -c Linux`.

## License

[MIT](LICENSE), with one addition for the code the generator writes into your compilation:
you may use it under terms of your choice, without carrying the notice. The exact words are
at the end of [LICENSE](LICENSE).
