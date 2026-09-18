# Performance work: cross-device continuation instructions

Snapshot: 2026-09-17. This is a handoff, not a specification or a claim that the
experiments below are enabled. Recheck Git and source before continuing.

## Start here

Continue optimizing generated Fix parsing toward handwritten-parser performance,
using a mixture of direct rule methods and the existing machine. Do not globally
replace the machine with recursion: previous versions overflowed the stack.
Preserve rollback, recovery, diagnostics, deferred factory timing and native
char/byte streaming. Measure small repeated calls as well as large inputs.

Read `CLAUDE.md`, `docs/coding-conventions.md`, `docs/README.md`,
`.claude/rules/emitted-code.md` and `.claude/rules/profiling.md` first.
Repository-facing writing is English; conversation with the user is Russian.
Preserve user edits. C#/project files use BOM, CRLF and tabs; Markdown uses no BOM
and CRLF. Run `git diff --check`.

## Exact source state and transfer

- Original checkout: `P:\WorkTrees\2909\dotgram`.
- Existing branch: `codex/parser-performance-streaming`.
- HEAD: `3e9709de5da7664308a5b7afcd051435c72f6258`.
- The retained scalar-reader and scalar-guard changes are **uncommitted**.
- No commit, push or merge was performed when writing this handoff.
- Current `CLAUDE.md` says to commit to main, without creating feature branches.
  The branch above predates that instruction; do not create another worktree or
  branch merely to continue. Inspect the destination checkout first.
- The user explicitly defines “synchronize with main” as merging in both directions.
  Preserve concurrent main changes; do not assume the destination main is this HEAD.

A transfer archive accompanies this document: `dotgram-performance-handoff-2026-09-17.zip`.
It contains `working-tree.patch` for the six modified tracked files, and `overlay/`
with the new reports, raw results, rejected patches, this instruction, and scratch
benchmark source projects. It excludes build outputs, baseline DLLs and generated
prototype files. Extract it outside the destination checkout.

For an otherwise clean checkout at the exact HEAD above, run from its root:

```powershell
git status --short
git rev-parse HEAD
git apply --check <extracted-archive>/working-tree.patch
git apply <extracted-archive>/working-tree.patch
```

Then copy the contents of `overlay/` into the checkout, preserving relative paths.
Review collisions before copying. For a newer or dirty checkout, review and reconcile
the patch against its changes instead; do not reset or overwrite those changes.
The archive is sufficient to restore the retained source changes and benchmark
harness sources, but obtaining the repository and its NuGet dependencies is separate.
A document alone, or merely fetching the original branch, does not transfer these
uncommitted changes.

## Retained implementation

1. **Typed scalar rule readers** in `Machine.Scan.cs` (`ScalarScanner`) and the
   `Node.Call` path in `Machine.cs`. Recognize pure concrete-range leaves such as
   Fix Tag and Size in a method. A Completed record supplies the whole capture's
   extent, eliminating the separate leaf Capture and its materializer walk.
   Factories remain demand-driven. Ordinary recognition handles refusal.
   String, buffered char and buffered byte input are supported.
2. **Direct scalar guard demand** in `Machine.Materialization.cs`
   (`MaterializeScalarGuard`) and `Machine.cs`. Up to four independent scalar
   captures can be constructed without the general dependency/materialization
   passes. Preserve the typed cache, rollback invalidation and reverse arena
   construction order. This step is enabled only for unbuffered input: a stream
   trial had inconsistent timings, including a 13.2% byte-stream regression.
3. `CSharpEmitter.cs` renders scalar scanners for buffered recognizers too.
4. `SwitchTests.cs` covers factory demand, rollback, optional captures, construction
   order, aliases with different rules, overlapping follows and stream behavior.

Changed tracked files: the four emitter files above, `Machine.cs`,
`tests/DotGram.Tests/SwitchTests.cs`, and the Finance benchmark README (six total:
CSharpEmitter, Machine, Machine.Scan, Machine.Materialization, tests, README).

Evidence, in order:

- [Initial handwritten gap](hand-fix-gap-2026-09-17.md).
- [Retained scalar methods](fix-rule-methods-2026-09-17.md).
- [Retained scalar guard demand](fix-scalar-guards-2026-09-17.md).
- [Rejected whole-field experiment](fix-selected-field-2026-09-17.md).
- Raw samples, hashes and experimental patches: `benchmarks/results/` with the
  corresponding dated names.

## Measurements: keep the baselines distinct

Scalar methods versus HEAD reduced string Order time by 10.5–11.2% and short binary
pairs by 13.9–17.4%. Scalar guard demand adds 5.6–8.6% on Order and 6.5–8.3% on binary
pairs versus the already improved scalar-method baseline. Do not add percentages
or compare absolute times from unrelated processes as if they were paired.

Warmed allocations after both changes: One 184 B, Order 1448 B, BinaryMany 6744 B.
Generated FixGrammar source after both changes: 575,143 bytes. The 8000-field case
still crosses arena retention limits and allocates about 24.36 MB per parse.
After scalar methods, a separate handwritten comparison still showed about 2.50x
Order and 4.14x binary-pair gaps. This is not a fresh comparison after all changes.

The final restored state passed Release Finance builds (netstandard2.0/net10.0),
71 switch/recovery tests and 6417 Finance tests. Earlier broader trials passed full
core runs (8392/8396 tests); do not describe those as full-suite validation of every
later edit. Compatibility builds and emitted C# 8 checks passed during the retained
steps. Rebuild dependencies on a fresh device before reusing targeted test commands.

## Rejected whole-field experiment: do not enable by default

The prototype directly reads Tag, `=`, selector and text payload, then rejoins the
machine. Binary arms resume after selection; restarting the rule there would repeat
user factories and selector side effects. Outer field construction remains deferred.

A manually specialized prototype omitting Run/CaptureOpen records saved 7–10% on
Order and 9–17% on One. The generic emitter could not prove those records unnecessary
for Fix: its follow information was `Anything`. With the records preserved, Order
savings ranged 0.3–6.9%, One included a regression, binary was effectively unchanged,
and allocations did not fall. Source grew 0.74% to 579,421 bytes.

The generic path was removed from active code after testing. There must be no
active `Machine.Selected.cs` or `CompileSelectedCapture` integration in this snapshot.
The source and six additional tests are preserved in
`benchmarks/results/fix-selected-field-experiment.patch`; `git apply --check` passed
against the retained working tree. Do not confuse this experimental patch with the
archive's `working-tree.patch`, which restores the retained implementation.

The experimental version passed 77 switch/recovery and 6417 Finance tests. These
checks are not a general proof of equivalence. The cause of the overly broad follow
set was not established; recovery was an early hypothesis, not a confirmed diagnosis.

## Next work, in order

1. Reestablish the retained baseline on the destination and save its Finance DLL
   outside bin/obj before making changes. Record commit, patch state, source bytes,
   generated-source hash and assembly hash.
2. Trace why Fix Field/Text gets an `Anything` continuation. Inspect
   `FollowSets.Settle`, `FollowSets.Precedes`, `FirstSets`, the specialized graph,
   delimiter/EOF alternatives, lookahead and publication/shared call sites.
   Do not narrow the set merely to make a benchmark pass.
3. Prove when shorter text cannot satisfy a caller. Add tests for delimiter versus
   EOF, overlapping suffixes, multiple callers, find/yield, trivia, lookahead,
   recovery, selected-arm rollback and side-effecting factories/selectors.
4. Remove only proven-unnecessary Run/CaptureOpen records. First measure whether
   that can improve the existing machine independently of a whole-field helper.
   Revisit the saved helper patch only if method extraction adds a repeatable gain.
5. Preserve deferred outer factories and once-per-derivation scalar caching.
   Do not enable Immediate globally, replace caches with one current-field local,
   or re-run selection after partial success. Ambiguous and recursive paths keep
   the machine; any recursive backend needs a bounded stack/fallback design.
6. Benchmark One, Order, binary pairs, long text/raw payload, recovery, 4000/8000
   fields, cold first call, and string/TextReader/byte Stream separately. Then check
   Fix44 and larger SQL/ExpressionLanguage output size and regressions before
   generalizing. Small parser initialization and million-call behavior matter.

## Build and test commands

Use the SDK pinned by `global.json`; all commands run at the repository root.
On a fresh device restore/build project references first. The source device avoided
shared compilation because it had stalled, using `-m:1 -nr:false` for reliable runs.

```powershell
dotnet build src/DotGram.Finance/DotGram.Finance.csproj -c Release -p:UseSharedCompilation=false -m:1 -nr:false
dotnet build tests/DotGram.Tests/DotGram.Tests.csproj -c Release -p:UseSharedCompilation=false -m:1 -nr:false
dotnet build tests/DotGram.Finance.Tests/DotGram.Finance.Tests.csproj -c Release -p:UseSharedCompilation=false -m:1 -nr:false
dotnet tests/DotGram.Tests/bin/Release/net10.0/DotGram.Tests.dll -class '*SwitchTests' -class '*RecoveryBoundaryTests'
dotnet tests/DotGram.Finance.Tests/bin/Release/net10.0/DotGram.Finance.Tests.dll
dotnet build tests/DotGram.Compatibility/DotGram.Compatibility.csproj -c Release -p:UseSharedCompilation=false -m:1 -nr:false
git diff --check
```

Run test executables directly rather than `dotnet test`. Full core tests omit the
`-class` filters. Only use `--no-restore` and `BuildProjectReferences=false` after
matching dependencies have been built; stale analyzer/dependency DLLs invalidate
results. Never run builds/tests concurrently with timing measurements.

## Reproducible paired measurements

The archive carries source for `.work/fix-scalar-guards/paired`,
`.work/fix-scalar-guards/scaling` and `.work/fix-methods/cold`. These are scratch
harnesses, not a supported public command. Rebuild them locally; old binary paths
and old DLL hashes are evidence, not portable build artifacts.

```powershell
dotnet build .work/fix-scalar-guards/paired/paired.csproj -c Release -p:UseSharedCompilation=false
$env:FIX_SAME_PARSER = '1'
$env:FIX_WORKLOADS = $null
dotnet .work/fix-scalar-guards/paired/bin/Release/net10.0/paired.dll <baseline-finance.dll> <candidate-finance.dll> string-only
dotnet .work/fix-scalar-guards/paired/bin/Release/net10.0/paired.dll <baseline-finance.dll> <candidate-finance.dll> string-only reverse
```

Omit `string-only` to include streams. Run processes sequentially and save JSONL.
The harness uses separate AssemblyLoadContexts, compares serialized values and
errors, warms each case for 1.5 seconds, and takes medians of nine alternating
batches. `FixUs/FixBytes` mean baseline; legacy `Fix44Us/Fix44Bytes` mean candidate
**FixParser**, not Fix44, when `FIX_SAME_PARSER=1`.

Build/run `scaling.csproj` analogously; useful filter:
`$env:FIX_WORKLOADS = 'Wire4000,Wire8000,Recovery'`. Clear it afterwards.
Build `cold.csproj`, then run `cold.dll <finance.dll>` in separate fresh processes
for each version. It measures the first reflected parse and enumeration/JIT after
loading, not process startup or working set. Run at least three alternating pairs.

For the repository handwritten comparison, build Finance benchmarks then run its
DLL with `--hand-fix-performance`; see `benchmarks/DotGram.Finance.Benchmarks/README.md`.
Report timings, managed allocations, generated code volume and limitations separately.
