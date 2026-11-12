# Adaptive value tables: 2026-09-15

## Outcome

Keep direct indexed arrays for small parsers and the existing final-only dense strategy.
For guarded Tape materialization, including Auto machines that stay on Tape, with at least 32 value types, when their shared store does not
also serve a final-only dense machine, use a flat prefix followed by lazy pages. The
32-type gate is a conservative initial policy backed by the MSSQL/ExpressionLanguage
experiments below, not a globally optimal cutoff. No parser option was added.

The worktree was fast-forwarded from 7bbc3ec to local main 0704f56, preserving the earlier
uncommitted optimizations. The baseline for this report includes those optimizations
and the updated grammar revision. It is not a comparison against an older grammar or
against unmodified main.

## Storage and access

Each typed table starts with the same 16-element array as before. Its contiguous prefix
can grow to 256 elements. Records beyond the prefix use 64-element pages, allocated only
when that particular table is accessed. An unused type does not allocate pages merely
because the parser recorded many values of other types.

Record numbers remain stable. Growing beyond the prefix does not move its values into
a different numbering scheme, rerun factories, or require record-to-value and reverse
ownership maps. Rollback retains the existing Built watermark semantics; overwritten
records replace old values, and the parse's high-water count bounds clearing on return.
Page allocation is bounded by the record positions accessed, rather than the cumulative
number of backtracking attempts. Arrays are still subject to the existing aggregate
1048576-element retention budget; page-directory entries are included in that count.

The final materializer caches the flat arrays and emits a direct access when the record
index fits. Only the other branch calls the adapter. Unsigned comparisons cover both
array bounds in one test. Room is called only when the flat prefix actually needs to
grow. The adapter is a struct embedded in DirectValues, so there is no separate adapter
object per type. Reads by guards can use its ref-returning indexer.

A materialization walk must not retain array aliases across Room calls. The generated
walk takes fresh aliases after Room; allocating a page leaves the existing prefix and
other pages in place. Reentrant parsers rent their own stores.

## MSSQL results

Two assemblies are loaded in separate contexts in one process. Input strings are
identical, calls are warmed, and execution order alternates across 15 rounds. No builds
or tests run alongside timing samples. Ratios are medians of paired rounds, not ratios
of independently calculated medians. The host was not isolated, so absolute timings
are observations rather than machine-independent claims.

| SELECT WHERE conditions | Warm allocation before | After | Before/after time, tiering off | Before/after time, tiered JIT/PGO on |
| --- | ---: | ---: | ---: | ---: |
| 1 | 1384 B | 1384 B | 1.141 | 1.178 |
| 64 | 28096 B | 28096 B | 1.189 | 1.226 |
| 1000 | 424960 B | 424960 B | 1.186 | 1.255 |
| 10000 | about 109.56 MB | about 4.24 MB | 1.617 | 2.248 |

Thus this version improves short inputs as well as the large one. The large-input gain
also depends on the new store fitting within the retention budget and being reused.
The JIT profiles were measured separately; their absolute timings must not be mixed.
PGO results are an additional profile, not a guarantee that every workload reaches the
same optimized code with the same warm-up.

### Against ScriptDom

Both versions accept the same ordered 7716-statement corpus, SHA256:
6088BEB886476100640001E0C60D430272E9E21D463A3C9241BBE19695AEF136.

| Measurement | Run 1 | Run 2 |
| --- | ---: | ---: |
| Previous located / adaptive located time | 1.2020 | 1.1496 |
| ScriptDom / adaptive located time | 4.0186 | 3.8535 |
| Adaptive located median time per statement | 7.070 us | 7.146 us |
| Previous located median time per statement | 8.376 us | 7.869 us |
| ScriptDom median time per statement | 28.315 us | 26.740 us |

Warm allocation stays at 1716.59 bytes per statement, versus about 42544 bytes for
ScriptDom. Located is the appropriate comparison because both retain source positions.
The corpus confirms identical acceptance for the timed inputs; tree checks are separate.

### Retention and first use

These payload measurements include backing arrays and page directories but exclude
object headers and scalar fields. They are not process working-set measurements.

| Conditions, in successive calls | Cached payload before | After |
| --- | ---: | ---: |
| 1 | 7076 B | 7076 B |
| 64 | 470240 B | 126720 B |
| 1000 | 7292152 B | 551112 B |
| 10000 | 2143804 B | 4082472 B |
| 1 after the large call | 2150044 B | 4082472 B |

At 10000 conditions the old value store is evicted; the adaptive store is retained.
Keeping its buffers consumes more cached memory after that call but avoids reallocating
about 105 MB per subsequent large parse. All stored value references are cleared.

A small parse after discarding the pools, with JIT already warmed and pool reset outside
the measurement, allocates 10488 bytes before and 11256 bytes after: **768 extra bytes**
for the inline adapter fields. Ordinary warm small parses remain at 1384 bytes. This is
fresh-store allocation, not cold-process startup or JIT time.

## Interface replacement was measured too

A separate prototype implemented IValueTable<T> with FlatValueTable<T> and
PagedValueTable<T>. Room returned the same adapter or a replacement object retaining the
old prefix. All value access went through the interface. It passed the five transition,
rollback, factory-count, reference-clearing and reentrancy tests.

| Conditions | Concrete/interface time, tiering off | Concrete/interface time, tiered JIT/PGO on |
| --- | ---: | ---: |
| 1 | 0.873 | 0.954 |
| 64 | 0.923 | 0.934 |
| 1000 | 0.953 | 0.953 |
| 10000 | 0.961 | 0.956 |

Ratios below one mean the concrete implementation is faster. The interface version is
about 4-15% slower without tiering and about 4-7% slower in the PGO run, with the same warm
allocation. Replacing an interface-backed object is semantically valid; in this tested
implementation the runtime dispatch and surrounding adapter work did not match the
explicit direct path. This comparison does not isolate the cost of one interface call
or prove that every possible interface implementation is slower.

## ExpressionLanguage, URL and code size

Applying the compact adapter to ExpressionLanguage initially cost about 1-3% on short
expressions. Explicit direct branches then increased its materialization code and made
several short cases about 5-12% slower. Removing a redundant signed bounds check did not
resolve that regression. The interface experiment also did not establish a general
improvement there. The retained 32-type policy leaves both ExpressionLanguage variants
unchanged, as confirmed by source and IL measurements.

URL retains its original strategy and warm allocations. In the full-URL comparison,
`http://example.com` measured 167 ns versus 529 ns for compiled Regex; the
port/query/fragment case measured 194 ns versus 505 ns. Paired compiled/grammar ratios
were 3.07 and 2.62 respectively. Before/after parser ratios were 1.012 and 1.018, which
are not evidence of a new URL optimization in this step.

| Parser | Additional normalized source | Source growth | Additional method IL | IL growth |
| --- | ---: | ---: | ---: | ---: |
| MSSQL | 758940 B | 2.375% | 392769 B | 3.933% |
| MSSQL Located | 833006 B | 2.510% | 425135 B | 4.144% |
| SQL92 / SQL Standard / both ExpressionLanguage variants | 0 | 0% | 0 | 0% |

These are generated UTF-8 source bytes with repository paths normalized and compiled
method instruction bytes, including nested parser support. They are not native JIT code
sizes. The compact first prototype added only about 7-9 KB to MSSQL source, but was slower
on short inputs. The accepted direct path deliberately trades more code for throughput.

## Evidence and reproduction

Local artifacts are under .work/adaptive-values. before-bin is the baseline after
synchronizing main; concrete-bin is the measured selected implementation; interface-bin
is the interface experiment. Corresponding source snapshots are retained there.

- final-paired.txt and final-paired-repeat.txt: URL/Regex and MSSQL/ScriptDom.
- final-scale.txt: final MSSQL size sweep with tiering off.
- concrete-vs-before-pgo.txt: MSSQL size sweep with tiered JIT/PGO enabled.
- v2-resource.txt: cache payload and fresh-store allocation; later changes affect access,
  not the storage layout.
- interface-vs-concrete{,-pgo}.txt and interface-expression{,-pgo}.txt: interface experiments.
- before-size.jsonl and final-size.jsonl: compare their After measurements; their Before
  measurements refer to the earlier archive used by the code-size utility.

The paired harness is .work/paired-comparison; the MSSQL size harness is
.work/mssql-dense-speed; resource measurements use .work/adaptive-values/resource.
Run the compiled harnesses with paths to the two saved benchmark/SQL assemblies and set
DOTNET_TieredCompilation=0, or DOTNET_TieredCompilation=1 plus DOTNET_TieredPGO=1 for the
additional JIT profile. The ExpressionLanguage harness checks 176 language/tree shapes
before timing. BenchmarkDotNet benchmark additions from the preceding work remain available.


## Selection timing and potential grammar options

Adaptive-store planning must not resolve Mixed/Auto carriers early. Those modes first
need RenderReader to register value arms and determine whether fallback to Tape is
required. An initial planning pass accidentally bypassed that ordering: two existing
carrier tests exposed invalid C# 8 code and a lost SourceSpan. The final pass plans explicit Tape and the provisional Tape store exposed by Auto,
without committing Auto to that carrier. Mixed is left alone until its normal selection
stage. Dense-store sharing also remains at its original stage. No change to Mixed shape constructors is retained. The focused 77-case
carrier/adapter run passed after this correction.

A grammar-level storage option is compatible with the automatic policy. If added, Auto
could retain the default decision while Flat/Adaptive explicitly select generated code,
without interface dispatch at every access. The ownership scope needs care: several
rules and publications can share a typed table, so contradictory per-rule choices need
an explicit definition. The initial experiment added no public option. The subsequent implementation is
described under "Explicit storage strategies" below.

## Final validation

After the final provisional-Auto planning correction, all 6,560 DotGram tests and
14,628 SQL tests passed. The benchmark project built successfully. The compatibility
project built with C# 8 for net8.0, netstandard2.0, and net472, with no warnings or
errors. verified-size.jsonl exactly matches final-size.jsonl, confirming that the
final generated source and compiled size measurements match the measured concrete
adapter variant. git diff --check passed. No Mixed constructor change remains.

## Explicit storage strategies

ValueStorage now exposes Auto, Flat, and Adaptive on Gram/GramOptions and the compiler
API. Auto remains the default with the measured policy unchanged. It is a conservative
compile-time policy, not a prediction of the fastest strategy for every workload.
Explicit choices bypass both dense and paged heuristics and apply to a compilation's
shared direct value tables, including a fallback tape. Named compilations inherit the
host choice unless overridden, including an explicit override back to Auto.

Auto is retained because its selection adds no runtime dispatch and avoids asking every
small-parser author to tune storage. Representative measurements still decide explicit
choices for performance-critical consumers. This does not remove or redefine Carrier.Auto,
which chooses value construction behavior under separate eligibility rules. Per-rule
storage choices are deferred until ownership of shared tables can make them meaningful.

See docs/syntax.md, "Choosing value-table storage", for usage and the current heuristics.

Validation of the option integration: 6,579 main tests and 14,628 SQL tests passed,
including 19 added cases. The benchmark project and C# 8 compatibility targets
(net8.0, netstandard2.0, net472) built without warnings or errors. The code-size report
in .work/value-storage-size.jsonl preserves all previous generated-source size and
parser-method IL measurements for MSSQL, SQL92, SQLStandard, and ExpressionLanguage.
The new attribute property and enum increase each measured assembly by 512 bytes.
No new throughput measurements were taken for this option-only integration.

A subsequent explicit Paged variant removes the flat prefix entirely. Its measured
memory/code-size savings and throughput costs are documented in
[paged-value-tables-2026-09-15.md](paged-value-tables-2026-09-15.md). Auto remains unchanged.
