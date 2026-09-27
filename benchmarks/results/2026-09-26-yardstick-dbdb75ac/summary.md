# The generated parser against its hand-written one, 2026-09-26 (AFTER: sha dbdb75ac, `origin/main` HEAD)

`--stand --rebuild --repeat 5`, this worktree rebased onto `origin/main` and HEAD checked with `git log -1` immediately before the run — `dbdb75ac`. Window 23:37:10-00:11:52 (34.7 minutes), pinned 0-15, high priority, five runs, run 1 with `--rebuild`. Controls: 31.8, 31.6, 31.5, 31.5, 31.5 ns — all inside the gate, **no run was dropped** by the automated check.

**Run 1 was disturbed and is kept, per the architect's ruling.** Between the harness's own unannounced build (23:36:04-23:37:10, see the note below) and the announced window's start, two other sessions used the machine: performance-9f built `DotGram.Tests` from 23:37:56 for 46 s (killed on noticing), and expr's `DotGram.Tests.Slow`/Sql/Finance/Fix44 suites ran on 16-31 until 23:39:58. Run 1's rebuild (5 grammar-hosting projects) had not finished by 23:38:26; the disturbance's overlap with run 1's actual TIMING phase (as opposed to its build phase, which costs nothing but wall clock) is not established to the second. Run 1's control, 31.8 ns, is inside the gate, so it is kept rather than dropped, and the five worst rows below carry both the five-run median and the runs-2-5-only median for comparison — they agree to within each row's own run-to-run spread (see the table in §3), so nothing here is read as materially disturbed.

**Procedural note, not a measurement:** this run used `dotnet run --project ...`, which builds the harness itself (66 seconds, 23:36:04-23:37:10) before `Main` can announce the window — the same defect class as an unannounced build, from the stand's own tooling. Fixed going forward: `benchmarks/README.md` now says to build first and run the compiled `.dll` directly, never `dotnet run`, for any timing.

**This is the "after" side of a before/after comparison; read `diff.md` in this directory before quoting anything below as an improvement or a regression** — several of the swings below (the whole EL family) are dominated by a change to the hand-written yardstick's own cost, not by the generated parser moving, and `diff.md` is where that is split apart and checked against the source diff.

Raw files: `stand.md`, `stand.json` here; `run-1/` … `run-5/` on `T:\TEMP\dotgram-stand\yardstick2-out`, not copied (per-run JSON is large; ask if wanted).

## 1. Median ratio and allocation per row, per family

### FIX (text / bytes / stream), generated over hand

| row | hand ns | ratio | hand B | generated B |
| --- | ---: | ---: | ---: | ---: |
| fix/One.text | 70.5 | 0.75x | 264 | 136 |
| fix/One.bytes | 73.1 | 1.09x | 264 | 192 |
| fix/One.stream | 148.9 | 0.79x | 4512 | 392 |
| fix/Order.text | 563.8 | 0.70x | 880 | 752 |
| fix/Order.bytes | 592.9 | 0.89x | 880 | 808 |
| fix/Order.stream | 731.8 | 0.82x | 5048 | 928 |
| fix/BinaryMany.text | 5167.7 | 0.82x | 8888 | 8760 |
| fix/BinaryMany.bytes | 5422.9 | 0.96x | 8888 | 8816 |
| fix/BinaryMany.stream | 7221.7 | 0.76x | 12120 | 8000 |
| fix/Orders128.text | 66662.4 | 0.70x | 89272 | 89144 |
| fix/Orders128.bytes | 70481.1 | 0.87x | 89272 | 89200 |
| fix/Orders128.stream | 106068.9 | 0.63x | 82264 | 78144 |
| fix/OrderMalformed.text | 584.7 | 0.71x | 944 | 912 |
| fix/OrderMalformed.bytes | 602.7 | 0.90x | 944 | 968 |
| fix/OrderMalformed.stream | 729.4 | 0.83x | 5112 | 1088 |
| fix/slope-{0,1,2,4,8,16}.text | 25.7-889.4 | 0.73x-0.81x | 160-1752 | 32-1624 |
| fix/slope-{0,1,2,4,8,16}.bytes | 31.6-923.0 | 0.90x-1.34x | 184-1888 | 112-1816 |
| fix/slope-{0,1,2,4,8,16}.stream | 91.4-1138.1 | 0.81x-0.86x | 4464-6016 | 344-1896 |

Unchanged from the before window within noise (see `diff.md`): every FIX row's before/after ranges overlap.

### Web, generated over hand

| row | hand ns | ratio | hand B | generated B |
| --- | ---: | ---: | ---: | ---: |
| web/url.plain | 90.5 | 2.25x | 152 | 296 |
| web/url.full | 172.0 | 1.59x | 328 | 472 |
| web/url.ipv4 | 108.6 | 1.93x | 176 | 320 |
| web/url.long-path | 179.0 | 2.23x | 304 | 448 |
| web/url.refused | 58.3 | 2.88x | 0 | 0 |
| web/json.object | 542.5 | 1.56x | 1976 | 2520 |
| web/json.array | 606.7 | 1.21x | 2112 | 2336 |
| web/date-time.utc | 27.1 | 1.93x | 112 | 112 |
| web/date-time.offset | 48.9 | 1.61x | 144 | 144 |
| web/date-time.refused | 18.5 | 1.88x | 32 | 32 |

Unchanged within noise (see `diff.md`).

### EL, tape and immediate over hand — **read `diff.md` before quoting these**

| row | hand ns | tape ratio | immediate ratio | hand B | tape B | immediate B |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| el/floor | 750.5 | 1.09x | 1.08x | 1256 | 912 | 912 |
| el/ladder | 2863.1 | 0.69x | 0.66x | 2872 | 1528 | 1528 |
| el/nest7 | 1873.3 | 1.09x | 1.08x | 1832 | 1464 | 1464 |
| el/block | 2181.9 | 0.87x | 0.87x | 2912 | 2072 | 2072 |
| el/try | 8545.5 | 0.78x | 0.79x | 8568 | 5864 | 5864 |
| el/loop | 5468.9 | 0.74x | 0.74x | 6328 | 4632 | 4632 |
| el/overloads | 5167.1 | 0.81x | 0.82x | 7472 | 5744 | 5744 |
| el/string | 339.3 | 2.97x | 2.93x | 1248 | 1056 | 1056 |
| el/interpolation | 2231.0 | 1.78x | 1.80x | 2632 | 2056 | 2056 |
| el/untyped | 27627.1 | 0.99x | 0.97x | 26112 | 24600 | 24600 |
| el/refused-early | 895.6 | 1.42x | 1.38x | 1256 | 752 | 752 |
| el/refused-late | 1936.1 | 1.11x | 1.12x | 2464 | 752 | 752 |

**Every ratio here fell from the before window, and most of that fall is the hand column's `HandExpression.cs` growing to support `using static` and default namespaces (D146) — not the generated parser improving.** `diff.md` splits hand and generated apart per row; three rows (`el/try`, `el/overloads`, `el/untyped`) show a genuine generated-side regression that this table alone would hide.

### SQL:2023, generated over hand

| row | hand ns | ratio | hand B | generated B |
| --- | ---: | ---: | ---: | ---: |
| sql/literal | 21.5 | 4.66x | 40 | 40 |
| sql/column | 131.3 | 2.44x | 392 | 272 |
| sql/arithmetic | 1745.5 | 2.11x | 4208 | 3832 |
| sql/nest8 | 2037.8 | 2.75x | 5976 | 6528 |
| sql/condition | 2139.6 | 2.02x | 5696 | 4680 |
| sql/select1 | 720.7 | 2.09x | 1576 | 1584 |
| sql/select20 | 8688.5 | 2.38x | 23976 | 21648 |
| sql/values | 505.6 | 3.03x | 1648 | 1816 |
| sql/comment | 2351.8 | 2.15x | 5408 | 5064 |
| sql/conditions100 | 63165.2 | 2.03x | 188160 | 153696 |
| sql/conditions1000 | 635156.2 | 2.10x | 1872904 | 1536096 |
| sql/create | 873.1 | 3.03x | 1584 | 1448 |
| sql/refused-late | 3223.6 | 4.23x | 8376 | 13240 |

`sql/nest8` fell from 5.01x to 2.75x, a real generated-side improvement (`diff.md`); the rest are flat or moved by the small hand-side noise common to the whole window (~5%).

### T-SQL, generated over scriptdom (its own column — no hand-written parser)

| row | scriptdom ns | ratio | scriptdom B | generated B |
| --- | ---: | ---: | ---: | ---: |
| tsql/select-join | 30337.4 | 0.12x | 61512 | 3280 |
| tsql/insert-values | 12073.4 | 0.21x | 48504 | 2360 |
| tsql/create-table | 19209.4 | 0.19x | 55656 | 4608 |
| tsql/update-subquery | 19035.0 | 0.15x | 54632 | 2528 |
| tsql/select-long | 92441.8 | 0.15x | 138064 | 13224 |
| tsql/comment | 20086.9 | 0.08x | 53584 | 1072 |

Unchanged within noise (see `diff.md`).

## 2. First-call row, per family

Median of three fresh processes, milliseconds. Web and T-SQL have no designated first-call row.

| row | reading | ms |
| --- | --- | ---: |
| fix/One.text | hand | 26.89 |
| fix/One.text | generated | 26.17 |
| fix/One.bytes | hand | 25.99 |
| fix/One.bytes | generated | 27.12 |
| el/floor | hand | 34.00 |
| el/floor | tape | 22.52 |
| el/floor | immediate | 21.37 |
| sql/literal | hand | 1.12 |
| sql/literal | generated | 10.96 |
| sql/select20 | hand | 16.69 |
| sql/select20 | generated | 36.41 |
| fixmsg/Order44.strict | generated | 41.61 |
| fixmsg/Order44.strict | generated-loaded | 225.52 |
| fixmsg/Order44.strict | reference-QuickFIXn | 28.40 |

Two rows worth flagging rather than trusting outright, both medians of only THREE fresh processes: `el/floor hand` rose from 11.70 ms (before window) to 34.00 ms here — consistent in direction with the `HandExpression.cs` growth (§1 EL, `diff.md`) since the hand parser's static setup would also cost more on the very first call, but three samples is not enough to separate a real per-process fixed cost from noise; and `fixmsg/Order44.strict generated-loaded` fell from 792.20 ms to 225.52 ms, which is a large swing for QuickFIX/n's one-time dictionary-loading cost and unexplained by anything in this window (no source under the reference library's path changed). Neither is asserted as a finding; both are named so nobody re-derives them from scratch.

## 3. The five worst rows by ratio, with the spread across the five runs

| row | reading | median-table ratio | per-run ratios | median of 5 | range | median of runs 2-5 (run 1 excluded) |
| --- | --- | ---: | --- | ---: | --- | ---: |
| sql/literal | generated | 4.66x | 4.75x, 4.53x, 5.23x, 4.76x, 4.66x | 4.75x | [4.53x .. 5.23x] | 4.71x |
| sql/refused-late | generated | 4.23x | 4.34x, 4.37x, 4.39x, 3.94x, 4.23x | 4.34x | [3.94x .. 4.39x] | 4.30x |
| sql/values | generated | 3.03x | 3.28x, 2.98x, 3.04x, 3.12x, 3.03x | 3.04x | [2.98x .. 3.28x] | 3.04x |
| sql/create | generated | 3.03x | 3.10x, 2.99x, 3.03x, 2.98x, 3.09x | 3.03x | [2.98x .. 3.10x] | 3.01x |
| el/string | tape | 2.97x | 2.82x, 2.88x, 3.06x, 3.03x, 2.89x | 2.89x | [2.82x .. 3.06x] | 2.96x |

`sql/nest8`, the before window's worst row (5.01x), is no longer in the worst five (2.75x — the improvement in `diff.md`). The runs-2-5 medians agree with the five-run medians to within 0.04-0.07 points on every row, well inside each row's own range: run 1's disturbance changed nothing here.

## 4. Rows with ratio under 1.0 (generated/tape/immediate faster than its base)

| row | family | base | ratio | note |
| --- | --- | --- | ---: | --- |
| tsql/select-join, comment, update-subquery, select-long, create-table, insert-values | tsql | scriptdom | 0.08x-0.21x | unchanged, see §1 |
| fix/Orders128.stream | fix | hand | 0.63x | unchanged |
| fix/Orders128.text, Order.text, slope-0.text, BinaryMany.stream, One.text, OrderMalformed.text, slope-16.text | fix | hand | 0.70x-0.76x | unchanged |
| feeds/stock-count.good.text | feeds | hand | 0.80x | unchanged |
| **el/ladder** | el | hand | **0.69x / 0.66x** | **new since the before window — see the caveat below** |
| fix/slope-1/2/4/8.text, slope-{0..4}.stream | fix | hand | 0.79x-0.86x | unchanged |
| feeds/stock-count.broken.text | feeds | hand | 0.93x | unchanged |
| **el/try** | el | hand | **0.78x / 0.79x** | **new; generated side is 35% SLOWER in absolute terms, see below** |
| fix/BinaryMany.text | fix | hand | 0.82x | unchanged |
| **el/loop** | el | hand | **0.74x** | **new since the before window** |
| feeds/stock-count.good.reader, small.reader, small.text | feeds | hand | 0.88x-0.93x | unchanged |
| fix/Order.bytes, slope-16.bytes, Orders128.bytes | fix | hand | 0.87x-0.90x | unchanged |
| feeds/stock-count.broken.reader | feeds | hand | 0.95x | unchanged |
| fix/slope-8.bytes, slope-4.bytes | fix | hand | 0.95x-0.97x | unchanged |
| **el/overloads** | el | hand | **0.81x / 0.82x** | **new; generated side is 57% SLOWER in absolute terms, see below** |
| fix/OrderMalformed.bytes | fix | hand | 0.90x | unchanged |
| **el/block** | el | hand | **0.87x** | **new since the before window** |
| **el/untyped** | el | hand | **0.99x / 0.97x** | **new (was 1.07-1.10x before)** |

**Do not read the five bolded EL rows as "the generated parser now beats hand": for `el/try`, `el/overloads` and `el/untyped` the generated side got SLOWER in absolute terms (+35%, +57%, +34%; §1 EL, `diff.md`), and for all five the drop below 1.0x is carried almost entirely by `HandExpression.cs` growing to support D146's language additions.** They belong here only because the question was phrased as a ratio; `diff.md` is where the honest version of this row lives.

## What this does not say

Same caveat as the before-window's own summary: this is one window's five runs, not an explanation of why anything moved. `sql/nest8`'s improvement is consistent with `b09d95aa`'s marks-walk fix by shape (a nested/backtracking walk no longer replayed) but that is an observation, not a verified causal link — nobody has bisected it. The three EL generated-side regressions (`el/try`, `el/overloads`, `el/untyped`) are unexplained here and are the one piece of this whole exercise that looks like it wants an owner's attention, not the stand's.
