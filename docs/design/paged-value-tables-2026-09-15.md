# Value tables with pages from the first value

## Scope

This experiment compares explicit `GramValueStorage.Paged` with the previously measured
Adaptive tables. Paged allocates no per-type arrays at construction, keeps stable record
numbers, and allocates a directory and 64-slot page only when that type is written.
The first 256 records receive no special array. Writes create pages; reads of already
materialized records use a separate direct page lookup. Existing rollback, reachability,
Built flags, clearing, and aggregate pool-retention limits remain in effect.

The new choice is available through Gram, GramOptions, and ValueStorageKind in the
compiler API. Auto does not select it. The MSSQL source was temporarily compiled with
Paged for measurement; the shipping source keeps its previous Auto setting.

## Method

Baseline binaries are in .work/paged-values/adaptive-bin. The first page-only prototype
is retained in paged-v1-bin; it used the creating indexer for reads as well as writes.
The final variant separates those operations. SQL correctness is checked before timing
against the full SQL tests. The paired corpus harness verifies the same 7,716 ordered
statements (SHA256 6088BEB886476100640001E0C60D430272E9E21D463A3C9241BBE19695AEF136).

The scale harness uses 1, 64, 1,000, and 10,000 AND conditions. Each comparison rotates
execution order for 15 rounds, with warmup before collection. Tiering-off and tiered-PGO
runs are separate profiles; absolute numbers must not be combined across them.

The tiny benchmark deliberately forces typed storage with a repeated valued rule and
a value guard on a two-character input. A simpler fixed-literal grammar compiled without
value tables; its original tiny-off/tiny-pgo results are not evidence about table access
and are excluded. The corrected benchmark is retained as ValueStorageBenchmarks.

Fresh-store allocation is measured after warming JIT and discarding thread pools outside
the measured call. Retained payload counts backing array elements and directories, not
object headers or process working set. Steady-state allocation includes returned values.

## Initial prototype

Using the creating indexer for every read made MSSQL slower than Adaptive in both JIT
profiles. On the statement corpus, paired Adaptive/Paged throughput ratio was 0.831
(about 20% longer execution time for Paged). This prompted the direct-read variant.
The allocation layout was already useful: fresh short-SQL storage fell from 11,256 to
7,944 bytes, and retained backing-array payload after 64 conditions fell from 126,720
to 33,888 bytes. These are storage savings, not claims of improved throughput.

## Final direct-read variant

Times below compare Adaptive and Paged within the same process, with rotating order.
The paired ratio is Adaptive time / Paged time; below one means Paged is slower.

| SQL conditions | Adaptive, us (tiering off) | Paged, us (tiering off) | Paired ratio | Adaptive, us (PGO) | Paged, us (PGO) | Paired ratio |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 2.983 | 3.366 | 0.886 | 2.610 | 2.837 | 0.919 |
| 64 | 83.419 | 98.861 | 0.840 | 73.970 | 87.836 | 0.839 |
| 1,000 | 1,250.692 | 1,506.636 | 0.830 | 1,119.092 | 1,345.338 | 0.830 |
| 10,000 | 12,638.160 | 15,275.640 | 0.832 | 11,254.480 | 13,512.260 | 0.838 |

Warm allocations are identical: respectively 1,384; 28,096; 424,960; and 4,240,960 bytes.
The direct-read variant still takes about 9-20% longer with PGO. The initial and final
prototype runs happened at different host speeds; their absolute times are not a direct
measurement of the read specialization alone.

On the 7,716-statement corpus with locations and tiering off, Adaptive took 4,982.74 ns
per statement and Paged 6,198.24 ns. The paired ratio was 0.8060: Paged takes about 24%
longer. Allocation remained 1,716.59 bytes per statement. ScriptDom took 18,222.08 ns and
42,543.97 bytes; Paged was still 2.9422 times faster than ScriptDom in this run, but slower
than Adaptive. These are local controlled-harness results, not a universal guarantee.

### Tiny parser

The corrected benchmark uses a repeated valued rule and a typed guard on input `aa`.
Both source inspection and nonzero fresh-store allocation confirm that tables are used.

| Strategy | Time, ns (tiering off) | Time, ns (PGO) | Warm bytes/call | Fresh-store bytes/call |
| --- | ---: | ---: | ---: | ---: |
| Flat | 73.20 | 65.26 | 64 | 1,104 |
| Adaptive | 76.91 | 69.47 | 64 | 1,120 |
| Paged | 75.90 | 69.15 | 64 | 1,392 |

Paged and Adaptive are effectively tied on this tiny parser; a sub-2% difference is not
convincing evidence of a win. Flat has the lowest measured time and initialization
allocation here. Paged's first page plus directory costs more than a 16-slot prefix
when the grammar uses only one value type. Lazy allocation is therefore not inherently
cheaper for small grammars.

### Memory and generated code

The final resource probe confirms the same layout savings as the initial prototype.
Fresh short-SQL allocation is 11,256 -> 7,944 bytes. Retained array payload is:

| Conditions | Adaptive bytes | Paged bytes |
| ---: | ---: | ---: |
| 1 | 7,076 | 4,964 |
| 64 | 126,720 | 33,888 |
| 1,000 | 551,112 | 458,184 |
| 10,000 | 4,082,472 | 3,989,544 |

The last row remains retained when returning to a one-condition input. First-growth
allocation is not always lower: after the preceding inputs, the first 10,000-condition
parse allocates 11,385,752 bytes with Adaptive and 11,391,520 with Paged. This first-use probe can also include helper/JIT effects and
should not be read as pure backing-array allocation.

MSSQL normalized generated-source bytes fall from 32,714,898 to 31,994,660 (-2.20%),
and located source from 34,020,149 to 33,230,227 (-2.32%). Parser IL bytes fall from
10,380,178 to 10,037,609 (-3.30%), and located IL from 10,684,730 to 10,315,075 (-3.46%).
These measure source and IL, not native JIT code.

## Decision and interpretation

Retain Paged as an explicit memory/code-size tradeoff. Do not add it to Auto or replace
Adaptive with it. Removing the prefix saves eager per-type arrays, but every access
then follows the directory and page. Adaptive can access its prefix through a cached
array. The direct-read helper removes allocation checks from reads; it cannot remove
that additional level of indirection. The measurements establish the tradeoff; they do
not isolate individual CPU costs with hardware counters.

This step measures MSSQL and a tiny valued parser. It does not establish performance
for ExpressionLanguage or URL. Their default strategies are unchanged.

Validation: 6,589 main tests and 14,628 SQL tests passed; the SQL suite used the final
Paged variant. All C# 8 compatibility targets (net8.0, netstandard2.0, net472) built
without warnings or errors. After restoring the normal MSSQL option, the benchmark
build passed and default-size.jsonl matched the previous source-size and parser-IL
measurements. git diff --check passed.

Final evidence is in .work/paged-values/v2-{scale-off,scale-pgo,tiny-off,tiny-pgo,corpus,
resource}.txt and v2-size.jsonl. Final measured binaries are in paged-v2-bin. The tiny
probe is .work/paged-values/tiny; the scale and corpus harnesses are the same ones used
in the Adaptive report. Run comparisons sequentially, with DOTNET_TieredCompilation=0,
or with DOTNET_TieredCompilation=1 and DOTNET_TieredPGO=1 for the separate PGO profile.
