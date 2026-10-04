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

On the Linux machine the solution builds with `-c Linux` (the solution configuration that leaves
out the Visual Studio extension). A build or test run started by hand or by a script is best run
through `benchmarks/Aside.sh` (`benchmarks/Aside.sh dotnet build DotGram.slnx -c Linux`), which
waits while the stand is timing and keeps a timing from starting while it runs (D147, *Measuring*
below).

What costs more than it is worth on every run lives in `tests/DotGram.Tests.Slow` (D12): the
whole refusal record — `DotGram.Tests` compiles one reading in five of it — the streaming
memory bounds, `GRAM5003`'s parts at every size and a split of nine hundred rules
(`OversizeTests`), the scaling tests, which hold a parser to time proportional to its
input (`ExpressionScalingTests`: ten times the terms within fifteen times the time;
`SqlConditionScalingTests`, `StockCountScalingTests`), and the round trip over somebody else's
SQL — the 1,086 files of `tests/Corpus/ScriptDom` read by T-SQL, written back and put to
ScriptDom again, held per file to `CorpusBaseline.txt` (`CorpusRoundTripTests`, three seconds).
`dotnet test DotGram.slnx` and CI run it;
`dotnet test` over `DotGram.Tests` does not, and nothing is filtered to leave it out. Run it
before any change to how failures are recorded, before merging anything that touches
retention, before one that touches how a walk reads what it has built, and before one that
changes what T-SQL reads or what `SqlWriter` prints.
The examples are compiled by the real generator during that build, so a member the
generator stopped producing fails the build rather than a test.

A trace build (syntax.md §6.9) is held to what it observes by `tests/DotGram.Trace.Tests`, over the
shipped grammars themselves: `tests/DotGram.Traced` compiles the sources of DotGram.Sql,
DotGram.ExpressionLanguage and DotGram.Web again with `DotGramTrace`, and
`tests/DotGram.Traced.Unfolded` the same with `DOTGRAM_NO_COLLAPSE`, which calls a rule that only
forwards another's value as written instead of collapsing it into its callers. The tests reference
the three builds under the aliases `plain`, `traced` and `unfolded` and read refused corpora of
T-SQL (mutations of the ScriptDom batches, by a fixed seed), the expression language (the refused
lines of its refusal record), JSON and URI: the trace build answers every input as the library does,
`GramWhy`'s message and position are the match's on every row and every row is explained, every rule
on the unfolded build's stacks is on the trace build's (the frames of the collapsed rules), and every
rule entered is left. `TraceCostTests` holds the memory of a deep refusal (`tests/DotGram.Trace.Deep`,
a process of its own: peak working set under 256 MB at fifty thousand levels) and the time of a
profile on T-SQL (within ten times the library) and of `GramWhy` on a refused script of 100 KB
(under a second). The two copies cost the solution build two more compilations of the SQL grammars,
for net10.0 only. `tests/DotGram.Tests/TraceTests.cs` holds the same over small grammars, a reading
shape at a time.

The engine's own step trace — one line on standard error per step of the automaton, for working on
the generator, not for grammar authors — is compiled where the build defines `DOTGRAM_STEPS`. It was
`DOTGRAM_TRACE` until the trace build came; a consumer that defined `DOTGRAM_TRACE` gets no step lines
from a parser generated since and has to define `DOTGRAM_STEPS` instead (the release notes say so).
The rename keeps the symbol a `DotGramTrace` user would guess from flooding standard error.

A build prints the generator's warnings and errors and not its information: `GRAM5009`, which
says a grammar cut into kinds reads something other than it is written, is information, and so is
every other diagnostic about what an author cannot see. `-v:detailed` prints them —
`dotnet build src/DotGram.Sql -t:Rebuild -v:detailed | grep GRAM` is the list for one project.

`-p:DotGramPositionalFollow=true` builds every grammar with the experimental switch of syntax.md
§6.8, under which a `parse` is compiled knowing it is also read from a position. Under it the FIX
grammar, which pins `Carrier = Immediate`, reports `GRAM5015`, and `TreatWarningsAsErrors` turns
that into a failed build: add `-p:WarningsNotAsErrors=NU1900%3BGRAM5015` (the `%3B` is the
separator; `NU1900` is the one `Directory.Build.props` already lists).
`-p:DotGramPositionalFollow=split` is the same switch, experimental and off by default like it, with
the positional end told apart where that changes no answer (syntax.md §6.8); FIX keeps its carrier
and reports nothing under it. `PositionalSplitWebTests` (DotGram.Tests.Slow) holds `split` to `true`
on the shipped web grammars, in both renderings.

The generator keeps the parsers it compiled in a static cache for as long as the compiler server
that loaded it lives (syntax.md §6.8, `DotGramNoCache`), so a second build after an edit to C#
alone reports `cached` where the generation time was. Two consequences for working here. A
measurement of the generator's own time must not take a kept parser: the stand and
`Gate-Generation.ps1` build with `-p:UseSharedCompilation=false`, a compiler process per
compilation, which keeps nothing; anything else that times generation sets `-p:DotGramNoCache=true`.
And the cache is found by every input of the compile, which is a claim a test can only sample:
`-p:DotGramVerifyCache=true`, a property only this repository declares (`Directory.Build.targets`),
compiles afresh every grammar it finds in the cache, holds the two parsers to each other and fails
the build with `GRAM0001` where they differ; a report line reads `cached and verified` where it
took one. The `checked` job in CI builds the solution with it — which hits only where two
compilations happen to share every input — and then rebuilds `DotGram.Sql` after touching a C#
file, once for both target frameworks and once for each on its own, and requires each of its three
parsers to be cached and verified for each framework.

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

## The library packages

`tests/DotGram.Sql.PackageSmoke`, `DotGram.Web.PackageSmoke`,
`DotGram.ExpressionLanguage.PackageSmoke` and `DotGram.Finance.PackageSmoke` do the same for
the four parser libraries: each is a consumer of its own package and nothing else, with empty
`Directory.Build.props` and `Directory.Packages.props` of its own, and like the generator's
they are outside the solution. They exercise SQL's three dialects and both writers, JSON
Pointer and Patch over JSON values, compiled expression delegates and a plugin whose
references are found in its own load context, and a typed FIX heartbeat with its schema.

Each runs on .NET 8, which consumes the package's `netstandard2.0` asset, and on .NET 10,
which consumes its `net10.0` one, so both runtimes must be installed; CI installs .NET 8
beside the SDK `global.json` selects. The version floats, and a `nuget.config` beside each
project maps every DotGram package to `artifacts` and everything else to nuget.org, so a check
with nothing packed fails at restore rather than passing on a published package. After a
Release build and a pack of the libraries into `artifacts`, CI runs them with a package cache
of their own, which is also what keeps a stale package of the same version out locally. By
hand:

```
dotnet build DotGram.slnx -c Linux
for library in Sql Web ExpressionLanguage Finance; do
    dotnet pack src/DotGram.$library/DotGram.$library.csproj --no-build --configuration Release --output artifacts
done
export NUGET_PACKAGES=$(mktemp -d)
for library in Sql Web ExpressionLanguage Finance; do
    for framework in net8.0 net10.0; do
        dotnet run --project tests/DotGram.$library.PackageSmoke/DotGram.$library.PackageSmoke.csproj \
            --configuration Release --framework $framework
    done
done
```

### The public surface

`DotGram.Finance` holds its public surface to text: every public type and member is a line of
`src/DotGram.Finance/PublicAPI.Shipped.txt` or `PublicAPI.Unshipped.txt`, and
Microsoft.CodeAnalysis.PublicApiAnalyzers compares the two with the assembly on every build. A
member added without its line is RS0016 and a line whose member is gone is RS0017, both errors here,
so the surface cannot move without the diff showing it. Shipped is what a published version has.
A change between releases goes to Unshipped — a new member as its line, a removed one as its
shipped line prefixed `*REMOVED*` — and at a release Unshipped is emptied into Shipped, a
`*REMOVED*` line taking its shipped line with it; every file keeps `#nullable enable` as its first
line and the rest sorted. The two frameworks agree but for a record's clone, which returns its own
type on net10.0 and the base type on netstandard2.0, having no covariant returns: what only one
framework has is in `PublicAPI/<framework>/`, beside the shared files, and read by that
framework's build alone. The generated FIX versions are most of the surface, so a change to
`generate.cs` or a template that changes a version's members changes these files too.

The lines are written by the analyzer, not by hand. The editor's fix for RS0016 adds one, and a
build that lets the analyzer warn rather than fail prints every missing one, each RS0016 naming
the line it wants — which is how the 0.2.0 surface, some forty thousand lines, was recorded.
Build each framework with `-f` and compare: a line only one of them reports goes in its folder.

```
dotnet build src/DotGram.Finance/DotGram.Finance.csproj -c Release -f net10.0 --no-incremental \
    -p:TreatWarningsAsErrors=false \
    | grep -oP "RS0016: Symbol '\K.*(?=' is not part of the declared public API)" | sort -u
```

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

## Analysing the generator's own output

Roslyn's own style analyzers (`IDE0059` unnecessary value assignment, `IDE0051`/`IDE0052`
unused private member/value, `IDE0060` unused parameter, `IDE0035` unreachable code, `IDE0004`
unnecessary cast) skip generated trees by design — a source generator's output is exactly
that. Nothing catches a dead method or a redundant local the emitter writes, and Roslyn
compiles every line it writes regardless of whether anything ever calls it: generated size is
first-class, so this is checked permanently rather than on request. The compiler's own
`CS0168`/`CS0219`/`CS0162` are NOT generated-code-specific — they fire on generated code
exactly as they do anywhere else — so an ordinary `-warnaserror` build already enforces them
with no separate harness; they are not part of what follows.

It runs in two parts, both behind one opt-in property, **`DotGramAnalyzeGenerated`** — off
everywhere by default (an ordinary build and the IDE never pay for it), on only in the one
required CI job (`generated-analysis` in `.github/workflows/build.yml`), which turns it on by
rebuilding the five packages it emits from with it set:

```
dotnet build DotGram.slnx -c Linux -warnaserror -p:DotGramAnalyzeGenerated=true
```

**Why not always on.** A full, clean solution build measures within noise of itself with the
property on or off — the property adds no measurable time, since our own analyzer is one
extra walk Roslyn was already going to make over code it was already going to compile. The
stock-rule half is the opposite: `tools/generated-analysis/run-all.sh`, which rebuilds the five
packages (with the property on, folding in the check above) and then compiles each one's
throwaway `Analysis.csproj`, measures a little under seven minutes end to end on the machine
this was built on — too slow to ask of every local build or the IDE, which is exactly why it
runs as the one required CI job rather than under the same property gate as our own analyzer.

- **Our own rules**, `tools/DotGram.GeneratedAnalyzers` — never packed, referenced by the four
  shipped packages and the examples only under the property. One analyzer, one walk per method
  body, rather than one pass per rule:
  - **DGA001**, error: a LOCAL written and, on the very next line, copied into another local
    that is never read again anywhere ordinary control flow can still reach from there
    (`last0 = Construct_X(...); fold = last0;` where nothing reads `last0` afterward). The
    source is confirmed through the semantic model to be a true local, never a field — a
    reader's own register field is written this same shape on purpose, to end the turn current
    for a caller that reads it later, which a check scoped to one method body cannot see.
    Suppressible only with a reason: a bare `#pragma warning disable DGA001` fails a second
    rule, `DGA001R`, which a trailing `// reason` comment on the same line satisfies.
  - **DGA003**, info: a `switch` or `if`-chain over one variable with more than 32 constant
    arms, each returning or assigning a constant. A judgement about data-encoding, not a
    defect; never enforced.
  - **DGA004**, info, report-only: a likely allocation (a closure or delegate, an array
    allocation, or a LINQ call) in a `Read_`/`Recognize_`/`Scan_`/`Materialize_`/`Construct_`-
    prefixed method or the buffered `Ensure` path. Construction of a rule's own result value is
    excluded on purpose — that allocation is the parser's job, not overhead.

  DGA001, DGA003 and DGA004 are all silent for a finding whose location is mapped by a `#line`
  directive into the grammar author's own file (a `=>`, a `when`, a switch selector): that is
  their C# to answer for, not the emitter's.

- **The stock rules above**, which cannot be turned on for generated code by editing
  `.editorconfig` in the ordinary way — `generated_code = false` and stripping the
  `<auto-generated>` header are the documented override, but neither reaches a tree a live
  source generator produced in the same build (measured: several `.globalconfig` forms
  matching the generator's own hint path were all rejected by the compiler as not an absolute
  path, on this SDK). What does work is analysing the emitted **text** on its own, outside the
  generator: `tools/generated-analysis/emit.sh` rebuilds each shipped package and the examples
  with `EmitCompilerGeneratedFiles=true`; `prepare-analysis.sh` copies the emitted `.g.cs` that
  belong to that package into a throwaway `Analysis.csproj` (from `templates/`), header-stripped,
  beside the package's own hand-written sources compiled in place, with `EnforceCodeStyleInBuild`
  and the six rules above turned on as warnings in the throwaway project's own
  `.editorconfig`; `analyze.sh` builds it for a SARIF errorlog. That override's own glob
  matters: `[gen/*.g.cs]`, one directory level, not `[gen/**/*.g.cs]` — EditorConfig's `**`
  needs an intervening path segment and never matches a file directly inside `gen/`, so the
  double-star form silently applies to nothing and every one of these rules keeps skipping the
  copied files as generated regardless. That single character is why an early measurement of
  this said zero findings across every shipped package; the real count, once the glob matched,
  was in the thousands (below). `run-all.sh` runs all three scripts for all five packages in
  order (Finance's own extra reference on `DotGram.ExpressionLanguage` included), so a local
  run and the CI job measure identically.

  `tools/generated-analysis/SarifGate` then reads the SARIF. Before trusting any count, it
  checks every result for `"level":"error"` first: a compile error in the throwaway
  `Analysis.csproj` would otherwise silently compile less code and report FEWER findings,
  which would read as an improvement to the ratchet below rather than the broken run it
  actually is. A finding counts toward its package's total only when it is under the analysis
  project's own `gen/` directory — `prepare-analysis.sh` never copies anything else there, so a
  result anywhere else is either the grammar author's own `#line`-mapped file (whose SARIF
  location is already that file; there is no further mapping to do) or the package's
  hand-written source compiled alongside the copy for symbol resolution, and either way it is
  not the emitter's text to gate — and not already suppressed in source (a `#pragma warning
  disable` the compiler itself already keeps from failing anyone's build, such as the
  suppress/restore fence `Support.cs`'s `Suppressed` writes around a `Failure` field nothing
  assigns for a given grammar). The job measures all five (the four shipped packages and the
  examples); the rules do not all start from zero, so rather than a fixed enforced/report-only
  split this is a **count ratchet**: a checked-in ceiling per (package, rule) pair,
  `tools/generated-analysis/counts.txt`, that `SarifGate check` compares the fresh measurement
  against, failing on ANY difference, in either direction. An increase is the regression the
  job exists to catch; an unrecorded decrease would only leave the ceiling looser than reality,
  so that fails too — `SarifGate update <counts-file> <sarif...>` rewrites the file from a
  fresh measurement, and refuses (without `--allow-increase`) to write a number that goes up.
  Measured on today's output: `IDE0035` is zero everywhere, the same hard-zero standing a fixed
  "enforced" list gave it before; `IDE0059`, `IDE0060`, `IDE0051`, `IDE0052` and `IDE0004` start
  from a real, substantial nonzero count (thousands for `DotGram.Sql` alone, dominated by
  `IDE0059` and `IDE0060`) that a separate remediation has not caught up with yet, so those are
  ratcheted from where they stand rather than reported without limit — the current ceiling for
  every (package, rule) pair is `counts.txt` itself, not duplicated here, since the numbers
  move as a remediation proceeds while this file does not track day-to-day counts.

  A companion check, `tools/generated-analysis/pragma-inventory.sh`, holds the emitter's own
  `#pragma warning disable` sites (never the grammar author's `#line`-mapped ones) to a
  checked-in inventory, `pragma-inventory.txt`, the same way: `check` fails on a pragma the
  file does not already list (or one the file lists that nothing emits any more); `scan` prints
  the current one to update it from.

**The five known dead copies.** The immediate carrier's own fold step
(`Machine.Immediate.cs`'s `End`, `Machine.Carrier.cs`'s `Accumulated`) wrote a step's value
into the type's shared "last" register and then, on the next line, copied that same register
into `fold` — two statements for one value. The register still has to end the turn current
(something else may read it), so the fix is not to skip the write, only the second statement:
`Machine.Reader.cs`'s `EmitRecord` now asks whether the two texts have exactly that shape and,
where they do, chains them into one assignment, `fold = last0 = Construct_X(...);` — the same
two writes, one statement, nothing left for the stock rules to find (DGA001 never flagged this
particular shape: `last0` here is a reader's own register field, not a local, which DGA001's
semantic-model check now confirms explicitly). A carrier whose `Accumulated` says something
else (the tape, reading a log) is untouched.

**A stale `CS0649` question.** `Failure.OutOfInput`, `Failure.Looking` and `Held<T>.Value`'s
`CS0649` already carry the suppress/restore pragma fence (`Support.cs`'s `Suppressed`, and an
unconditional one around `Held<T>` itself) in every instance sampled across `DotGram.Sql`,
`DotGram.Web` and `DotGram.Finance` — a warning a `#pragma` silences does not fail
`-warnaserror` today, whatever a SARIF listing of it (suppressed or not) suggests. Not a defect
to fix; a measurement that needed the SARIF's own `suppressions` field read before being
believed.

## Measuring

Benchmarks are a project of their own and are not run by CI — a number from a shared
runner is a number about the runner.

```
benchmarks/Aside.sh dotnet build benchmarks/DotGram.Benchmarks/DotGram.Benchmarks.csproj -c Release
pwsh benchmarks/Run-Bdn.ps1 -Assembly benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll -Label url -BdnArgs '--filter','*UrlBenchmarks*','--job','short'
```

`--job short` is enough to see a regression; the error bars are wide, so read the order
of magnitude rather than the second digit. `Run-Bdn.ps1` takes the window described below.

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

**A timing is taken in a window, and nothing heavy runs beside it** (D147). Builds and tests started
by hand or by a script are best run through `benchmarks/Aside.sh`:

```console
benchmarks/Aside.sh dotnet build DotGram.slnx -c Linux
benchmarks/Aside.sh dotnet test tests/DotGram.Tests/DotGram.Tests.csproj -c Release
pwsh benchmarks/Window.ps1
```

A window is a lock: the process that times holds `/ramdisk/locks/timing-window.lock` exclusively
for as long as it times, and the kernel lets it go when that process ends, however it ends;
`timing-window.txt` beside it names the holder, and `pwsh benchmarks/Window.ps1` reads both in
one line that begins with a word (IDLE, STALE, WITHIN, BUSY). `Aside.sh` holds
`timing-builds.lock` shared while its command runs and waits while a window is open; a window
waits for those commands to end (at most twenty minutes, naming them), then waits as long again for
the machine to be quiet and refuses one that is not, which is the safety net for a heavy process
that did not go through `Aside.sh`. **While a window is open nothing heavy runs, on either half of the
machine**, and the supervisor starts no heavy work during one: the halves share the package's
power budget and the memory controller, and a build pinned to the other half has doubled the
control before (2026-09-27). Timings run on logical processors 0-7 and 16-23, the first CCD with
its SMT siblings, set by `taskset` as the process starts; the windows are taken by
`benchmarks/Run-Announced.ps1` (the stand, and any command), `Run-Bdn.ps1` and `Run-BdnQueue.ps1`
(BenchmarkDotNet, with every worker's processors read back) and `Gate-Generation.ps1`; a stand
run started by hand takes one itself, but is not pinned, says so in its header and is not quoted. Output goes to `/ramdisk/build/dotgram`, never the SSD; what is
worth keeping is copied into `benchmarks/results/`. **Two timed loads at once are one disturbed
measurement, whichever halves of the machine they sit on**; the window lock admits one at a time.
Timings are for trends; a decision rests on counts (allocations, steps), which do not need a window.
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

## SQL engine checks on Linux

`--engine` (`Engine.cs`) asks a real SQL Server whether a statement is syntax, which is why
it needs one on the machine — see "Measuring" above and `benchmarks/README.md` for what it
answers. `--roundtrip` (`RoundTrip.cs`/`CorpusRoundTrip.cs`) does not: it holds ScriptDom's
own printer against itself and opens no connection, so it runs the same everywhere and needs
nothing here.

`Engine.cs` used to hard-code `Server=localhost;…Integrated Security=true`, which is a
Windows-only connection (a local instance, trusted). It now reads the **base** connection
string — everything except the database — from the environment variable
**`DOTGRAM_SQLSERVER`**, and falls back to that same Windows default when the variable is
unset, so nothing changes there. The database is never string-built onto it: the base is
parsed with `SqlConnectionStringBuilder` and only `InitialCatalog` is set to
`DotGram_{version}`, so a password holding a character concatenation would trip over survives
intact.

On Linux, point it at a SQL Server 2025 container and set the login in the variable, e.g.:

```console
export DOTGRAM_SQLSERVER='Server=localhost,14330;User Id=sa;Password=<its password>;TrustServerCertificate=True'
```

`--engine` needs one database per compatibility level it can ask about, made once and kept:
`DotGram_100` through `DotGram_170` (100 is the lowest a 2025 server accepts; 80 and 90 have
no engine to ask — see `Engine.Connected`'s remarks). Create them with `sqlcmd` against the
container, compatibility level matching the name:

```console
docker exec <container> /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '<password>' -C -Q "
  CREATE DATABASE DotGram_100; ALTER DATABASE DotGram_100 SET COMPATIBILITY_LEVEL = 100;
  ... same for 110 120 130 140 150 160 170"
```

No connection string, login or password is ever committed; where they live is machine-local
and outside the repo.

## Where a change goes

The layout and the file-format rules are in `CLAUDE.md`; the two seams that keep
`Grammar/` free of Roslyn are in `.claude/rules/grammar-half.md`, and what may be emitted
into a consumer's assembly is in `.claude/rules/emitted-code.md`. Those three are worth
reading once before the first change and not again. The fourth, `.claude/rules/profiling.md`,
is worth reading before measuring the generator.

## Applying a style rule to the whole tree

`dotnet format` has three entry points, and two of them answer a question you did not ask.

- `dotnet format <project> --diagnostics IDE0022 …` also **reformats whitespace**, whatever
  the diagnostics say. On this tree that means rewriting the grammars that live in raw
  strings, which is thousands of lines nobody asked for.
- `dotnet format analyzers --diagnostics IDE0022 …` **does not see IDE rules at all** and
  exits silently reporting no changes. Believe it and you conclude there is nothing to fix.
- `dotnet format style --diagnostics IDE0022 IDE0021 IDE0061 --severity error` is the one
  that applies them. It sorts using directives as it goes, which is a second change: revert
  that separately if the commit is meant to carry one.

A rule in `.editorconfig` is seen by an editor and by `dotnet format`, and by nothing else
unless `EnforceCodeStyleInBuild` is on — it is, in `Directory.Build.props`, and that is what
makes a style rule fail a build rather than wait to be noticed. Its cost was measured at six
full rebuilds alternating: below what that measurement resolves.

After a pass of this size, check that nothing but the intended form changed by comparing the
**token stream** of every changed file against the revision before it, with the tokens the
rewrite adds and drops (`{`, `}`, `return`, `=>`) removed from both sides. Compare token
*text*, not token kind: two identifiers are both identifiers, and a checker that compares
kinds passes a renamed one. Try the checker against a change it must refuse before trusting
what it says about the pass.

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

## The script that writes the FIX versions

`src/DotGram.Finance/Fix/generate.cs` reads `tests/Corpus/FixRepository` — the FIX repository's
machine-readable form of each version — and writes everything that is a version's own into its
directory, `Fix/Fix44` for FIX 4.4: `FixMessage.Types.cs`, the class of each message type with the
switch that reads its fields; `FixMessage.Header.cs`, the standard header and trailer;
`FixComponents.cs`, an interface a component; `FixValidator44.cs`, the check of every message type,
component and group entry; `FixValidator44.Fields.cs`, the check of every field against its type
and code set; and `FixStandard.cs`, the type of the value of every field. The rest of a version —
its parser, message base, context and header check — is written from `Fix/Templates/`, with the
version's names and the tables the repository gives (the message types, the length/data pairs, the
header's fields) put in. One more file goes into what every version shares: `Fix/FixTag.cs`, every
tag of every version, named as the newest version that has it names it.

`generate.cs` is a file-based app — no project file, run straight from the `.cs` file by the SDK
already needed to build the repository — and is not itself part of the solution build: the Finance
project's `.csproj` excludes it from compilation so it is only ever run on purpose.
**The build does not run it.** Its output is checked in and read as ordinary source; it is needed
only by whoever changes the script, a template or the repository under it. A change to a version's
code is made in a template or in the script, never in the version's directory.

```
dotnet run src/DotGram.Finance/Fix/generate.cs
```

Names are the data dictionaries', so that a dictionary loaded at run time is written into checks
by putting its names in place: a field by the repository's name (in FIX 4.4 two differ, and follow
the dictionaries), a message by the repository's name except four in each version the dictionaries
name otherwise, a component as the interface `I` and its name, and a group as the class
`<Counter>Group` nested in whatever carries it, its entries the list `<Counter>Groups`. `FixTag` is shared by every version and
so takes the repository's names, not one version's: `FixTag.IOIID` is the constant of what FIX 4.4
reads as `IOIid`. What it does not write is the shared code in `Fix/`.

After running it, build: a mistake in the script is a compile error in every message type at once
rather than one. Then run the repository agreement tests — `Fix44RepositoryAgreementTests` and one
class a version beside it, over `FixRepositoryAgreementTests` — which hold every type against the
repository read afresh, and the load tests, which write the same checks from a dictionary at
run time.

## The script that writes the case-fold tables

What an ignore-case literal reads (`syntax.md`) comes from the generator's own table of simple
case mappings, `src/DotGram/Grammar/Model/CaseFold.Table.cs`, never from the runtime the generator
is hosted on. `CaseFold.generate.cs` beside it writes the table from the Unicode Character
Database's `UnicodeData.txt` at the version pinned in the script (`Version`, now 16.0.0, which the
table's header and `CaseFold.UnicodeVersion` repeat). Like `Fix/generate.cs` it is a file-based app
that the DotGram project excludes from compilation, and the build does not run it.

```
dotnet run src/DotGram/Grammar/Model/CaseFold.generate.cs [path/to/UnicodeData.txt]
```

Without a path it downloads `UnicodeData.txt` of the pinned version from unicode.org. For a new
Unicode version, change `Version`, run it, and read the table's diff: a new pair changes what
literals beyond ASCII read, so it belongs in the release notes. `CaseFoldTests` pins the version
and a few mappings, and the full generated-code comparison (above) shows what the change reaches.

## The FIX package's reading, for its maintainers

The package's own page (`src/DotGram.Finance/Fix/README.md`) says what a consumer writes; this is
how the package reads, which that page no longer carries.

### Fields: recovery and binary data

The grammar uses `Field* recover Separator`. A syntax failure returns one
`FixField.Invalid` in source order, then parsing resumes after the next separator
(or finishes at EOF). `Invalid.Position` and `Length` describe the rejected input,
excluding the synchronization separator; `Tag` is 0 and `IsValid` is false.
`RawText` owns the original character input, while `RawBytes` owns the original
byte input (`IsByteInput` distinguishes them). `Message` describes the failure.
I/O errors and exceptions from user C# code still propagate during enumeration.

Valid binary payloads are consumed by length, including embedded separators.
After a malformed binary header or length, separator recovery is best effort:
the next separator may be inside damaged payload data. Primitive conversion
failures still come back as the field of the tag's type, with `IsValid == false`; `recover`
handles recognition failures, not semantic validation.
Use `.ToArray()` when a complete list is needed. String, byte-array and memory overloads
materialize the complete result. Empty input returns no fields.
Concatenated messages are read as one ordered field sequence.

### The field parser and its grammar

`FixParser` is the main parser, with string, byte-array, `ReadOnlyMemory<byte>`, `TextReader`
and byte `Stream` input forms. Its small [grammar](../src/DotGram.Finance/Fix/FixGrammar.gram) reads a
numeric tag and uses `switch` to select text or a length/data pair. C# supplies
classification and typed field construction.

The large `Fix44` grammar is retained in
`tests/DotGram.Finance.Fix44` for regression tests and benchmarks.
It is not included in the Finance package.

### Logs

The log grammar uses `LogSeparator = ' '* & '|' & ' '*`. ASCII spaces immediately
before or after a pipe belong to that separator. Spaces inside text values are
preserved; spaces before EOF are also preserved when no pipe follows. Length-delimited
binary payloads are never trimmed, even when they contain ` | ` or end in spaces.
Field positions refer to the original input, including its formatting spaces.
A streamed log field reads ahead through the padding to the next character or
EOF before yielding. Wire parsing can yield as soon as SOH is read.
A length field makes the field right after it its data: when that field has the paired data tag,
the parser consumes exactly the declared number of data bytes, including any delimiter bytes
inside the payload. A data field anywhere else is rejected as an `Invalid`; a length followed by
something else is still a length, and the message calls refuse it. The final field may end at EOF without a separator. Separators between fields
remain required; the declared binary length still determines the complete payload.
`FixParser.ParseFields` returns the completed field sequence, including `Invalid` fields.
Use `FixParser.TryParseMessage` or `FixParser.BuildMessage` to build a message; both refuse a
recovered syntax error with the first syntax diagnostic, whatever the framing. Typed values own their data;
no complete source string is retained by a field. Octets held in an array or memory are read
where they lie; native byte-stream parsing creates no complete character view.

The common grammar declares `Separator` and specializes the log publication with
`with (Separator = LogSeparator)`. The generator recognizes the guarded text run
`(?!Separator & any)+` and emits a linear scan for these delimiters.
Only structural SOH separators are rendered as pipes. Raw-data payload octets must
remain untouched; a log that replaces or escapes payload bytes is not lossless and
requires its own decoding before this API. Arbitrary log prefixes are not accepted.
BodyLength counts the wire's octets and the pipe rendering does not change it; CheckSum is summed
as the SOH representation would be, a separator counted as one SOH, never pipes inside raw data.
Positions refer to the log as it was given.

### Streams

The adapter frames messages using `BodyLength`, then performs the same grammar, raw-data and
group recognition as the string API. Checking a
message against the schema is a separate call: see **Validation** below.
It reuses a growing buffer across `ReadMessages` iterations and copies each frame out of it
before reading it. Buffering is bounded by the largest frame seen, not the length of the stream;
a message keeps its fields and not the frame. The default `maxMessageLength` is 16 MiB per
message, including header and trailer; callers can set a different positive
limit. This is a frame-size limit, not a total allocation budget.

Clean EOF ends `ReadMessages`; EOF before a complete frame is an error.
`TryReadMessage` returns false with a diagnostic for malformed, oversized, truncated,
or empty input. `ReadMessage` and `ReadMessages` throw `FormatException` for these
errors (except clean EOF for enumeration). Diagnostic positions are relative to
the current frame. I/O exceptions propagate. A failed parse may consume input;
there is no automatic resynchronization or rollback of the underlying stream.

The framing adapter prevents read-ahead from consuming the next message. The byte
path retains a byte frame and passes it to the generated buffered byte parser;
field conversion does not transcode numeric input. This is not a zero-copy API: each field owns
its value, independently of subsequent stream reads. See the finance benchmarks for total
parsing costs.

### Where things are

Paths are under `src/DotGram.Finance/` unless they say otherwise.

A version's field types, message classes, components and checks are written by `Fix/generate.cs`
from the FIX repository and are not edited by hand: a change is made in the script or a template
and the versions are written again. The FIX 4.4 field grammar the tests hold the package against,
and the test fixtures, are maintained by hand beside them.

What every version shares is in `Fix/`:

- `Fix/FixField.cs`: field base, typed-value access, locations and the class of each value type.
- `Fix/FixFieldBuilder.cs`: construction of a field from its tag's type.
- `Fix/FixContext.cs`: the context every version's derives from, and the table the reader indexes.
- `Fix/FixDictionary.cs`: a data dictionary as a value, read, merged and edited.
- `Fix/FixValidator.cs`, `Fix/FixValidator.Load.cs`: what every check says a finding with, and the
  application of a dictionary to a version's checks, compiled in the background after it.
- `Fix/FixTag.cs`: the number of every tag of every version as a constant named for it.
- `Fix/generate.cs`, `Fix/Templates/`: the file-based app that writes every version's directory
  and `FixTag.cs` from the FIX repository; nothing in a version's directory is edited by hand.

FIX 4.4's own is in `Fix/Fix44/`:

- `Fix/Fix44/Fix44Context.cs`: the version's context, its standard pairs, checks and loads.
- `Fix/Fix44/FixMessage.cs`, `Fix/Fix44/FixMessage.Header.cs`: the message base, the standard header and trailer.
- `Fix/Fix44/FixMessage.Types.cs`, `Fix/Fix44/FixComponents.cs`, `Fix/Fix44/FixValidator44.cs`,
  `Fix/Fix44/FixStandard.cs`: the 93 message classes, the 24 component interfaces, the check of
  every message type, component and group entry, and the type of every tag.
- `Fix/Fix44/FixValidator44.Fields.cs`: the check of every field against its type and its code set.

FIX 4.2's is in `Fix/Fix42/`, the same files with 42 for 44 and no `FixComponents.cs`: FIX 4.2
writes its groups inline and names no component. FIX 5.0 SP2's is in `Fix/Fix50/`, over FIXT 1.1's
header, trailer and session messages.

A group is named for its counter: the class `<Counter>Group`, nested in whatever carries it —
a message, a component's interface or another group's entry — and its entries are the list
`<Counter>Groups` beside the counter: `order.NoPartyIDsGroups`, of `IParties.NoPartyIDsGroup`.
`tests/DotGram.Finance.Tests/FieldCases.json` holds field IDs and code-value regression cases;
`Fixtures.json` beside it holds message test inputs. Maintain both alongside the definitions.
DotGram compiles `.gram` files during builds.

Tests and BenchmarkDotNet workloads are separate solution projects. The coverage
and measurement records are in `docs/design/finance-fix44.md` and
`benchmarks/DotGram.Finance.Benchmarks/README.md`.

### Field construction and locations

`FixGrammar` parses the tag and selects one branch through `switch`.
`FixFieldBuilder.cs` constructs the field of the tag's type in C#.
`Field` reads the field contents. `Fields` repeats a constructing group that adds
the separator or EOF and records the actual separator length. Publications support
eager and `yield` parsing.

The `Fix44Grammar` fixture inherits `FixFieldGrammar`, whose
`FixField.gram` contains one alternative per standard field. Tests compare
its results with the production parser using the same shared field model.

`FixField.cs` contains the field base, `FixField.Typed<T>` and a class a value type.
The base classes implement locations and typed-value access; the classes contain no
conversion or location logic.

The builder constructs a field of the tag's type, for example
`new FixField.Integer(FixTag.LegProduct, value.ToInteger())`. Primitive conversions return
`(Valid, Value)` for the field constructor. Plain text conversion returns a string
without a validation flag; a string's typed value is always available. Restrictions
on a particular field (such as currency syntax or a code set) remain semantic checks.

`LocationType = typeof(IFixLocation)` supplies field coordinates through `Locate`.
The constructing group in `Fields` covers the complete tag, equals sign, value
and optional final separator; it supplies the field's source extent. `Position`, `ValuePosition` and
`Length` mean here what they mean everywhere else on this page, unknown and binary
fields included.

The `Fix44Grammar` fixture inherits `FixFieldGrammar`, whose
`FixField.gram` contains one alternative per standard field. Tests compare
its results with the production parser using the same shared field model.

`FixField.cs` contains the field base, `FixField.Typed<T>` and a class a value type.
The base classes implement locations and typed-value access; the classes contain no
conversion or location logic.

The builder constructs a field of the tag's type, for example
`new FixField.Integer(FixTag.LegProduct, value.ToInteger())`. Primitive conversions return
`(Valid, Value)` for the field constructor. Plain text conversion returns a string
without a validation flag; a string's typed value is always available. Restrictions
on a particular field (such as currency syntax or a code set) remain semantic checks.

`LocationType = typeof(IFixLocation)` supplies field coordinates through `Locate`.
The constructing group in `Fields` covers the complete tag, equals sign, value
and optional final separator; it supplies the field's source extent. `Position`, `ValuePosition` and
`Length` mean here what they mean everywhere else on this page, unknown and binary
fields included.

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
