# Compile only the selected machines

## Change

The node-operation counters found 38 Machine instances for SqlStandardParser,
of which only four survived publication joining. Machine's constructor emitted
states before CSharpEmitter chose the final groups, so discarded candidates had
already compiled their reachable rule bodies.

Machine now supports deferred rule compilation. Its constructor still prepares
capture layouts, factories, recovery metadata, rule entries, execution plans,
value types and FOLLOW information needed by strategy selection. CSharpEmitter
defers compilation for multiple publication groups and for replacement machines,
then calls CompileRules on the final list before registering readers or sharing
value tables. CompileRules is idempotent. Other constructor callers keep eager
compilation, including the single-group path.

This changes when states are emitted, not the decisions about which publications
can share a reader. It adds no runtime parser infrastructure, global cache or
content identifier registry. Those remain separate candidates for repeated analysis.

## Measurements

Two fresh processes per variant, ordered baseline/candidate/candidate/baseline.
The measured region runs the source generator over the complete DotGram.Sql
project, without compiling the resulting C#. Preparing Roslyn inputs and writing
source dumps are outside the timer. No build or test run overlapped a measurement.
The baseline includes the pending FirstSets and FollowSets optimizations on b90d0a2c.

| Metric | Baseline mean | Candidate mean | Change |
|---|---:|---:|---:|
| Source-generator elapsed time | 26.567 s | 19.045 s | -28.31% |
| Managed allocation during generation | 17.073 GB | 11.537 GB | -32.42% |
| Generated C# characters | 58,866,983 | 59,608,778 | +1.26% |
| SqlStandardParser Compiled stage | 13.080 s | 5.712 s | -56.33% |

The two full-generator samples were 25.129/28.005 seconds before and
19.141/18.948 seconds after. This is a small paired experiment, not a confidence
interval. Allocation is cumulative allocation, not retained memory or peak RSS.
No parser runtime speedup or C# compiler speedup is claimed from this measurement.

Raw runs and generator assembly hashes are in
`benchmarks/results/lazy-machines-2026-09-17.json`.

## Generated code differences

The SQL generated executable code is unchanged after ignoring expected-array
declarations and references to their generated names. Attribute support, the
embedded attribute and Sql92Parser remain byte-identical.

Expected-array registration changes because discarded machines no longer add
entries to the shared diagnostic table registry. Their former names and counters
could influence later machines, including replacements reusing an earlier tag.
Consequently this is not a byte-identical optimization of diagnostic output:
the names and some referenced expected lists change. SqlStandardParser has 1,126
distinct expected arrays instead of 1,019, with 107 additional distinct contents.
These declarations account for most of its source-size increase. They are real
generated fields, so the change is relevant to static initialization and memory.

The Minimal snapshot changes the declaration order of two otherwise identical
scanner helpers. It was reviewed and updated. A sibling-publication regression
compares failure position and error text against separately generated controls,
covering missing prefixes, bad bodies and missing closing parentheses.

## Validation

- Core suite: 8,306 tests, with only the reviewed Minimal snapshot ordering
  mismatch on the initial run. After updating it and adding the failure-expectation
  regression, all nine sibling-publication and snapshot cases passed.
- Rebuilt Release/net10.0 DotGram.Sql: all 14,701 SQL tests passed.
- Rebuilt Release/net10.0 DotGram.Web: all 5,848 Web tests passed.
- ExpressionLanguage and 12 of 13 Web grammar outputs are byte-identical,
  including Rfc3986 (URL). Rfc5322 changes diagnostic table registration.
- git diff --check passed. The Release genprof executable was rebuilt with the
  normal generator, without the temporary node-operation counters.
## Scope left for later

Candidate machines still prepare analysis metadata and reserve entry states.
This change avoids compiling discarded bodies; it does not make construction free
and does not skip engine-state compilation for retained direct readers. Sharing
stable analysis, memoizing continuation queries and reconsidering transitive
inlining costs remain separate steps.
