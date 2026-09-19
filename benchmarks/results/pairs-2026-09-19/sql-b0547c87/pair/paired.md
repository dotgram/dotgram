Median of 5 of 5 runs, each in a process of its own; control 30.9 ns (the runs' controls: 31.4, 30.9, 30.9, 30.9, 30.9).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 16:14

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.9 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 75.4 | 81.0 | 1.07x | 82.1 | 1.09x | +1.4% | 192 | 192 |
| tsql/columns1000 | generated | 1939631.2 | 514500.0 | 0.27x | 456418.8 | 0.24x | -11.3% | 200464 | 200464 |
| tsql/conditions1000 | generated | 876696.1 | 1066725.0 | 1.22x | 937114.1 | 1.07x | -12.2% | 424424 | 424424 |
| tsql/rows1000 | generated | 589406.2 | 819923.4 | 1.39x | 727335.2 | 1.23x | -11.3% | 296497 | 296497 |
| web/json.object10000 | generated | 808262.5 | 1303676.6 | 1.61x | 1206052.3 | 1.49x | -7.5% | 1599240 | 1599240 |
| web/url.full | generated | 153.7 | 281.4 | 1.83x | 287.2 | 1.87x | +2.1% | 536 | 536 |
| web/json.object | generated | 583.1 | 940.6 | 1.61x | 934.5 | 1.60x | -0.7% | 2584 | 2584 |
| el/string | generated | 271.8 | 1036.5 | 3.81x | 1064.9 | 3.92x | +2.7% | 1056 | 1056 |
| el/string | immediate | 271.8 | 657.9 | 2.42x | 651.2 | 2.40x | -1.0% | 1032 | 1032 |
| sql/literal | generated | 50.9 | 131.4 | 2.58x | 133.9 | 2.63x | +1.9% | 160 | 160 |
| sql/comment | generated | 2118.8 | 17328.8 | 8.18x | 16583.7 | 7.83x | -4.3% | 5136 | 5136 |
| sql/conditions100 | generated | 49049.5 | 410377.6 | 8.37x | 391395.4 | 7.98x | -4.6% | 161736 | 161736 |
| sql/conditions1000 | generated | 494439.1 | 4102543.8 | 8.30x | 3929422.7 | 7.95x | -4.2% | 1616203 | 1616203 |
| tsql/comment | generated | 19286.7 | 3991.7 | 0.21x | 3621.4 | 0.19x | -9.3% | 1192 | 1192 |
| sql/column | generated | 135.5 | 1147.0 | 8.46x | 1139.0 | 8.40x | -0.7% | 392 | 392 |
| sql/arithmetic | generated | 1603.4 | 13177.6 | 8.22x | 12597.4 | 7.86x | -4.4% | 3472 | 3472 |
| sql/nest8 | generated | 2721.2 | 33387.9 | 12.27x | 33229.9 | 12.21x | -0.5% | 6504 | 6504 |
| sql/condition | generated | 1783.1 | 13773.2 | 7.72x | 13188.5 | 7.40x | -4.2% | 4928 | 4928 |
| sql/select1 | generated | 655.8 | 5652.3 | 8.62x | 5449.1 | 8.31x | -3.6% | 1688 | 1688 |
| sql/select20 | generated | 7028.8 | 73647.0 | 10.48x | 69931.1 | 9.95x | -5.0% | 21448 | 21448 |
| sql/values | generated | 453.6 | 6118.7 | 13.49x | 5992.7 | 13.21x | -2.1% | 1904 | 1904 |
| sql/create | generated | 776.1 | 4487.2 | 5.78x | 4485.6 | 5.78x | 0.0% | 1568 | 1568 |
| sql/refused-late | generated | 2661.6 | 44445.7 | 16.70x | 41674.6 | 15.66x | -6.2% | 13552 | 13552 |
