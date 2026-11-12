# Share publication machines across grammar readings

## Cause and change

The overlap gate rejected every publication with a nonzero Reading. T-SQL version
publications therefore prevented Statement, Sql and Script from sharing a machine,
even though their reachable rule sets overlap by more than 99% (995/998, 995/1001,
and 997/1001). Instrumentation confirmed all other eligibility checks passed.

Remove only this exclusion. Reading is already supplied by the publication as a
runtime parameter; it does not require a separate copy of the rule bodies. Keep the
128-rule minimum, 90% overlap, streaming exclusion, compatible guard construction
and tape-carrier requirements. Small parsers retain their existing eligibility.
Temporary diagnostic instrumentation was removed before measurements.

## Measurements

Release standalone genprof on full SQL, no concurrent builds/tests during measured
runs. Two runs each in candidate/baseline/candidate/baseline order. Baseline includes
the synchronized main and previous factory/site optimizations.

| Metric | Baseline | Candidate |
| --- | ---: | ---: |
| Mean generation time | 16.544 s | 15.024 s |
| Mean cumulative allocations | 9.460 GB | 7.827 GB |
| Generated characters, all sources | 59,610,816 | 39,682,426 |
| T-SQL generated UTF-8 bytes | 22,018,319 | 12,445,143 |
| Located generated UTF-8 bytes | 23,026,313 | 12,671,083 |

This is about 9.2% less generator time, 17.3% less allocated memory, and 33.4% less
source text. SQL92 and SQL Standard output sizes are unchanged. Harness sizes differ
slightly from the user's Visual Studio artifact because generation context differs;
all before/after figures above use the same harness.

Generated source intentionally changes through machine sharing, so byte identity is
not the acceptance criterion for this change. Runtime parser speed and comparative
C# compilation time have not been benchmarked.

## Validation

- SQL project/test build: zero warnings and errors.
- All 14,701 SQL tests pass.
- 13 focused sibling-publication and static-condition tests pass, including a new
  regression with a shared recursive 130-rule chain, distinct Old/New readings,
  distinct result types, and interleaved positive/negative calls. The regression
  also asserts one shared materializer is generated.
- Expanded reader, reader coverage, sibling and static-condition selection: 98 tests passed (includes the 13 above).
- git diff --check passes.

## Recursion

The SQL path already uses recursive generated reader methods. Stack margin probes
and Deepen support moving deep parsing onto another stack; buffered-window input
has limitations. This change shares existing reader machines rather than replacing
an automaton or changing the stack strategy.

Raw measurements: benchmarks/results/reading-machine-sharing-2026-09-17.json.
