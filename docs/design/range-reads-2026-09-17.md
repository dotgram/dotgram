# CharRange.From read attribution

Current generator with root reachability, per-literal boundary membership, and
normalizer-local boundary FIRST cache. One fresh SqlStandardParser generation.

## Counting experiment

A temporary explicit From getter called a non-inlined diagnostic helper. Every
read was counted; a randomized interval of 2048..6143 reads selected a stack sample.
The helper serialized accounting with a lock. Source was restored and normal
profiler rebuilt before the diagnostic executable was run from its own directory.
Diagnostic elapsed time and allocations are not performance measurements.

Exact total: 80,155,377 From reads. Stack samples: 19,661. Attribution is approximate;
inlining can hide intermediate frames. Other generated record operations may read
backing fields directly and are outside this getter count. To was not instrumented.
Output hashes match an uninstrumented generation of the same parser.

| Operation | Sample share | Estimated From reads |
| --- | ---: | ---: |
| First.Or | 37.02% | 29.68 million |
| First.Normalized (including sorting) | 33.31% | 26.70 million |
| First.Covers | 13.59% | 10.89 million |
| ContinuationCache.FirstContents.GetHashCode | 5.30% | 4.25 million |
| Machine.Dispatchable | 3.77% | 3.02 million |
| First.Overlaps | 2.50% | 2.01 million |

Stage shares: Emit 57.94%, Normalize 41.89%, other 0.16%.
These are shares of getter reads, not CPU time. Or attribution excludes samples
whose nearer visible FIRST operation is Covers.

## Separate uninstrumented CPU sample

JetBrains dotTrace Sampling, ThreadTime; milliseconds, approximate.

| Method | Own | Inclusive |
| --- | ---: | ---: |
| First.Or | 266 | 375 |
| First.Normalized | 47 | 250 |
| First.Covers | 141 | 156 |
| First.Overlaps | 94 | 94 |
| FirstContents.GetHashCode | 109 | 109 |
| CharRange.get_From | 0 | 0 |
| Normalize | 16 | 953 |
| Emit | 16 | 2969 |

Inclusive times overlap and must not be summed. Zero getter samples do not prove
zero cost: JIT inlining and sampling attribution hide trivial property accesses.
The count is evidence for repeated set operations, not for replacing properties.

## Next candidates

1. Measure identity/equal-range fast paths and repeated operand pairs in Or/Covers.
   Preserve public First semantics, including Anything/Nothing/Ends and potentially
   unsorted caller-supplied ranges. Avoid introducing a global cache.
2. Inspect callers of Normalized and the FirstSets.ByRule fixed-point full scans;
   a dependent-rule work queue may eliminate work before it needs caching.
3. Content IDs may help repeated set pairs, but hashing itself is only 5.3% of reads.
   Interning must be measured with allocation and tiny-grammar controls.

Raw stack counts: benchmarks/results/range-reads-sqlstandard-2026-09-17.tsv.
No production behavior changes were made for this investigation.
