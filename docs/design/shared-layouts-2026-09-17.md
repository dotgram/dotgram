# Rejected shared capture preparation experiment

Baseline includes the shared factory descriptions experiment. Candidate added a
ResultTypes-local lazy cache of (CaptureLayout, repeated-capture node set), keyed by
(RecognitionGraph, RuleSymbol), used by Machine construction. Per-machine offsets
and states were unchanged. Fold-aware layouts were not combined with plain layouts.

Full SQL standalone Release generation, fresh processes, order baseline/candidate/
candidate/baseline, no overlapping tests/builds. All six source hashes match.

| Metric | Baseline mean | Candidate mean |
| --- | ---: | ---: |
| Generation | 17.397 s | 18.518 s |
| Cumulative allocated bytes | 9,471,451,264 | 9,411,988,096 |

Two pairs show +6.44% elapsed time for only 59,463,168 bytes saved (0.63%). No peak
memory measurement or causal claim about GC is made. The result does not justify
retaining this cache; the implementation was removed while the factory cache remains.

TinyScalar, 21 drivers/process excluding first: median 6.105 -> 5.817 ms,
983,768 -> 984,488 allocated bytes. One smoke pair is inconclusive about speed.

252 focused sibling-publication, buffered-input, emitter, snapshot and recovery
boundary tests passed with the candidate. The production source was restored to the
pre-experiment version and genprof rebuilt. git diff --check passes.

Raw measurements: benchmarks/results/shared-layouts-2026-09-17.json.
