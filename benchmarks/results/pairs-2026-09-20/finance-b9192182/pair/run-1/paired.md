# Paired stand, 2026-09-20 03:16

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 74.1 | 79.5 | 1.07x | 77.8 | 1.05x | -2.2% | 192 | 192 |
| fix/One.bytes | generated | hand | 75.3 | 94.9 | 1.26x | 95.9 | 1.27x | +1.0% | 248 | 248 |
| fix/One.stream | generated | hand | 150.4 | 328.5 | 2.18x | 334.5 | 2.22x | +1.8% | 888 | 888 |
| fix/Order.text | generated | hand | 687.6 | 673.5 | 0.98x | 642.0 | 0.93x | -4.7% | 1096 | 1096 |
| fix/Order.bytes | generated | hand | 703.0 | 755.8 | 1.08x | 740.6 | 1.05x | -2.0% | 1152 | 1152 |
| fix/Order.stream | generated | hand | 847.7 | 1086.3 | 1.28x | 1067.0 | 1.26x | -1.8% | 1712 | 1712 |
| fix/slope-16.text | generated | hand | 916.0 | 811.9 | 0.89x | 761.1 | 0.83x | -6.3% | 1752 | 1752 |
| fix/Order.span | generated | hand | 692.0 | 547.3 | 0.79x | 513.4 | 0.74x | -6.2% | 1008 | 1008 |
| fix/One.log-text | generated | hand | 82.2 | 84.3 | 1.03x | 86.3 | 1.05x | +2.3% | 192 | 192 |
| fix/One.log-bytes | generated | hand | 83.2 | 112.9 | 1.36x | 105.3 | 1.27x | -6.7% | 248 | 248 |
| fix/One.log-stream | generated | hand | 84.4 | 359.8 | 4.26x | 365.7 | 4.33x | +1.6% | 888 | 888 |
| fix/Order.log-text | generated | hand | 765.4 | 685.8 | 0.90x | 652.5 | 0.85x | -4.9% | 1096 | 1096 |
| fix/Order.log-bytes | generated | hand | 728.6 | 874.2 | 1.20x | 828.2 | 1.14x | -5.3% | 1152 | 1152 |
| fix/Order.log-stream | generated | hand | 768.4 | 1245.8 | 1.62x | 1196.3 | 1.56x | -4.0% | 1712 | 1712 |
| fix/One.yield-reader | generated | hand | 211.3 | 364.4 | 1.72x | 347.7 | 1.65x | -4.6% | 856 | 856 |
| fix/One.yield-string | generated | hand | 78.2 | 229.0 | 2.93x | 221.3 | 2.83x | -3.4% | 272 | 272 |
| fix/One.yield-memory | generated | hand | 80.2 | 127.3 | 1.59x | 120.7 | 1.50x | -5.2% | 352 | 352 |
| fix/One.whole-stream | generated | hand | 159.1 | 222.3 | 1.40x | 224.1 | 1.41x | +0.8% | 448 | 448 |
| fix/One.whole-reader | generated | hand | 155.4 | 228.8 | 1.47x | 236.0 | 1.52x | +3.2% | 416 | 416 |
| fix/Order.yield-reader | generated | hand | 902.1 | 1157.3 | 1.28x | 1080.9 | 1.20x | -6.6% | 1680 | 1680 |
| fix/Order.yield-string | generated | hand | 679.6 | 2158.1 | 3.18x | 2095.7 | 3.08x | -2.9% | 1096 | 1096 |
| fix/Order.yield-memory | generated | hand | 691.4 | 802.2 | 1.16x | 786.0 | 1.14x | -2.0% | 1176 | 1176 |
| fix/Order.whole-stream | generated | hand | 888.5 | 969.8 | 1.09x | 921.4 | 1.04x | -5.0% | 1352 | 1352 |
| fix/Order.whole-reader | generated | hand | 878.2 | 946.7 | 1.08x | 921.6 | 1.05x | -2.7% | 1320 | 1320 |
