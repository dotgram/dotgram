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

A project's own packages, and the generators among their analyzers, are read from what restore
wrote (`obj/project.assets.json`): run `dotnet restore` first; the tool never restores. Types that
come from elsewhere — another project's build output, a `.gram` without a project — take
`--reference <file.dll|dir>` or `--package <Name>@<Version>` (from the global packages folder).

If a name in the project's code cannot be resolved — a missing reference, or a half-written file —
the tool names it, writes nothing and exits 3, as the build would fail.

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
- Exit codes: 0 done; 2 grammar errors (nothing written); 3 usage, evaluation, an unresolved type
  a grammar asks about (nothing written), frameworks that differ.

## What not to do

- Do not reference the `DotGram` package and compile the generated folder in the same project.
- Do not edit the generated files: the next run overwrites them.
- Do not commit the output of one framework for a multi-targeted project generated with
  `--framework`: the folder is compiled by every framework the project targets.
