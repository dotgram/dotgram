# The command-line tool

`dotnet dotgram` is the same generator as a .NET tool. It writes the C# a build would generate into a
folder, and a project then compiles that folder like any other code, without referencing the `DotGram`
package. This page covers when that is worth doing and how; every command, option and exit code is in
the reference, [`cli.md`](../../cli.md).

## When to use it

A grammar of hundreds of rules takes seconds to generate, and a cold build — every CI run — pays for it
each time. Keep a large grammar in a project of its own first, so that only that project pays. Where
that is still too much, generate the files with the tool, commit them, and let the build compile them
as they are.

A project does **one or the other**: it references the package and generates in its build, or it
compiles files the tool wrote. Both at once generate every parser twice, and the build fails on every
member (`CS0102`, `CS0111`).

The usual course is to write a grammar with the package — the editor shows its diagnostics as you type
— and move to the tool when the grammar settles. The tool prints the same diagnostics, but an editor
shows none for a project that no longer references the package.

## Installing

```shell
dotnet new tool-manifest              # once, in the repository's root
dotnet tool install DotGram.Cli
dotnet dotgram --version
```

```text
dotnet-dotgram 0.3.0 (generator 0.3.0)
```

The tool's version is the version of the generator it carries. It runs on .NET 8 and later, on the
newest runtime installed. On .NET 10, `dnx DotGram.Cli generate …` runs it without installing.

## A whole project

Given a project, the tool asks MSBuild for what the compiler would be given — the C# files, the
references, the language version, the additional files, the analyzers — runs the generator over
them, and writes what it generated:

```shell
dotnet restore src/Grammars
dotnet dotgram generate src/Grammars/Grammars.csproj -o src/Grammars/Generated
```

```text
dotnet dotgram generate /repo/src/Grammars/Grammars.csproj --output /repo/src/Grammars/Generated --configuration Release --line-paths relative
  net10.0: compiler inputs from MSBuild in 1.0 s (referenced projects 0.5 s, arguments 0.6 s)
  net10.0: 3 files, 54,231 characters in 1.6 s
  net10.0: sources checked in 0.3 s
Wrote 3 files into /repo/src/Grammars/Generated (0 unchanged, 0 deleted) in 3.7 s.
The generator writes CRLF line ends: 'src/Grammars/Generated/** -text' in the .gitattributes at the repository's root keeps them as written.
```

The first line is every argument as the tool understood it. Then the project drops its `DotGram`
reference, and its build compiles `Generated/` with the rest of its C#.

- **What it promises.** The files are byte for byte what `dotnet build` of the project would generate
  (configuration `Release` unless `--configuration` says otherwise), for a restored project built
  with the same SDK. Two differences are by design: `#line` paths are relative to the output folder,
  so that committed files do not carry the path of the machine that wrote them (`--line-paths
  absolute` gives the build's text), and the [report](04-msbuild.md#what-the-generator-did-dotgramreportgeneration)
  files are not written.
- **What it does not do.** It does not restore (run `dotnet restore` first), and it does not compile
  the project: an error in a grammar's C#, in the generated parser or elsewhere shows when the build
  compiles the files. It runs what an IDE runs when it opens the project — MSBuild up to the
  compiler, as a design-time build.
- **The project decides.** Language version, symbols, references and packages are the project's; the
  options that set them for a single grammar (below) are an error with a project.
- **Every target framework.** A multi-targeted project is generated for each framework, and each must
  produce the same files. `--framework` picks one of the project's.

## One grammar

A single `.gram` file needs no project. The tool writes the host class from its options — the class
is named after the file unless `--class` says otherwise, `internal` unless `--public` — and generates
the grammar into it. `--sources` adds the C# the grammar calls: a `partial` declaration of the same
class with the members its `@` hooks name, and any types it builds:

```shell
dotnet dotgram generate grammars/Calc.gram -o Generated --namespace Demo --sources grammars/Calc.cs
```

```text
  net10.0: DotGram: Demo.Calc, 3 normalized rules, 30078 bytes UTF-8 C#, 300.76 ms generation
  net10.0: 3 files, 53,951 characters in 1.3 s
  net10.0: sources checked in 0.3 s
Wrote 4 files into /repo/Generated (0 unchanged, 0 deleted) in 2.0 s.
```

The grammar, the partial class beside it, and the host the tool writes, `Calc.Host.g.cs`, compiled
together:

```dotgram file=grammars/Calc.gram example=one-grammar
trivia = { ' '* }

Sum  : @int = left: Sum & '+' & right: Term << 1 => @(left + right)
            | value: Term                       => @(value)
Term : @int = digits: { ['0'..'9']+ } => @(Number(digits))

parse Sum
```

```csharp example=one-grammar
namespace Demo;

partial class Calc
{
	static int Number(string digits)
	{
		return int.Parse(digits, System.Globalization.CultureInfo.InvariantCulture);
	}
}
```

```csharp example=one-grammar
// <auto-generated/>
// The host class of Calc.gram, written by dotnet dotgram generate.
namespace Demo
{
	[global::DotGram.Gram("Calc.gram")]
	internal partial class Calc
	{
	}
}
```

```text run example=one-grammar
Calc.ParseSum("1 + 2 + 3") → 6
```

A project compiles `grammars/Calc.cs` and `Generated/*.cs` and has `Demo.Calc.ParseSum`. Without
`--sources`, the hook `Number` is nowhere, and the tool says so and writes nothing: for a single
grammar it compiles the parser with its host and sources, since there is no build to do it.

The options of a single grammar:

| Option | What it sets | Default |
| --- | --- | --- |
| `--namespace <Ns>` | the host's namespace | none |
| `--class <Name>` | the host's name | the file's name |
| `--public` | a public host | `internal` |
| `--option <Name>=<value>` | an option of `[Gram]`: `Lexical=true`, `Carrier=Deferred`, `Name=Fast`, `LocationType=Demo.ILocated` | none |
| `--sources <file.cs\|glob>` | C# compiled with the grammar | none |
| `--reference <file.dll\|dir>` | an assembly the grammar's types come from | none |
| `--package <Name>@<Version>` | a package's assemblies, from the global packages folder | none |
| `--define <SYMBOL>` | preprocessor symbols, several separated by `;` or `,` | the framework's |
| `--langversion <version>` | the C# version | the SDK's for the framework |
| `--framework <tfm>` | `netstandard2.0`, `net8.0` or later | `net10.0` |

`--option` takes every option of [the host's attributes](02-hosts-and-attributes.md#the-options-of-a-reading)
by name, without regard to case, and checks the value against its type; a name `[Gram]` does not have is
an error that lists the ones it has.

## Keeping the files current

Nothing in the build notices that a grammar changed. Run the tool again after editing a grammar or a
type it builds, and read the diff of the folder. Keep the arguments in a **response file** so that
every run uses the same ones:

```text
# grammars.rsp
src/Grammars/Grammars.csproj
-o src/Grammars/Generated
```

```shell
dotnet dotgram generate @grammars.rsp
```

A response file is read as `csc` reads one: words separated by blanks or line ends, `"quoted"` to keep
blanks, `#` beginning a comment line, `@` nesting another; a relative path in it is read against the
file's own folder.

The tool rewrites a file only when its text changed, so a run that changes nothing changes no
timestamp, and it deletes a file it wrote before and no longer writes — the folder's `.dotgram.json`
lists what it wrote, with the versions that wrote it. Nothing else in the folder is touched.

**In CI**, generate and fail if anything moved:

```shell
dotnet dotgram generate @grammars.rsp
git status --porcelain -- src/Grammars/Generated    # fails the job if it prints anything
```

The generated C# is written with CRLF line ends. Keep Git from converting them, and pin the grammars'
own line ends, so that every machine generates from the same text and the check stays clean under
`core.autocrlf=true`. The output folder's line comes last, since later lines win:

```gitattributes
*.gram                    text eol=lf
src/Grammars/**/*.cs      text eol=lf
src/Grammars/Generated/** -text
```

The tool prints the `-text` line for its output folder at the end of every run.

## When it fails

| Exit code | Meaning |
| --- | --- |
| `0` | generated |
| `2` | the grammar has errors, which are printed and nothing is written; for a single grammar also a parser that does not compile with its host and sources |
| `3` | a usage error, an input or output that cannot be read or written, a project that is not restored, MSBuild failing, a generator that will not load, or a project error its own build would report too |

```text
$ dotnet dotgram generate grammars/Bad.gram -o Bad
grammars/Bad.gram(1,7): error GRAM3002: No rule, parameter or capture named 'Missing'.
Grammar errors: nothing written.
$ echo $?
2
```

An error the tool prints names what it could not do and, for a usage error, points at `--help`.
`--verbose` adds the stack trace where the generator itself failed. The diagnostics are those a build
would show, at the severities the project gives them.

