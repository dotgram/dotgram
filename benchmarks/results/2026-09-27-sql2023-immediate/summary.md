# SQL:2023 on the immediate carrier, beside the shipped tape, 2026-09-27

For Igor, via architect: whether `SqlStandardParser` (SQL:2023, `SqlStandard.gram`) should switch
from the tape carrier it ships on to the immediate one — the same question the 2026-09-05 SQL-92
comparison answered in the immediate carrier's favour (1.1-1.6x of hand-written vs the tape's
2.1-3.0x). This grammar had not been measured that way before.

**Verdict: no. The immediate carrier is 3-4x *slower* than the shipped tape here**, the reverse of
the SQL-92 result. SQL:2023 has 664 normalized rules against SQL-92's far fewer, and 302 value
tables and eight value types the immediate carrier's per-alternative factory calls exercise far
more of; the mechanism that won at SQL-92's scale loses at this one (`unit cost does not
extrapolate` — a mechanism cheap at one scale is not the same mechanism at another).

Setup (host, `InternalsVisibleTo`, `--stand-check`) was committed and pushed earlier as `e44f7ea6`.
The timing window below ran at that HEAD. `origin/main` has since taken two unrelated commits
(`c9d3e11b` T-SQL DROP records, `9572f952` README cleanup — neither touches `SqlStandard.gram`,
the generator, or any carrier code) plus two of this session's own (`5e3588cf` a `Stand.cs`
generator-report regex fix found while pulling §4 below, and `2576beb5` the `--sql2023-stack` tool
used in §3); none of the four bear on the numbers here. Current HEAD: `49992cdd`.

Raw files: `stand.md`, `stand.json` here (the `sql/` three-column run); `run-1/` … `run-5/` on
`T:\TEMP\dotgram-stand\sql2023`, not copied.

## 1. The `sql/` rows: hand, tape, immediate

`--stand --rebuild --repeat 5`, worktree synced and `git log -1` verified (`e44f7ea6`) immediately
before opening. Window 01:29:30-02:09:37 (40.1 minutes), pinned 0-15, high priority, run 1 with
`--rebuild`. Controls: 31.5, 31.8, 31.5, 31.5, 31.7 ns — median 31.5 ns, **no run was dropped**.
Every row's tree agreed across all three readings before timing (`--stand-check`, part of the
setup commit).

Bytes are identical between tape and immediate on every row (same tree built either way); only
`hand B` sometimes differs, because the hand-written reader's own representation isn't always the
generated tree's.

| row | hand ns | hand B | tape ns | tape/hand | immediate ns | immediate/hand | immediate/tape | spread |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/literal | 22.7 | 40 | 100.5 | 4.43x | 88.7 | 3.91x | 0.88x | 4% |
| sql/column | 134.8 | 392 | 323.7 | 2.40x | 1859.7 | 13.80x | 5.75x | 5% |
| sql/arithmetic | 1835.0 | 4208 | 3907.0 | 2.13x | 12778.8 | 6.96x | 3.27x | 3% |
| sql/nest8 | 2170.0 | 5976 | 5795.6 | 2.67x | 24938.5 | 11.49x | 4.30x | 6% |
| sql/condition | 2267.9 | 5696 | 4470.2 | 1.97x | 15043.6 | 6.63x | 3.37x | 5% |
| sql/select1 | 759.0 | 1576 | 1542.6 | 2.03x | 5872.0 | 7.74x | 3.81x | 4% |
| sql/select20 | 9253.8 | 23976 | 21446.5 | 2.32x | 73344.0 | 7.93x | 3.42x | 5% |
| sql/values | 530.0 | 1648 | 1593.0 | 3.01x | 6995.7 | 13.20x | 4.39x | 22% |
| sql/comment | 2499.3 | 5408 | 5155.1 | 2.06x | 16999.8 | 6.80x | 3.30x | 15% |
| sql/conditions100 | 66874.2 | 188160 | 132995.6 | 1.99x | 406387.9 | 6.08x | 3.06x | 33% |
| sql/conditions1000 | 706478.1 | 1872904 | 1349255.5 | 1.91x | 4235335.9 | 5.99x | 3.14x | 17% |
| sql/create | 893.9 | 1584 | 2689.7 | 3.01x | 5124.1 | 5.73x | 1.90x | 15% |
| sql/refused-late | 3401.7 | 8376 | 13875.0 | 4.08x | 45408.1 | 13.35x | 3.27x | 19% |

`generated B` (tape) and immediate's own B, for the record, both equal to `hand B` on `sql/literal`
and equal to each other and each *unequal* to `hand B` on every other row: 272 (`sql/column`),
3832 (`sql/arithmetic`), 6528 (`sql/nest8`), 4680 (`sql/condition`), 1584 (`sql/select1`), 21648
(`sql/select20`), 1816 (`sql/values`), 5064 (`sql/comment`), 153696 (`sql/conditions100`), 1536096
(`sql/conditions1000`), 1448 (`sql/create`), 13240 (`sql/refused-late`) — full detail in `stand.md`.
**The immediate carrier allocates the same as the tape on every row; the cost is entirely wall
clock**, consistent with the extra work being failed-alternative construction calls, not extra
retained heap.

`immediate/tape` clusters 3.06x-4.39x on every row big enough for the ratio to mean something;
`sql/literal` (40 bytes) and `sql/create` (1.90x) are the two exceptions, both among the smallest
inputs, where fixed per-call overhead dominates and the ratio is noisier (spread 4% and 15%
respectively, but on a small absolute base).

## 2. First call, both carriers

Already covered by the same run: `sql/literal` and `sql/select20` are in `Stand.cs`'s `FirstRows`,
and the "immediate" reading added to those workloads for this task rides along automatically.
Fresh process, median of three:

| row | hand ms | tape ms | immediate ms |
| --- | ---: | ---: | ---: |
| sql/literal | 1.10 | 10.95 | 6.44 |
| sql/select20 | 16.83 | 36.84 | 40.77 |

Mixed: on the tiny input the immediate carrier's first call is *cheaper* than the tape's (6.44 vs
10.95 ms — plausibly less JIT/type-loading machinery for a carrier with no prefix-table dispatch to
warm up), but on the more representative `select20` it costs slightly more (40.77 vs 36.84 ms).
Neither difference is large enough, against a process's first ~300 ms being five times slower
regardless (`perf-lab-2026-09`), to change anything about a consumer's cold-start experience either
way.

## 3. Stack cost per nesting level

`--sql2023-stack` (new, `Sql2023Stack.cs`, committed `2576beb5`): the same `VirtualQuery`
high-water measurement `StackFrameBudgetTests` takes of the shipped grammars
(`tests/DotGram.Tests.Slow/StackFrameBudgetTests.cs`), adapted to `SearchCondition` refused at
depths 400/800 on both carriers, one thread, shallow first. Not run through an announced window —
it measures committed memory, not wall-clock nanoseconds, the way the xUnit test it's adapted from
never has either. Built at HEAD `49992cdd`, Release:

```
tape:      shallow  1,380,352 B, deep  2,674,688 B,   3.16 KiB/level
immediate: shallow  1,368,064 B, deep  2,674,688 B,   3.19 KiB/level
ratio (immediate / tape): 1.01x
```

**Essentially identical — the immediate carrier does not recurse meaningfully deeper per level.**
This is the one place the a-priori worry (an extra factory call per alternative tried costing extra
stack, not just extra time) does not show up: whatever the immediate carrier's per-alternative
construction costs in time (§1), it is not spent in frames that outlive the call, so it does not
compound against the refusal-depth budget the way the timing cost compounds against wall clock.

(For context, not a comparison: `StackFrameBudgetTests` last measured the shipped tape's SQL:2023
level at 2.38 KiB in Debug — the note in `stack-cost-of-a-nesting-level.md`. The 3.16 KiB here is
Release, a different configuration, and the two numbers are not the same measurement; the Debug
vs. Release difference StackFrameBudgetTests documents (~20%) doesn't fully cover the gap, so this
is flagged rather than reconciled, since reconciling it isn't what this task asked and the
tape-vs-immediate comparison under one config, taken together in the same process run, is unaffected
either way.)

## 4. Generated size and generation time

Read directly off each host's own `*.DotGramReport.g.cs` and the files/DLLs on disk — no window
needed, these are filesystem facts and a single incidental build reading, not a repeated timing.
The tape's generation time was captured cleanly inside an announced (but `-Probe`, dirty-tree)
window taken to verify the `Stand.cs` regex fix (§ commit `5e3588cf`); it is one reading, not a
five-run median, the way §1's timing rows are.

| | tape (`SqlStandardParser`) | immediate (`ImmediateSqlStandard`) |
| --- | ---: | ---: |
| rules | 664 | 664 |
| generated .g.cs, bytes (incl. 3-byte UTF-8 BOM) | 8,914,032 | 30,251,272 |
| generated .g.cs, lines | 218,593 | 892,896 |
| generation time | 3712 ms | 5108 ms |

The immediate carrier's own generated source is **3.4x the tape's** in both bytes and lines, and
takes **1.4x as long to generate** — expected, since the immediate carrier inlines each
alternative's construction into the reader itself rather than deferring to the tape's separate
value-table/tree-builder split.

DLL size is not a clean like-for-like point here: the tape ships as its own package,
`DotGram.Sql.dll` (net10.0, Release) is 20,239,872 bytes; the immediate carrier is not a separate
assembly, it is one host among dozens compiled into `DotGram.Benchmarks.dll` (13,025,280 bytes),
so that number is not attributable to `ImmediateSqlStandard` alone and isn't reported as if it
were (`a ratio needs both numbers` — there is no isolated immediate-carrier binary to put one on
each side of a ratio). The generated-source bytes above are the honest comparison.

## Bottom line

Every measured axis but stack depth favours the shipped tape for SQL:2023: 3-4x slower per parse,
3.4x more generated source, 1.4x longer to generate, and no compensating win in allocation, first
call, or stack budget. The SQL-92 result does not generalize to SQL:2023 at this grammar's size;
`ImmediateSqlStandard` stays a benchmark-only comparison host, not a candidate for what ships.
