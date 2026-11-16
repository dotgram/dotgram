# Writing grammars with a coding agent

A grammar is a small language of its own, and learning one costs time before it pays back. A
coding agent can carry most of that cost: every .Gram package ships a `SKILL.md`, a guide written
for an agent rather than for a reader. It covers the notation, the generated API, what each
diagnostic means and the mistakes an agent tends to make. This page says where those files are, how
to hand one to an agent, and how to check what the agent wrote.

## Where the skills are

Each package carries its skill at the root of the package, beside its `README.md`. After a restore
it is in the NuGet package folder (`~/.nuget/packages` on Linux and macOS,
`%UserProfile%\.nuget\packages` on Windows, unless `NUGET_PACKAGES` says otherwise), under the
package id in lower case and the version:

| Package | For | In the package folder | In the repository |
| --- | --- | --- | --- |
| `DotGram` | writing any grammar: start here | `dotgram/<version>/SKILL.md` | [`src/DotGram/SKILL.md`](https://github.com/dotgram/dotgram/blob/main/src/DotGram/SKILL.md) |
| `DotGram.Cli` | generating outside the build | `dotgram.cli/<version>/SKILL.md` for a local tool; `dotnet dotgram --skill` prints it either way | [`src/DotGram.Cli/SKILL.md`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Cli/SKILL.md) |
| `DotGram.Sql` | the SQL parsers and their tree | `dotgram.sql/<version>/SKILL.md` | [`src/DotGram.Sql/SKILL.md`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/SKILL.md) |
| `DotGram.Web` | the web formats (JSON, URIs, media types, …) | `dotgram.web/<version>/SKILL.md` | [`src/DotGram.Web/SKILL.md`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Web/SKILL.md) |
| `DotGram.Finance` | FIX messages | `dotgram.finance/<version>/SKILL.md` | [`src/DotGram.Finance/SKILL.md`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Finance/SKILL.md) |
| `DotGram.ExpressionLanguage` | the C#-style expression language | `dotgram.expressionlanguage/<version>/SKILL.md` | [`src/DotGram.ExpressionLanguage/SKILL.md`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.ExpressionLanguage/SKILL.md) |

The skill in the package folder is the one for the version the project uses; the repository's is
for the next one. Each file opens with a short `name` and `description` header, and the rest is
plain Markdown, so any agent that can read a file can use it.

## Pointing an agent at one

No agent-specific setup is needed. Either say it in the prompt:

```text
Before writing the grammar, read ~/.nuget/packages/dotgram/0.3.0/SKILL.md and follow it.
```

or put one line in the instructions file the agent already reads in your project (`AGENTS.md`,
`CLAUDE.md` or your tool's equivalent), so every session finds it without being told:

```text
Parsers here are written in .Gram (the DotGram package). Before editing a [Gram] attribute or a
.gram file, read SKILL.md in the DotGram package folder (~/.nuget/packages/dotgram/<version>/).
```

Add the skill of a library package when the work uses it: `DotGram.Sql` for code that reads SQL
with the shipped parsers, and so on.

## A loop that works

1. **Describe the input and what you want back.** Give the agent two or three real samples,
   including a bad one, and say what the result should be: a typed record, a list of them, a
   number, a tree, or just "accepted or refused".
2. **Let the agent write the grammar and its host**: the `partial` class with the `[Gram]`
   attribute, or a `.gram` file, and the code that calls the generated `Parse` or `TryParse`.
3. **Build.** The generator runs in the compiler, so a grammar it cannot use fails the build with
   a `GRAMnnnn` diagnostic that points into the grammar.
4. **The agent reads the diagnostics and fixes the grammar.** Each identifier is explained in
   [Diagnostics](diagnostics.md), and the skill lists the ones worth knowing in advance. Repeat
   from step 3 until the build is clean.
5. **Turn the samples into tests**: each good sample with the result it must give, each bad one
   with the refusal it must give. The tests keep the grammar honest when it changes later.

## What a trial showed

Before this page was written, five agents that had never seen .Gram were each given one task and nothing but the
0.3.0 packages from nuget.org and their skills: a CSV export with quoted fields, an nginx access log line into a typed
record, a calculator over named variables, the tables a T-SQL `SELECT` reads (with DotGram.Sql), and a multi-gigabyte
feed of key=value records with recovery. All five finished with a working, tested parser; the first green build came
after one to four attempts, each failed one stopped by a `GRAMnnnn` diagnostic that pointed into the grammar. What they
had to guess — a file grammar's attribute form, exceptions thrown inside `=>`, line ends beside empty records, the
shape of recovery for multi-line records — is now in the skills.

## What to check yourself

A clean build means the grammar is well formed, not that it reads your format the way you meant.
Before you rely on it, look at two things:

* **Refusals.** Feed it input that must fail: a truncated line, a wrong separator, an empty
  field. It must refuse, and say where. An agent can make a grammar pass its samples by accepting
  too much.
* **The typed result.** Read the generated result type and the values it holds for your samples:
  the names, the types, what an optional part holds when it is missing. That type is the API the
  rest of your code is written against.

When a refusal is not where you expect, a trace build names the rules that were tried and the
guard that said no ([why an input was refused](tour.md#why-an-input-was-refused)).

## Further reading

* [Language guide](guide/language/README.md): the notation, a page a topic, each with a grammar
  that uses it.
* [Diagnostics](diagnostics.md): every message the generator reports, by identifier.
* [Generator guide](guide/generator/README.md): setup, project files, the command line and
  troubleshooting.
