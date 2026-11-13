# When something is wrong

Symptoms, roughly in the order people meet them, with what causes each and what to do. A message
with a `GRAM` number is described in full in [diagnostics](07-diagnostics.md).

## `[Gram]` is not found (CS0246)

The attribute is written into the project by the generator, so `The type or namespace name 'Gram'
could not be found` means the generator did not run for this project.

- **The project does not reference the package.** Every project that declares a grammar references
  `DotGram` itself; a reference in another project does not reach it
  ([setup](01-setup.md#adding-it-to-a-project)).
- **`using DotGram;` is missing.** The attributes are in the namespace `DotGram`.
- **Only in Visual Studio, while a command-line build is clean.** The editor has lost the generator
  for the session: under **Dependencies → Analyzers** the DotGram node is empty. Close every Visual
  Studio, delete `%TEMP%\Roslyn\AnalyzerPathResolver`, and open the solution again
  ([`visual-studio.md`](../../visual-studio.md#when-the-generator-disappears-from-the-editor)).

## The parse methods do not exist

`[Gram]` compiles, but `Numbers.ParseDigits` is an unknown name.

- **The class is not `partial`**, or a class around it is not: `GRAM0002`.
- **The grammar was not found**: `GRAM0003` (the file is not an additional file) or `GRAM0004` (two
  are) — [grammar files](03-grammar-files.md#how-a-host-finds-its-file).
- **The rule is not published.** A rule is not a method until a directive publishes it: without
  `parse Letters` there is no `ParseLetters`. Where the grammar publishes something else, a rule that
  nothing reaches is a warning:

```csharp example=unpublished GRAM4018
using DotGram;

[Gram("""
	Digits  = ['0'..'9']+
	Letters = ['a'..'z']+

	parse Digits
	""")]
public static partial class Numbers;
```

*Nothing reaches 'Letters': no publication names it, no rule the grammar reads calls it, and no
rebinding puts it in place of one. Publish it, call it, or delete it.*

- **The grammar publishes nothing at all.** A grammar with no `parse`, `find` or `yield` gives its
  class no method, and is of use only included by another grammar. Where no `[Gram]` class of the
  compilation includes it or derives from its class, the build takes it for a grammar whose `parse`
  was forgotten:

```csharp example=publishes-nothing GRAM4059
using DotGram;

[Gram("""
	Digits = ['0'..'9']+
	""")]
public static partial class Numbers;
```

*The grammar of 'Numbers' publishes nothing (no parse, find or yield) and no [Gram] class in this
compilation includes it; a grammar for other assemblies to include says [Gram(ExportGrammar = true)].*
It is said once per class, only where the grammar has no errors, and never of a `[GramParserVariant]`.
A grammar written for other assemblies to include says so in so many words — that a public class
exports its grammar by default is not enough, since every public class does:

```csharp example=library
using DotGram;

[Gram("""
	Digit = ['0'..'9']
	""", ExportGrammar = true)]
public static partial class Lexemes;
```

- **The method is in a nested class.** A reading of a `[GramParserVariant("Fast")]`, or of a
  `[Gram(Name = "Fast")]`, is `Numbers.Fast.ParseDigits`.
- **Any grammar error stops the parser.** The first error in the Error List is the one to fix; the
  unknown method is its consequence.

## Every member is defined twice (CS0102, CS0111)

The project both references the `DotGram` package and compiles files `dotnet dotgram` wrote, so every
parser is generated a second time. Keep one: drop the package reference, or delete the generated
folder and the `Compile` items that include it ([the tool](05-cli.md#when-to-use-it)).

## A name from before 0.3.0 (GRAM0012, CS0246, CS0117)

A host written for 0.2 uses names 0.3.0 renamed: `[GramOptions]`, `Suffix`, `IncludedAs`, `Portable`,
`GramCarrier.Tape`, and more. The C# compiler reports the name as unknown, and the generator says what
it is called now — `'IncludedAs' was renamed to 'DefaultAlias' in 0.3.0` — which is the message to act
on ([the options](02-hosts-and-attributes.md#the-options-of-a-reading)). A library built by DotGram 0.2
can still be included: its classes are read under the old names as well.

A library grammar of 0.2 that publishes nothing and is included only by other assemblies is now told
`GRAM4059`; `[Gram(ExportGrammar = true)]` says what it is
([above](#the-parse-methods-do-not-exist)).

## The build breaks with warnings as errors after an upgrade

A new version of the generator may say more about a grammar that has not changed: new analyses report
new warnings, and under `TreatWarningsAsErrors` each is an error. The release notes list the new ones.
Read the message first — most point at something worth changing in the grammar. Where the grammar is
right as it stands, silence that one number at the narrowest place, with the reason beside it
([diagnostics](07-diagnostics.md)).

## The parser behaves like an older grammar

The grammar was edited, the build succeeded, and the parser still answers as before.

- **A command-line build took the parser from the compiler server.** It keys a kept parser by
  everything its compile read, so this should not happen; if it does, `dotnet build-server shutdown`
  or `-p:DotGramNoCache=true` rules it out, and the case is worth reporting
  ([MSBuild properties](04-msbuild.md#reusing-a-parser-dotgramnocache)).
- **The files are generated by the tool.** Nothing in the build notices a changed grammar: run
  `dotnet dotgram generate` again ([keeping the files current](05-cli.md#keeping-the-files-current)).
- **Another file of the same name is read.** With two candidates the build says `GRAM0004`; with a
  path in `[Gram("…")]` it reads that path, which may not be the file being edited.

## Older frameworks: CS0246 `ReadOnlySpan`, CS0518 `IsExternalInit`, GRAM0010

- `ReadOnlySpan<>` could not be found: a `netstandard2.0` or .NET Framework project references
  `System.Memory` itself.
- `IsExternalInit` is not defined (CS0518): a grammar builds `record` values on a framework without
  that type; declare it once in the project.
- GRAM0010: the project compiles C# below 8; set `<LangVersion>`.

All three are in [setup](01-setup.md#language-version-and-older-frameworks).

## A message in a `.gram` file will not be silenced

An `.editorconfig` section for `*.gram` does not reach a grammar file's messages, and a `#pragma`
cannot stand in one. Use `NoWarn` in the project, or a global analyzer configuration
([diagnostics](07-diagnostics.md)).

## Generated files appear in the project's folder

A project that imports `DotGram.targets` by hand — with the generator referenced as a plain analyzer
rather than through the package — and imports it in the project file itself gets a `DotGramGenerated`
folder beside the project when a report is on: the path is worked out before the SDK has said where
`obj/` is. Import it from `Directory.Build.targets` instead, which is read after that.

## The generator failed (GRAM0001)

*The generator failed during …* is a defect in .Gram, not in the grammar: the message names the stage
and carries the exception. Please report it with the message and, where you can, the grammar. The rest
of the project is still compiled; only that host has no parser.

