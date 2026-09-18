# FIX: rule methods inside the recovering machine

## Implemented scope

The generator recognizes a typed leaf with one factory over one full-input capture
using a normal `Read_<Rule>` method. The initial shapes are one character class
followed by another class repeated zero or more times, and a class repeated one
or more times. This covers Finance Fix `Tag` and `Size` without changing its grammar.

Selection is automatic inside machines that already cache values for guards. Only
concrete character ranges qualify. Atomic captures commit by definition; other
runs require a disjoint follow set. User predicates, token input, recursive rules,
multiple captures and factories, and runs that may need to give characters back
retain the existing implementation. String, buffered char and buffered byte inputs
use their native access operations without a common runtime input interface.

On success the caller writes one Completed entry and continues directly. The
capture extent is that same entry's start/end, so it no longer needs a separate
Capture entry or a materializer capture walk. Factory execution remains deferred
until the existing guard/final-value demand; the existing cache prevents repeated
construction. On refusal, the ordinary recognizer preserves diagnostic and recovery
behavior. Caller RuleCapture records and the general materialization pass remain.
This is the first mixed lowering step, not a replacement of the entire FIX engine.

## Paired measurements

Baseline: `3e9709de`; candidate: the working-tree implementation. Release net10.0,
1.5 s warmup per implementation/case, nine alternating batches. Two fresh processes
reverse initialization and sample order. Fields, including error messages, are
compared before timing. No builds or tests ran concurrently. These are exploratory
measurements, not confidence intervals. The table shows the reverse run and the
range of time changes over both runs; negative means less time.

| Workload | Input | Before, us | After, us | Time change across runs |
| --- | --- | ---: | ---: | ---: |
| One field | string | 0.257 | 0.237 | -7.7% to -6.5% |
| Order, 15 fields | string | 3.060 | 2.739 | -11.2% to -10.5% |
| Order, 15 fields | TextReader | 4.111 | 4.173 | -0.9% to +1.5% |
| Order, 15 fields | byte stream | 4.061 | 3.914 | -9.3% to -3.6% |
| 64 short binary pairs | string | 17.763 | 14.668 | -17.4% to -13.9% |
| 64 short binary pairs | TextReader | 24.403 | 21.030 | -13.8% to -7.5% |
| 64 short binary pairs | byte stream | 22.549 | 19.623 | -15.6% to -13.0% |

The ordinary TextReader case has no established improvement. Managed allocations
per warmed call are unchanged on these cases. A single scaling run measured 4000
string fields at 922 -> 805 us. For 8000 string fields the two runs disagree
(3720 -> 3874 us and 3956 -> 3494 us), so no reliable speed claim is made there.
Allocations at that recycling boundary fell from 25,705,624 to 24,362,160 bytes;
the large arena recycling cost remains. The 64 KiB payload and recovery checks
showed no large new penalty in the exploratory run.

Three fresh first-call processes per version measured median 7.641 -> 7.841 ms
for one string field. Loading the assembly and reflection lookup precede timing;
timing includes the first reflected call, enumeration and JIT, not process startup.
Current-thread managed allocations were 11,072 -> 3552 bytes. This is not an RSS
measurement. Generated FixGrammar source is 573,705 -> 572,887 bytes.

A fresh comparison with the handwritten parser still leaves a substantial gap:
Order/string is 2.592 vs 1.036 us (2.50x); short binary pairs/string are 15.343 vs
3.707 us (4.14x). Direct rule recognition helps, but the remaining per-field arena
records, guard materialization and field construction must be addressed next.
Do not move arbitrary user factories earlier to close this gap.

## Validation and evidence

- 8392 core tests passed; prebuilt example/package dependencies were reused.
- 6417 Finance tests passed against the rebuilt Finance assembly.
- After narrowing eligibility to concrete ranges, all 67 switch/recovery tests passed.
- New tests cover deferred factory calls on abandoned paths, demand before a switch,
  repeated demand after rollback, one-item streaming buffers and overlapping follows.
- Emitted-code tests compile at C# 8; the compatibility project also builds cleanly.
- [Paired samples, allocations and assembly hashes](../../benchmarks/results/fix-rule-methods-2026-09-17.json).
- [Current generated/handwritten quick comparison](../../benchmarks/results/fix-rule-methods-hand-2026-09-17.csv).
- Scratch harnesses and full logs: `.work/fix-methods`; paired harness source:
  `.work/hand-fix-analysis/paired/Program.cs` and `.work/fix-comparison/probe/Program.cs`.
  The legacy JSON property `Fix44Us` means the candidate Finance FixParser in this
  comparison, not the Fix44 grammar.
