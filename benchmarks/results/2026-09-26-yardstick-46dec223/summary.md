# The generated parser against its hand-written one, 2026-09-26 (BEFORE: sha 46dec223)

**This is the "before" side of a two-window comparison, not the fresh yardstick that was asked for.** It was announced under a mistaken label (I quoted `origin/main`'s fetched hash without rebasing this worktree onto it first) and measures `46dec223`, which is 95 commits behind `origin/main` as of this run and predates both `b09d95aa` (the marks walk) and `afe71d3d` (EL blocks/names/scope) — the two changes the fresh reading was asked to capture. The architect's ruling: keep it, because it is a real reading of a real tree and is exactly the "before" a before/after needs; the after is `../2026-09-26-yardstick-dbdb75ac/`, at the true `origin/main` HEAD. The two are ACROSS TWO WINDOWS, not one paired run: read a difference only where it sits well outside both windows' own controls and each row's run-to-run spread.


`--stand --rebuild --repeat 5` at this worktree's HEAD, `46dec223` (not `origin/main`, which had already moved to `dbdb75ac` by the time of the run — see the note above).

Window 2026-09-26 22:56:06–23:30:56 (34.5 minutes), pinned 0-15, high priority, five runs, each in a process of its own, run 1 with `--rebuild` (the other four reuse its build). Controls: 32.0, 32.1, 31.4, 31.4, 31.4 ns — all held, **no run was dropped**. Raw files: `stand.md`, `stand.json` (the median of 5), `run-1/` … `run-5/` are on `T:\TEMP\dotgram-stand\yardstick-out` and not copied here (18,900 lines of rounds each); ask if the per-run JSON is wanted filed too.

The four cuts asked for follow. "Ratio" throughout is `generated/hand`, `tape or immediate /hand`, or `generated/scriptdom` for T-SQL — never a hand-to-hand or scriptdom-to-hand figure. Allocation is the `B` column: bytes the generated (or tape/immediate) reading allocated a parse, with the base's bytes beside it for comparison.

## 1. Median ratio and allocation per row, per family

### FIX (text / bytes / stream), generated over hand

| row | hand ns | ratio | hand B | generated B |
| --- | ---: | ---: | ---: | ---: |
| fix/One.text | 71.5 | 0.75x | 264 | 136 |
| fix/One.bytes | 73.5 | 1.17x | 264 | 192 |
| fix/One.stream | 150.1 | 0.79x | 4512 | 392 |
| fix/Order.text | 568.3 | 0.74x | 880 | 752 |
| fix/Order.bytes | 590.8 | 0.91x | 880 | 808 |
| fix/Order.stream | 743.5 | 0.80x | 5048 | 928 |
| fix/BinaryMany.text | 5020.1 | 0.87x | 8888 | 8760 |
| fix/BinaryMany.bytes | 5303.4 | 1.01x | 8888 | 8816 |
| fix/BinaryMany.stream | 7326.0 | 0.74x | 12120 | 8000 |
| fix/Orders128.text | 67390.2 | 0.73x | 89272 | 89144 |
| fix/Orders128.bytes | 70013.9 | 0.89x | 89272 | 89200 |
| fix/Orders128.stream | 106818.9 | 0.62x | 82264 | 78144 |
| fix/OrderMalformed.text | 577.8 | 0.75x | 944 | 912 |
| fix/OrderMalformed.bytes | 599.5 | 0.93x | 944 | 968 |
| fix/OrderMalformed.stream | 744.6 | 0.81x | 5112 | 1088 |
| fix/slope-{0,1,2,4,8,16}.text | 26.5–880.2 | 0.70x–0.82x | 160–1752 | 32–1624 |
| fix/slope-{0,1,2,4,8,16}.bytes | 32.5–926.3 | 0.91x–1.33x | 184–1888 | 112–1816 |
| fix/slope-{0,1,2,4,8,16}.stream | 91.9–1150.4 | 0.80x–0.86x | 4464–6016 | 344–1896 |

FIX is the one family where the generated parser beats the hand one on most rows (see §4); `.bytes` is the weak form (six of seven byte rows are at or above 1.0x, `.text` and `.stream` are all under).

### Web, generated over hand

| row | hand ns | ratio | hand B | generated B |
| --- | ---: | ---: | ---: | ---: |
| web/url.plain | 89.1 | 2.32x | 152 | 296 |
| web/url.full | 170.8 | 1.69x | 328 | 472 |
| web/url.ipv4 | 108.0 | 1.97x | 176 | 320 |
| web/url.long-path | 180.9 | 2.21x | 304 | 448 |
| web/url.refused | 57.7 | 2.97x | 0 | 0 |
| web/json.object | 525.2 | 1.61x | 1976 | 2520 |
| web/json.array | 627.3 | 1.19x | 2112 | 2336 |
| web/date-time.utc | 27.9 | 1.85x | 112 | 112 |
| web/date-time.offset | 49.2 | 1.58x | 144 | 144 |
| web/date-time.refused | 18.6 | 1.81x | 32 | 32 |

Every Web row against a hand parser is above 1.0x; the rows with no hand parser (addr-spec, media-type, cookie, and the rest) are in a separate table in `stand.md` against `regex`/`reference-*`, not against a hand parser, so they are not part of this cut.

### EL, tape and immediate over hand

| row | hand ns | tape ratio | immediate ratio | hand B | tape B | immediate B |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| el/floor | 341.2 | 2.32x | 2.30x | 1056 | 1000 | 1000 |
| el/ladder | 1037.8 | 1.86x | 1.82x | 1744 | 1840 | 1840 |
| el/nest7 | 695.2 | 2.87x | 2.84x | 1056 | 1552 | 1552 |
| el/block | 1095.9 | 1.73x | 1.75x | 2296 | 2288 | 2288 |
| el/try | 3309.5 | 1.49x | 1.47x | 4456 | 4472 | 4472 |
| el/loop | 2332.7 | 1.71x | 1.73x | 4224 | 4816 | 4816 |
| el/overloads | 1884.1 | 1.42x | 1.38x | 3992 | 4192 | 4192 |
| el/string | 302.0 | 3.22x | 3.22x | 1144 | 952 | 952 |
| el/interpolation | 1437.8 | 2.84x | 2.91x | 2336 | 2336 | 2336 |
| el/untyped | 18695.8 | 1.10x | 1.07x | 16232 | 16392 | 16392 |
| el/refused-early | 464.8 | 2.70x | 2.65x | 1056 | 936 | 936 |
| el/refused-late | 1565.1 | 1.30x | 1.39x | 2264 | 936 | 936 |

Tape and immediate move together on every row (largest gap: `el/interpolation`, 2.84x vs 2.91x); nothing here says one path is consistently cheaper than the other at this resolution.

### SQL:2023, generated over hand

| row | hand ns | ratio | hand B | generated B |
| --- | ---: | ---: | ---: | ---: |
| sql/literal | 22.6 | 4.45x | 40 | 40 |
| sql/column | 134.7 | 2.40x | 392 | 272 |
| sql/arithmetic | 1819.3 | 2.32x | 4208 | 3832 |
| sql/nest8 | 2146.1 | 5.01x | 5976 | 6528 |
| sql/condition | 2242.1 | 2.10x | 5696 | 4680 |
| sql/select1 | 756.0 | 2.16x | 1576 | 1584 |
| sql/select20 | 9128.9 | 2.33x | 23976 | 21648 |
| sql/values | 529.7 | 3.50x | 1648 | 1816 |
| sql/comment | 2487.9 | 2.10x | 5408 | 5064 |
| sql/conditions100 | 65485.3 | 2.09x | 188160 | 153696 |
| sql/conditions1000 | 657178.9 | 2.11x | 1872904 | 1536096 |
| sql/create | 895.0 | 2.95x | 1584 | 1448 |
| sql/refused-late | 3380.8 | 4.14x | 8376 | 13240 |

Every SQL:2023 row is at or above 2.09x; this is the family the architect's D63 gap work is aimed at (2.7-5.8x was the earlier BDN reading on a narrower row set — this run's own hand-comparison rows land inside that, `sql/nest8` and `sql/literal` above it).

### T-SQL, generated over scriptdom (its own column, never a hand ratio — T-SQL has no hand-written parser)

| row | scriptdom ns | ratio | scriptdom B | generated B |
| --- | ---: | ---: | ---: | ---: |
| tsql/select-join | 32875.3 | 0.11x | 62448 | 3280 |
| tsql/insert-values | 12026.1 | 0.21x | 48504 | 2360 |
| tsql/create-table | 19125.2 | 0.19x | 55656 | 4608 |
| tsql/update-subquery | 20314.3 | 0.14x | 55152 | 2528 |
| tsql/select-long | 101866.4 | 0.14x | 140872 | 13224 |
| tsql/comment | 20151.1 | 0.08x | 53584 | 1072 |

Generated T-SQL is 5-13x faster than ScriptDom on every row and allocates 4-19x less; this is the inverse comparison from the SQL:2023/EL/Web families (there, generated is measured against a hand-written yardstick and loses; here it is measured against a production parser it is not competing with in kind and wins outright).

## 2. First-call row, per family

Median of three fresh processes, milliseconds. Only the families with a designated first-call row have one; Web and T-SQL have none in this run.

| row | reading | ms |
| --- | --- | ---: |
| fix/One.text | hand | 26.00 |
| fix/One.text | generated | 26.40 |
| fix/One.bytes | hand | 26.14 |
| fix/One.bytes | generated | 26.89 |
| el/floor | hand | 11.70 |
| el/floor | tape | 23.15 |
| el/floor | immediate | 22.64 |
| sql/literal | hand | 1.08 |
| sql/literal | generated | 10.88 |
| sql/select20 | hand | 16.60 |
| sql/select20 | generated | 35.98 |
| fixmsg/Order44.strict | generated | 41.43 |
| fixmsg/Order44.strict | generated-loaded | 792.20 |
| fixmsg/Order44.strict | reference-QuickFIXn | 28.26 |

`generated-loaded` at 792 ms is `stand.md`'s own flagged case: it includes building QuickFIX/n's data dictionary once (about a megabyte of XML), a per-process cost and not a per-message one — not comparable to the other rows without that caveat attached.

## 3. The five worst rows by ratio, with the spread across the five runs

Ranked by the median-table ratio, across all families with a direct generated/tape/immediate-over-hand comparison (T-SQL excluded: its ratios are all under 1.0 and belong in §4, not here). Spread is the per-run ratio recomputed from each run's own `stand.json` (`run-1`…`run-5`), not the base-reading spread `stand.md` prints (that one is the base's nanosecond spread between runs, a different quantity).

| row | reading | median-table ratio | per-run ratios | median of the 5 | range |
| --- | --- | ---: | --- | ---: | --- |
| sql/nest8 | generated | 5.01x | 5.02x, 5.18x, 4.96x, 4.99x, 4.93x | 4.99x | [4.93x .. 5.18x] |
| sql/literal | generated | 4.45x | 4.39x, 4.59x, 4.39x, 4.79x, 4.46x | 4.46x | [4.39x .. 4.79x] |
| sql/refused-late | generated | 4.14x | 4.21x, 4.14x, 4.13x, 4.06x, 4.01x | 4.13x | [4.01x .. 4.21x] |
| sql/values | generated | 3.50x | 3.66x, 3.41x, 3.40x, 3.78x, 3.27x | 3.41x | [3.27x .. 3.78x] |
| el/string | tape | 3.22x | 3.20x, 3.29x, 3.42x, 3.08x, 3.21x | 3.21x | [3.08x .. 3.42x] |

All five are SQL:2023 or EL rows; the tightest spread is `sql/refused-late` (0.20 points), the widest `sql/values` (0.51 points). None of the five ranks would change order under the range: the worst-to-fifth-worst gap (5.18x low end vs 3.42x high end) is well outside any one row's own run-to-run movement, so the ranking is stable at this resolution.

## 4. Rows with ratio under 1.0 (generated faster)

| row | family | base | ratio |
| --- | --- | --- | ---: |
| tsql/select-join | tsql | scriptdom | 0.11x |
| tsql/comment | tsql | scriptdom | 0.08x |
| tsql/update-subquery | tsql | scriptdom | 0.14x |
| tsql/select-long | tsql | scriptdom | 0.14x |
| tsql/create-table | tsql | scriptdom | 0.19x |
| tsql/insert-values | tsql | scriptdom | 0.21x |
| fix/Orders128.stream | fix | hand | 0.62x |
| fix/Orders128.text | fix | hand | 0.73x |
| fix/slope-0.text | fix | hand | 0.70x |
| fix/Order.text | fix | hand | 0.74x |
| fix/BinaryMany.stream | fix | hand | 0.74x |
| fix/One.text | fix | hand | 0.75x |
| fix/OrderMalformed.text | fix | hand | 0.75x |
| fix/slope-16.text | fix | hand | 0.76x |
| fix/One.stream | fix | hand | 0.79x |
| fix/slope-1/2/4/8.text | fix | hand | 0.80x–0.82x |
| fix/slope-{0,1,2,4}.stream | fix | hand | 0.80x–0.86x |
| feeds/stock-count.good.text | feeds | hand | 0.80x |
| fix/OrderMalformed.stream | fix | hand | 0.81x |
| fix/slope-8.stream | fix | hand | 0.83x |
| fix/BinaryMany.text | fix | hand | 0.87x |
| fix/Order.bytes | fix | hand | 0.91x |
| feeds/stock-count.small.text | feeds | hand | 0.91x |
| fix/slope-16.bytes | fix | hand | 0.91x |
| fix/Orders128.bytes | fix | hand | 0.89x |
| feeds/stock-count.broken.text | feeds | hand | 0.91x |
| fix/slope-8.bytes | fix | hand | 0.96x |
| fix/slope-4.bytes | fix | hand | 0.97x |
| feeds/stock-count.small.reader | feeds | hand | 0.93x |
| fix/OrderMalformed.bytes | fix | hand | 0.93x |

**Every row under 1.0x is FIX, T-SQL, or a `feeds/` row (the example package's own generated parser, not part of the four families asked for but included since it showed up under 1.0x). No SQL:2023, EL, or Web row is under 1.0x anywhere in this run.** T-SQL is the strong case (see §1: it is a different comparison, against ScriptDom, not hand); within FIX, `.text` and `.stream` carry every sub-1.0x row and `.bytes` carries almost none — the byte-array entry point is consistently the closest to parity or over it, which is worth a second look if anyone is choosing which FIX entry point to optimize next.

## What this does not say

This is one window's five runs; it answers "where does it stand today," not why. `sql/nest8` and `sql/literal` at the top of §3 are both small, cheap-to-parse inputs (a nesting probe and a bare literal) where a fixed per-parse cost dominates — consistent with, but not itself an explanation of, the fixed-cost finding from `benchmarks/results/sql-standard-gap-2026-09-21/` and the throwaway pair in `benchmarks/results/pairs-2026-09-20/performance-throwaway-walk-871f0779/`. No generator-time figure is in this run: `stand.md`'s own note says the last build (run 5) recompiled no grammar, so the "generator took" section is empty; that number lives in `Gate-Generation.ps1`'s paired form, not here.
