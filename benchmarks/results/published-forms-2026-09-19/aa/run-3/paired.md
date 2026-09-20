# Paired stand, 2026-09-19 21:22

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | 7263.4 | 17335.3 | 2.39x | 17405.8 | 2.40x | +0.4% | 21416 | 21416 |
| sql/select20.window | generated | 6881.6 | 17883.3 | 2.60x | 18316.4 | 2.66x | +2.4% | 21416 | 21416 |
| tsql/insert-values.at | generated | 8421.6 | 1421.8 | 0.17x | 1500.1 | 0.18x | +5.5% | 1328 | 1328 |
| sql/select20.scan | generated | 374.0 | 378.5 | 1.01x | 384.9 | 1.03x | +1.7% | 0 | 0 |
| sql/conditions100.scan | generated | 3542.9 | 3583.1 | 1.01x | 3772.1 | 1.06x | +5.3% | 0 | 0 |
| tsql/select20.scan | generated | 385.9 | 383.2 | 0.99x | 380.6 | 0.99x | -0.7% | 0 | 0 |
| el/ladder.scan | generated | 140.0 | 136.6 | 0.98x | 137.5 | 0.98x | +0.6% | 0 | 0 |
| fixmsg/Order.parse-stream | generated | 1950.1 | 2212.0 | 1.13x | 2156.8 | 1.11x | -2.5% | 8224 | 8224 |
| fixmsg/Order.parse-reader | generated | 1853.0 | 2082.8 | 1.12x | 2153.2 | 1.16x | +3.4% | 12112 | 12112 |
| fixmsg/Order.parse-span | generated | 1898.8 | 1823.5 | 0.96x | 1853.0 | 0.98x | +1.6% | 3672 | 3672 |
| fixmsg/Order.read-stream100 | generated | 197813.3 | 204372.9 | 1.03x | 203689.1 | 1.03x | -0.3% | 389248 | 389248 |
| fixmsg/Order.read-reader100 | generated | 196040.8 | 193813.3 | 0.99x | 194146.3 | 0.99x | +0.2% | 375712 | 375712 |
| fix/Order.span | generated | 672.5 | 517.1 | 0.77x | 509.4 | 0.76x | -1.5% | 1008 | 1008 |
| feeds/streaming.1000 | generated | 211618.2 | 221041.0 | 1.04x | 219064.3 | 1.04x | -0.9% | 240760 | 240760 |
| fix/One.yield-string | generated | 87.4 | 235.6 | 2.70x | 264.4 | 3.03x | +12.2% | 272 | 272 |
| fix/One.yield-memory | generated | 87.5 | 130.4 | 1.49x | 129.7 | 1.48x | -0.5% | 352 | 352 |
| fix/Order.yield-string | generated | 698.1 | 2218.7 | 3.18x | 2445.3 | 3.50x | +10.2% | 1096 | 1096 |
| fix/Order.yield-memory | generated | 718.9 | 833.2 | 1.16x | 845.0 | 1.18x | +1.4% | 1176 | 1176 |
| fix/BinaryMany.yield-string | generated | 3597.5 | 15443.0 | 4.29x | 16013.7 | 4.45x | +3.7% | 6832 | 6832 |
| fix/BinaryMany.yield-memory | generated | 3567.7 | 4945.9 | 1.39x | 4915.5 | 1.38x | -0.6% | 6912 | 6912 |
| fix/Orders128.yield-string | generated | 83092.9 | 266632.6 | 3.21x | 293221.0 | 3.53x | +10.0% | 117936 | 117936 |
| fix/Orders128.yield-memory | generated | 81777.1 | 93786.7 | 1.15x | 92519.4 | 1.13x | -1.4% | 118016 | 118016 |
| fix/slope-0.yield-string | generated | 26.6 | 44.8 | 1.68x | 51.0 | 1.92x | +13.9% | 176 | 176 |
| fix/slope-0.yield-memory | generated | 26.4 | 63.8 | 2.41x | 62.3 | 2.36x | -2.3% | 256 | 256 |
| fix/slope-16.yield-string | generated | 949.2 | 2980.0 | 3.14x | 3310.9 | 3.49x | +11.1% | 1712 | 1712 |
| fix/slope-16.yield-memory | generated | 940.2 | 1100.1 | 1.17x | 1085.9 | 1.15x | -1.3% | 1792 | 1792 |
