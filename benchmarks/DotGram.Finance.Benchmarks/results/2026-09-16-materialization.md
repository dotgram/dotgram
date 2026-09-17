# Partition large materialization choices, 2026-09-16

## Change and scope

The baseline is `30774a2`, with prefix tables already enabled by default.
The grammar and public API are identical in both builds. The generator now
partitions a rule with more than 64 construction alternatives into local helpers,
each holding at most 64 factories. It selects the helper before gathering captures
or creating values. The input span is an explicit argument, never a closure field.
Small choices, identity constructions, folds, external rules and lexical rereads
retain their existing paths. This is a general emitter change, with no FIX cases.

The existing rule-level partition could not split one huge rule. Its hundreds of
span/position temporaries gave the JIT a large stack frame even in optimized code.
The new limit bounds construction cases independently of the existing estimated
basic-block budget. Diagnostics still check each resulting construction helper;
a complex capture layout can remain oversized. The cap is conservative, not a
claim that 64 is optimal for every grammar or runtime.

## Unprofiled comparison

Release, .NET 10.0.12, Windows x64. Three fresh processes per variant/workload,
variant order reversed in the second round. Both assemblies use the same existing
`profile` harness with ordinary runtime defaults. No builds, tests or profilers
ran alongside these timings. Each operation fully enumerates returned fields;
stream construction is included, assembly loading and delegate binding are not.
Order/tag workloads: 500000 operations after 50000 warmups. Groups: 500 operations
after 100 warmups. These are diagnostic loops, not statistical confidence intervals.

Durations are microseconds per input, median (min-max). Order has 15 fields;
Groups has 3011. Allocations are identical before and after in every measured row.

| Input | Workload | Before, us | After, us | Speedup | B/op, both |
| --- | --- | ---: | ---: | ---: | ---: |
| Bytes | Order | 6.310 (6.262-6.341) | 3.266 (3.217-3.287) | 1.93x | 5712 |
| Bytes | Groups | 1205.099 (1186.164-1205.675) | 607.547 (601.495-629.229) | 1.98x | 173472 |
| Bytes | Tag1 | 0.928 (0.912-0.939) | 0.707 (0.688-0.717) | 1.31x | 4552 |
| Bytes | Tag198 | 0.912 (0.894-0.920) | 0.670 (0.668-0.715) | 1.36x | 4552 |
| Characters | Order | 7.776 (7.632-7.812) | 4.712 (4.596-4.714) | 1.65x | 9632 |
| String | Order | 8.531 (8.487-8.634) | 5.581 (5.570-5.604) | 1.53x | 1376 |

Raw runs: [bytes](2026-09-16-materialization-bytes.txt),
[characters and string](2026-09-16-materialization-characters.txt).

## CPU profile and native code

Separate dotTrace Sampling/ThreadTime runs collect only the measured loop after
warmup: 2000000 byte orders / 30000000 fields in each run. Inclusive times must
not be added together because callers include callees.

| Method | Before total CPU, ms | After total CPU, ms |
| --- | ---: | ---: |
| Recognize byte fields, including materialization | 12250 | 5750 |
| Materialize byte fields, including helpers | 8188 | 2219 |

The original giant construction helper alone consumed 5938 ms own CPU and
7109 ms inclusive CPU. The corresponding first construction partition consumes
313 ms own / 1250 ms inclusive. The order's ordinary fields use this first
partition. Materialization as a whole falls about 73%; other parser costs now
make up a larger share of the remaining time. Sampling is approximate.

Full exports: [before](2026-09-16-materialization-before.xml),
[after](2026-09-16-materialization-after.xml).

A separate JIT disassembly run disables tiering to inspect FullOpts code, not to
produce the timings above. The original helper reserves 26760 bytes of stack and
has 149297 bytes of native code. The first construction partition reserves 72
bytes and has 4004 bytes of native code. Its caller now reserves 1176 bytes and
has 13570 bytes of native code, including small rules that previously lived in
another helper. These figures describe the methods exercised by the order, not
the total size of all partitions. Source/JIT excerpts are saved in
[native evidence](2026-09-16-materialization-jit.txt).

The managed Finance assembly grows from 15610880 to 15756800 bytes (+0.93%).
Splitting duplicates capture setup across helpers; this trades some code size
for much smaller hot method frames. No claim is made here about cold startup.

## Reproduction and identity

```powershell
$app = './benchmarks/DotGram.Finance.Benchmarks/bin/Release/net10.0/DotGram.Finance.Benchmarks.exe'
& $app profile <saved-finance.dll> Bytes Order 500000
& $app profile <saved-finance.dll> Bytes Groups 500
& $app profile <saved-finance.dll> Bytes Tag1 500000
& $app profile <saved-finance.dll> Bytes Tag198 500000
& $app profile <saved-finance.dll> Characters Order 500000
& $app profile <saved-finance.dll> String Order 500000
```

For dotTrace, set `DOTGRAM_DOTTRACE_API` to its `JetBrains.Profiler.Api.dll`,
start the same harness with `--profiling-type=Sampling --time-measurement=ThreadTime
--use-api`, and use 2000000 byte orders. Export using Reporter with pattern `.*`.
For native code, in a separate process unset that API variable and set
`DOTNET_TieredCompilation=0`,
`DOTNET_JitDisasm=*Materialize_DotGram_Buffered_ReadFields_Bytes*`, and
`DOTNET_JitStdOutFile=<output.asm>`.

SHA-256 of saved measured Finance assemblies:

- Before: `2043E5ED555B09AF5BEDF511AFFCBF28B326BFA923AC303985384D8C04C8AFF6`
- After: `A8C279BBAD3210247EAA9633E9D7A91797DD619D2E42FFC51CE7051CE80A33F0`

## Validation

The regression tests compile 129 distinct constructions at the C# 8 floor,
exercise every partition through strings, TextReader and one-byte Stream reads,
check captured values and parser spans, reject truncated input, and check typed
guards with backtracking across partition boundaries. Both span-capture modes
are covered. The Finance suite covers standard fields, binary pairs, locations,
fixtures and the separately invoked semantic API.

Completed checks: core tests 8193/8193, Finance tests 3596/3596; Release Finance
builds for net10.0 and netstandard2.0; C# 8 compatibility builds for net8.0,
netstandard2.0 and net472. Builds completed with zero warnings/errors. The final
Finance net10.0 assembly hash matches the measured after assembly exactly.
