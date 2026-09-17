# Value-dependent switch: FIX header experiment and materialization fix

## Question

Can a grammar choose its continuation from a value it has already parsed, and can
keeping that value avoid duplicate work? FIX is a real instance: the tag and the
configured length/data dictionary decide whether the next input is ordinary text
or a length followed by a binary payload. A binary payload may contain separators.

## Experiment

The existing FixDispatch selector classifies a span containing the tag. Construction
later parses the tag again and looks up its binary partner. The trial added a Header
rule returning one int, shared by the selector, binary-length guard and construction:

- Positive: ordinary field tag.
- Negative: the paired binary tag.
- Zero: invalid input/tag.

This avoids another object or a wider header structure, repeated numeric conversion,
and repeated dictionary classification. It still requires a typed grammar value to be
built during recognition and retained for later construction. This cost matters even
when its tables are pooled and warm allocations remain unchanged.

The trial preserved public APIs, native byte/character spans, user-defined length/data
pairs, EOF termination and field locations. All Finance tests passed; the new boundary
test includes int.MaxValue tags and one-character/one-byte reader chunks.

## Quadratic work exposed by the trial

The shared engine's cached materializer linked newly appended entries incrementally,
but searched all prior arena entries for needed owners and again for completed values
on every guard. One new typed Header per field therefore revisited a growing history.
Streaming enumeration hides much of this because it releases records between yielded
fields. Whole-string parsing retains the field history until the final array is built.

The fix passes the earliest unbuilt capture requested by the guard to the materializer.
A descendant's call has a later arena index than its parent, so both value walks can
start at that bound. A guard asking for an older capture moves the bound back. Final
acceptance keeps the default zero bound; link maintenance, cached values and rollback
invalidation remain unchanged. This applies to ordinary when guards as well as switches.

Recovery grammars conservatively keep the zero bound. State-mark processing still
maintains its complete enclosing-mark chain: this change does not claim to remove every
possible quadratic path in grammars that use parserState.

## Validation

The focused regressions exercise nested typed values, an unused throwing factory,
1,000 repeated records, flat/adaptive/paged storage, and a guard that first asks for a
later capture and then an earlier unbuilt capture. No elapsed-time assertions are used.

- 54 focused tests, including switch and source snapshots, passed.
- All 8,281 core tests passed.
- All 3,802 Finance tests passed on the trial with the fix and after restoring the
  production grammar. Its generated FixDispatch source matches the baseline byte for byte.
- Compatibility builds passed for net8.0, netstandard2.0 and net472; the benchmark
  runner and final focused tests also passed.

## Measurement method

FixProfile loads each saved Release assembly in a separate load context. Each process
measures a public Parse call, full enumeration and input disposal. Each timed case has
three processes per variant, alternating variant order. No builds or tests run alongside
the measurements. Runtime: Windows x64, .NET 10.0.12, default tiered compilation settings.
Numbers are warmed microbenchmark medians, not confidence intervals or cold-start results.

Short cases use 50,000 warmup operations and 500,000 measured operations. Groups uses
100 warmups and 2,000 measured operations (3,011 fields); Raw uses 100 warmups and 15,000
measured operations (65,536 binary payload bytes). The scaling experiment uses 100 warmups
and 1,000 / 200 / 20 measured operations for 100 / 300 / 1,000 groups respectively.

The initial 2,000-iteration whole-string run of the unbounded typed-header trial was
stopped because it took too long. It is not included as a completed timing. The bounded
scaling runs replace it. Allocation figures are managed bytes allocated per operation,
not peak working set or retained pool capacity.

## Timing results

The same typed-header grammar before/after the materializer fix, **tiered compilation
disabled** in this scaling experiment to separate the growth curve from tier transitions:

| Groups / fields | Before, ms | Bounded walk, ms | Ratio |
| --- | ---: | ---: | ---: |
| 100 / 311 | 0.868 | 0.091 | 9.6x |
| 300 / 911 | 6.847 | 0.263 | 26.1x |
| 1000 / 3011 | 77.096 | 0.853 | 90.4x |

Ten times as many groups previously cost roughly 89 times as much; with the bound,
the cost grows approximately tenfold. This is the typed-header experiment, **not an
90x improvement to the shipping FIX grammar**, which does not create that typed value.

Current FixDispatch versus the typed-header trial **after** the materializer fix,
with normal tiered compilation enabled. Microseconds per public parse/enumeration:

| Input / workload | Current grammar | Typed header, fixed | Allocated B/op, both |
| --- | ---: | ---: | ---: |
| Bytes / Tag1 | 0.608 | 0.614 | 440 |
| Bytes / Order | 2.225 | 2.861 | 1,600 |
| Bytes / Groups | 400.399 | 506.353 | 169,360 |
| Bytes / Raw | 36.270 | 37.121 | 66,808 |
| Characters / Order | 2.455 | 3.236 | 1,424 |
| Characters / Groups | 484.262 | 612.319 | 265,096 |
| String / Order | 2.314 | 2.876 | 1,296 |
| String / Groups | 521.832 | 599.212 | 288,936 |

The grammar refactoring was rejected: ordinary fields lose time even with the quadratic
walk removed. Parsing a handful of tag digits again is cheaper here than adding a typed
rule, its arena records and early materialization. Warm allocation equality reflects
pooled tables; it does not establish equal retained memory.

Generated FixDispatch source sizes: current 488,686 bytes; typed header 520,850 bytes;
typed header with bounded materialization 523,800 bytes. The factory is unchanged and
excluded from all three sizes. The final production grammar is restored to the current
span-based classifier. The general materializer fix and regression tests are retained.

## Reproduction and implications

[Raw runs](../../benchmarks/DotGram.Finance.Benchmarks/results/2026-09-16-value-dependent-switch-raw.jsonl)
include assembly hashes. The [trial patch](../../benchmarks/DotGram.Finance.Benchmarks/results/2026-09-16-value-header.patch)
contains only the rejected grammar/context changes. Apply it on 5f4d0eb for the unbounded
trial or on this change for the bounded one using `git apply --unidiff-zero` (the patch has no context lines). Save each Release Finance assembly separately.
The existing FixProfile CLI now accepts Groups:N for scaling experiments:

```powershell
dotnet benchmarks/DotGram.Finance.Benchmarks/bin/Release/net10.0/DotGram.Finance.Benchmarks.dll profile BASELINE.dll String Groups:1000 20 FixDispatch
```

For the scaling table set DOTNET_TieredCompilation=0 for both assemblies. For the main
comparison leave tiered compilation at its default. Do not benchmark during builds/tests.
The normal tiered scaling trial had visible tier transitions on short samples and is
not used for the growth-curve table.

Switch is appropriate when a parsed value dictates the wire structure: record type,
protocol version, a length/data pairing, flags selecting optional sections. Its benefit
comes from eliminating wrong structural parses. It is not a reason to introduce a new
typed grammar value solely to cache a very cheap conversion. Prefer values already
needed by recognition, or span-based classification as the current FIX parser does.
