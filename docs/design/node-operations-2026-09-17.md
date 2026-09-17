# Repeated node operations, 2026-09-17

## Scope and measurement

Measured one fresh source-generator run for SqlStandardParser, one for the complete
DotGram.Sql project (including its imported grammars), and one for the TinyScalar
fixture. No generated parser processed input and no generated C# was compiled.
The working tree includes the pending FirstSets and FollowSets optimizations on
top of b90d0a2c. Temporary counters were removed after collecting the data.

Nodes, graphs, machines, seams and nullable resolver delegates are identified by
reference, not structural equality. FIRST values used in continuation keys are
compared structurally, including Anything, Nothing, Ends and all ranges.

The TSV columns are calls, distinct node identities, distinct (node, scope) pairs,
distinct observed contexts, and contexts including the failure target. Compile's
scope is the machine; its context includes next, Plain, AfterSeam and seam. Other
contextual operations use the graph. Nullable uses resolver delegate identity;
Element has no graph argument. These are observed keys, not a proof that all
mutable state needed for safe memoization has been captured.

The instrumentation retains identities and sets until process exit and increases
allocation and CPU cost. Its elapsed times and allocation counters must not be
used as production performance measurements. SqlStandardParser's generated source
hashes match the uninstrumented output. All three reported runs finished without
generator errors. A failed attempt to isolate TransactSqlParser without its Sql92
dependency is excluded; the successful complete-project run replaces that attempt.

## SqlStandardParser

| Operation | Calls | Distinct node/scope pairs | Distinct observed contexts |
|---|---:|---:|---:|
| FIRST requests | 4,182,975 | 32,512 | 32,512 |
| FIRST calculations during fixed point | 192,518 | 14,059 | estimates change; not a cache key |
| FIRST calculations after stabilization | 28,795 | 28,795 | 28,795 |
| Nullable, including recursive calls | 3,401,707 | 26,483 | resolver estimates can change |
| Precedes | 1,141,503 | 23,900 | 42,808 |
| Plainly | 950,778 | 18,695 | 30,174 |
| FOLLOW contributions | 82,257 | 33,539 | 52,262 |
| NeverGivesBack entry | 146,422 | 612 | 2,044 |
| Possessive public entry | 81,132 | 266 | 783 |
| Compile | 1,461,554 | 242,332 | 1,455,505, including failure target |
| OfElement | 11,658 | 28 | 28 |

FIRST's settled cache works: no node is recalculated twice in the same graph.
Most requests are cache lookups, not repeated range construction. Fixed-point
calculations cannot reuse answers solely by node because rule estimates change.

Precedes repeats an observed context in 96.25% of calls; Plainly in 96.83%;
NeverGivesBack in 98.60%; public Possessive in 99.03%. These are stronger initial
memoization candidates than Compile. Its duplicate observed contexts account for
only 6,049 calls (0.41%); additional unobserved emitter state can further limit reuse.

## Machine construction before publication joining

Compile is invoked in 38 Machine instances. Four instances survive in the final
main machine list after Joined and the overlapping sibling-publication merge.
Their Compile calls total 142,244. The other 34 instances account for 1,319,310
calls, or 90.27% of the total.

Machine's constructor compiles its rule bodies immediately. CSharpEmitter then
assesses strategies, joins publications and may construct a replacement machine.
The non-final machines can supply analysis needed for these decisions, so the
90.27% figure is not a measured removable CPU fraction. It does show why choosing
the final machine groups before state emission, or making that emission lazy,
deserves priority. SqlStandardParser is not lexical and this run has no buffered
publications, so the non-final label here does not stand for a retained lexical
value-reader or buffered machine.

## Inlining and backtracking

There are 278,666 Inline.call expansions of 64 distinct body nodes across the 38
machines. SimpleComment alone is expanded 42,955 times, Comment 42,917 times,
Separator 42,879 times, and trivia 42,289 times. Whitespace/comment structure also
dominates the hottest Compile, Nullable and continuation nodes.

ExecutionPlan currently bounds a rule's own node count at 64 when allowing
inlining. It does not charge the fully expanded transitive body or multiply by
call-site frequency. Expansion and repeated candidate machines compound each other.
Reducing inlining requires parser-runtime measurements, especially for small inputs.

The run performs backtracking analysis, not actual backtracking over input:

- NeverGivesBack: 146,422 entry calls, only 2,044 observed contexts.
- Public Possessive: 81,132 entry calls, only 783 observed contexts.
- Inside the instrumented determinism entry regions: 415,749 FIRST requests
  (9.94% of all requests) and 636,925 nullable calls (18.72%). These are call
  shares, not time shares, and do not cover every backtracking-related operation.
- General CompileRepeat paths report 93,132 unsettled and 51,058 settled sites.
  This excludes other repeat-lowering paths and is not a count of runtime rollbacks.
- Adding the failure target to Compile's key increases unique contexts from
  1,449,469 to 1,455,505: 6,036 extra contexts (0.41% of calls). Failure targets
  also serve lookahead/atomic control flow; this is not a causal attribution of
  all compilation work to backtracking.

The measured repetitions therefore cannot be explained as the generator running
the parser and backtracking. They arise from repeated analysis, inlining and
constructing candidate machines, with different continuation contexts in emission.
Measuring runtime rollback counts requires a separate input corpus and parser trace.

## Other sizes

Complete DotGram.Sql: 8,886,259 FIRST requests, 7,037,616 nullable calls,
2,730,473 Precedes calls and 2,050,947 Compile calls. Settled FIRST calculations
remain exactly once per node/graph pair (225,089).

TinyScalar: 179 FIRST requests, 150 nullable calls, 62 Precedes calls and 10 Compile
calls in one machine. Ten Compile calls have ten observed contexts including the
failure target. A broad compilation cache offers no reuse in this fixture.

## Recommended order

1. Separate machine analysis from state emission, so joining does not discard
   already emitted machines. Preserve all metadata needed for strategy selection.
2. Memoize continuation and determinism queries within a stable graph lifetime;
   first reuse summaries independent of following when possible. Measure small
   grammars before introducing per-node infrastructure everywhere.
3. Cache nullable results only after the rule estimates have stabilized.
4. Reassess inlining using transitive expansion and call-site cost, with paired
   parser-runtime and generated-code-size measurements.
5. Do not prioritize a general Compile cache or mechanically replace recursion.

Raw counters: benchmarks/results/node-operations-{sqlstandard,sql-all,tiny}-2026-09-17.tsv.
The diagnostic build and instrumentation scripts are in .work/node-probe; the
normal genprof executable is rebuilt without counters after the experiment.
