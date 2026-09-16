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

## Fourth optimization batch: scan method budgets without copying bodies

The final-file oversized-method warning pass now scans line and method ranges directly in the emitted string. It no longer splits the entire file into line strings, trims a copy of each line, or rebuilds method bodies in StringBuilder. Method-name recognition retains the same prefixes, whitespace handling and identifier-character rules. Branch counting accepts a bounded range; existing callers still count their complete strings. No new runtime parser infrastructure is emitted.

### Initial measurements

Saved step-3 binaries and the candidate were run sequentially per fixture. Medians exclude cycle zero: two warm observations for library projects, nine for tiny fixtures. All **127 generated source hashes matched exactly**. Calling the old and new warning pass on these 127 files also produced identical diagnostics (none of these files triggers the oversized-method warning). Separate regression cases exercise non-empty warnings, the 2,000/2,001 boundary, nested local functions, Unicode whitespace/names, CRLF/LF and a final unterminated line.

| Fixture | Step 3 time | Step 4 time | Step 3 allocation | Step 4 allocation |
|---|---:|---:|---:|---:|
| SQL | 19,431.84 ms | 20,554.29 ms | 22,596.86 MiB | 21,463.78 MiB |
| Finance | 3,442.80 ms | 3,152.44 ms | 6,375.03 MiB | 5,706.95 MiB |
| ExpressionLanguage | 1,107.91 ms | 1,031.85 ms | 434.74 MiB | 404.75 MiB |
| Web | 496.71 ms | 426.29 ms | 231.87 MiB | 209.73 MiB |
| Tiny | 4.41 ms | 4.64 ms | 1.05 MiB | 0.94 MiB |
| TinyStreams | 5.52 ms | 5.46 ms | 2.70 MiB | 2.20 MiB |

The initial SQL timing regressed despite eliminating **1,133.08 MiB** of allocation. Finance allocated **668.08 MiB** less. An isolated invocation of the warning pass across all 127 already-generated files took 1,142.35 / 351.34 ms (step 3 / step 4), excluding file loading, in one sequential check. This supports the local optimization but is not a full-generation timing result; a reverse-order repeat is required before interpreting the initial SQL timing.

Artifacts are `*-step3/4-oversee.jsonl`, matching `.hashes`, `compare-oversee.ps1` and `oversee-comparison.txt` under `.work/generator-analysis/`.

### Reverse-order repeat

A separate five-cycle SQL run executed the candidate before the saved baseline, without builds or tests running alongside it. Excluding cycle zero, medians were **21,141.04 / 19,971.92 ms** (step 3 / step 4). Warm ranges were 20,951.30–21,644.78 / 18,776.18–20,413.59 ms. Allocation was **22,471.65 / 21,449.47 MiB**, a reduction of **1,022.17 MiB**. This repeat favors the candidate by 5.5%, but the initial ordering favored the baseline; no stable full-generation speedup percentage is established. Allocation reductions reproduced in both runs.

A reverse-order thirty-cycle tiny-grammar repeat gave warm medians of **4.587 / 4.683 ms** and **1.049 / 0.944 MiB**. Timing ranges overlap widely (4.22–10.54 / 4.22–8.36 ms), so there is no established small-parser generation timing improvement. Generated parser code and runtime behavior are unchanged; this batch does not measure parser throughput or peak memory. The full solution rebuild was not repeated for this batch, and its earlier 3 min 44 s result must not be presented as a new measurement.

### Fourth-batch validation

Release build of DotGram.Tests and dependencies succeeded with zero warnings/errors using one MSBuild node and shared compilation disabled. All **8,160 tests passed**, zero errors/failures/skips, in 157.509 seconds, including the five additional warning-scan regression cases and the existing oversized-method and snapshot coverage. All 127 source hashes matched, and BOM/CRLF/tab indentation and git whitespace checks passed. Logs: `tests-step4-build.log` and `tests-step4-all.log` under `.work/generator-analysis/`.

## Refreshed CPU and allocation profiles after step 4

Profiled commit `b276f8d` using the existing Release generator harness. The harness generator DLL SHA-256 matched the current Release build (`F827CED35D8953A5D58CE76D49AF575458056687AA6C7A0124E8E678D481029E`). No production code was changed for this investigation.

### Method and scope

- dotTrace 2026.2.0.1 Sampling / ThreadTime, two fresh generator runs per process (initial and warm), separately for SQL and Finance. CPU percentages below use the sum of reported own times, including GC and unattributed/native work. The profile includes harness setup and both runs; it does not isolate only the warm run.
- dotnet-trace 10.0.745401 `gc-verbose`, one fresh generation per project in separate processes. Runtime allocation samples were grouped by type and first DotGram stack frame using the existing TraceEvent reader. Its output labels MiB as MB; the values below use the correct binary units.
- The source generator ran successfully with no error diagnostics in both CPU profiles. Both allocation collection processes exited successfully.
- These profiles cover the generator, not Roslyn compilation of its emitted source or the complete solution build. Profiling overhead changes elapsed time and GC pauses; none of these timings is a new solution build benchmark. Own-time percentages can be added across disjoint functions; inclusive times overlap and must not be added.

### CPU findings

| Sampled work | SQL | Finance |
|---|---:|---:|
| Sum of reported own time, two-run process | 56.375 s | 15.139 s |
| GC own time | 11.859 s (21.0%) | 1.656 s (10.9%) |
| First.Or + First.Normalized own time | 3.922 s (7.0%) | 0 ms sampled |
| LexerEmitter.Field own time | 1.859 s (3.3%) | Not a leading cost |
| NodeWalk iterator own time | 1.328 s (2.4%) | 0.156 s (1.0%) |
| GrammarNormalizer.Sources own time | 0.406 s (0.7%) | 0.375 s (2.5%) |
| Oversee inclusive time | 0.344 s (0.6%) | 0.156 s (1.0%) |

Zero sampled time is not proof of zero cost. First.Normalized's inclusive time is 4.203 s and First.Or's is 4.359 s in SQL; these overlap. Writer.Write inclusive time is 1.141 s for SQL and 0.875 s for Finance. Native/optimized attribution accounts for 10.3% / 25.5%, so this is a prioritization profile rather than a complete attribution of every instruction.

### Allocation findings

Sampled allocation totals are **21,541 MiB for SQL** and **5,762 MiB for Finance**. They describe cumulative allocation, not simultaneously live memory or peak working set. String plus char-array allocations account for about 41.8% / 54.0% respectively. SQL also allocates approximately 2,659 MiB of CharRange arrays.

| First DotGram frame on allocation stack | SQL | Finance |
|---|---:|---:|
| First.Normalized | 1,639 MiB (7.6%) | Not in top 20 |
| Writer.Write | 1,487 MiB (6.9%) | 677 MiB (11.8%) |
| ReaderWriter.Render | 1,277 MiB (5.9%) | 366 MiB (6.4%) |
| Writer.Line | 1,213 MiB (5.6%) | 219 MiB (3.8%) |
| NodeWalk iterator | 975 MiB (4.5%) | 130 MiB (2.3%) |
| GrammarNormalizer.Sources | 658 MiB (3.1%) | 462 MiB (8.0%) |
| CaptureLayout.Walk | 539 MiB (2.5%) | 317 MiB (5.5%) |
| CSharpEmitter.Numbered | 466 MiB (2.2%) | 282 MiB (4.9%) |

These stack buckets are sampled attribution, not exact allocation counters or exclusive costs of all descendants of a method. Inlining can affect which frame is visible.

### Next experiments, based on the profile and code inspection

1. **Writer.Write / line-ending handling:** avoid normalizing and copying already-normalized blocks; eventually append line ranges directly. This is a shared allocation source for SQL and Finance. Preserve CRLF output, blank lines, trailing whitespace rules and mapped user-code columns, verified with exact source hashes.
2. **FIRST-set unions:** First.Or merges sorted ranges into a temporary list, then calls Chars/Normalized, which copies and merges them again. Try a single normalized merge and reuse unchanged results. Preserve Nothing/Anything/Ends semantics, range invariants and fixed-point convergence; current public First construction means input invariants must be checked before bypassing normalization. Do not predict savings equal to all Normalized allocations: other callers still need normalization.
3. **LexerEmitter.Field:** it fills each bit individually for every range before checking whether a table already exists. Test full-byte filling of range interiors with boundary masks. Its CPU cost is visible, but its maximum direct benefit is bounded by the sampled 3.3% own time.
4. **Finance forwarding analysis:** Sources allocates a list before knowing whether a rule forwards anything and is repeatedly reached by forwarding resolution. Inspect call frequency and mutation lifetime before caching; lazy allocation is a smaller experiment. The current profile's first-frame allocation estimate is 462 MiB.

The method-size warning pass is no longer a primary target. After generator experiments, remeasure a real SQL build with analyzer timing and a binlog, then profile the C# compilation phase if it remains dominant. Generator CPU savings do not translate one-for-one into solution wall-time savings.

Artifacts under `.work/generator-analysis/`: `sql-current.dtp*`, `finance-current.dtp*`, `*-current-report.xml`, `*-current-profile.log`, `*-current-gc.nettrace`, `*-current-gc.nettrace.etlx`, `*-current-allocations.txt`, and `current-cpu-summary.json`. Raw profiler files remain local and ignored by git.

## dotMemory retention investigation

Used dotMemory Console 2026.2.1 with the existing installed dotMemory UI available for opening the resulting workspace. A separate Release harness exercises SQL generation at commit `b276f8d` and takes five named snapshots. The production generator is unchanged. Incremental-step history recording is disabled, and only the current GeneratorDriver is held deliberately. Every checkpoint forces full garbage collection before measuring.

Sequence: build the input compilation; generate once; perform two more fresh generations, replacing the current driver; perform five unrelated C# edits through the current incremental driver; release the current driver. Old drivers are recorded only as weak references. This tests a bounded sequence in a harness, not a long Visual Studio session, grammar changes or compilation of the generated C#.

The dotMemory command-line tool captures snapshots but does not export heap/root reports. At the same paused checkpoints, a supplemental ClrMD 3.1.512801 inspector enumerates non-free objects and follows GC-root paths. The numerical type breakdown and root chains below come from that inspector, not an unperformed analysis through the dotMemory GUI. GC.GetTotalMemory values are measured before the snapshot; ClrMD enumerates shortly after it, accounting for small differences caused by checkpoint bookkeeping.

### Live memory under dotMemory

| Checkpoint | Managed live data after GC | Prior drivers still alive |
|---|---:|---:|
| Input compilation, before generation | 13.34 MiB | 0 |
| First generation retained | 295.95 MiB | 0 |
| Third fresh generation retained | 296.53 MiB | 0 of 2 |
| Five unrelated C# edits completed | 296.55 MiB | 0 of 7 |
| Current driver released | 29.97 MiB | 0 of 8 |

There is no observed accumulation of previous drivers or their generated source in this sequence. The roughly 0.6 MiB growth after the first generation is small compared with a retained generation; this is not proof that every workload or a long-lived IDE session is leak-free.

While the result is retained, the largest type buckets are strings (**237.73–237.74 MiB**), Answer arrays (**20.93 MiB**) and Question arrays (**8.97 MiB**). The largest string has a measured root chain through the harness's current CSharpGeneratorDriver, GeneratorState, GeneratedSyntaxTree and StringText: it is the generated source. An Answer-array root chain goes through the driver's incremental DriverStateTable / StateTableStore. Their retention is expected while the current generator result is cached.

After release, strings fall to **5.31 MiB** and the large Answer/Question arrays are absent from the leading retained types. The largest remaining buckets include Int32 arrays (**7.05 MiB**) and Roslyn token/string caches. A directly observed static-root chain retains the approximately **0.5 MiB First[]** array; code inspection identifies the fixed-size character-folding cache in FirstSets. The original input compilation and grammar inputs remain alive deliberately, so the released checkpoint is not an empty-process baseline.

### Measurement trap found and corrected

The first exploratory harness version loaded old/current drivers directly in its outer method. JIT stack lifetimes retained those temporary references across checkpoints even after the static current-driver field was cleared. ClrMD found a stack root to a released driver; this was an instrumentation artifact, not evidence of a generator leak. Moving replacement, edits and release into separate non-inlined methods eliminated that root and allowed all eight old/current drivers to collect. The initial workspace in `memory-sql/` is excluded from the conclusions; the corrected workspace is in `memory-sql-isolated/`.

### Heap fragmentation and process memory

Under dotMemory, the third-generation GC heap reports 611.53 MiB, of which 315.00 MiB is fragmentation, while managed live data is 296.53 MiB. After release the values are 139.37 / 109.39 / 29.97 MiB respectively. Live data, heap extent/fragmentation, cumulative allocation and process working set are different measures and must not be conflated. Process working set includes additional runtime/native/profiler state; it cannot be explained by live object size alone. Profiling changes GC behavior, so a separate unprofiled run is recorded below.

Corrected dotMemory workspace: `.work/generator-analysis/memory-sql-isolated/sql-retention.dmw` (384,672,730 bytes), containing `baseline`, `first`, `repeated`, `edited` and `released`. The directory also contains `metrics.jsonl`, five `*-heap.json` reports and `profiler.log`. Local reproducibility helpers are `memory-harness/`, `heap-inspector/`, `capture-memory.py` and `check-retention.py`. These diagnostic artifacts remain ignored by git.

### Control without the profiler

The same corrected harness was then run without dotMemory or heap inspection, releasing checkpoints immediately after reading their counters. Live data again stayed at approximately **295.96 / 296.53 / 296.55 MiB** (first / third generation / edits), fell to **29.97 MiB** on release, and all eight prior/current drivers were collected. This independently reproduces the retention result.

The process peak working set reached **4,945,256,448 bytes (4.61 GiB)** across the three fresh generations. After release the working set was still **3,216,236,544 bytes (3.00 GiB)**, despite only 29.97 MiB of live managed data; the checkpoint's GC heap size was 191.27 MiB with 161.30 MiB fragmentation. These are Windows process measurements from one run, not a Visual Studio memory measurement or a memory-leak diagnosis. The retained input compilation, runtime caches, committed memory and native state also matter, and this experiment does not attribute the whole working set to one cause.

The practical priority remains reducing transient allocation and large intermediate buffers, then measuring peak working set again. The current experiment gives no evidence that old generator results accumulate indefinitely. Unprofiled control counters and output are in `memory-sql-unprofiled/metrics.jsonl` and `run.log`. Both corrected runs completed successfully with six generated sources per generation and no generator errors; no production sources were changed, so the runtime test suite was not rerun for this investigation.

## Fifth optimization batch: write original line ranges

Writer.Write now reads LF/CRLF line ranges directly from the original block and appends normalized CRLF output. It no longer creates a normalized copy (previously two Replace passes) or concatenates a final newline onto the whole block. Writer.Line trims trailing spaces/tabs by shortening the appended range instead of allocating a trimmed string. AppendIndented retains its CRLF-only interpretation and mapped-source handling. The existing bare-LF final blank-line convention is deliberately preserved.

Six fixtures were measured sequentially with saved step-4 and candidate binaries. Fresh-generation medians exclude cycle zero (two warm observations per library, nine per tiny fixture). No tests or builds ran alongside these measurements.

| Fixture | Step 4 time | Step 5 time | Step 4 allocation | Step 5 allocation |
|---|---:|---:|---:|---:|
| SQL | 20,837.84 ms | 20,272.68 ms | 21,459.29 MiB | 19,910.85 MiB |
| Finance | 3,292.27 ms | 2,985.06 ms | 5,707.01 MiB | 4,987.01 MiB |
| ExpressionLanguage | 1,059.82 ms | 1,003.11 ms | 403.69 MiB | 370.38 MiB |
| Web | 433.54 ms | 445.28 ms | 199.47 MiB | 185.38 MiB |
| Tiny | 4.89 ms | 4.86 ms | 0.94 MiB | 0.90 MiB |
| TinyStreams | 6.03 ms | 5.93 ms | 2.20 MiB | 1.96 MiB |

SQL allocated **1,548.44 MiB less (7.2%)**, Finance **720.00 MiB less (12.6%)**. SQL time fell 2.7% and Finance 9.3% in this small sample; Web time increased despite lower allocation. These are observations from one ordering, not established timing guarantees. No parser runtime or emitted-code-size improvement is claimed: **all 127 generated source hashes matched exactly**.

Sixteen new focused writer cases passed, covering empty/blank lines, CRLF/LF/mixed endings, lone carriage returns, an unterminated last line, whitespace in mapped C# and nested writer indentation. The Release test project and dependencies built without warnings/errors. Artifacts: `*-step4/5-writer.jsonl`, `.hashes`, `tests-step5-build.log`, `tests-step5-writer.log` and `measure-writer.py` under `.work/generator-analysis/`.

### Process-memory comparison and limitation

The corrected retention harness was run without profiling, using saved step-4 and candidate binaries, three fresh SQL generations and five unrelated C# edits per process. A second pair reversed the executable order. Neither pair ran concurrently with compilation or tests.

| Process measurement | Step 4, first pair | Step 5, first pair | Step 4, reverse pair | Step 5, reverse pair |
|---|---:|---:|---:|---:|
| Peak working set | 4.316 GiB | 4.737 GiB | 4.181 GiB | 4.744 GiB |
| Working set after releasing the result | 3.770 GiB | 3.135 GiB | 3.270 GiB | 3.197 GiB |

**Peak working set increased in both comparisons**, by 9.7% and 13.4%, despite lower cumulative allocation. This batch must not be described as a peak-memory improvement. The cause of the peak increase was not established; altered allocation/GC timing is a hypothesis, not a measured explanation. No forced GC or heap-size tuning was added to the production generator to disguise that result.

All old/current drivers collected in both versions. Candidate live data after the edits was approximately 294.6 MiB and after release 28.0 MiB, compared with 296.6 / 30.0 MiB for the baseline. Those small differences do not explain the working-set peak. The earlier 4.61 GiB baseline result also shows that absolute process peaks vary between runs; the two fresh comparisons nevertheless both favored the baseline on this metric.

The change reduces source-writing allocations and has favorable SQL/Finance timing observations, with an observed peak-memory tradeoff. Further work on peak consumption must locate when large buffers coexist and measure their lifetime; lower cumulative allocation alone is insufficient evidence. Raw counters: `memory-sql-step4/5-writer/metrics.jsonl` and `memory-sql-step4/5-writer-repeat/metrics.jsonl`, with `check-writer-memory.py` reproducing the phases. A full solution rebuild was not repeated for this batch.

### Fifth-batch final validation

All **8,176 DotGram.Tests tests passed**, zero errors/failures/skips, in 156.577 seconds. This includes the sixteen new writer cases, existing mapped-C# diagnostics and golden-source tests. Release build had zero warnings/errors; all 127 representative generated source hashes and BOM/CRLF/tab/whitespace checks passed. The peak-working-set regression above remains a measured limitation of this candidate, not a test failure or an improvement claim. Full test log: `tests-step5-all.log`.

## Buffer reuse feasibility audit

This is a source/lifetime audit of `c386380`, not a pooling benchmark or an implemented pool.

| Buffer or result | Reuse assessment |
|---|---|
| Local `code` / `head` writers in ReaderWriter.Render | Good initial candidate. Each method returns independent strings, so builders can be returned after their final use through explicit scoped ownership. Nested rendering must rent a distinct active buffer. |
| Trial writers in materializer/direct-value budget calculations | Good candidate with an explicit end of scope after branch counting. |
| Machine state writers | Longer-lived: Reserve stores them in `_states`; `_edges` also uses writer identity. PlanLayout creates `_raw` strings while retaining the writers. A later buffer-release boundary may remove simultaneous builder/string storage, but repeated PlanLayout/rendering and graph access must be audited before releasing anything. Returning the Writer object itself early would be unsafe. |
| Whole-file writer and Numbered's `StringBuilder(text.Length)` | Technically reusable after final output creation, but unsuitable for an unbounded static pool. Retaining their large capacities across grammar compilations may increase persistent memory and process peak. |
| FirstSets sorting/merging lists | Already reused per thread, removed from their slots while borrowed and restored in finally. Their returned CharRange arrays belong to First results and cannot simply be returned to a temporary-buffer pool. |
| LexerEmitter bit-table scratch | Already reuses an 8,192-byte thread-local Filling array. Dictionary keys are cloned because modifying a retained key would invalidate lookup; those clones are not interchangeable scratch while the table remains in use. |
| Generated strings / SourceText | Owned by the consumer and Roslyn driver. Ordinary mutable-buffer pooling cannot reclaim them while the result remains live. |

Writer.ToString is non-consuming: for example, the reader members writer is inspected for `ways.` and read again when the reader is emitted. A pool return hidden inside ToString would break existing callers. Explicit rent/return (or disposal) must restore indentation and return only after the final use, including exception paths.

Recommended first experiment: a small bounded pool of temporary writer builders within an emission scope, limited by both retained capacity and entry count, discarding oversized buffers before resetting them. A giant builder must not be kept merely because Clear resets Length; retained capacity and reset behavior need accounting. Per-thread storage alone does not bound total IDE retention when several compiler threads participate. Keep the whole-file buffers and public result arrays out of the initial experiment.

The first comparison should include allocation, CPU time, process peak and post-release memory, exact generated hashes, nested use, concurrent compilations and exceptions. Existing profiles support the candidate selection but do not establish how much pooling would save or whether it would reverse the observed peak regression.

## Sixth optimization batch: one builder per reader method

ReaderWriter.Render now inserts the required local declarations before the body in its existing Writer. It no longer materializes the body as a string and copies that string into a second header/body writer. Fold-state detection searches the existing builder, starting beyond inserted declarations so that declarations cannot make an unused accumulator appear necessary. Mapped body columns are preserved.

This is one builder per rendered reader method, not yet one builder for the entire generated file. Method/part strings, the two reader-rendering passes and final file assembly remain. No static buffer pool or long-lived capacity cache was introduced. The user's preference for eliminating intermediate buffers takes precedence over the pool experiment proposed in the preceding audit.

### Measurements

Saved step-5 binaries were compared with the candidate over the same six fixtures. Library runs have two warm observations after a discarded cold cycle; tiny fixtures have nine. The table contains warm medians from the final control and candidate runs. No builds or tests ran concurrently with the measurements.

| Fixture | Step 5 time | Step 6 time | Step 5 allocation | Step 6 allocation |
|---|---:|---:|---:|---:|
| SQL | 22,450.47 ms | 20,634.67 ms | 20,021.85 MiB | 19,296.65 MiB |
| Finance | 2,885.81 ms | 3,305.65 ms | 4,985.49 MiB | 4,759.26 MiB |
| ExpressionLanguage | 925.93 ms | 1,030.20 ms | 370.34 MiB | 363.73 MiB |
| Web | 421.53 ms | 451.60 ms | 181.89 MiB | 185.57 MiB |
| Tiny | 4.80 ms | 5.02 ms | 0.90 MiB | 0.90 MiB |
| TinyStreams | 5.59 ms | 5.98 ms | 1.96 MiB | 1.96 MiB |

SQL allocated 725.20 MiB less (3.6%) and Finance 226.23 MiB less (4.5%) in this comparison. These are whole-generator allocation measurements, including runtime/cache variation, not an isolated count of removed builder allocations. Web allocation increased slightly. All **127 generated sources matched exactly** by hash, with no generator errors.

Timing does not establish an overall speed improvement: Finance and ExpressionLanguage were slower, while SQL was faster in the final pair. An earlier run of the same final candidate measured SQL at 27,649.62 ms and Finance at 4,174.94 ms; the subsequent control was 22,450.47 / 2,885.81 ms. That large run-to-run variation prevents attributing the final SQL speedup to this change. Builder searching and repeated prefix insertion also remain possible CPU costs; neither has been isolated by this experiment. This batch reduces intermediate body storage, but is not presented as a general build-speed win.

### Process memory

The corrected unprofiled retention harness ran three fresh SQL generations followed by five unrelated C# edits, then released its current driver. The control and candidate processes ran sequentially.

| Measurement | Step 5 | Step 6 |
|---|---:|---:|
| Peak working set | 4.749 GiB | 4.906 GiB |
| Working set after release | 2.478 GiB | 2.982 GiB |
| Managed live data after edits | 294.62 MiB | 294.61 MiB |
| Managed live data after release | 28.04 MiB | 28.03 MiB |
| Prior/current drivers alive after release | 0 of 8 | 0 of 8 |

Peak working set increased approximately 3.3% in this pair; post-release working set also increased. Removing one intermediate builder did **not** solve peak memory consumption. Managed live data was essentially unchanged and previous results collected in both versions. Reducing the overlap of the remaining method strings, retained machine-state writers and whole-file buffers remains the next ownership/lifetime investigation. No full Visual Studio solution-build improvement is claimed.

Artifacts under `.work/generator-analysis/`: `*-step5/6-single-writer.jsonl` and `.hashes`, the earlier candidate observations in `*-step6-single-writer-first-final.*`, `memory-sql-step5/6-single-writer/metrics.jsonl`, `measure-single-writer-final.py`, `measure-single-writer-control.py`, and `check-writer-memory.py`.

### Validation

Release build of the test project and dependencies passed with zero warnings/errors. All 19 focused Writer cases passed, including three new cases for prefix insertion, mapped columns and searching only the original body across builder chunks. Exact output hashes matched for all six fixtures. All **8,179 tests passed** with zero errors, failures or skips in 155.131 seconds (`tests-step6-all.log`). BOM, CRLF, tab indentation and `git diff --check` passed. A fetch confirmed that no commits from origin/main were missing from this branch at validation time.
