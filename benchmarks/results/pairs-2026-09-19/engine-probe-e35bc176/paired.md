Median of 5 of 5 runs, each in a process of its own; control 30.9 ns (the runs' controls: 30.9, 30.8, 31.0, 31.0, 30.9).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 06:25

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.9 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.stream | generated | 137.4 | 511.5 | 3.72x | 534.6 | 3.89x | +4.5% | 824 | 824 |
| fix/Order.stream | generated | 745.7 | 2707.5 | 3.63x | 2642.8 | 3.54x | -2.4% | 1648 | 1648 |
| fix/BinaryMany.stream | generated | 4421.3 | 18592.6 | 4.21x | 19106.5 | 4.32x | +2.8% | 7384 | 7384 |
| fix/Orders128.stream | generated | 109963.3 | 290549.4 | 2.64x | 284540.7 | 2.59x | -2.1% | 118488 | 118488 |
| fix/OrderMalformed.stream | generated | 763.7 | 2749.3 | 3.60x | 2742.3 | 3.59x | -0.3% | 1800 | 1800 |
| el/string | generated | 285.4 | 1032.1 | 3.62x | 1024.5 | 3.59x | -0.7% | 1056 | 1056 |
| el/string | immediate | 285.4 | 646.0 | 2.26x | 644.0 | 2.26x | -0.3% | 1032 | 1032 |
| sql/literal | generated | 57.0 | 208.1 | 3.65x | 199.1 | 3.50x | -4.3% | 160 | 160 |
| sql/comment | generated | 2123.5 | 23557.9 | 11.09x | 23634.2 | 11.13x | +0.3% | 5136 | 5136 |
| sql/conditions100 | generated | 48635.4 | 3420624.2 | 70.33x | 3431885.2 | 70.56x | +0.3% | 20959077 | 20958925 |
| sql/conditions1000 | generated | 492421.5 | 32523656.2 | 66.05x | 32858200.0 | 66.73x | +1.0% | 167803884 | 167803866 |
| tsql/comment | generated | 19990.0 | 3900.6 | 0.20x | 3917.4 | 0.20x | +0.4% | 1192 | 1192 |
| sql/column | generated | 139.3 | 1339.1 | 9.61x | 1353.5 | 9.71x | +1.1% | 392 | 392 |
| sql/arithmetic | generated | 1619.4 | 16682.4 | 10.30x | 16933.7 | 10.46x | +1.5% | 3472 | 3472 |
| sql/nest8 | generated | 2702.0 | 44227.4 | 16.37x | 44736.5 | 16.56x | +1.2% | 6504 | 6504 |
| sql/condition | generated | 1760.4 | 17590.9 | 9.99x | 17624.8 | 10.01x | +0.2% | 4928 | 4928 |
| sql/select1 | generated | 665.6 | 7073.8 | 10.63x | 7040.3 | 10.58x | -0.5% | 1688 | 1688 |
| sql/select20 | generated | 6860.6 | 104094.8 | 15.17x | 103459.5 | 15.08x | -0.6% | 21448 | 21448 |
| sql/values | generated | 460.3 | 8801.4 | 19.12x | 8809.7 | 19.14x | +0.1% | 1904 | 1904 |
| sql/create | generated | 783.3 | 5991.2 | 7.65x | 5953.4 | 7.60x | -0.6% | 1568 | 1568 |
| sql/refused-late | generated | 2613.3 | 61281.6 | 23.45x | 62276.3 | 23.83x | +1.6% | 13832 | 13832 |
