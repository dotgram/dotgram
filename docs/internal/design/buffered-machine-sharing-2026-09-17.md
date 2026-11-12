# Share large buffered publication machines

## Runtime validation status

Subsequent measurements found about a 16% character-stream throughput regression
on the FIX fixture. This candidate is not ready for acceptance based on generation
gains alone. See sharing-runtime-compilation-2026-09-17.md.

A subsequent partitioning fix removes this regression in two paired FIX runs while
preserving the source-size reduction. See [the follow-up](buffered-publication-parts-2026-09-17.md).

## Implementation

Buffered machines were created and emitted independently for every publication and
input form. Stage their construction, defer state compilation, then combine large
rule sets with at least 90% overlap. Character and byte machines remain separate;
context requirements must match to preserve public signatures. Compile and register
all publication roots only after grouping. Public Parse/Find/Yield wrappers retain
their own whole-input, recovery ordinal and buffer-release behavior.

The size gate requires more than one rule and at least 128 descendant occurrences.
This preserves single-rule incremental release proofs and avoids combining small
parsers. Grammars without buffered publications return before grouping allocation.
The existing bufferedFind flag is conservative for mixed groups; it only affects
the single-rule release proof, which these groups cannot use.

This does not enable recursive buffered readers or alter the FIX grammar. Four
ordinary recognizers remain. Buffered recognizers fall from eight to four, so the
example's total recognizer count falls from twelve to eight.

## FIX measurements

Standalone Release generation of the unchanged Fix44 example, with the user's
Finance assembly providing the field model. Baseline and candidate use identical
inputs and references. Two fresh processes per version, baseline/candidate then
candidate/baseline, no simultaneous build/test workload.

| Metric | Baseline | Candidate |
| --- | ---: | ---: |
| Mean generation | 5.926 s | 4.587 s |
| Mean cumulative allocations | 5.022 GB | 3.361 GB |
| All generated characters | 71,561,901 | 48,645,051 |
| Main Fix44 source UTF-8 bytes | 71,545,183 | 48,628,333 |
| Main recognizers | 12 | 8 |

Approximately 22.6% less generation time, 33.1% fewer allocated bytes and 32.0%
less generated source. The user's earlier build file differs slightly because its
generation context differs; use the paired harness values for comparison.
Runtime throughput and comparative C# compilation time were measured subsequently; see the runtime status and follow-up above.

## Validation

- 40 buffered-input tests passed, including a new regression requiring exactly two
  buffered materializers (one per input form) for a large Parse/Yield grammar.
  It checks normal records, recovery after a malformed record, one-byte/character
  buffer capacity and bounded retention while yielding.
- Real Finance/examples build: zero warnings/errors; 3,813 Finance tests passed.
- Final buffered/snapshot selection: all 45 tests passed. Final FIX output hashes match the measured candidate after adding the context compatibility guard.
- git diff --check passes.

An initial harness invocation referenced an outdated local Finance assembly and
reported unresolved field types. The harness threw on these diagnostics, causing
the genprof application-error dialog. The reference was corrected and the scratch
harness now reports a failing exit code without an unhandled exception. Failed runs
are excluded from the results.

Raw runs: benchmarks/results/buffered-machine-sharing-2026-09-17.json.
