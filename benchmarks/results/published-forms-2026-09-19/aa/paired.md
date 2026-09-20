Median of 5 of 5 runs, each in a process of its own; control 31.1 ns (the runs' controls: 30.8, 30.5, 31.1, 31.2, 31.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 21:26

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | 6785.6 | 17738.2 | 2.61x | 17830.2 | 2.63x | +0.5% | 21416 | 21416 |
| sql/select20.window | generated | 6960.9 | 18281.6 | 2.63x | 18316.4 | 2.63x | +0.2% | 21416 | 21416 |
| tsql/insert-values.at | generated | 8256.8 | 1396.4 | 0.17x | 1419.3 | 0.17x | +1.6% | 1328 | 1328 |
| sql/select20.scan | generated | 371.2 | 377.6 | 1.02x | 377.0 | 1.02x | -0.2% | 0 | 0 |
| sql/conditions100.scan | generated | 3491.8 | 3518.3 | 1.01x | 3477.7 | 1.00x | -1.2% | 0 | 0 |
| tsql/select20.scan | generated | 373.6 | 378.9 | 1.01x | 378.1 | 1.01x | -0.2% | 0 | 0 |
| el/ladder.scan | generated | 138.2 | 137.0 | 0.99x | 137.5 | 0.99x | +0.4% | 0 | 0 |
| fixmsg/Order.parse-stream | generated | 1782.1 | 2129.2 | 1.19x | 2112.7 | 1.19x | -0.8% | 8224 | 8224 |
| fixmsg/Order.parse-reader | generated | 1837.2 | 2083.2 | 1.13x | 2072.1 | 1.13x | -0.5% | 12112 | 12112 |
| fixmsg/Order.parse-span | generated | 1818.4 | 1805.0 | 0.99x | 1776.7 | 0.98x | -1.6% | 3672 | 3672 |
| fixmsg/Order.read-stream100 | generated | 194971.5 | 203625.4 | 1.04x | 196864.3 | 1.01x | -3.3% | 389248 | 389248 |
| fixmsg/Order.read-reader100 | generated | 192831.4 | 189366.8 | 0.98x | 186546.7 | 0.97x | -1.5% | 375712 | 375712 |
| fix/Order.span | generated | 654.3 | 496.8 | 0.76x | 497.1 | 0.76x | +0.1% | 1008 | 1008 |
| feeds/streaming.1000 | generated | 208853.1 | 218079.3 | 1.04x | 209132.0 | 1.00x | -4.1% | 240760 | 240760 |
| fix/One.yield-string | generated | 82.3 | 237.6 | 2.89x | 248.5 | 3.02x | +4.6% | 272 | 272 |
| fix/One.yield-memory | generated | 83.3 | 126.9 | 1.52x | 124.8 | 1.50x | -1.6% | 352 | 352 |
| fix/Order.yield-string | generated | 657.5 | 2248.7 | 3.42x | 2269.9 | 3.45x | +0.9% | 1096 | 1096 |
| fix/Order.yield-memory | generated | 657.6 | 787.7 | 1.20x | 790.2 | 1.20x | +0.3% | 1176 | 1176 |
| fix/BinaryMany.yield-string | generated | 3536.9 | 15472.5 | 4.37x | 16097.9 | 4.55x | +4.0% | 6832 | 6832 |
| fix/BinaryMany.yield-memory | generated | 3526.7 | 4838.6 | 1.37x | 4861.0 | 1.38x | +0.5% | 6912 | 6912 |
| fix/Orders128.yield-string | generated | 80311.3 | 283451.9 | 3.53x | 281049.0 | 3.50x | -0.8% | 117936 | 117936 |
| fix/Orders128.yield-memory | generated | 79783.2 | 93114.9 | 1.17x | 93714.6 | 1.17x | +0.6% | 118016 | 118016 |
| fix/slope-0.yield-string | generated | 26.2 | 44.4 | 1.69x | 50.4 | 1.92x | +13.6% | 176 | 176 |
| fix/slope-0.yield-memory | generated | 26.3 | 61.1 | 2.32x | 61.4 | 2.33x | +0.4% | 256 | 256 |
| fix/slope-16.yield-string | generated | 903.0 | 3037.8 | 3.36x | 3125.2 | 3.46x | +2.9% | 1712 | 1712 |
| fix/slope-16.yield-memory | generated | 905.5 | 1090.1 | 1.20x | 1085.0 | 1.20x | -0.5% | 1792 | 1792 |
