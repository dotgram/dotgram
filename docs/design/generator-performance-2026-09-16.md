# Generator performance baseline — 2026-09-16

## Scope and method

Measured commit `152ad90` in the existing parser-performance-streaming worktree. The initial analysis made no production generator changes. The tables in the baseline sections describe that original state; the final section records the subsequent implementation and before/after comparison.

Environment: Windows, .NET SDK 10.0.400, runtime 10.0.12, Roslyn 4.14.0, Release/net10.0. Runs were sequential, without concurrent builds. A standalone GeneratorDriver harness reads each project's sources and AdditionalFiles, tracks incremental stages, and measures `RunGenerators`. Source loading and initial C# parsing are outside the timer. References use the runtime trusted-platform assembly set, so this is a controlled generator experiment rather than a complete Visual Studio simulation.

Each project ran three fresh-driver cycles in one process, followed by unchanged input, an unrelated C# edit, and (where available) a grammar comment edit. Tiny fixtures ran ten cycles. Warm figures below are medians excluding the first cycle; large projects therefore have only two warm observations. First-cycle JIT/caches and timing variability must not be interpreted as grammar work alone. A full GC precedes each measured scenario, outside its timer. Allocation is cumulative managed allocation across threads, not peak live heap or process RAM. All recorded generator error lists were empty; the harness does not compile the emitted output.

## Full generation

| Input | Warm time | Warm range | Allocated | Generated C# characters | First cycle |
|---|---:|---:|---:|---:|---:|
| DotGram.Sql | 25.66 s | 25.53–25.79 s | 26,387.7 MiB | 144,221,331 | 31.25 s |
| DotGram.Finance | 4.32 s | 4.17–4.48 s | 6,821.3 MiB | 72,765,276 | 8.76 s |
| DotGram.ExpressionLanguage | 1.16 s | 0.86–1.45 s | 469.8 MiB | 3,499,302 | 2.15 s |
| DotGram.Web | 535 ms | 508–562 ms | 245.8 MiB | 2,163,317 | 1.40 s |
| Tiny number grammar | 7.00 ms | 4.9–12.9 ms | 1.08 MiB | 30,698 | 580 ms |
| Same tiny grammar, both streaming forms | 7.79 ms | 6.3–13.2 ms | 2.74 MiB | 73,338 | 592 ms |

SQL includes SQL Standard, SQL92 and both TransactSQL hosts, not just MSSQL. Web is the entire project, not an isolated URL parser. Tiny uses a captured digit repetition and an integer result; its streaming variant enables BufferedInput, BufferedBytes and SpanCaptures. The tiny timing distributions overlap, so the small timing difference is inconclusive; the allocation and output-size increases are clear. Its first cycle primarily exposes shared startup costs and is not representative of every subsequent grammar.

A separate real SQL project Release/net10.0 rebuild succeeded in **62.69 seconds**. This includes generated C# compilation and build overhead and must not be reported as generator time. During one warm harness run, SQL stage times were Asked 436 ms, Answered 1,387 ms and Compiled 23,699 ms.

## Incremental behavior

| Input | Unchanged time / allocation | Unrelated C# edit | Grammar comment edit |
|---|---:|---:|---:|
| SQL | 1,266 ms / 620.8 MiB | 1,329 ms / 620.7 MiB | 12,218 ms / 12,585.9 MiB |
| Finance | 475 ms / 211.0 MiB | 477 ms / 210.9 MiB | 611 ms / 340.3 MiB |
| ExpressionLanguage | 207 ms / 10.6 MiB | 200 ms / 10.6 MiB | 865 ms / 500.7 MiB |
| Web | 69 ms / 17.9 MiB | 69 ms / 17.9 MiB | Not measured |
| Tiny | 2.62 ms / 0.22 MiB | 2.84 ms / 0.22 MiB | 4.85 ms / 0.88 MiB |
| Tiny streams | 2.67 ms / 0.22 MiB | 3.20 ms / 0.22 MiB | 6.56 ms / 2.56 MiB |

For SQL, Asked and Compiled outputs are cached for unchanged input and unrelated C# edits. Answered executes again and returns unchanged answers: parser regeneration is avoided, but semantic validation still costs about 1.3 seconds and 621 MiB per run.

For AdditionalFiles projects, the comment is appended to the shortest grammar file; inclusion dependencies determine the affected hosts. For inline grammars, the first Gram string literal is replaced with an equivalent literal plus a comment, also changing its C# syntax representation and source positions. These are specific invalidation probes, not estimates for every possible grammar edit. Comment edits are not automatically safe to ignore: source positions, diagnostics and embedded grammar text may change.

## SQL CPU and allocation profiles

CPU was collected with dotTrace Sampling/ThreadTime. Aggregate sampled CPU time was 35.67 seconds, with GC accounting for 6.83 seconds (**19.1%**). Inclusive samples include CSharpEmitter.Emit 19.31 s, Machine construction 10.13 s, GrammarNormalizer.Normalize 4.38 s, FollowSets.Precedes 3.09 s, First.Normalized 2.81 s, First.Or 2.47 s, and RoslynSymbolResolver.TypeNamed 2.48 s. **Inclusive values overlap and must not be added.** This profiled first run is not directly comparable to warm unprofiled wall time.

A separate gc-verbose allocation trace estimated 26,380 MiB across process setup and generation. Allocation ticks are sampled estimates. Largest types:

| Type | Estimated allocation | Share |
|---|---:|---:|
| String | 7,799 MiB | 29.6% |
| Char[] | 3,252 MiB | 12.3% |
| CharRange[] | 2,662 MiB | 10.1% |
| StringBuilder | 1,504 MiB | 5.7% |
| Int32[] | 953 MiB | 3.6% |
| DirectMember | 906 MiB | 3.4% |
| Node[] | 871 MiB | 3.3% |

Grouping allocations by the first DotGram frame identifies Normalized (1,650 MiB), Writer.Write (1,594), DirectMembers (1,305), ReaderWriter.Render (1,283), Writer.Line (1,195), Oversee (1,084), NodeWalk.Descendants (988), EscapeExpected (924), and TypeNamed (571). These are allocation-stack attribution buckets, not retained-memory sizes or guaranteed removable amounts.

## Recommended optimization order

### 1. Cache semantic name resolution within each resolver

`Generation/RoslynSymbolResolver.cs:347` repeatedly resolves names, tries nested-name variants and handles ambiguity without a local result cache. Questions about existence, assignability, properties and constructors can resolve the same names repeatedly.

Cache successful and unsuccessful lookups for the fixed compilation and host owned by the resolver. Consider caching repeated property/constructor queries after measurement. Do not retain symbols globally across compilations or omit host identity. This directly targets incremental IDE latency as well as full generation. The measured 621 MiB incremental cost is the opportunity envelope, not a promised saving.

### 2. Remove repeated capture metadata construction and graph traversal allocations

`Grammar/Emit/Machine.Direct.Values.cs:89` builds fresh lists and DirectMember objects for repeated `(rule, factory)` requests. Cache metadata only after the graph/layout phase that determines it is stable, and audit whether callers mutate the returned collection. The nearby Shaped helper uses `Any(repeated.Contains)`, with approximately 403 MiB attributed to Func allocations; a direct loop is a small initial change.

`Grammar/Model/NodeWalk.cs` materializes children while walking, and repeated reachability/capture traversals compound this cost. Prefer direct child enumeration/push operations preserving traversal order; cache reachability against a finalized graph. Avoid constructing large shared indexes for tiny grammars.

### 3. Reduce temporary text and escaping work

`Grammar/Emit/Machine.cs:5185` already has a plain-text fast path. In the escaping path, printable characters still call `character.ToString()`. Append chars directly and append strings only for escape sequences; consider value-based reuse of escaped diagnostics.

`Grammar/Emit/Writer.cs:89` and AppendIndented normalize/copy text and convert nested builders to strings. `Grammar/Emit/CSharpEmitter.cs:66` Oversee splits and rebuilds method text for branch checks. Scan existing text ranges instead of materializing all lines/methods. Preserve exact line endings, indentation, diagnostics and branch checks.

### 4. Reuse normalized First/Follow results

`Grammar/Model/FirstSets.cs:91` already pools temporary lists but still produces fresh range arrays. Union operations feed normalized ranges through further normalization/copies. Reuse immutable canonical results, avoid re-normalizing known-normalized merges, and investigate per-node memoization after normalization. Follow analysis caches must include the continuation and EOF/nullability context; node identity alone is insufficient. The 2.6 GiB of CharRange arrays and CPU profile make this worthwhile, but it requires more correctness work than local text changes.

### 5. Deduplicate generated diagnostic data

The largest SQL file is 76,576,858 UTF-8 bytes; the two TransactSQL files are 34,085,469 and 32,780,169 bytes. SQL92 adds 780,407 bytes. Output size also burdens downstream C# parsing/compilation.

In the largest SQL file, 13,425 Expected array declarations have only 1,126 distinct initializer texts. Their initializers contain 7,245,583 characters, of which **6,525,399 are duplicate copies**. This is a concrete deduplication opportunity, not a claim that every duplicate can be removed without checking scope/accessibility and diagnostic semantics. Sharing identical arrays within compatible scopes could reduce generation, compilation and runtime initialization costs. Sharing larger machines between publication roots is a separate, higher-risk design change requiring parser throughput/JIT measurements.

## Validation and next experiment

Start with resolver-local lookup caching and the small EscapeExpected/Any changes as separate experiments. Repeat the same fresh and incremental measurements, checking tiny grammars as well as SQL, ExpressionLanguage and Finance. Compare generated output byte-for-byte for changes intended to preserve output; run relevant semantic, diagnostic and emitter tests. For deliberate diagnostic-table deduplication, verify parser behavior and emitted compilation in addition to size and performance.

Do not start by parallelizing all hosts: current allocation pressure is already substantial and concurrency could increase live memory and GC contention. First remove repeated work, then measure whether bounded parallelism helps. No peak-memory measurement or optimization speedup is claimed in this report.

## Local reproduction artifacts

Scratch artifacts are intentionally untracked in `.work/generator-analysis/`: `harness/Program.cs`, `harness/genprof.csproj`, per-project JSONL files, `sql-build.log`, `sql.dtp`, `sql-report.xml`, `sql-gc.nettrace`, and `sql-allocations.txt`. They are available in this checkout but are not a portable benchmark suite.

Example harness command from the repository root:

```powershell
dotnet .work/generator-analysis/harness/bin/Release/net10.0/genprof.dll P:/WorkTrees/2909/dotgram DotGram.Sql 3
```

Real build command:

```powershell
dotnet build src/DotGram.Sql/DotGram.Sql.csproj -c Release -f net10.0 --no-dependencies -t:Rebuild -m:1 -nr:false -p:UseSharedCompilation=false -p:NuGetAudit=false -p:ReportAnalyzer=true -v:normal
```

NuGet audit was disabled because the vulnerability feed was unavailable; this does not alter generator logic. Profiling followed `.claude/rules/profiling.md`.

## First optimization batch

Implemented resolver-local positive/negative type lookup caching, removed the predicate delegate from capture repetition checks, and appended ordinary escaped-message characters directly instead of creating one-character strings. Added regression tests for type appearance/disappearance across compilations and host-specific short-name resolution. No global cache or additional parser infrastructure was introduced.

The baseline executable and its dependencies were copied before production edits, avoiding another worktree. Both executables used the same harness, grammar inputs and hash collection. The original baseline table above remains unchanged; the following comparison uses newly measured baseline runs from this batch. Runs were sequential, with two warm observations per large project and nine per tiny fixture. Small timing differences are provisional.

| Input / scenario | Before | After | Allocated before | Allocated after |
|---|---:|---:|---:|---:|
| DotGram.Sql / fresh | 23,917.30 ms | 22,561.32 ms | 26,387.07 MiB | 24,965.35 MiB |
| DotGram.Sql / csharp-edit | 1,192.59 ms | 169.70 ms | 620.71 MiB | 100.07 MiB |
| DotGram.Finance / fresh | 3,763.43 ms | 3,710.32 ms | 6,821.14 MiB | 6,697.84 MiB |
| DotGram.Finance / csharp-edit | 405.89 ms | 203.27 ms | 210.73 MiB | 92.76 MiB |
| DotGram.ExpressionLanguage / fresh | 950.24 ms | 906.55 ms | 466.62 MiB | 458.52 MiB |
| DotGram.ExpressionLanguage / csharp-edit | 178.99 ms | 80.66 ms | 10.60 MiB | 4.23 MiB |
| DotGram.Web / fresh | 484.65 ms | 443.05 ms | 245.73 MiB | 234.43 MiB |
| DotGram.Web / csharp-edit | 75.62 ms | 111.43 ms | 17.83 MiB | 7.77 MiB |
| Tiny / fresh | 5.68 ms | 5.10 ms | 1.08 MiB | 1.05 MiB |
| Tiny / csharp-edit | 2.68 ms | 2.09 ms | 0.22 MiB | 0.19 MiB |
| TinyStreams / fresh | 7.45 ms | 6.11 ms | 2.74 MiB | 2.71 MiB |
| TinyStreams / csharp-edit | 2.85 ms | 2.17 ms | 0.22 MiB | 0.19 MiB |

SHA-256 hashes of all **127 generated source outputs** match the original generator across these six fixtures, including support sources. The optimization preserves generated C# text and its size; it therefore does not change the generated parser algorithms.

SQL unrelated-C#-edit latency fell from 1,192.59 to 169.70 ms (7.0x), with allocation falling 83.9%. Full SQL generation fell 5.7% and allocated 1,421.7 MiB less (5.4%). Finance and ExpressionLanguage incremental results also improved. The initial Web incremental timing regressed despite lower allocation, requiring a longer repeat before drawing conclusions. Full-generation timing ranges overlap for Finance, ExpressionLanguage, Web and the tiny fixtures, so those small median differences are not established speedups.

### Longer Web repeat and validation

The suspicious initial Web timing was not reproduced over ten cycles, run in reverse executable order (candidate before baseline), after tests had finished. Excluding cycle zero, median unrelated-C#-edit time was **52.10 ms before / 33.88 ms after**, with **17.79 / 7.76 MiB** allocated. On the last five cycles it was 42.67 / 33.55 ms. Fresh-generation medians were 416.92 / 246.25 ms, but both distributions had substantial warm-up drift (before 293.66–533.42 ms; after 185.85–532.44 ms). These repeats support lower allocation and do not establish a stable full-generation speedup percentage for Web. The initial three-cycle timing alone was insufficient evidence of a regression.

Release build of DotGram.Tests and its project dependencies succeeded with zero warnings and errors. The full DotGram.Tests run passed **8,150 tests**, zero failed/skipped, including the three added resolver tests. Generated-source hashes matched for all 127 outputs. Formatting checks passed (CRLF, BOM conventions, git whitespace checks). Production changes are limited to RoslynSymbolResolver, Machine.Shaped and Machine.EscapeExpected. No parser runtime or generated-code-size improvement is claimed for this batch.

## Second optimization batch

DirectMembers now caches its read-only-in-practice member lists by rule and factory index inside each Machine. Capture layout and factories are fixed before these shapes are requested. All callers were checked for mutation; no cache is shared across machines or compilations. NodeWalk pushes the bodies of the six unary node kinds directly, avoiding temporary child collections, while preserving the existing Children fallback for other and custom nodes. Tests cover traversal order, identity, conditional/custom nodes and a 100,000-level unary tree.

Compared the first optimization batch (step1-bin) with this candidate, sequentially per project. All 127 generated source hashes matched. The following fresh-generation medians exclude the first cycle (two warm observations for large projects, nine for tiny fixtures).

| Input | Step 1 | Step 2 | Allocation step 1 | Allocation step 2 |
|---|---:|---:|---:|---:|
| DotGram.Sql | 20,133.02 ms | 19,204.88 ms | 24,839.26 MiB | 23,021.15 MiB |
| DotGram.Finance | 3,520.28 ms | 3,344.99 ms | 6,697.77 MiB | 6,374.89 MiB |
| DotGram.ExpressionLanguage | 1,039.45 ms | 1,000.88 ms | 460.41 MiB | 428.05 MiB |
| DotGram.Web | 447.30 ms | 494.40 ms | 223.49 MiB | 232.36 MiB |
| Tiny | 4.36 ms | 4.47 ms | 1.05 MiB | 1.05 MiB |
| TinyStreams | 6.04 ms | 5.75 ms | 2.71 MiB | 2.71 MiB |

SQL allocation fell another **1,818.1 MiB (7.3%)**. Its median time fell 4.6%, but the ranges overlap: step 1 18.73–21.53 s, step 2 18.91–19.50 s. This is not evidence of a stable 4.6% speedup. The first pair looked more favorable than the complete sample. Small-grammar timing ranges also overlap.

The initial Web result again showed warm-up sensitivity. A reverse-order ten-cycle repeat produced fresh medians of 269.90 / 260.60 ms and 211.07 / 208.82 MiB (step 1 / step 2). The last five cycles were 189.54 / 198.16 ms, with 211.05 / 208.79 MiB. Allocation is lower; a reliable timing improvement is not established.

### Actual solution build investigation

The user clarified that the approximately four-minute wait was for the entire solution. The four shipping parser libraries each target netstandard2.0 and net10.0. The solution also builds benchmarks, examples, tests, compatibility targets and VSIX.

A step-1 SQL-only Debug rebuild through dotnet, both targets, succeeded in **123.52 s**, including 122.94 s across three Csc calls (generator plus two SQL compilations). Dependencies were already restored. Output was redirected because another process held the normal Debug generator DLL.

The full step-2 solution was then rebuilt using installed Visual Studio MSBuild 18.9.1 (64-bit), Debug, one build node, shared compilation disabled and isolated output. Restore was completed from the existing local package cache. The build succeeded and the binary log records **253.368 s (4 min 13 s)**. This is a controlled serial rebuild, not a direct reproduction of every IDE setting and not a before/after solution comparison. The SQL-only dotnet run uses a different build host/scope and must not be compared directly with the solution totals.

Csc task durations, extracted by matching TaskStarted/TaskFinished events (including generator execution):

| Project | Csc calls | Wall time in Csc |
|---|---:|---:|
| DotGram.Sql | 2 | 139.95 s |
| DotGram.Finance | 2 | 53.28 s |
| DotGram.ExpressionLanguage | 2 | 10.29 s |
| DotGram.Benchmarks | 1 | 5.90 s |
| DotGram.Web | 2 | 5.62 s |
| DotGram.Compatibility | 3 | 5.55 s |
| DotGram.VisualStudio | 1 | 5.29 s |
| Other projects combined | 8 | 19.57 s |

All 21 Csc calls took 245.44 s, about 97% of the rebuild. SQL plus Finance took 193.23 s, about 76% of total wall time. Nested MSBuild project/target durations must not be added: they include their dependencies. Harness generation times use a different host and cannot be subtracted from Csc durations to calculate an exact pure-C# compilation time.

The four-minute solution build is **not solved by these changes**. Further work should prioritize reducing emitted SQL/Finance code and repeated per-publication work, starting with identical diagnostic tables. The current batch deliberately preserves generated output and therefore does not reduce the downstream compiler input. Changing the default target frameworks or excluding solution projects was not part of this batch.

Local artifacts: `solution-step2.binlog`, `solution-step2.log`, `solution-step2-timings.txt`, `build-timings/`, per-project `*-step1.jsonl` and `*-step2.jsonl`, hashes and `web-repeat-step*.jsonl` under `.work/generator-analysis/`. The binlog reader sums Csc task wall time rather than inclusive project time.

### Second-batch validation

The complete Debug solution rebuild succeeded, including both library targets, VSIX and all three compatibility targets. The subsequent standard-output Release build of DotGram.Tests succeeded with zero warnings/errors; all **8,153 tests passed**, zero failures or skips, in 156.0 seconds. The three added traversal tests passed. An earlier run from the isolated solution output was stopped because existing fuzz tests locate `tests/Snapshots` relative to the normal bin directory; this was a test-launch layout issue and was resolved by rebuilding/running from the normal output path. All 127 generated-source hashes and formatting checks passed. No target frameworks or solution build selections were changed.

## Third optimization batch: shared diagnostic arrays

Sibling recognition machines now share exact ordered Expected array initializers within a single emitted parser class. Buffered character and byte machines use the same registry. Machine-local usage tracking is retained, and the first emitted machine that actually uses a shared field writes its declaration. Thus a declaration first requested by an unused/discarded machine is not lost. Lexical seam/value helper machines retain their separate tables. Registries are local to an Emit invocation; single-machine parsers without buffered forms do not allocate one. No cross-class runtime dependency, lookup or adapter was added.

Only identical initializer text is shared, preserving ordering and escaping (including custom diagnostic markers). Expected arrays are read-only in the generated runtime; error formatting copies before merging/deduplicating values. The public API and recognizer control flow are unchanged.

### Generated size

| SQL file | Before, UTF-8 bytes without BOM | After | Expected arrays before / after |
|---|---:|---:|---:|
| SQL Standard | 76,576,855 | 68,248,459 | 13,425 / 1,126 |
| TransactSQL | 32,780,166 | 24,930,793 | 7,898 / 1,983 |
| TransactSQL Located | 34,085,466 | 26,230,178 | 7,898 / 1,983 |
| SQL92 | 780,404 | 780,404 | 192 / 192 |

Total SQL output fell from **144,221,331 to 120,188,274 characters (16.7%)**. Expected arrays fell from **29,413 to 5,284**. TransactSQL file size fell 23.9%; its Located variant fell 23.0%. Finance and the tiny non-streaming fixture remain byte-for-byte unchanged. ExpressionLanguage output fell by 31,430 UTF-8 bytes, Web by 24,070, and the tiny streaming fixture by 394.

All 127 outputs were checked by replacing diagnostic identifiers with a hash of their exact initializer, removing the Expected declarations and ignoring empty lines. The resulting source matched the step-2 baseline in every case: no differences outside diagnostic declarations and their references were found. This is a focused textual equivalence check, complemented by compilation and runtime tests, not a formal semantic proof. Three affected golden files (Feed, Minimal, Notation) were updated only after the same check passed.

### Generator and actual compiler measurements

SQL fresh generation, median of two warm cycles after the first:

- Time: **20,342.82 -> 18,918.03 ms** (7.0% lower in this sample).
- Allocations: **23,021.29 -> 22,599.43 MiB** (421.86 MiB less).

For an actual controlled build comparison, each isolated output directory received either the saved step-2 generator DLL or the step-3 DLL. Visual Studio MSBuild 18.9.1 rebuilt the same SQL project in Debug for both frameworks, one node, shared compilation disabled, dependencies already restored, and BuildProjectReferences=false so the selected generator could not be replaced by current sources. Generated file sizes confirmed that the intended generator was used. Both builds succeeded.

| Both-target SQL rebuild | Step 2 | Step 3 |
|---|---:|---:|
| Process elapsed | 154.94 s | 130.55 s |
| MSBuild binary-log elapsed | 154.793 s | 130.408 s |
| Csc tasks, two calls | 153.920 s | 129.574 s |

The paired rebuild saved **24.39 s (15.7%)**. This is one sequential pair, not a statistical distribution of IDE rebuilds. Raw artifacts are `sql-build-step2/3.log`, `.binlog`, isolated output directories, `sql-step2/3-repeat.jsonl` and source dumps under `.work/generator-analysis/`. The source comparison utility is `compare-expected.py`.

The paired Debug SQL assemblies also shrank: net10.0 72,797,184 -> 60,635,648 bytes; netstandard2.0 72,829,952 -> 60,669,952 bytes. These DLLs include embedded debugging information; these are not NuGet package size measurements.

All **8,155 DotGram.Tests tests passed** (zero failures/skips), including the two new table-sharing cases for string, character-reader and byte-stream input, repeated errors and successful parses. The updated golden sources compile at the C# 8 floor before comparison. No parser throughput or peak-memory improvement is claimed from this batch's generator/build measurements.

### Full solution comparison

The complete Debug solution rebuild also succeeded with the same Visual Studio MSBuild host and settings as the step-2 solution measurement: one node, shared compilation disabled, and isolated output. Both shipping library frameworks, VSIX and all three compatibility targets remain included.

| Full solution rebuild | Step 2 | Step 3 |
|---|---:|---:|
| Binary-log wall time | 253.368 s | 224.299 s |
| All Csc tasks, 21 calls | 245.44 s | 220.671 s |
| SQL Csc, two calls | 139.948 s | 128.650 s |
| Finance Csc, two calls | 53.278 s | 47.594 s |
| ExpressionLanguage Csc, two calls | 10.289 s | 9.100 s |

Observed solution wall time fell by **29.069 s (11.5%)**, from approximately **4 min 13 s to 3 min 44 s**. This is a single controlled sequential rebuild comparison, not a guarantee for Visual Studio incremental builds. Finance output is byte-identical, yet its compilation also became faster in this run; therefore the entire observed solution improvement cannot be attributed to diagnostic sharing. The separate paired SQL build and the verified source/DLL reductions provide more direct evidence for this change. SQL and Finance still account for 176.244 s of Csc time, so further build optimization remains necessary.

Artifacts: `solution-step3.binlog`, `solution-step3.log` and `solution-step3-timings.txt` under `.work/generator-analysis/`, alongside the step-2 baseline.
