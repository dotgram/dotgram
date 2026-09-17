# Sharing sibling publications — 2026-09-17

## Implementation

After the existing reachable-root joining pass, compatible large direct readers
with overlapping rule sets can share one machine. The union is constructed once
for the group, giving its captures and construction records one numbering scheme.
Each published root retains its entry point and result type. This shares generated
readers and materialization code, without runtime delegates or an adapter layer.

The initial gate requires at least 128 reachable rules per participating machine
and at least 90% overlap relative to the larger rule set. Flat parsers, streamed
publications and nonzero reading modes are excluded. Every participant must already
use the tape: explicitly requested, or forced in Auto by a reachable constructed
value that cannot safely be built immediately. Participants must agree on whether
they build values during recognition. The combined publication list must still pass
`CanDirect`; otherwise the original machines remain. These are conservative initial
bounds, not a claim that the thresholds are globally optimal.

The first strategy checks still construct the individual machines. The new pass
then builds each accepted union once, rather than repeatedly rebuilding it for
every added root. File splitting remains disabled by default.

## Controlled SQL generation and compilation

Same probe and compilation settings as the production source-splitting comparison:
fresh process per observation, handwritten inputs/references prepared before timing,
then generator execution and Debug DLL emission with embedded PDB. Source dumping
happens after timing. Baseline: `17383239`; candidate: the implementation accompanying
this report. No tests or builds overlapped these timed observations. Run order was
candidate, baseline, baseline, candidate.

| Run | Generation, s | Generation + compilation, s | Allocated bytes | Peak working set, bytes |
|---|---:|---:|---:|---:|
| Candidate 1 | 31.649 | 64.118 | 28,040,406,992 | 3,892,158,464 |
| Baseline 1 | 32.415 | 81.475 | 38,556,980,248 | 5,563,072,512 |
| Baseline 2 | 31.725 | 77.371 | 38,404,362,304 | 5,628,399,616 |
| Candidate 2 | 31.368 | 63.841 | 28,027,928,776 | 3,901,190,144 |

Two-run mean generation + compilation: **79.423 -> 63.979 seconds (-19.45%)**.
Mean allocated bytes decrease approximately **27.15%**, and mean peak working set
approximately **30.36%**. Generator time is close (32.070 -> 31.508 seconds); most
of the total time saving occurs after generation. Two observations per variant are
not a confidence interval or a measurement of a full Visual Studio solution build.

SQL generated text: **120,103,615 -> 58,867,005 characters (-50.99%)**, still six
source files. All 38 generated Examples files, including legacy FIX and
ExpressionLanguage, are byte-identical to the baseline; no performance improvement
for those parsers is claimed.

[Raw observations](../../benchmarks/results/sibling-publications-2026-09-17.json)
include the generator hashes. The `generate` mode of
[CompilationSplitExperiment](../../benchmarks/CompilationSplitExperiment/README.md)
preserves the measurement procedure.

## Validation

New tests cover independent root result types, mandatory EOF, keeping small readers
separate and refusing to combine readers with different guard-materialization modes.
The synthetic large grammar uses arithmetic factories so normalization cannot collapse
its aliases below the size threshold. SQL tests pass for both the measured Debug DLL
and the rebuilt Release DLL (14,701 tests each). C# 8 compatibility builds pass for
net8.0, netstandard2.0 and net472. All 8,299 core tests and 3,808 Finance tests also pass; the full core/example build is clean.

## Release parser checks

Loaded baseline and candidate Release SQL assemblies into separate assembly load
contexts. Calls use compiled delegates, not per-operation reflection. For each input,
three warmup passes precede 15 alternating-order rounds; reported times are medians.
These exploratory runs check for obvious regressions, not confidence intervals.

| Input | Elements | Baseline, us | Candidate, us | Allocated bytes per parse, both |
|---|---:|---:|---:|---:|
| T-SQL WHERE conjunction | 1 | 2.432 | 2.456 | 1,384 |
| T-SQL WHERE conjunction | 64 | 68.820 | 69.392 | 28,096 |
| T-SQL WHERE conjunction | 1,000 | 1,066.990 | 1,077.734 | 424,960 |
| T-SQL WHERE conjunction | 10,000 | 10,851.820 | 10,935.100 | 4,240,960 |
| Standard numeric sum | 1 | 4.027 | 4.286 | 752 |
| Standard numeric sum | 64 | 235.145 | 239.125 | 32,696 |
| Standard numeric sum | 1,000 | 31,200.580 | 31,188.756 | 41,948,944 |

The numeric short-input observation initially suggested a slowdown. Repeating in a
fresh process with reversed assembly load order changed the sign: baseline/candidate
were 6.037/5.798 us at one element and 310.184/299.781 us at 64 elements. Absolute
numbers also drifted considerably. These observations do not establish a repeatable
short-input regression or improvement; allocations remained identical. T-SQL timing
differences were approximately 1% in the initial run, without an allocation change.

The numeric 10,000-element probe was stopped without a reported result after the
1,000-element case exposed substantial pre-existing allocation in both variants.
Investigating the approximately 42 MB allocated for that 1,000-element numeric sum
is a separate performance task; this change does not solve it.

Debug DLL including embedded PDB: 53,075,456 -> 27,142,656 bytes. Release SQL DLL:
51,828,224 -> 27,139,584 bytes. No instruction-layout or DLL-identity claim is made.

[Raw runtime observations](../../benchmarks/results/sibling-publications-runtime-2026-09-17.json).
