# SQL corpus comparison recheck

## Correction

The September 15 statement that DotGram Located is over three times faster than
ScriptDom was a warm comparison over the shared 7,716-statement corpus. The later
sharing-validation run substituted synthetic SELECT statements with 1/64/1,000 AND
predicates, used the nonlocated parser and normal tiered compilation. Those results
cannot establish that the historical corpus advantage disappeared.

The suggestion in chat that the historical threefold result was caused by insufficient
warmup was unsupported and incorrect. The historical harness disabled tiered compilation
and warmed the full corpus. The initially underwarmed synthetic run was a separate
problem in the new measurement, not an explanation of the earlier corpus result.

## Reproduction

Reused .work/paired-comparison with the existing ScriptDomBenchmarks implementation.
Loaded the before/after sharing SQL assemblies from .work/sharing-validation into
separate AssemblyLoadContexts via copies of the benchmark output directory. Both
setups selected identical inputs and all timed methods accepted all 7,716 statements.
The ordered NUL-separated input SHA256 is exactly the historical hash:
6088BEB886476100640001E0C60D430272E9E21D463A3C9241BBE19695AEF136.

.NET 10.0.12 x64, DOTNET_TieredCompilation=0, three full-corpus warmups, fifteen rotating
measurement rounds. Each operation parses the full corpus; results are divided by
7,716. The second process reverses assembly loading order. ScriptDom uses each corpus
file's designated parser version, as in the historical test. No concurrent builds/tests.

| Parser | Run 1 us/statement | Run 2 us/statement | Allocated bytes/statement |
| --- | ---: | ---: | ---: |
| Before sharing, Located | 4.789 | 4.715 | 1,716.51 |
| After sharing, Located | 4.821 | 4.771 | 1,716.51 |
| Before sharing, ordinary | 4.142 | 4.106 | 1,716.51 |
| After sharing, ordinary | 4.164 | 4.119 | 1,716.51 |
| ScriptDom | 18.295 | 17.994 | about 42,543.6 |

Paired ScriptDom/after-Located speed ratios are 3.8109 and 3.7730. Thus the historical
advantage remains on the exact corpus. Sharing has not produced a substantial throughput
change here. DotGram's allocation remains about 24.8 times smaller.

The synthetic long-conjunction finding remains valid for that workload: even with
tiering disabled, ScriptDom is faster on the large AND chains. This is a distinct
stress case worth profiling; it must not replace the established corpus benchmark
when reporting overall comparison continuity. Future reports should show corpus and
synthetic scaling cases separately and include Located for the positional comparison.

This does not change the independently observed FIX character-stream regression.
No production code changes were made during the investigation.

Raw output: benchmarks/results/sql-corpus-recheck-2026-09-17-run1.txt and run2.txt.
