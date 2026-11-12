# URL and MSSQL parser comparison, 2026-09-15

## Scope and method

Before is commit 7bbc3ec (current main) without the uncommitted parser optimizations;
after is the working tree with those optimizations. This holds the grammar revision
constant. The baseline was built from a git archive into .work/strategy-audit/main-before;
no shared Git metadata or existing checkout was changed.

Both Release assemblies were loaded into separate AssemblyLoadContexts in one process.
The existing UrlBenchmarks and ScriptDomBenchmarks methods were bound to delegates;
reflection was used for setup only, outside timed loops. Methods ran in rotating order
for 15 rounds after three warmups. DOTNET_TieredCompilation=0; .NET 10.0.12 x64.
A second process reversed assembly loading order. A third process repeated SQL alone.
Allocation counts were taken outside timing. Ratios are medians of paired rounds,
not necessarily the quotient of the independently reported timing medians.

URL setup verified success and extracted parts against Regex for both versions.
SQL setup selected exactly the same 7716 statements in both versions and in every run;
each timed parser, including the located variant, successfully read all of them.
The SHA256 of the ordered, NUL-separated statements was
6088BEB886476100640001E0C60D430272E9E21D463A3C9241BBE19695AEF136.
This establishes matching benchmark populations and success counts, not a new proof
of AST equivalence. ScriptDom uses the corpus file's designated parser version.

## URL

Representative medians from the second run, extracting all parts, in nanoseconds:

| Input | Before | After | Regex | Compiled Regex | Our allocation |
| --- | ---: | ---: | ---: | ---: | ---: |
| http://example.com | 169 | 166 | 1017 | 505 | 264 B |
| https://user@example.com:8080/a/b/c?q=1&r=2#top | 214 | 192 | 986 | 490 | 352 B |
| https://192.168.0.1/ | 141 | 132 | 948 | 496 | 200 B |

The port/query/fragment input took about 10% less time in both runs, for both host-only
and all-part extraction. IPv4 all-part extraction took about 6-7% less time; host-only
extraction did not show the same improvement. Short URL and long-path changes were
small or unstable. The invalid URL all-part path was 1-2% slower in the observed paired
ratios, but that small difference should not be treated as a confirmed regression.

For successful inputs with all parts requested, the current parser was 2.1-3.8 times
faster than compiled Regex and 4.9-8.0 times faster than interpreted Regex across the
two runs. Host-only advantages were smaller: 1.4-2.4 times over compiled Regex.
These ranges include different inputs, not confidence intervals for one input.

Allocations did not change between our before/after builds on any tested URL. For the
three successful rows above, Regex allocated 1064, 1336 and 1088 bytes respectively
when all parts were requested. Refusal allocated zero bytes in both implementations.

## MSSQL / ScriptDom

The fairer comparison is the located parser because ScriptDom always stores positions.
One timed operation read the full shared corpus; figures below are divided by 7716.

First run, with the smallest SQL timing spread:

| Parser | Before | After | Allocation per statement |
| --- | ---: | ---: | ---: |
| DotGram with locations | 5.578 us | 5.548 us | 1716.59 B, unchanged |
| DotGram without locations | 4.427 us | 4.430 us | 1716.59 B, unchanged |
| ScriptDom | 17.930 us | same reference measurement | about 42544 B |

The located parser was 3.19, 3.38 and 3.41 times faster than ScriptDom in the three
paired runs and allocated about 24.8 times less. Absolute times drifted upward in the
later runs: the third run measured 8.485 us before, 8.749 us after and 29.164 us for
ScriptDom. The median paired before/after ratio for the located parser was 1.0015,
0.9195 and 0.9963. This variability does not support a claim that the optimizations
sped up MSSQL; it also does not establish a stable regression. The allocation result
is unchanged in all runs. The nonlocated parser likewise showed no consistent change.

These are warm corpus comparisons. They do not override the separate repeated-large
SQL92 result: eviction of oversized pools increases allocation substantially there.

## Artifacts

Harness: .work/paired-comparison/Program.cs and paired.csproj.
Baseline build log: .work/strategy-audit/before-build.txt.
Raw measurements in .work/strategy-audit:

- paired-results.txt
- paired-reverse-results.txt
- paired-sql-results.txt

The executable takes baseline and candidate benchmark DLL paths. An optional third
argument reverses load order; `sql` additionally skips URL cases. Run without builds
or tests in parallel, under the same DOTNET_TieredCompilation setting for comparisons.

## Generated code size

Source byte counts below normalize both repository roots to /_ and exclude the UTF-8 BOM. This removes the unrelated difference in absolute #line paths caused by the longer baseline directory. Other formatting is preserved. Both builds use the same grammar revision and Release net10.0.

| Parser | Before, bytes | After, bytes | Change | Lines before | Lines after |
| --- | ---: | ---: | ---: | ---: | ---: |
| DotGram.Sql.Standard.Sql92Parser.g.cs | 766826 | 767361 | +0.070% | 16932 | 16940 |
| DotGram.Sql.Standard.SqlStandardParser.g.cs | 27038028 | 27038470 | +0.002% | 826630 | 826637 |
| DotGram.Sql.TransactSql.TransactSqlParser.g.cs | 31933634 | 31955958 | +0.070% | 694263 | 694961 |
| DotGram.Sql.TransactSql.TransactSqlParser.Located.g.cs | 33163914 | 33187143 | +0.070% | 716002 | 716700 |
| DotGram.ExpressionLanguage.ExpressionParser.g.cs | 1847641 | 1842916 | -0.256% | 49582 | 49465 |
| DotGram.ExpressionLanguage.ExpressionParser.Immediate.g.cs | 1624648 | 1607036 | -1.084% | 42249 | 41643 |

| Parser type and nested types | IL before, bytes | IL after, bytes | Change | Methods before | Methods after |
| --- | ---: | ---: | ---: | ---: | ---: |
| DotGram.Sql.TransactSql.TransactSqlParser | 9982198 | 9987409 | +0.052% | 13906 | 13921 |
| DotGram.Sql.Standard.Sql92Parser | 89503 | 89730 | +0.254% | 358 | 358 |
| DotGram.Sql.Standard.SqlStandardParser | 5927302 | 5927421 | +0.002% | 13655 | 13655 |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 10254333 | 10259595 | +0.051% | 14126 | 14141 |
| DotGram.ExpressionLanguage.ExpressionParser | 373382 | 372192 | -0.319% | 1885 | 1883 |
| DotGram.ExpressionLanguage.ExpressionParser.Immediate | 257966 | 254619 | -1.297% | 1422 | 1387 |

IL counts sum method instruction bytes, excluding method headers, exception tables, metadata and embedded PDBs. A parser group includes its nested types and handwritten partial members; Located and Immediate variants are separated. It is compiled IL, not JIT native-code size.

Raw DLL sizes fell from 43898880 to 43808768 bytes for DotGram.Sql and from 1646080 to 1556480 for ExpressionLanguage. Both contain embedded debug information, so those reductions must not be interpreted as equivalent reductions in executable code. SQL IL actually increased slightly.

The result is a small size improvement for ExpressionLanguage, especially Immediate; MSSQL source and IL grew slightly. Line counts alone are insufficient: SQL source size and instruction bytes tell more than file size with unnormalized paths.

Reproduction: dotnet run --project benchmarks/DotGram.CodeSize -c Release -- <before-repository> <after-repository>. Build both parser projects for Release net10.0 first. The tool reads existing generated artifacts and PE metadata without loading or executing the measured assemblies. JSON-lines output is saved locally in .work/strategy-audit/code-size-normalized.jsonl.


## Subsequent dense-storage step

The comparison above records the state before dense tables were introduced. The next
step changes SQL92 final materialization with at least eight value types; it does not
change the generated MSSQL, SQL Standard or ExpressionLanguage source/IL from the
preceding measurements. SQL92 adds 10352 normalized source bytes (1.349%) and 4428 IL
bytes (4.93%). The before-dense source/IL totals were 767361/89730 bytes and are now
777713/94158 bytes. Guards that build values and can rewind still use sparse tables.

Repeated 10000-condition allocation falls from about 11.42 to 2.96 MB, and 50000 from
80.15 to 48.49 MB. Paired warm runs show 3.27-3.46x and 1.31-1.33x speedups respectively.
A 64-condition input was 1-2.5% slower; 1000 conditions were near parity. Short input
allocation stayed at 256 bytes. See automatic-parser-strategies.md for the cache,
cleanup and validation details, including the final 3849-test run.


## Guarded dense-storage experiment (not adopted)

A subsequent prototype extended dense tables to machines that materialize values in
`when` guards. It kept a separate record-to-slot map and a per-type slot-to-record map.
On rollback it removed abandoned values by moving the last value of that type into the
vacated slot and updating its owner's index. This handles values built out of record
order without retaining every failed attempt. The final-only dense SQL92 path was unchanged.

The baseline for this experiment is the completed final-only dense step, not `main`.
Both benchmark binaries were loaded in separate assembly contexts in one process,
with tiered compilation disabled, warm-up and 15 alternating rounds. No builds or tests
ran alongside the timed calls. Paired ratios are medians of per-round ratios, not
quotients of independently calculated medians. Absolute timings were variable on this
host; repeat runs and allocation counts are more useful than a single time median.

### MSSQL

| Conditions in SELECT WHERE | Warm bytes before | Warm bytes prototype | Before/prototype time ratio, run 1 | Run 2 |
| --- | ---: | ---: | ---: | ---: |
| 1 | 1384 | 1384 | 0.957 | 0.992 |
| 64 | 28096 | 28096 | 0.934 | 0.887 |
| 1000 | 424960 | 424960 | 0.956 | 0.923 |
| 10000 | about 109.56 MB | about 11.18 MB | 1.352 | 1.573 |

Ratios above one favor the prototype. The largest case improves substantially, but
64 conditions regress by about 7-13% and 1000 by about 5-8%. One condition ranges from
near parity to about 4.5% slower. This does not meet the goal of preserving frequent
short calls.

Cached array payload after 1000 conditions falls from 7292152 to 726024 bytes, including
Ways and DirectValues. At 10000 conditions both versions discard their oversized value
store; the prototype allocates a much smaller replacement on each call. After the next
small parse the DirectValues payload is 9440 rather than 6240 bytes: the extra ownership
arrays also make small fresh infrastructure more expensive. These are array payloads,
not total managed heap size or process working set. Initial first-call allocation also
contains JIT/runtime setup and is not used as a parser-startup comparison.

Both versions accept the same ordered 7716-statement ScriptDom corpus, with SHA256
6088BEB886476100640001E0C60D430272E9E21D463A3C9241BBE19695AEF136.
On that corpus the two runs give before/prototype located ratios of 0.9835 and 1.0215:
there is no established throughput improvement. Prototype located/ScriptDom comparisons
favor our parser by 3.3252x and 3.4072x, with 1716.59 versus about 42544.1 bytes per statement.
These are measurements of the rejected prototype, not a new performance claim for the
working generator. URL code was unaffected by this experiment; it was not timed again.

### ExpressionLanguage and generated code

Both versions pass the 176-shape language/tree equivalence check. Short expressions
mostly regress by about 5-10%, without a reduction in warm allocation. For example,
`(int x) => x` goes from 1.714 to 1.838 microseconds in the first run, with 936 bytes in
both versions. A block of 1000 `x += 1;` operations stays near parity in both runs
(paired ratio 0.998), with 209504 bytes allocated in both versions.

| Parser | Additional normalized source bytes | Source growth | Additional method IL bytes |
| --- | ---: | ---: | ---: |
| MSSQL | 729850 | 2.28% | 479962 |
| MSSQL Located | 775288 | 2.34% | 504823 |
| ExpressionLanguage Tape | 59654 | 3.24% | 27452 |
| SQL92 / SQL Standard / ExpressionLanguage Immediate | 0 | 0% | 0 |

The prototype passed six targeted tests, including an artificial out-of-order ownership
case and 1000 repeated rollbacks without table growth. It was not run through the full
suite because the performance gate already rejected it. Its emitter changes were
removed. Two grammar-level rollback regression cases remain, covering character and
lexical parsing where a rejected guard's int-valued record is replaced by a long-valued
record, including repeated calls and growth.

The next design must select storage for the particular parse, not only count grammar
types. Any size-dependent choice must be fixed before the first guard builds a value,
keep the short sparse path inexpensive, and preserve Built flags, values and rollback
ownership throughout the parse. Switching representations at an arbitrary later
materialization is unsafe without migrating live values. Duplicating materializers
also has a code-size cost that must be measured before accepting such a design.

Local evidence in .work/strategy-audit: guard-dense-sql{,-repeat}.txt,
guard-dense-mssql-scale{,-repeat}.txt, guard-dense-expression{,-repeat}.txt,
guard-dense-resource.txt and guard-dense-size.jsonl. The prototype sources and binaries
are preserved in guard-dense-prototype and guard-dense-prototype-bin; the preceding
binaries are in pre-guard-dense-bin. The resource and timing harnesses are under
.work/guard-dense-resource, .work/guard-dense-speed and .work/mssql-dense-speed.

Final verification after removing the prototype: the five DenseValuesTests cases pass;
the test and benchmark projects build with zero warnings/errors; git diff --check passes.
All source-size and compiled-IL measurements exactly match pre-guard-dense-size.jsonl,
verified against guard-dense-restored-size.jsonl. The three emitter files also match the
saved pre-experiment bytes exactly. The full suite was not rerun for the restored emitter;
its prior 3849-test result is recorded above. HEAD and local main both remain at 7bbc3ec.


## Adaptive tables follow-up

The next experiment keeps a direct flat prefix and adds lazy pages beyond it. The
selected implementation improves measured MSSQL short and large parses and leaves
ExpressionLanguage and URL on their previous strategies. A true interface-backed
adapter replacement was also implemented and measured, including a tiered JIT/PGO run.
It preserved correctness but was slower than the selected direct path. Results,
retention and first-use costs, generated-source/IL growth, the refreshed main baseline,
and reproduction artifacts are in [Adaptive value tables](adaptive-value-tables-2026-09-15.md).
