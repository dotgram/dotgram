# Working on this

How the project is built, checked and measured. Standing process rather than plans —
[`next.md`](next.md) says what to do next, this says what to do every time.

## Build and test

```
dotnet build DotGram.slnx
dotnet test DotGram.slnx --no-build
```

`dotnet test` goes through Microsoft.Testing.Platform rather than VSTest, which xunit 4
requires on the .NET 10 SDK and `global.json` opts into. One project is the same command with
its path — `dotnet test tests/DotGram.Tests/DotGram.Tests.csproj` — and not `--project`,
which reports that zero tests ran. Every test project is also an executable, and the runner
it builds into can be started directly:
`tests/DotGram.Tests/bin/Debug/net10.0/DotGram.Tests.exe`,
`tests/DotGram.Sql.Tests/bin/Debug/net10.0/DotGram.Sql.Tests.exe`, and — on Windows only,
being net472 — `tests/DotGram.VisualStudio.Tests/bin/Debug/net472/DotGram.VisualStudio.Tests.exe`.
The two on net10.0 also run as `dotnet <path>.dll`. `-filter "/*/*/ClassName/MethodName"`
runs one test. `DotGram.Tests` runs everything in about two minutes.

What costs more than it is worth on every run lives in `tests/DotGram.Tests.Slow` (D12): the
whole refusal record — `DotGram.Tests` compiles one reading in five of it — the streaming
memory bounds, `GRAM5003`'s parts at every size and a split of nine hundred rules
(`OversizeTests`), and the scaling tests, which hold a parser to time proportional to its
input (`ExpressionScalingTests`: ten times the terms within fifteen times the time;
`SqlConditionScalingTests`, `StockCountScalingTests`). `dotnet test DotGram.slnx` and CI run it;
`dotnet test` over `DotGram.Tests` does not, and nothing is filtered to leave it out. Run it
before any change to how failures are recorded, before merging anything that touches
retention, and before one that touches how a walk reads what it has built.
The examples are compiled by the real generator during that build, so a member the
generator stopped producing fails the build rather than a test.

A build prints the generator's warnings and errors and not its information: `GRAM5009`, which
says a grammar cut into kinds reads something other than it is written, is information, and so is
every other diagnostic about what an author cannot see. `-v:detailed` prints them —
`dotnet build src/DotGram.Sql -t:Rebuild -v:detailed | grep GRAM` is the list for one project.

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
written with Unix line endings — a stray `
` reaches the shell as part of a path.

A run is then one command, naming a tree under `/src`: `main`, or a worktree as
`worktrees/<name>`. Name one every time: the default the script falls back to,
`worktrees/docs`, is not a worktree that exists.

```
docker exec dotgram-linux ci main
```

`Linux` is a solution configuration of its own — everything at Release, minus the two
projects that need Visual Studio. Without it the build fails on those, which is not a
finding. The package cache is a volume, so only the first run pays for restore, and
`docker start dotgram-linux` brings the container back after a reboot.

## The package, as a stranger gets it

Everything else builds the generator from source and takes it on trust that the package
is the same thing. `tests/DotGram.PackageSmoke` is the one that does not. It is a project
whose only connection to this repository is a version on a `PackageReference`, it carries
a `Directory.Build.props` of its own that is empty on purpose — so that none of the
repository's settings reach it — and it names the compiler it wants:

```
<RoslynCompilerType>Package</RoslynCompilerType>
<PackageReference Include="Microsoft.Net.Compilers.Toolset" Version="4.14.0" PrivateAssets="all" />
```

4.14 is the floor the README states, and that property is what makes the package be the
compiler rather than merely be restored: the SDK sets its own Roslyn paths after anything
a project or a package can say, under exactly that condition.

It is not in the solution, because it cannot be restored until the package it names
exists. CI runs it after the pack step; by hand it is

```
dotnet pack src/DotGram/DotGram.csproj --configuration Release --output artifacts
dotnet run --project tests/DotGram.PackageSmoke/DotGram.PackageSmoke.csproj --configuration Release
```

and it prints `1 + 41 = 42`. Locally the second run may serve the first run's package out
of the global cache, so clear `dotgram/<version>` from it when the change under test is to
the package rather than to the grammar.

What it catches is what nothing else can: an analyzer that will not load under the floor,
an emitted file that only compiles because of a setting this repository happens to have,
and an analyzer folder the compiler does not look in.

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

### Comparing the whole of a change's emission

Where a change reaches further than the snapshots — anything in the emitters — the check is
every generated file of every project, before and against after. Build with
`-p:EmitCompilerGeneratedFiles=true` and compare
`obj/GeneratedFiles/DotGram/DotGram.Generation.GramGenerator/` between a worktree at the
commit being changed and this one, after replacing each worktree's path with a constant:
`#line` directives carry the grammar's absolute path, so nothing travels between worktrees
until that is normalized.

Two things make a difference appear that is not one:

- **Build both sides with `-t:Rebuild`.** An incremental build does not rewrite
  `*.DotGramReport.g.cs`, so those files look as though the change stopped emitting them.
- **Ignore the report files' contents.** They carry the generation time in milliseconds,
  which differs between any two runs.

### Verifying an emitter change

Three things have each let a broken change look finished, and the check below is built
against all three.

- **The parent moves.** A snapshot taken on an older `main` charges another session's change
  to yours, or hides yours under it. Take the "before" on the commit your change lands on —
  after the rebase, in a worktree of its own — and compare each commit of a series with its
  immediate parent.
- **A test run can outlive a failed build.** When a project fails to build, its previous
  assembly is still on disk and the tests run against it and pass. Stop at the first build
  that does not succeed, before any test is read.
- **A warning in emitted code is an error at the consumer.** Code the generator writes is
  compiled in someone else's project, often with warnings as errors. `DotGram.Compatibility`
  builds that way (it is what caught a field never assigned in one of two emitted classes);
  count the CS warnings of every build and treat any as a failure of the change.

The scripts live in `.work/gencheck/` (ignored by git) and are recreated from here. `snap.sh`
rebuilds every project that hosts a grammar and records a path-normalized hash of each
emitted file; `cmp.sh` compares two snapshots; `verify.sh` runs a snapshot and then every
build a result depends on, stopping at the first that fails.

`snap.sh`:

```bash
#!/usr/bin/env bash
# snap.sh <name>: rebuild every project that hosts a grammar and record a sha256 of each emitted
# .g.cs (generation reports excluded: they carry timings). Compare two with cmp.sh.
set -e
cd "$(dirname "$0")/../.."
name="$1"
out=".work/gencheck/$name"
rm -rf "$out"; mkdir -p "$out"
dotnet build src/DotGram/DotGram.csproj -c Release -p:UseSharedCompilation=false -m:1 -nr:false -v:q 2>&1 | grep -E " error |Build succeeded" | head -2
for p in src/DotGram.Sql/DotGram.Sql.csproj src/DotGram.ExpressionLanguage/DotGram.ExpressionLanguage.csproj \
         src/DotGram.Web/DotGram.Web.csproj src/DotGram.Finance/DotGram.Finance.csproj \
         examples/DotGram.Examples/DotGram.Examples.csproj; do
	dotnet build "$p" -c Release -t:Rebuild -p:UseSharedCompilation=false -m:1 -nr:false -p:BuildProjectReferences=false -v:q 2>&1 \
		| grep -E " error |Build succeeded" | head -2 | sed "s#^#  $(basename $p .csproj): #"
done
root_win="$(pwd -W | sed 's#/#\\#g')"
for d in src/DotGram.Sql src/DotGram.ExpressionLanguage src/DotGram.Web src/DotGram.Finance examples/DotGram.Examples; do
	find "$d/obj/GeneratedFiles" -name '*.g.cs' ! -name '*DotGramReport*' 2>/dev/null | while read f; do
		rel="${f#./}"
		mkdir -p "$out/$(dirname "$rel")"
		# #line directives carry the worktree's absolute path; normalize it away.
		python -c "import sys; d=open(sys.argv[1],'rb').read(); r=sys.argv[3].encode(); open(sys.argv[2],'wb').write(d.replace(r, b'<ROOT>'))" "$f" "$out/$rel" "$root_win"
	done
done
(cd "$out" && find . -name '*.g.cs' | sort | xargs sha256sum) > "$out.sha256"
echo "$(wc -l < "$out.sha256") files recorded in $out.sha256"
```

`cmp.sh`:

```bash
#!/usr/bin/env bash
# cmp.sh <before> <after>: which emitted files differ, which appeared, which went away.
cd "$(dirname "$0")"
join -j 2 -a1 -a2 -e MISSING -o 0,1.1,2.1 <(sort -k2 "$1.sha256") <(sort -k2 "$2.sha256") \
	| awk '{ s = ($2 == $3) ? "same   " : ($2 == "MISSING" ? "added  " : ($3 == "MISSING" ? "removed" : "DIFF   ")); print s, $1 }' \
	| sort | awk '{c[$1]++} $1 != "same" {print} END {for (k in c) print "  total", k, c[k]}'
```

`verify.sh`:

```bash
#!/usr/bin/env bash
# verify.sh <snapshot-name> <parent-snapshot>: emitted-code compare, then every build that must
# succeed before a test result means anything, stopping at the first that does not.
cd "$(dirname "$0")/../.."
name="$1"; parent="$2"
build() {
	local out; out=$(dotnet build "$1" -c Release -p:UseSharedCompilation=false -m:1 -nr:false 2>&1)
	if ! grep -q "Build succeeded" <<<"$out" || grep -qE " error " <<<"$out"; then
		echo "BUILD FAILED: $1"; grep -E " (error|warning) " <<<"$out" | sort -u | head -15; exit 1
	fi
	local w; w=$(grep -cE "warning CS" <<<"$out"); echo "built $(basename "$1" .csproj) (CS warnings: $w)"
}
.work/gencheck/snap.sh "$name" 2>&1 | grep -E " error |recorded" | head -5
.work/gencheck/cmp.sh "$parent" "$name" | tail -12
build tests/DotGram.Tests/DotGram.Tests.csproj
dotnet tests/DotGram.Tests/bin/Release/net10.0/DotGram.Tests.dll 2>&1 | grep -E "Total:|\[FAIL\]" | head -15
build tests/DotGram.Finance.Tests/DotGram.Finance.Tests.csproj
dotnet tests/DotGram.Finance.Tests/bin/Release/net10.0/DotGram.Finance.Tests.dll 2>&1 | grep -E "Total:|\[FAIL\]" | head -8
build tests/DotGram.Compatibility/DotGram.Compatibility.csproj
build benchmarks/DotGram.Finance.Benchmarks/DotGram.Finance.Benchmarks.csproj
build benchmarks/DotGram.Benchmarks/DotGram.Benchmarks.csproj
echo "ALL BUILDS SUCCEEDED"
```

A series of two commits is checked as: a worktree at the new parent, `snap.sh parent`;
check out the first commit there, `snap.sh first` and `cmp.sh parent first`; then, at the
second commit in your own tree, `verify.sh second first`.

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

The instrument for a question of the form "is this faster, and than what" is **the stand**
(`benchmarks/DotGram.Benchmarks/Stand*.cs`), and it is not run like the modes above:

- `--stand [dir] [--only a,b] [--repeat N]` times every generated parser against its
  hand-written one — FIX as a string, bytes and a stream, the web's formats, the expression
  language on the tape and on the immediate carrier, SQL:2023, T-SQL against ScriptDom, the stock
  count — with a regular expression beside it where one can be written honestly, the allocation
  of a call, and the first call in a fresh process. `--repeat 5` takes five processes and reports
  medians; a run whose control (a row of plain arithmetic timed in every round) is more than 5%
  off the others is dropped.
- `--stand-paired beforeDir afterDir` reads two builds in one process, round-robin, each loaded
  from its own directory of DLLs: the only way a change to what the generator emits is measured,
  since it holds a commit against its parent under one runtime and one profile. The base of a
  row (a hand-written parser, ScriptDom, or this process's own build of the same parser, named `hand`, `scriptdom` or `control`) is the process's own and is a constant, never a party to the pair. `--stand-paired-check`
  holds the rows of a pair to what they say and times nothing; `--stand-held` and
  `--stand-held-whole` read what a stream form holds while it is read.
- `linearity` times a parser at three sizes ten times apart and flags an exponent above 1.2, and
  says whether it is the algorithm or the collector; run it when a change touches how a walk or
  a carrier reads what it has built.
- `linearity-refused` times a refusal after a growing head at sizes a quarter apart, under a budget, and flags an exponent above 1.10: the two shipped exponential refusals (D57) were visible to no accepted ladder. Run it when a grammar gains a repetition inside a repetition, or a rule whose first sets overlap.
- `benchmarks/Gate-Generation.ps1 -Base <commit> -Head <commit>` holds the time the generator
  takes to write the grammars of the solution to a base commit's, the two rebuilt alternately in
  the same run.

**A timing is taken in an announced window**, on logical processors 0-15 at high priority, with
nothing else timing; builds and tests of others run meanwhile only if they are pinned to 16-31. **Two timed loads at
once are one disturbed measurement, whichever halves of the machine they sit on**: the generation gate times the
generator on 16-31, and a stand window on 0-15 beside it shares the caches, the memory and the boost, so neither can see
through the other afterwards (2026-09-19: a window ran through a gate, the gate's `TransactSqlParser.Located` came back
+23%, and it was rerun in a slot with nothing else timing). A timed load asks for the machine, not for cores.
A before and an after are medians of five runs or more, never one run, and a lean on a row whose
code did not change is read alone and with `DOTNET_TieredPGO=0` before it is believed. The rows,
the medians, the paired stand's rules and what was learned of each are in
[`benchmarks/README.md`](../benchmarks/README.md), and every measurement of a day is filed with its
raw data under `benchmarks/results/`.

The other modes — `--against`, `--hand`, `--speed`, `--roundtrip` and the rest — are in
[`benchmarks/README.md`](../benchmarks/README.md), with what each was built to ask and what
it answered. Profiling the generator itself rather than what it generates, over
`DotGram.Sql`, is `.claude/rules/profiling.md`: the time in the build, a harness a profiler
can start, and the check that a change to speed changed no output.

What has already been measured, and what came of it, is in [`status.md`](status.md) under
*What has been measured*.

## Where a change goes

The layout and the file-format rules are in `CLAUDE.md`; the two seams that keep
`Grammar/` free of Roslyn are in `.claude/rules/grammar-half.md`, and what may be emitted
into a consumer's assembly is in `.claude/rules/emitted-code.md`. Those three are worth
reading once before the first change and not again. The fourth, `.claude/rules/profiling.md`,
is worth reading before measuring the generator.

## What a change owes

- A grammar-level feature owes a row in the [`status.md`](status.md) table, in the column
  it actually reaches — parsed, bound, normalized, emitted, run.
- A change to what the emitter writes owes a build of `tests/DotGram.Compatibility`. It
  runs no tests and asserts nothing; building it is the assertion, on the frameworks a
  consumer might be on rather than the one the generator is developed on, and at C# 8, the
  language version the emitted code declares as its floor. A member that stopped being
  emitted, or a language feature that started being, fails there rather than in somebody
  else's project. What each framework needs is written at the top of its
  project file — today, `System.Memory` on netstandard2.0 and net472, and nothing on
  net8.0.
- A change to what the emitter writes owes its pair: `--stand-paired` of the commit against its
  parent, over the rows of every family the emitted code changes in (a change that leaves the
  output of the solution byte for byte the same owes none, only the gate below), and the
  `linearity` family if it touches how a walk reads. A change that no pair reads is invisible to
  every other pair: a commit once made the expression language's tape quadratic in the terms of a
  list, and nothing saw it for a day because no row was longer than twenty terms.
- A change to the generator — to its analysis or to what it writes — owes the generation gate:
  `benchmarks/Gate-Generation.ps1 -Base <parent> -Head <commit>`. The time of the generator is
  measured on the grammars of the solution, and an analysis that goes over every rule once more
  has taken T-SQL from 4 to 86 seconds.
- A change that means to take a grammar off the tape owes a run of `--carriers` before and after:
  the difference in `docs/carriers.md` is the claim, and a grammar that is still there says why.
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

## Large generated source files

File splitting is experimental and disabled by default, including in the Roslyn
source generator. To opt in through `GramCompiler.Compile`, set
`GramCompilerOptions.SourceFileSize` to a positive character threshold, for example
`2_000_000`. Complete engine methods and direct-reader groups at least that large
move into additional partial-class files. This is a group threshold, not a maximum
file size: an individual method or nested reader type stays whole.

The main file retains host fields and their initialization order, shared types,
and declaration attributes. A reader's fields and constructors move together with
its complete nested type. Parser entry points and method bodies are unchanged.
This controls source layout independently of `PartSize`, which controls method
subdivision for the JIT. Extra files use stable `.part-0001.g.cs` names.

When opting in, compile **all** entries in `GramCompilation.Sources`.
`SourceFileSize = 0` retains one file and is the default. Direct callers of
`CSharpEmitter.Emit` must supply both a positive `sourceFileSize` and a
`sourceParts` collection to receive additional files.

The [measurements](design/file-split-2026-09-16.md) did not establish enough benefit
to enable splitting by default: FIX improved modestly with higher memory use,
and SQL showed no meaningful time improvement. Reconsider automatic splitting only
after a repeatable benefit in actual solution builds has been demonstrated.

## Shared publication machines

Large direct readers with at least 90% rule overlap may share a machine when each
already needs deferred value construction. Small and streamed publications remain
separate. This reduces duplicate generated methods independently of source-file
splitting. See the [implementation and measurements](design/sibling-publications-2026-09-17.md).
