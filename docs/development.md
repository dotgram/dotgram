# Working on this

How the project is built, checked and measured. Standing process rather than plans —
[`next.md`](next.md) says what to do next, this says what to do every time.

## Build and test

```
dotnet build DotGram.slnx
tests/DotGram.Tests/bin/Debug/net10.0/DotGram.Tests.exe
```

The test runner takes no filter arguments; it runs everything, in about eight seconds.
The examples are compiled by the real generator during that build, so a member the
generator stopped producing fails the build rather than a test.

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

Three grammars are covered: `Url` and `Feed` are the frozen subset and hand no C# across,
`Csv` carries a `=>`, a `when` and the `#line` directives of §7.6.

## Measuring

Benchmarks are a project of their own and are not run by CI — a number from a shared
runner is a number about the runner.

```
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --filter "*UrlBenchmarks*" --job short
```

`--job short` is enough to see a regression; the error bars are wide, so read the order
of magnitude rather than the second digit.

Nesting depth is measured by a child process, because a `StackOverflowException` cannot
be caught and takes the process with it:

```
benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.exe --depth 4562
```

Walk the number up until a run exits non-zero; the last depth that printed `ok` is the
answer. What has already been measured, and what came of it, is in
[`status.md`](status.md) under *What has been measured*.

## Where a change goes

The layout and the file-format rules are in `CLAUDE.md`; the two seams that keep
`Grammar/` free of Roslyn are in `.claude/rules/grammar-half.md`, and what may be emitted
into a consumer's assembly is in `.claude/rules/emitted-code.md`. Those three are worth
reading once before the first change and not again.

## What a change owes

- A grammar-level feature owes a row in the [`status.md`](status.md) table, in the column
  it actually reaches — parsed, bound, normalized, emitted, run.
- A refused construct owes a test that it is refused, and by which diagnostic. A construct
  that parses and then quietly means nothing is the failure this project is most careful
  about — and a row of `status.md` reading *refused* is that same claim, made in prose.
  `SemanticTests.Still_refused` holds those rows to it, so a feature built and never
  marked is caught by the suite rather than by somebody trusting the table.
- A rewrite that builds new nodes owes a thought about what was recorded against the old
  ones. `RecognitionGraph.Orphans()` answers that question and `GraphIntegrityTests` asks
  it of every grammar in the repository.
- An example owes assertions in `tests/DotGram.Tests/ExampleTests.cs`. Nothing under
  `examples/` may reference a test framework — an example that needs a fixture to make
  sense is not an example.
