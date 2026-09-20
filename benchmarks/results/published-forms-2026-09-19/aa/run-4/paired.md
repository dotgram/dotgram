# Paired stand, 2026-09-19 21:24

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | 7275.5 | 18386.9 | 2.53x | 17830.2 | 2.45x | -3.0% | 21416 | 21416 |
| sql/select20.window | generated | 7009.0 | 19254.0 | 2.75x | 18550.4 | 2.65x | -3.7% | 21416 | 21416 |
| tsql/insert-values.at | generated | 8166.8 | 1418.2 | 0.17x | 1478.1 | 0.18x | +4.2% | 1353 | 1353 |
| sql/select20.scan | generated | 373.8 | 367.5 | 0.98x | 371.2 | 0.99x | +1.0% | 0 | 0 |
| sql/conditions100.scan | generated | 3495.3 | 3501.8 | 1.00x | 3459.4 | 0.99x | -1.2% | 0 | 0 |
| tsql/select20.scan | generated | 373.3 | 373.8 | 1.00x | 373.8 | 1.00x | 0.0% | 0 | 0 |
| el/ladder.scan | generated | 145.0 | 137.4 | 0.95x | 136.3 | 0.94x | -0.8% | 0 | 0 |
| fixmsg/Order.parse-stream | generated | 1758.9 | 2106.0 | 1.20x | 2098.6 | 1.19x | -0.4% | 8224 | 8224 |
| fixmsg/Order.parse-reader | generated | 1842.0 | 2191.9 | 1.19x | 2187.2 | 1.19x | -0.2% | 12112 | 12112 |
| fixmsg/Order.parse-span | generated | 1818.4 | 1865.4 | 1.03x | 1835.7 | 1.01x | -1.6% | 3672 | 3672 |
| fixmsg/Order.read-stream100 | generated | 195344.3 | 203625.4 | 1.04x | 196864.3 | 1.01x | -3.3% | 389248 | 389248 |
| fixmsg/Order.read-reader100 | generated | 195853.1 | 197667.2 | 1.01x | 192870.3 | 0.98x | -2.4% | 375712 | 375712 |
| fix/Order.span | generated | 654.3 | 513.2 | 0.78x | 515.8 | 0.79x | +0.5% | 1008 | 1008 |
| feeds/streaming.1000 | generated | 208853.1 | 206773.4 | 0.99x | 213211.1 | 1.02x | +3.1% | 240760 | 240760 |
| fix/One.yield-string | generated | 81.3 | 250.4 | 3.08x | 238.1 | 2.93x | -4.9% | 272 | 272 |
| fix/One.yield-memory | generated | 83.5 | 126.9 | 1.52x | 124.6 | 1.49x | -1.8% | 352 | 352 |
| fix/Order.yield-string | generated | 655.0 | 2356.7 | 3.60x | 2212.5 | 3.38x | -6.1% | 1096 | 1096 |
| fix/Order.yield-memory | generated | 657.9 | 796.8 | 1.21x | 790.2 | 1.20x | -0.8% | 1176 | 1176 |
| fix/BinaryMany.yield-string | generated | 3633.7 | 17068.0 | 4.70x | 16097.9 | 4.43x | -5.7% | 6832 | 6832 |
| fix/BinaryMany.yield-memory | generated | 3526.7 | 4965.1 | 1.41x | 4869.0 | 1.38x | -1.9% | 6912 | 6912 |
| fix/Orders128.yield-string | generated | 80311.3 | 298640.2 | 3.72x | 275774.8 | 3.43x | -7.7% | 117936 | 117936 |
| fix/Orders128.yield-memory | generated | 79783.2 | 93114.9 | 1.17x | 94298.2 | 1.18x | +1.3% | 118016 | 118016 |
| fix/slope-0.yield-string | generated | 26.3 | 44.4 | 1.69x | 50.4 | 1.92x | +13.6% | 176 | 176 |
| fix/slope-0.yield-memory | generated | 25.8 | 61.8 | 2.39x | 61.5 | 2.38x | -0.5% | 256 | 256 |
| fix/slope-16.yield-string | generated | 904.5 | 3282.0 | 3.63x | 3024.7 | 3.34x | -7.8% | 1712 | 1712 |
| fix/slope-16.yield-memory | generated | 905.5 | 1093.5 | 1.21x | 1085.0 | 1.20x | -0.8% | 1792 | 1792 |
