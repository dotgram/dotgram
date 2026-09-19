Median of 4 of 4 runs, each in a process of its own; control 30.7 ns (the runs' controls: 30.7, 30.7, 30.7, 30.7).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 07:03

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| el/floor | generated | 477.4 | 1230.1 | 2.58x | 1243.8 | 2.61x | +1.1% | 880 | 880 |
| el/floor | immediate | 477.4 | 668.8 | 1.40x | 675.6 | 1.42x | +1.0% | 832 | 832 |
| el/ladder | generated | 1418.2 | 3091.2 | 2.18x | 3130.4 | 2.21x | +1.3% | 1328 | 1328 |
| el/ladder | immediate | 1418.2 | 1873.7 | 1.32x | 1880.8 | 1.33x | +0.4% | 1208 | 1208 |
| el/nest7 | generated | 1172.4 | 2678.4 | 2.28x | 2705.6 | 2.31x | +1.0% | 928 | 928 |
| el/nest7 | immediate | 1172.4 | 1606.5 | 1.37x | 1613.5 | 1.38x | +0.4% | 832 | 832 |
| el/block | generated | 1462.3 | 2894.2 | 1.98x | 2896.7 | 1.98x | +0.1% | 1992 | 1992 |
| el/block | immediate | 1462.3 | 1742.3 | 1.19x | 1736.3 | 1.19x | -0.3% | 1896 | 1896 |
| el/loop | generated | 3145.6 | 6676.4 | 2.12x | 6700.1 | 2.13x | +0.4% | 4168 | 4168 |
| el/loop | immediate | 3145.6 | 3945.7 | 1.25x | 3994.3 | 1.27x | +1.2% | 4144 | 4144 |
| el/overloads | generated | 3045.4 | 4284.0 | 1.41x | 4294.0 | 1.41x | +0.2% | 4088 | 4088 |
| el/overloads | immediate | 3045.4 | 3331.2 | 1.09x | 3353.5 | 1.10x | +0.7% | 4016 | 4016 |
| el/string | generated | 385.4 | 1196.1 | 3.10x | 1203.5 | 3.12x | +0.6% | 1024 | 1024 |
| el/string | immediate | 385.4 | 773.7 | 2.01x | 769.1 | 2.00x | -0.6% | 1000 | 1000 |
| el/interpolation | generated | 1853.7 | 5029.0 | 2.71x | 5272.0 | 2.84x | +4.8% | 1952 | 1952 |
| el/interpolation | immediate | 1853.7 | 4093.5 | 2.21x | 4347.6 | 2.35x | +6.2% | 1880 | 1880 |
| el/untyped | generated | 20828.5 | 23793.5 | 1.14x | 23853.2 | 1.15x | +0.3% | 16232 | 16232 |
| el/refused-early | generated | 577.7 | 1843.6 | 3.19x | 1853.9 | 3.21x | +0.6% | 744 | 744 |
| el/refused-early | immediate | 577.7 | 1465.2 | 2.54x | 1481.1 | 2.56x | +1.1% | 1368 | 1368 |
| el/refused-late | generated | 1930.9 | 2527.2 | 1.31x | 2565.2 | 1.33x | +1.5% | 744 | 744 |
| el/refused-late | immediate | 1930.9 | 4021.8 | 2.08x | 4060.7 | 2.10x | +1.0% | 3352 | 3352 |
| sql/literal | generated | 91.0 | 272.0 | 2.99x | 211.1 | 2.32x | -22.4% | 160 | 160 |
| sql/comment | generated | 3073.1 | 24943.4 | 8.12x | 19128.8 | 6.22x | -23.3% | 5136 | 5136 |
| sql/conditions100 | generated | 56585.1 | 2646157.0 | 46.76x | 445076.6 | 7.87x | -83.2% | 20958401 | 161761 |
| sql/conditions1000 | generated | 571398.2 | 32035637.5 | 56.07x | 4476536.7 | 7.83x | -86.0% | 167802341 | 1616205 |
| tsql/comment | generated | 44550.1 | 4612.9 | 0.10x | 4642.3 | 0.10x | +0.6% | 1192 | 1192 |
| sql/column | generated | 169.8 | 1411.2 | 8.31x | 1218.1 | 7.17x | -13.7% | 392 | 392 |
| sql/arithmetic | generated | 1881.6 | 17360.2 | 9.23x | 14284.7 | 7.59x | -17.7% | 3472 | 3472 |
| sql/nest8 | generated | 3259.2 | 46167.4 | 14.17x | 38671.5 | 11.87x | -16.2% | 6504 | 6504 |
| sql/condition | generated | 2079.4 | 18334.1 | 8.82x | 14839.4 | 7.14x | -19.1% | 4928 | 4928 |
| sql/select1 | generated | 863.7 | 7356.0 | 8.52x | 6116.4 | 7.08x | -16.9% | 1688 | 1688 |
| sql/select20 | generated | 8141.2 | 106285.1 | 13.06x | 80730.3 | 9.92x | -24.0% | 21448 | 21448 |
| sql/values | generated | 645.4 | 9127.9 | 14.14x | 6921.4 | 10.72x | -24.2% | 1904 | 1904 |
| sql/create | generated | 932.0 | 5929.9 | 6.36x | 4732.8 | 5.08x | -20.2% | 1568 | 1568 |
| sql/refused-late | generated | 3132.5 | 63687.6 | 20.33x | 48472.6 | 15.47x | -23.9% | 13832 | 13832 |
