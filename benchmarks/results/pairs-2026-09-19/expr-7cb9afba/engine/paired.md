Median of 5 of 5 runs, each in a process of its own; control 31.2 ns (the runs' controls: 31.3, 31.1, 31.2, 31.2, 31.1).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 09:33

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.stream | generated | 138.1 | 518.1 | 3.75x | 537.6 | 3.89x | +3.8% | 824 | 824 |
| fix/Order.stream | generated | 752.3 | 2735.8 | 3.64x | 2750.4 | 3.66x | +0.5% | 1648 | 1648 |
| el/floor | generated | 323.1 | 975.9 | 3.02x | 979.3 | 3.03x | +0.4% | 1104 | 1104 |
| el/floor | immediate | 323.1 | 478.9 | 1.48x | 468.4 | 1.45x | -2.2% | 1120 | 1120 |
| el/ladder | generated | 986.5 | 2194.7 | 2.22x | 2142.3 | 2.17x | -2.4% | 1776 | 1776 |
| el/ladder | immediate | 986.5 | 1175.6 | 1.19x | 1163.6 | 1.18x | -1.0% | 1784 | 1784 |
| el/nest7 | generated | 718.2 | 1969.8 | 2.74x | 1945.6 | 2.71x | -1.2% | 1152 | 1152 |
| el/nest7 | immediate | 718.2 | 1050.8 | 1.46x | 1045.9 | 1.46x | -0.5% | 1120 | 1120 |
| el/block | generated | 1033.3 | 2187.5 | 2.12x | 2146.9 | 2.08x | -1.9% | 2344 | 2344 |
| el/block | immediate | 1033.3 | 1170.2 | 1.13x | 1169.3 | 1.13x | -0.1% | 2440 | 2440 |
| el/loop | generated | 2219.3 | 5102.8 | 2.30x | 4516.1 | 2.03x | -11.5% | 4776 | 4776 |
| el/loop | immediate | 2219.3 | 2615.1 | 1.18x | 2584.3 | 1.16x | -1.2% | 5200 | 5200 |
| el/terms100 | generated | 11776.7 | 34053.4 | 2.89x | 21189.3 | 1.80x | -37.8% | 17904 | 17904 |
| el/terms100 | immediate | 11776.7 | 12806.3 | 1.09x | 12831.6 | 1.09x | +0.2% | 18720 | 18720 |
| el/terms1000 | generated | 113032.7 | 1342737.9 | 11.88x | 203615.6 | 1.80x | -84.8% | 169129 | 169129 |
| el/terms1000 | immediate | 113032.7 | 124418.4 | 1.10x | 122479.0 | 1.08x | -1.6% | 177120 | 177120 |
| el/overloads | generated | 1866.2 | 2830.9 | 1.52x | 2845.1 | 1.52x | +0.5% | 4248 | 4248 |
| el/overloads | immediate | 1866.2 | 2058.0 | 1.10x | 2052.5 | 1.10x | -0.3% | 4240 | 4240 |
| el/string | generated | 278.4 | 1046.1 | 3.76x | 1065.1 | 3.83x | +1.8% | 1056 | 1056 |
| el/string | immediate | 278.4 | 666.6 | 2.39x | 663.8 | 2.38x | -0.4% | 1032 | 1032 |
| el/interpolation | generated | 1908.3 | 4856.2 | 2.54x | 5030.2 | 2.64x | +3.6% | 2368 | 2368 |
| el/interpolation | immediate | 1908.3 | 3925.6 | 2.06x | 3989.6 | 2.09x | +1.6% | 2424 | 2424 |
| el/untyped | generated | 20327.9 | 20716.9 | 1.02x | 22843.6 | 1.12x | +10.3% | 16424 | 16424 |
| el/refused-early | generated | 474.6 | 1576.0 | 3.32x | 1542.9 | 3.25x | -2.1% | 1064 | 1064 |
| el/refused-early | immediate | 474.6 | 1242.2 | 2.62x | 1238.7 | 2.61x | -0.3% | 1944 | 1944 |
| el/refused-late | generated | 1547.0 | 2276.9 | 1.47x | 2268.5 | 1.47x | -0.4% | 1064 | 1064 |
| el/refused-late | immediate | 1547.0 | 3157.9 | 2.04x | 3141.7 | 2.03x | -0.5% | 3928 | 3928 |
| sql/conditions100 | generated | 70176.8 | 448858.6 | 6.40x | 451409.4 | 6.43x | +0.6% | 161736 | 161736 |
| sql/conditions1000 | generated | 709727.3 | 4533675.8 | 6.39x | 4576935.2 | 6.45x | +1.0% | 1616136 | 1616136 |
| tsql/comment | generated | 22126.7 | 3966.7 | 0.18x | 3990.5 | 0.18x | +0.6% | 1192 | 1192 |
| sql/column | generated | 185.0 | 1209.4 | 6.54x | 1197.4 | 6.47x | -1.0% | 392 | 392 |
| sql/arithmetic | generated | 2092.5 | 14116.5 | 6.75x | 14129.1 | 6.75x | +0.1% | 3472 | 3472 |
| sql/select20 | generated | 9482.0 | 82517.4 | 8.70x | 82492.3 | 8.70x | 0.0% | 21448 | 21448 |
| sql/refused-late | generated | 3616.0 | 48999.1 | 13.55x | 49515.4 | 13.69x | +1.1% | 13832 | 13832 |
