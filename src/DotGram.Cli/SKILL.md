---
name: dotgram-cli
description: Generate DotGram parsers ahead of time with `dotnet dotgram generate`, so that a project compiles the generated C# and does not run the generator in its build. Use when a project's grammars are large enough that regenerating them on every cold build hurts, or when asked to check committed parsers in CI.
---

# dotnet dotgram

`dotnet dotgram generate` runs the same generator the `DotGram` package runs inside the compiler,
and writes the files a build would have generated. The project then compiles those files like any
other C# and **does not reference the `DotGram` package** — referencing both generates every
parser twice (CS0102/CS0111 on every member).

## When to use it

- A grammar large enough that its generation shows in cold builds and CI (hundreds of rules).
  Keeping it in a project of its own comes first; this is the next step for that project.
- Not for a grammar still being written: the tool prints the GRAM diagnostics, the editor does not
  show them.

## The two inputs

The input's kind is its extension.

```shell
# A project: the compiler's inputs for it, as its own build gives them.
dotnet dotgram generate src/Grammars/Grammars.csproj -o src/Grammars/Generated

# One grammar, no project: the [Gram] options as --option, the C# it builds with --sources.
dotnet dotgram generate calc.gram -o out --namespace Demo --option Lexical=true
```

The tool does not compile the project. It asks MSBuild for the compiler's inputs for each target
framework: the C# files, the references, the language version and symbols, the source encoding, the
additional files, the analyzer configuration and the analyzers, the generators of other packages
among them, which run beside DotGram's. That runs the project's `InitialTargets` and the targets that
run before `CoreCompile` (they write `AssemblyInfo`, the global usings and the analyzer configuration
under `obj/`, and anything of the project's own hooked there), and not `Build`, `AfterBuild` or
anything after the compiler. The projects it references are built if they are not up to date; the
configuration is Release unless `--configuration` says otherwise. It needs .NET SDK 8.0.100 or newer.

**What it promises.** It writes the files DotGram would generate in `dotnet build` of the project
(Release unless `--configuration` says otherwise), byte for byte, in the encoding the compiler writes
them (UTF-8 with a byte order mark), with the tool's DotGram, for a restored project, built with the
same SDK, whose own targets do not change the compiler's inputs outside the IDE's design-time pass.
A difference on a project inside that condition, which every ordinary SDK-style project is, is a
defect in the tool: report it.

**What it does not do.** It does not compile the project and does not tell you whether it builds:
errors in grammar actions, in the generated parser or anywhere else in the project show when your
build compiles the files. It runs what an IDE runs when it opens the project — MSBuild's `Compile`
target with the compiler skipped, as a design-time build (`DesignTimeBuild=true`) — so a custom target
that does not skip design-time builds runs too, and one that follows the compiler finds no new
assembly. It does not restore: the project must be restored, and after changing package references
you run `dotnet restore` first (the tool warns when the restore looks older than the project files).
When this is not good enough, build with `EmitCompilerGeneratedFiles=true` and copy the generated
files.

What a project's build decides, the project decides here: `--define`, `--langversion`, `--reference`
and `--package` are for a lone `.gram` input only, which has no build to ask.

If the project's own sources have an error the compiler reports by default — a missing reference, a
half-written file — the tool lists it as the compiler does, writes nothing and exits 3. It applies
the compiler's default severities only (`/warnaserror`, `/nowarn`, rulesets and `.editorconfig`
severities are not applied), and it does not check the project as a whole.

## Keeping it current

Nothing in the build notices that a grammar changed: run the command again after editing a grammar
or a type it builds, and read the diff of the folder. Keep the arguments in a response file so that
every run uses the same ones:

```shell
dotnet dotgram generate @grammars.rsp
```

To check it in CI, run it there and fail on `git diff --exit-code` over the folder.
`Generated/** -text` in `.gitattributes` keeps the files as the generator writes them (CRLF).

## Reading what it says

- The first line is the arguments as the tool understood them, response files expanded.
- Diagnostics are `path(line,col): error GRAMnnnn: message`, as csc writes them.
- Exit codes: 0 done; 2 grammar errors (nothing written); 3 usage, MSBuild failing for a framework, a
  project not restored, a generator that will not load, an error the project's build would report
  too (nothing written), frameworks that differ.
- A project that references another version of `DotGram` than the tool carries is generated with the
  tool's, with a warning naming both; if that output does not compile in the project, exit 3.

## What not to do

- Do not reference the `DotGram` package and compile the generated folder in the same project.
- Do not edit the generated files: the next run overwrites them.
- Do not commit the output of one framework for a multi-targeted project generated with
  `--framework`: the folder is compiled by every framework the project targets.
