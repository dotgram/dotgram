# Skipping materialized subtrees in large tape readers

## Result and scope

Large, split materializers without state marks now visit only live records that still
need construction. Small materializers and materializers with marks retain their prior
generated walk. This is selected at generation time using the existing method partition,
not a new parser option or runtime policy. No arrays, maps, counters, or retained fields
are added to generated parsers.

The measured MSSQL implementation improves paired throughput by 4.0-9.2% with tiered PGO
in the scale sweep and by 2.64% on the mixed statement corpus with tiering off. Allocations
are unchanged. ExpressionLanguage, URL, and tiny parsers keep their previous strategy;
this step claims no speedup for them.

## Profiling evidence

Temporary counters were inserted into direct materializers and removed before timing.
The unmodified runtime baseline and instrumented binaries are preserved under
.work/walk-profile/baseline-bin and instrumented-bin. ProfileScanned counts the sum of
record-window lengths across calls, not the total of all three passes through them.
ProfileWrites counts values whose materialization body runs. Reachable counts includes
already-built records reached again. Stopwatch counters are intrusive diagnostic data,
not timings used to claim performance.

| Input | Materializer calls | Already-built requested roots | Sum of window lengths | Reachable visits | Values built |
| --- | ---: | ---: | ---: | ---: | ---: |
| MSSQL, 1 condition | 3 | 0 | 32 | 32 | 16 |
| MSSQL, 64 conditions | 3 | 0 | 1,166 | 1,166 | 583 |
| MSSQL, 1,000 conditions | 3 | 0 | 18,014 | 18,014 | 9,007 |
| MSSQL, 10,000 conditions | 3 | 0 | 180,014 | 180,014 | 90,007 |
| ExpressionLanguage, identity lambda | 3 | 0 | 9 | 9 | 8 |
| ExpressionLanguage, 1,000 assignments | 1,004 | 0 | 9,011 | 9,011 | 8,010 |

None of the sampled calls asked for an already-built root, so an entry-level early return
was not the useful optimization for these workloads. Previously built subtrees within
an unbuilt root are the relevant repetition. A first prototype only stopped descending
into their dependencies; it still walked the complete tape and did not give a convincing
general improvement. The final version also skips those records during construction.

## Implementation and rollback

The existing reverse reachability pass clears Live for records whose values are already
built, and does not descend through them. Their factories have already consumed their
children, so the requested root needs their stored values, not another traversal of the
children. An unbuilt record shared through another dependency is still marked normally.

During construction, a live record is processed directly. On a hole, IndexOf searches the
existing boolean Live array for the next needed record. Its tape address comes from the
existing Starts window index. This avoids reading record headers just to skip values
already built by a guard. Dense indexing is unaffected: final-only dense materializers
do not run this guarded path.

The existing rollback watermark invalidates Built for reused record numbers before the
reachability pass. It is preserved unchanged. Materializers with marks retain their
ordered walk because state transitions must still be replayed even across records whose
values do not need construction.

The final eligibility also requires an already-partitioned materializer. Broader trials
did not establish a benefit for tiny parsers, so they retain the original emitted code.
This uses the existing method-size decision rather than a new value-type threshold.

## Controlled timing

Baseline and candidate run in separate load contexts in one process. Each comparison
uses 15 rounds with rotating order. No build or test runs alongside timing. Ratios are
medians of paired baseline/candidate time ratios; they need not equal the ratio of the
two separately reported median times.

| SQL conditions | Before us, PGO | After us, PGO | Paired speedup | Before us, tiering off | After us, tiering off | Paired speedup |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 2.627 | 2.551 | 1.040 | 3.130 | 3.078 | 1.017 |
| 64 | 78.356 | 74.032 | 1.057 | 88.506 | 85.605 | 1.031 |
| 1,000 | 1,170.978 | 1,105.204 | 1.060 | 1,332.908 | 1,278.766 | 1.039 |
| 10,000 | 12,265.260 | 11,168.680 | 1.092 | 13,048.060 | 12,836.920 | 1.021 |

Warm allocation is respectively 1,384; 28,096; 424,960; and 4,240,960 bytes in both builds.
These are local measurements, not a guarantee for every grammar or machine.

### MSSQL versus ScriptDom

The harness verified identical ordered input: 7,716 statements, SHA256
6088BEB886476100640001E0C60D430272E9E21D463A3C9241BBE19695AEF136.
With positions retained and tiering off, median time per statement changed from
5,008.00 to 4,877.92 ns; paired speedup was 1.0264. ScriptDom took 18,779.48 ns, giving
3.8135 times the candidate's time in paired rounds. Allocation remained 1,716.59 bytes
per statement for DotGram versus 42,543.90 for ScriptDom.

### ExpressionLanguage and tiny-parser controls

The initial ExpressionLanguage PGO samples showed tiering changes during the first
inputs. The revised harness warms every input in both contexts before measuring any
input; results still did not establish a general benefit from pruning alone. Its marked
materializer is therefore restored to the previous walk. Broader selected-walk tiny
samples likewise did not justify changing tiny generated methods. Their final code is
restored by the partition eligibility rule. Do not report the experimental samples as
performance changes in the final implementation.

URL was included in the first corpus run as a control. This change does not alter its
materializer path. The final snapshot checks cover its generated code.

## Code size and evidence

For MSSQL, generated source grows by 1,673 normalized bytes and parser IL by 251 bytes;
the located variant grows by 1,722 source bytes and 257 IL bytes. No runtime profiling
fields remain. Final MSSQL generated-source hashes exactly match the measured selected
variant after narrowing eligibility. ExpressionLanguage and small materializers no longer
receive the experimental pruning-only change.

Artifacts under .work/walk-profile:

- counts.txt and the instrumented source copies: initial diagnostic counters.
- selected-sql-{pgo,off}.txt and selected-corpus.txt: accepted MSSQL measurements.
- selected-expression-{pgo,off}.txt and selected-tiny-pgo.txt: broader candidates, not
  claims about the final unchanged control parsers.
- final-size.jsonl: final generated-source and parser-IL measurements.
- final-full-tests.txt, final-sql-tests.txt, final-compatibility.txt: final validation.

The scale harness is .work/mssql-dense-speed, corpus harness .work/paired-comparison,
and improved ExpressionLanguage harness .work/walk-profile/expression-speed. Benchmark
candidate binaries are preserved in selected-bin; the original baseline is baseline-bin.
For the two JIT profiles set DOTNET_TieredCompilation=0, or DOTNET_TieredCompilation=1
and DOTNET_TieredPGO=1, respectively. Never mix absolute timings across those profiles.

Final validation completed: all 6,594 main tests and 14,628 SQL tests passed. The new
partitioned regression grammar exercises skipped gaps, repeated guards, and rollback
that reuses a record ordinal; small fixtures cover all four storage choices. C# 8
compatibility builds passed for net8.0, netstandard2.0, and net472, without warnings or
errors. Final ExpressionLanguage source-size and parser-IL measurements match the
original baseline. git diff --check passed; local main and HEAD remain synchronized.
