# Paired stand, 2026-09-19 21:20

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.8 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | 6664.5 | 17600.8 | 2.64x | 19803.6 | 2.97x | +12.5% | 21416 | 21416 |
| sql/select20.window | generated | 6765.0 | 17853.6 | 2.64x | 18059.9 | 2.67x | +1.2% | 21416 | 21416 |
| tsql/insert-values.at | generated | 8256.8 | 1396.4 | 0.17x | 1419.3 | 0.17x | +1.6% | 1328 | 1328 |
| sql/select20.scan | generated | 369.3 | 377.6 | 1.02x | 382.0 | 1.03x | +1.2% | 0 | 0 |
| sql/conditions100.scan | generated | 3460.5 | 3558.4 | 1.03x | 3477.7 | 1.00x | -2.3% | 0 | 0 |
| tsql/select20.scan | generated | 373.4 | 378.9 | 1.01x | 378.1 | 1.01x | -0.2% | 0 | 0 |
| el/ladder.scan | generated | 136.8 | 136.3 | 1.00x | 141.5 | 1.03x | +3.8% | 0 | 0 |
| fixmsg/Order.parse-stream | generated | 1707.6 | 2131.6 | 1.25x | 2093.2 | 1.23x | -1.8% | 8224 | 8224 |
| fixmsg/Order.parse-reader | generated | 1743.5 | 2086.2 | 1.20x | 2035.1 | 1.17x | -2.4% | 12112 | 12112 |
| fixmsg/Order.parse-span | generated | 1742.3 | 1802.9 | 1.03x | 1776.7 | 1.02x | -1.5% | 3672 | 3672 |
| fixmsg/Order.read-stream100 | generated | 184254.5 | 199022.7 | 1.08x | 195075.0 | 1.06x | -2.0% | 389248 | 389248 |
| fixmsg/Order.read-reader100 | generated | 183294.7 | 189366.8 | 1.03x | 185947.5 | 1.01x | -1.8% | 375712 | 375712 |
| fix/Order.span | generated | 649.0 | 496.8 | 0.77x | 497.1 | 0.77x | +0.1% | 1008 | 1008 |
| feeds/streaming.1000 | generated | 208327.1 | 221030.7 | 1.06x | 206507.2 | 0.99x | -6.6% | 240760 | 240760 |
| fix/One.yield-string | generated | 82.6 | 244.1 | 2.96x | 241.6 | 2.93x | -1.0% | 272 | 272 |
| fix/One.yield-memory | generated | 83.3 | 125.3 | 1.51x | 124.8 | 1.50x | -0.4% | 352 | 352 |
| fix/Order.yield-string | generated | 657.5 | 2390.1 | 3.63x | 2269.9 | 3.45x | -5.0% | 1096 | 1096 |
| fix/Order.yield-memory | generated | 657.6 | 787.7 | 1.20x | 785.9 | 1.20x | -0.2% | 1176 | 1176 |
| fix/BinaryMany.yield-string | generated | 3536.9 | 15633.9 | 4.42x | 16341.3 | 4.62x | +4.5% | 6832 | 6832 |
| fix/BinaryMany.yield-memory | generated | 3522.6 | 4823.5 | 1.37x | 4841.8 | 1.37x | +0.4% | 6912 | 6912 |
| fix/Orders128.yield-string | generated | 77737.3 | 294256.7 | 3.79x | 279109.3 | 3.59x | -5.1% | 117936 | 117936 |
| fix/Orders128.yield-memory | generated | 77283.8 | 92367.7 | 1.20x | 93714.6 | 1.21x | +1.5% | 118016 | 118016 |
| fix/slope-0.yield-string | generated | 26.2 | 44.4 | 1.69x | 50.9 | 1.94x | +14.7% | 176 | 176 |
| fix/slope-0.yield-memory | generated | 26.4 | 61.1 | 2.32x | 61.4 | 2.33x | +0.4% | 256 | 256 |
| fix/slope-16.yield-string | generated | 903.0 | 3298.1 | 3.65x | 3111.1 | 3.45x | -5.7% | 1712 | 1712 |
| fix/slope-16.yield-memory | generated | 897.7 | 1052.3 | 1.17x | 1068.9 | 1.19x | +1.6% | 1792 | 1792 |
