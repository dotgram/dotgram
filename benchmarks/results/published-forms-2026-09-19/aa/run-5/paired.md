# Paired stand, 2026-09-19 21:26

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | 6785.6 | 17763.8 | 2.62x | 17682.7 | 2.61x | -0.5% | 21416 | 21416 |
| sql/select20.window | generated | 7243.2 | 19183.9 | 2.65x | 18753.6 | 2.59x | -2.2% | 21416 | 21416 |
| tsql/insert-values.at | generated | 8472.6 | 1393.6 | 0.16x | 1387.3 | 0.16x | -0.5% | 1328 | 1328 |
| sql/select20.scan | generated | 371.2 | 388.6 | 1.05x | 377.0 | 1.02x | -3.0% | 0 | 0 |
| sql/conditions100.scan | generated | 3491.8 | 3518.3 | 1.01x | 3496.9 | 1.00x | -0.6% | 0 | 0 |
| tsql/select20.scan | generated | 375.6 | 386.6 | 1.03x | 382.4 | 1.02x | -1.1% | 0 | 0 |
| el/ladder.scan | generated | 138.2 | 139.5 | 1.01x | 137.9 | 1.00x | -1.1% | 0 | 0 |
| fixmsg/Order.parse-stream | generated | 1782.1 | 2129.2 | 1.19x | 2182.2 | 1.22x | +2.5% | 8224 | 8224 |
| fixmsg/Order.parse-reader | generated | 1837.2 | 2083.2 | 1.13x | 2038.8 | 1.11x | -2.1% | 12112 | 12112 |
| fixmsg/Order.parse-span | generated | 1833.1 | 1805.0 | 0.98x | 1774.4 | 0.97x | -1.7% | 3672 | 3672 |
| fixmsg/Order.read-stream100 | generated | 194971.5 | 204872.5 | 1.05x | 204999.8 | 1.05x | +0.1% | 389248 | 389248 |
| fixmsg/Order.read-reader100 | generated | 192831.4 | 188305.9 | 0.98x | 186546.7 | 0.97x | -0.9% | 375712 | 375712 |
| fix/Order.span | generated | 645.4 | 496.6 | 0.77x | 495.1 | 0.77x | -0.3% | 1008 | 1008 |
| feeds/streaming.1000 | generated | 219371.3 | 218079.3 | 0.99x | 209132.0 | 0.95x | -4.1% | 240760 | 240760 |
| fix/One.yield-string | generated | 82.1 | 229.9 | 2.80x | 248.5 | 3.03x | +8.1% | 272 | 272 |
| fix/One.yield-memory | generated | 82.8 | 127.1 | 1.54x | 127.9 | 1.55x | +0.6% | 352 | 352 |
| fix/Order.yield-string | generated | 642.9 | 2248.7 | 3.50x | 2257.0 | 3.51x | +0.4% | 1096 | 1096 |
| fix/Order.yield-memory | generated | 650.9 | 787.1 | 1.21x | 792.7 | 1.22x | +0.7% | 1176 | 1176 |
| fix/BinaryMany.yield-string | generated | 3521.0 | 15472.5 | 4.39x | 16351.8 | 4.64x | +5.7% | 6832 | 6832 |
| fix/BinaryMany.yield-memory | generated | 3567.0 | 4838.6 | 1.36x | 4861.0 | 1.36x | +0.5% | 6912 | 6912 |
| fix/Orders128.yield-string | generated | 81102.4 | 283451.9 | 3.49x | 284901.0 | 3.51x | +0.5% | 117936 | 117936 |
| fix/Orders128.yield-memory | generated | 80220.5 | 95455.0 | 1.19x | 96576.0 | 1.20x | +1.2% | 118016 | 118016 |
| fix/slope-0.yield-string | generated | 25.9 | 44.2 | 1.71x | 50.3 | 1.95x | +13.7% | 176 | 176 |
| fix/slope-0.yield-memory | generated | 26.3 | 61.0 | 2.32x | 60.5 | 2.30x | -0.9% | 256 | 256 |
| fix/slope-16.yield-string | generated | 896.6 | 3022.1 | 3.37x | 3164.9 | 3.53x | +4.7% | 1712 | 1712 |
| fix/slope-16.yield-memory | generated | 921.3 | 1090.1 | 1.18x | 1104.7 | 1.20x | +1.3% | 1792 | 1792 |
