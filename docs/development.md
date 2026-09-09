# Working on this

How the project is built, checked and measured. Standing process rather than plans —
[`next.md`](next.md) says what to do next, this says what to do every time.

## Build and test

```
dotnet build DotGram.slnx
dotnet test DotGram.slnx --no-build
```

`dotnet test` goes through Microsoft.Testing.Platform rather than VSTest, which xunit 4
requires on the .NET 10 SDK and `global.json` opts into. The runner can also be started
directly — `tests/DotGram.Tests/bin/Debug/net10.0/DotGram.Tests.exe`, and
`-filter "/*/*/ClassName/MethodName"` for one test. It runs everything in about two
minutes.
The examples are compiled by the real generator during that build, so a member the
generator stopped producing fails the build rather than a test.

## The same build on Linux

CI builds on Windows and on Linux, and the Linux job has caught what the Windows one
cannot: a filename character that is legal there, an API missing from a target framework,
a path assumption. Reproducing it locally is a container, made once and kept:

```
docker volume create dotgram-nuget
docker volume create dotgram-work
docker run -d --name dotgram-linux -v P:/dotgram:/src/main:ro -v P:/dotgram.WorkTrees:/src/worktrees:ro -v dotgram-nuget:/root/.nuget/packages -v dotgram-work:/work mcr.microsoft.com/dotnet/sdk:10.0 sleep infinity
```

The checkouts go in read-only and are never built in place: `bin/` and `obj/` there hold
Windows output, and a Linux build that reads it earns CS0579 on assembly attributes it
finds twice. The tree is copied into `/work` without them instead, which is what `ci`
does — a script put in the container once, `docker cp ci dotgram-linux:/usr/local/bin/ci`
and `chmod +x`:

```
#!/bin/bash
set -e
tree=${1:-worktrees/docs}
name=$(basename "$tree")

rm -rf "/work/$name"
mkdir -p "/work/$name"

tar -C "/src/$tree" --exclude=bin --exclude=obj --exclude=.git --exclude=.vs --exclude=.work --exclude=artifacts -cf - . | tar -C "/work/$name" -xf -

cd "/work/$name"
dotnet restore DotGram.slnx
dotnet build   DotGram.slnx --no-restore --configuration Linux -warnaserror
dotnet test    DotGram.slnx --no-build   --configuration Linux
```

The excludes are unanchored so that a name is dropped wherever it sits, and the script is
written with Unix line endings — a stray `` reaches the shell as part of a path.

A run is then one command, naming a tree under `/src`: `main`, or a worktree.

```
docker exec dotgram-linux ci worktrees/docs
```

`Linux` is a solution configuration of its own — everything at Release, minus the two
projects that need Visual Studio. Without it the build fails on those, which is not a
finding. The package cache is a volume, so only the first run pays for restore, and
`docker start dotgram-linux` brings the container back after a reboot.

## The snapshot baseline

`tests/Snapshots/*.gram.g.cs` are checked in beside the grammars they come from, and

```
git diff --stat -- tests/Snapshots examples/
```

is the standing check on any change that is meant to be structural. Empty means the
generated text is byte-for-byte what it was: the shape moved and the output did not. A
restructuring that changes the emitted text is a behaviour change wearing a disguise, and
belongs in its own commit with the diff read.

When a change is *meant* to alter the output, the snapshot test writes the new file and
fails once, saying so. Read the diff before committing it — that reading is the review,
and it is the only place the whole generated file is looked at.

Five grammars are covered: `Url` and `Feed` are the frozen subset and hand no C# across,
`Csv` carries a `=>`, a `when` and the `#line` directives of §7.6, `Minimal` is the
smallest thing that still recurses, and `Notation` is the notation's own grammar.

## Measuring

Benchmarks are a project of their own and are not run by CI — a number from a shared
runner is a number about the runner.

```
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --filter "*UrlBenchmarks*" --job short
```

`--job short` is enough to see a regression; the error bars are wide, so read the order
of magnitude rather than the second digit.

Nesting depth is bounded by the arena rather than by the machine's stack, so there is no
limit to walk up to: `CSharpEmitterTests` nests a rule inside itself a hundred thousand
times and the suite is where that claim lives. The `--depth` mode of the benchmarks runs
one parse in a child process and is what to reach for if a change is ever suspected of
putting grammar recursion back on the C# stack — a `StackOverflowException` cannot be
caught and takes the process with it, which is why it is a child.

What has already been measured, and what came of it, is in [`status.md`](status.md) under
*What has been measured*.

## Where a change goes

The layout and the file-format rules are in `CLAUDE.md`; the two seams that keep
`Grammar/` free of Roslyn are in `.claude/rules/grammar-half.md`, and what may be emitted
into a consumer's assembly is in `.claude/rules/emitted-code.md`. Those three are worth
reading once before the first change and not again.

## What a change owes

- A grammar-level feature owes a row in the [`status.md`](status.md) table, in the column
  it actually reaches — parsed, bound, normalized, emitted, run.
- A change to what the emitter writes owes a build of `tests/DotGram.Compatibility`. It
  runs no tests and asserts nothing; building it is the assertion, on the frameworks a
  consumer might be on rather than the one the generator is developed on. A member that
  stopped being emitted, or a language feature that started being, fails there rather than
  in somebody else's project. What each framework needs is written at the top of its
  project file — today, `System.Memory` on netstandard2.0 and net472, and nothing on
  net8.0.
- A refused construct owes a test that it is refused, and by which diagnostic. A construct
  that parses and then quietly means nothing is the failure this project is most careful
  about — and a row of `status.md` reading *refused* is that same claim, made in prose.
  `SemanticTests.Still_refused` holds those rows to it, so a feature built and never
  marked is caught by the suite rather than by somebody trusting the table.
- A refusal that is **lifted** owes the removal of its row, and nothing catches that one.
  `Still_refused` guards the table from one side only: it fails when a row says *refused*
  and the construct works. When a refusal stops existing, the test that asserted it is
  replaced by a test asserting the opposite — that is the natural way to make the change —
  and the row is left an orphan with nothing looking at it. It happened to the row for
  publishing a `SourceSpan`, which went on saying refused for as long as anybody read it.
- A rewrite that builds new nodes owes a thought about what was recorded against the old
  ones. `RecognitionGraph.Orphans()` answers that question and `GraphIntegrityTests` asks
  it of every grammar in the repository.
- An example owes assertions in `tests/DotGram.Tests/ExampleTests.cs`. Nothing under
  `examples/` may reference a test framework — an example that needs a fixture to make
  sense is not an example.
