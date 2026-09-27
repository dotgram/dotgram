# Before/after, 46dec223 (`../2026-09-26-yardstick-46dec223/`) against dbdb75ac (this directory), across two windows

Two separate windows, not a paired run: each side's own control is quoted, and a difference is read only where the two windows' five-run ranges do not overlap at all — not where the medians merely differ. Before: 22:56:06-23:30:56, controls 32.0/32.1/31.4/31.4/31.4 ns. After: 23:37:10-00:11:52, controls 31.8/31.6/31.5/31.5/31.5 ns (run 1 disturbed, see `summary.md`; kept, no material effect on the medians used here).

## The headline finding, checked against the source diff, not read off the ratio table alone

**The EL family's ratio table looks like a 40-63% improvement (`benchmarks/results/2026-09-26-yardstick-dbdb75ac/summary.md` §1 EL). It mostly is not one.** Splitting the ratio into its two absolute sides:

| row | hand before | hand after | hand Δ | generated (tape) before | generated after | generated Δ | ratio before | ratio after |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| el/floor | 341.2 | 750.5 | +119.9% | 792.0 | 818.3 | +3.3% | 2.32x | 1.09x |
| el/ladder | 1037.8 | 2863.1 | +175.9% | 1928.6 | 1961.9 | +1.7% | 1.86x | 0.69x |
| el/nest7 | 695.2 | 1873.3 | +169.5% | 1992.8 | 2050.2 | +2.9% | 2.87x | 1.09x |
| el/block | 1095.9 | 2181.9 | +99.1% | 1892.1 | 1891.8 | -0.0% | 1.73x | 0.87x |
| el/try | 3309.5 | 8545.5 | +158.2% | 4919.8 | 6651.6 | **+35.2%** | 1.49x | 0.78x |
| el/loop | 2332.7 | 5468.9 | +134.4% | 3979.6 | 4059.2 | +2.0% | 1.71x | 0.74x |
| el/overloads | 1884.1 | 5167.1 | +174.3% | 2672.6 | 4201.0 | **+57.2%** | 1.42x | 0.81x |
| el/string | 302.0 | 339.3 | +12.3% | 973.4 | 1008.8 | +3.6% | 3.22x | 2.97x |
| el/interpolation | 1437.8 | 2231.0 | +55.2% | 4079.9 | 3976.8 | -2.5% | 2.84x | 1.78x |
| el/untyped | 18695.8 | 27627.1 | +47.8% | 20497.2 | 27429.0 | **+33.8%** | 1.10x | 0.99x |
| el/refused-early | 464.8 | 895.6 | +92.7% | 1256.3 | 1270.8 | +1.1% | 2.70x | 1.42x |
| el/refused-late | 1565.1 | 1936.1 | +23.7% | 2036.7 | 2150.6 | +5.6% | 1.30x | 1.11x |

Every row's HAND baseline rose 12-176%; most rows' GENERATED (tape/immediate) time barely moved (0-6%); three rows' generated time genuinely rose (`el/try` +35.2%, `el/overloads` +57.2%, `el/untyped` +33.8%). **The ratio's fall is a hand-side effect on nine of twelve rows, not a generated-side win, and it hides three real generated-side regressions under an improving-looking number.**

Checked, not guessed: `git diff --stat 46dec223 dbdb75ac -- examples/DotGram.Handwritten` shows `HandExpression.cs | 110 +++++++++++++++++++---` (97 insertions, 13 deletions) and nothing else in that directory changed (`HandSqlStandard.cs`, the FIX and Web hand parsers: untouched). `src/DotGram.ExpressionLanguage` grew a new 317-line `ResolutionScope.cs` and substantial changes to `Names.cs`, `MemberResolver.cs`, `Overloads.cs`, `Caches.cs`, matching the commit range's own D146 work (`using static`, default namespaces, name scopes — see `45a28953`, `864fedbe`, `7f64378c`, `8b1a41a2`, `a3289437`, `55ae0752`, `74bb9d95`). **`HandExpression.cs` was updated to support the same language growth, and that support costs something on every parse — even `el/floor` (`(int x) => x`, using none of it) pays 120% more.** This is a real, intentional cost of a real feature addition, not a regression to chase, but it means the EL before/after in this window compares two different language surfaces, not the same surface measured twice: the 46dec223 hand parser predates default-namespace/using-static support entirely.

**What is trustworthy here and worth carrying forward is the three generated-side absolute regressions** (`el/try`, `el/overloads`, `el/untyped`), which moved on their own axis independent of the hand-side story and are not explained by anything above — `el/untyped`'s allocation also grew (`base-B` 16232 → 26112, before to after, +61%). Worth a look by whoever owns D146, not by the stand.

SQL:2023's hand parser (`HandSqlStandard.cs`) is confirmed unchanged in this range; its ~5% uniform speedup (see below) is not attributable to a source change and is closer to the size of the controls' own drift between windows (31.4-32.1 before, 31.5-31.8 after) than to a real effect — read it as noise, not a finding.

## SQL:2023, generated over hand, absolute sides (the improvement here IS on the generated side)

| row | hand before | hand after | hand Δ | generated before | generated after | generated Δ | ratio before | ratio after |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/nest8 | 2146.1 | 2037.8 | -5.0% | 10753.1 | 5610.3 | **-47.8%** | 5.01x | 2.75x |
| sql/values | 529.7 | 505.6 | -4.5% | 1854.0 | 1532.2 | **-17.4%** | 3.50x | 3.03x |
| sql/arithmetic | 1819.3 | 1745.5 | -4.1% | 4217.7 | 3681.2 | **-12.7%** | 2.32x | 2.11x |
| sql/select1 | 756.0 | 720.7 | -4.7% | 1630.6 | 1503.4 | -7.8% | 2.16x | 2.09x |
| sql/literal | 22.6 | 21.5 | -4.8% | 100.6 | 100.3 | -0.3% | 4.45x | 4.66x |
| sql/create | 895.0 | 873.1 | -2.5% | 2639.2 | 2646.4 | +0.3% | 2.95x | 3.03x |
| sql/refused-late | 3380.8 | 3223.6 | -4.6% | 14005.7 | 13636.3 | -2.6% | 4.14x | 4.23x |
| sql/select20 | 9128.9 | 8688.5 | -4.8% | 21230.6 | 20719.7 | -2.4% | 2.33x | 2.38x |

`sql/nest8` (deep nesting) is the standout: a genuine 47.8% generated-side improvement, well outside both windows' spreads (before [4.93x..5.18x], after [2.65x..2.97x], no overlap). It is consistent in shape with `b09d95aa` ("The marks a walk stands under are kept, not replayed from the log's start") — a fix aimed exactly at avoiding replay in a nested/backtracking walk — but that consistency is an observation, not a verified causal link; nobody has bisected it. `sql/values` and `sql/arithmetic` moved by less and are plausibly touched by the same or a related change. The other SQL:2023 rows did not move beyond the ~5% hand-side noise.

## FIX, Web, T-SQL: stable

All 28 non-EL, non-`sql/nest8`-family rows whose windows' ranges are disjoint (full list: `.work/yardstick_diff.py` output) differ by at most 2.2% (`web/date-time.offset` +2.2%, `fix/slope-2.stream` +2.1% — both barely outside overlap, likely noise dressed as a signal at this resolution, not reported as a finding). Every FIX, Web and T-SQL row's before/after ranges overlap; the largest overlapping medians differ by under 10% (`tsql/select-long` +9.8%, `fix/slope-0.text` +8.5%). Nothing here moved.

## Reading this comparison

This is two windows, not one paired run. Where a row's five-run range in one window does not touch its range in the other at all, the difference is real at this resolution (the EL and `sql/nest8`-family rows above); everywhere else, read nothing. The absolute-ns splitting done here for EL and SQL:2023 is what turned a misleading ratio-only reading into a correct one, and it is worth doing before quoting any before/after ratio, not only when the swing looks unusually large — a ratio's numerator and denominator can each move for a different, independent reason, and the ratio alone cannot tell which.
