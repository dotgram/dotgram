Median of 5 of 5 runs, each in a process of its own; control 31.7 ns (the runs' controls: 32.0, 31.3, 31.7, 32.5, 31.4).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-18 20:05

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/literal | generated | 51.1 | 263.2 | 5.15x | 211.3 | 4.13x | -19.7% | 184 | 160 |
| sql/column | generated | 137.4 | 2370.6 | 17.25x | 1431.5 | 10.42x | -39.6% | 464 | 392 |
| sql/arithmetic | generated | 1624.5 | 23830.1 | 14.67x | 17480.5 | 10.76x | -26.6% | 3592 | 3472 |
| sql/nest8 | generated | 2681.7 | 45721.7 | 17.05x | 42644.5 | 15.90x | -6.7% | 6528 | 6504 |
| sql/condition | generated | 1896.2 | 19484.8 | 10.28x | 17813.9 | 9.39x | -8.6% | 5096 | 4928 |
| sql/select1 | generated | 744.3 | 8960.6 | 12.04x | 6928.6 | 9.31x | -22.7% | 1736 | 1688 |
| sql/select20 | generated | 7437.7 | 125831.7 | 16.92x | 94941.0 | 12.76x | -24.5% | 21520 | 21448 |
| sql/values | generated | 485.4 | 8373.4 | 17.25x | 8009.9 | 16.50x | -4.3% | 1928 | 1904 |
| sql/create | generated | 833.8 | 6735.5 | 8.08x | 6273.2 | 7.52x | -6.9% | 1664 | 1568 |
| sql/refused-late | generated | 2851.8 | 88532.6 | 31.04x | 58782.7 | 20.61x | -33.6% | 14848 | 13832 |
