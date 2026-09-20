# Paired stand, 2026-09-20 03:17

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 73.9 | 81.1 | 1.10x | 79.8 | 1.08x | -1.6% | 192 | 192 |
| fix/One.bytes | generated | hand | 77.6 | 100.0 | 1.29x | 98.3 | 1.27x | -1.7% | 248 | 248 |
| fix/One.stream | generated | hand | 145.5 | 349.5 | 2.40x | 341.0 | 2.34x | -2.4% | 888 | 888 |
| fix/Order.text | generated | hand | 703.5 | 674.4 | 0.96x | 647.2 | 0.92x | -4.0% | 1096 | 1096 |
| fix/Order.bytes | generated | hand | 711.2 | 753.0 | 1.06x | 757.9 | 1.07x | +0.6% | 1152 | 1152 |
| fix/Order.stream | generated | hand | 844.0 | 1105.9 | 1.31x | 1081.3 | 1.28x | -2.2% | 1712 | 1712 |
| fix/slope-16.text | generated | hand | 922.4 | 857.9 | 0.93x | 786.6 | 0.85x | -8.3% | 1752 | 1752 |
| fix/Order.span | generated | hand | 668.3 | 514.1 | 0.77x | 511.3 | 0.77x | -0.5% | 1008 | 1008 |
| fix/One.log-text | generated | hand | 84.1 | 88.1 | 1.05x | 85.7 | 1.02x | -2.7% | 192 | 192 |
| fix/One.log-bytes | generated | hand | 85.0 | 108.7 | 1.28x | 106.4 | 1.25x | -2.1% | 248 | 248 |
| fix/One.log-stream | generated | hand | 84.8 | 408.9 | 4.82x | 350.4 | 4.13x | -14.3% | 888 | 888 |
| fix/Order.log-text | generated | hand | 741.8 | 658.0 | 0.89x | 668.0 | 0.90x | +1.5% | 1096 | 1096 |
| fix/Order.log-bytes | generated | hand | 742.7 | 863.8 | 1.16x | 854.2 | 1.15x | -1.1% | 1152 | 1152 |
| fix/Order.log-stream | generated | hand | 740.5 | 1238.0 | 1.67x | 1185.8 | 1.60x | -4.2% | 1712 | 1712 |
| fix/One.yield-reader | generated | hand | 190.2 | 349.1 | 1.84x | 341.6 | 1.80x | -2.1% | 856 | 856 |
| fix/One.yield-string | generated | hand | 79.7 | 237.3 | 2.98x | 234.3 | 2.94x | -1.3% | 272 | 272 |
| fix/One.yield-memory | generated | hand | 79.4 | 126.0 | 1.59x | 120.9 | 1.52x | -4.0% | 352 | 352 |
| fix/One.whole-stream | generated | hand | 145.5 | 223.7 | 1.54x | 217.5 | 1.49x | -2.8% | 448 | 448 |
| fix/One.whole-reader | generated | hand | 144.8 | 244.1 | 1.69x | 229.3 | 1.58x | -6.1% | 416 | 416 |
| fix/Order.yield-reader | generated | hand | 873.1 | 1076.3 | 1.23x | 1110.5 | 1.27x | +3.2% | 1680 | 1680 |
| fix/Order.yield-string | generated | hand | 696.1 | 2208.6 | 3.17x | 2225.9 | 3.20x | +0.8% | 1096 | 1096 |
| fix/Order.yield-memory | generated | hand | 691.4 | 788.2 | 1.14x | 773.5 | 1.12x | -1.9% | 1176 | 1176 |
| fix/Order.whole-stream | generated | hand | 827.1 | 906.8 | 1.10x | 897.0 | 1.08x | -1.1% | 1352 | 1352 |
| fix/Order.whole-reader | generated | hand | 820.7 | 903.8 | 1.10x | 936.8 | 1.14x | +3.6% | 1320 | 1320 |
