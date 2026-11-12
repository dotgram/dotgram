# Reuse boundary FIRST sets across literals

The normalizer now lazily caches boundary FIRST sets by resolved element identity.
Lowering replaces elements rather than mutating their ranges, so a replacement body
has a new key. The cache belongs to one normalizer, does not escape with the result,
and is absent for grammars without boundary queries. It neither caches by rule name
nor merges structurally equal nodes.

## Measurements

Baseline includes the per-literal optimization in boundary-first-2026-09-17.md.
Full SQL Release genprof, fresh processes, baseline/candidate/candidate/baseline,
without concurrent builds or tests. All six source hashes match.

| Metric | Baseline mean | Candidate mean |
| --- | ---: | ---: |
| Generation | 18.213 s | 18.284 s |
| Cumulative managed allocation | 9.776 GB | 9.674 GB |

Allocation decreases 102,390,936 bytes (1.05%). Elapsed increases 0.39%, too small
in two pairs to establish a regression or speedup. Retained as a small allocation
reduction; this is not evidence of lower peak memory.

TinyScalar, 21 drivers/process excluding first: median 6.501 -> 6.205 ms,
982,552 -> 982,600 bytes. One smoke pair does not establish a speed change.

## Validation

111 focused tests passed on a successfully rebuilt assembly, including the nine
boundary membership cases from the previous change and the new reuse/isolation case.
The previous boundary-first report accidentally ran an old test DLL after a compile
failure caused by accessing internal NodeWalk. Tests now traverse public Children;
the successful rebuild and 111-test run validate both changes together.
Build exit status is checked before launching the test runner.
All SQL outputs match. git diff --check passes. The profiling harness is rebuilt.

Raw results: benchmarks/results/boundary-cache-2026-09-17.json.
