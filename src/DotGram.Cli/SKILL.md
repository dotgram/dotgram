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
# A project: its C# files, global usings, grammars and settings, read by an MSBuild evaluation.
dotnet dotgram generate src/Grammars/Grammars.csproj -o src/Grammars/Generated

# One grammar, no project: the [Gram] options as --option, the C# it builds with --sources.
dotnet dotgram generate calc.gram -o out --namespace Demo --option Lexical=true
```

Types the grammar builds that live in another assembly: `--reference <file.dll|dir>` or
`--package <Name>@<Version>` (from the global packages folder; netstandard2.0 projects need
`--package System.Memory@<version>`). If a type a grammar asks about cannot be resolved, the tool
names it, writes nothing and exits 3.

## In CI

Run the same command with `--verify`: it generates in memory, writes nothing, and exits 1 with the
first difference when a grammar, an option, a type it builds or the generator changed without the
files being regenerated. Keep the arguments in a response file so both runs use the same ones:

```shell
dotnet dotgram generate @grammars.rsp           # regenerate
dotnet dotgram generate @grammars.rsp --verify  # in CI
```

Line ends are not compared; `Generated/** -text` in `.gitattributes` keeps the files as written.

## Reading what it says

- The first line is the arguments as the tool understood them, response files expanded.
- Diagnostics are `path(line,col): error GRAMnnnn: message`, as csc writes them.
- Exit codes: 0 done or equal; 1 `--verify` differs; 2 grammar errors (nothing written);
  3 usage, evaluation, an unresolved type a grammar asks about, frameworks that differ.

## What not to do

- Do not reference the `DotGram` package and compile the generated folder in the same project.
- Do not edit the generated files: `--verify` reports the edit, and the next run overwrites it.
- Do not commit the output of one framework for a multi-targeted project generated with
  `--framework`: the folder is compiled by every framework the project targets.
