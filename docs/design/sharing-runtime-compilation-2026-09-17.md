# Runtime and compilation validation of machine sharing

## Status

The generator/source-size gains are real, but buffered character FIX has a
reproducible throughput regression. Do not treat buffered sharing as performance
validated or merge it solely on the generation results. Investigate the character
path before accepting it. No production changes were made during this measurement.

## Method

Compile the saved pre/post-sharing source dumps with Roslyn 4.14, Release optimization,
concurrentBuild=true, in fresh processes. Include handwritten source and common
framework references; exclude source generation. SQL includes all four generated
parser variants. FIX includes the example and its field model reference, not the
entire Examples project. Time parsing plus binding/optimization/IL emission; source
file reading is outside the timed interval. DLL output is saved after timing.
These are not full Visual Studio build times or Debug compilation timings.

Runtime uses separate AssemblyLoadContexts for before/after, with delegates bound
outside measurement. SQL calls TryParseStatement; ScriptDom 18.0.102.0 uses a reused
TSql170Parser and Parse. Both build their respective ASTs; they do not construct an
identical AST representation. This comparison is limited to the tested SELECT shape.
All parsers must report success. FIX uses the unchanged public Fix44 wrappers and
checks the fully consumed field count. Inputs and encoded byte arrays are prepared
outside timing; per-call StringReader/MemoryStream construction is included.
String FIX materializes an array; streaming FIX is fully enumerated without building
an equivalent array, so cross-form figures include this API distinction.

The final runtime results average the medians of nine rotating-order batches in each
of two fresh processes. Each parser/input combination warms for at least 1.5 seconds;
batches target about 50 ms, minimum three operations. Normal .NET tiered compilation
is enabled. Runtime allocations are current-thread allocated bytes over three warmed
operations, not retained/peak memory. FIX uses the same Release Finance assembly for
both variants; the earlier exploratory Debug-dependency runs are retained in raw data
but excluded from the final FIX table.

## Comparison with the historical SQL corpus

The synthetic results below do not replace the historical 7,716-statement corpus.
A subsequent exact-corpus recheck reproduced DotGram Located being 3.77-3.81 times
faster than ScriptDom. See sql-corpus-recheck-2026-09-17.md. The historical result
was not an insufficient-warmup artifact.

## SQL runtime

Input: SELECT * FROM t WHERE a0 = 1 AND ...; time in microseconds.

| Predicates | Before | After | ScriptDom | DotGram bytes/op | ScriptDom bytes/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| 1 | 2.48 | 2.49 | 5.62 | 1,384 | about 37,795 |
| 64 | 70.02 | 71.26 | 53.61 | 28,096 | 154,016 |
| 1,000 | 1,073.75 | 1,094.23 | 787.88 | 424,960 | 1,765,512 |

After sharing, SQL runtime is roughly unchanged to 2% slower in these runs; allocations
are unchanged. DotGram wins the short case, ScriptDom wins the larger conjunctions;
DotGram allocates substantially less in all three. Small differences are not a claim
of a statistically established regression or gain.

The first insufficiently warmed run appeared to show a ~3x regression. Disabling tiered
compilation made before/after agree, and longer warmup with normal tiering removed that
large difference. Keep cold/JIT-transition behavior separate from warmed throughput.
No controlled cold-start benchmark was performed.

## FIX runtime with Release Finance

Input repeats four fields: 55=ABC, 38=100, 54=1, 44=12.50, SOH-delimited.
Short input has four fields; large input has 4,000 fields.

| Input | Form | Before | After | Observation |
| --- | --- | ---: | ---: | --- |
| 4 fields | String | 3.64 us | 3.61 us | Essentially unchanged |
| 4 fields | Character stream | 3.07 us | 3.57 us | About 16% slower |
| 4 fields | Byte stream | 2.26 us | 2.32 us | Small difference, varies between runs |
| 4,000 fields | String | 293.63 ms | 293.04 ms | Essentially unchanged |
| 4,000 fields | Character stream | 2.937 ms | 3.405 ms | About 16% slower |
| 4,000 fields | Byte stream | 2.156 ms | 2.214 ms | About 3% slower on average |

Allocation sizes remain effectively unchanged: short string/character/byte paths
456/640/608 bytes; large paths about 15.36 MB/346.6 KB/280.3 KB. Small variation on
the string path is negligible relative to its total. The character regression is
repeated across processes and also appears with tiering disabled; it cannot be
explained solely by insufficient warmup. Its mechanism has not yet been established.

The extreme large-input string cost belongs to the existing stress grammar/API and
is present before sharing too. It is a separate optimization target.

## C# compilation

SQL has two runs each, baseline/candidate then candidate/baseline. FIX has one run
each, candidate then baseline; its timings are preliminary single-process samples.
All six compilations succeeded with no errors.

| Target | Before | After | Allocated before -> after | DLL before -> after |
| --- | ---: | ---: | ---: | ---: |
| Full SQL | 22.90 s | 17.07 s | 9.477 -> 6.451 GB | 22.28 -> 16.04 MB |
| FIX example | 174.07 s | 114.77 s | 29.409 -> 19.806 GB | 18.32 -> 12.62 MB |

Only ~2 seconds of FIX compilation is parsing C# text; most time occurs later in the
compiler. Reducing giant generated methods matters beyond merely reducing file I/O.

Raw samples and assembly hashes:
benchmarks/results/sharing-runtime-compilation-2026-09-17.json.
Scratch harness: .work/sharing-validation/Program.cs and check.csproj. Modes:
compile sql|fix before|after; run sql|fix. Generated dumps remain under
.work/sql-sharing and .work/fix-sharing. No build/test ran concurrently with timings.
