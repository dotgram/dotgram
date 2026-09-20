# Paired stand, 2026-09-19 21:21

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | 6666.6 | 17738.2 | 2.66x | 17834.4 | 2.68x | +0.5% | 21416 | 21416 |
| sql/select20.window | generated | 6960.9 | 18281.6 | 2.63x | 18308.3 | 2.63x | +0.1% | 21416 | 21416 |
| tsql/insert-values.at | generated | 7839.2 | 1394.4 | 0.18x | 1349.2 | 0.17x | -3.2% | 1328 | 1328 |
| sql/select20.scan | generated | 369.4 | 370.3 | 1.00x | 370.0 | 1.00x | -0.1% | 0 | 0 |
| sql/conditions100.scan | generated | 3461.7 | 3418.9 | 0.99x | 3419.9 | 0.99x | 0.0% | 0 | 0 |
| tsql/select20.scan | generated | 373.6 | 372.5 | 1.00x | 372.2 | 1.00x | -0.1% | 0 | 0 |
| el/ladder.scan | generated | 136.5 | 137.0 | 1.00x | 135.7 | 0.99x | -0.9% | 0 | 0 |
| fixmsg/Order.parse-stream | generated | 1800.0 | 2110.6 | 1.17x | 2112.7 | 1.17x | +0.1% | 8224 | 8224 |
| fixmsg/Order.parse-reader | generated | 1824.8 | 2070.8 | 1.13x | 2072.1 | 1.14x | +0.1% | 12112 | 12112 |
| fixmsg/Order.parse-span | generated | 1775.9 | 1745.7 | 0.98x | 1739.6 | 0.98x | -0.4% | 3672 | 3672 |
| fixmsg/Order.read-stream100 | generated | 190274.0 | 193279.1 | 1.02x | 196632.6 | 1.03x | +1.7% | 389248 | 389248 |
| fixmsg/Order.read-reader100 | generated | 187773.2 | 184068.0 | 0.98x | 181710.2 | 0.97x | -1.3% | 375712 | 375712 |
| fix/Order.span | generated | 657.5 | 487.6 | 0.74x | 485.8 | 0.74x | -0.4% | 1008 | 1008 |
| feeds/streaming.1000 | generated | 202172.1 | 203445.7 | 1.01x | 206630.9 | 1.02x | +1.6% | 240760 | 240760 |
| fix/One.yield-string | generated | 82.3 | 237.6 | 2.89x | 252.1 | 3.06x | +6.1% | 272 | 272 |
| fix/One.yield-memory | generated | 81.7 | 122.0 | 1.49x | 121.4 | 1.49x | -0.5% | 352 | 352 |
| fix/Order.yield-string | generated | 663.2 | 2237.3 | 3.37x | 2292.0 | 3.46x | +2.4% | 1096 | 1096 |
| fix/Order.yield-memory | generated | 652.3 | 771.4 | 1.18x | 771.3 | 1.18x | 0.0% | 1176 | 1176 |
| fix/BinaryMany.yield-string | generated | 3466.2 | 14779.0 | 4.26x | 15906.2 | 4.59x | +7.6% | 6832 | 6832 |
| fix/BinaryMany.yield-memory | generated | 3464.6 | 4776.6 | 1.38x | 4746.2 | 1.37x | -0.6% | 6912 | 6912 |
| fix/Orders128.yield-string | generated | 78606.2 | 275970.7 | 3.51x | 281049.0 | 3.58x | +1.8% | 117936 | 117936 |
| fix/Orders128.yield-memory | generated | 78254.6 | 90514.8 | 1.16x | 90514.7 | 1.16x | 0.0% | 118016 | 118016 |
| fix/slope-0.yield-string | generated | 25.4 | 43.2 | 1.70x | 49.1 | 1.93x | +13.6% | 176 | 176 |
| fix/slope-0.yield-memory | generated | 25.2 | 59.4 | 2.36x | 59.4 | 2.36x | 0.0% | 256 | 256 |
| fix/slope-16.yield-string | generated | 893.6 | 3037.8 | 3.40x | 3125.2 | 3.50x | +2.9% | 1712 | 1712 |
| fix/slope-16.yield-memory | generated | 899.8 | 1059.5 | 1.18x | 1042.5 | 1.16x | -1.6% | 1792 | 1792 |
