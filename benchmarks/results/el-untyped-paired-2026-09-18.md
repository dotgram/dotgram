# el/untyped, paired, ten runs — 2026-09-18

`--stand-paired --only el/untyped`, before `3857460f` against after `e30cbb40` (expr's step 3),
one run after another on a quiet machine. Times in ns; change is after against before.

| run | hand | before | after | change |
| ---: | ---: | ---: | ---: | ---: |
| 1 | 40,218 | 34,843 | 44,461 | +27.6% |
| 2 | 30,882 | 45,805 | 42,790 | -6.6% |
| 3 | 27,908 | 43,854 | 41,450 | -5.5% |
| 4 | 28,624 | 45,521 | 43,125 | -5.3% |
| 5 | 23,255 | 41,682 | 42,077 | +0.9% |
| 6 | 26,911 | 45,123 | 42,915 | -4.9% |
| 7 | 27,342 | 44,444 | 41,086 | -7.6% |
| 8 | 23,559 | 42,052 | 42,647 | +1.4% |
| 9 | 27,685 | 44,327 | 41,409 | -6.6% |
| 10 | 22,728 | 41,460 | 42,707 | +3.0% |

Run 1 is an outlier: its hand reading is 40 µs against 23-31 in every other run. Over runs 2-10
the change is -7.6 to +3.0%, median -5%; the before side is bimodal (41.5-42 or 44-46 µs), the
after side steady at 41-43 µs. Allocation 16,408 to 16,552 B (+144 B, +0.9%) in every run.

The control of a one-row `--only` run read ~138 ns here, against ~30 in a full run: the control
loop was still tier 0 code. It is warmed now, and reads 31 ns.
