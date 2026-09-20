# Paired stand, 2026-09-20 03:19

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 74.9 | 79.4 | 1.06x | 79.5 | 1.06x | +0.1% | 192 | 192 |
| fix/One.bytes | generated | hand | 75.1 | 96.9 | 1.29x | 96.4 | 1.28x | -0.5% | 248 | 248 |
| fix/One.stream | generated | hand | 151.5 | 343.0 | 2.26x | 343.1 | 2.26x | 0.0% | 888 | 888 |
| fix/Order.text | generated | hand | 702.0 | 640.2 | 0.91x | 634.9 | 0.90x | -0.8% | 1096 | 1096 |
| fix/Order.bytes | generated | hand | 677.7 | 737.6 | 1.09x | 752.7 | 1.11x | +2.0% | 1152 | 1152 |
| fix/Order.stream | generated | hand | 803.4 | 1098.3 | 1.37x | 1080.8 | 1.35x | -1.6% | 1712 | 1712 |
| fix/slope-16.text | generated | hand | 924.1 | 812.7 | 0.88x | 765.0 | 0.83x | -5.9% | 1752 | 1752 |
| fix/Order.span | generated | hand | 690.3 | 509.3 | 0.74x | 494.0 | 0.72x | -3.0% | 1008 | 1008 |
| fix/One.log-text | generated | hand | 82.9 | 84.3 | 1.02x | 84.5 | 1.02x | +0.2% | 192 | 192 |
| fix/One.log-bytes | generated | hand | 82.9 | 129.0 | 1.56x | 104.9 | 1.26x | -18.7% | 248 | 248 |
| fix/One.log-stream | generated | hand | 83.6 | 358.6 | 4.29x | 355.7 | 4.25x | -0.8% | 888 | 888 |
| fix/Order.log-text | generated | hand | 797.6 | 676.8 | 0.85x | 653.9 | 0.82x | -3.4% | 1096 | 1096 |
| fix/Order.log-bytes | generated | hand | 782.4 | 973.5 | 1.24x | 849.1 | 1.09x | -12.8% | 1152 | 1152 |
| fix/Order.log-stream | generated | hand | 770.6 | 1244.8 | 1.62x | 1240.4 | 1.61x | -0.4% | 1712 | 1712 |
| fix/One.yield-reader | generated | hand | 213.7 | 373.0 | 1.75x | 351.6 | 1.65x | -5.7% | 856 | 856 |
| fix/One.yield-string | generated | hand | 79.1 | 222.7 | 2.81x | 228.5 | 2.89x | +2.6% | 272 | 272 |
| fix/One.yield-memory | generated | hand | 80.9 | 125.8 | 1.55x | 123.9 | 1.53x | -1.6% | 352 | 352 |
| fix/One.whole-stream | generated | hand | 155.4 | 224.6 | 1.45x | 223.6 | 1.44x | -0.4% | 448 | 448 |
| fix/One.whole-reader | generated | hand | 158.1 | 236.2 | 1.49x | 234.3 | 1.48x | -0.8% | 416 | 416 |
| fix/Order.yield-reader | generated | hand | 923.4 | 1170.7 | 1.27x | 1091.5 | 1.18x | -6.8% | 1680 | 1680 |
| fix/Order.yield-string | generated | hand | 741.3 | 2231.1 | 3.01x | 2207.2 | 2.98x | -1.1% | 1096 | 1096 |
| fix/Order.yield-memory | generated | hand | 729.3 | 863.3 | 1.18x | 809.6 | 1.11x | -6.2% | 1176 | 1176 |
| fix/Order.whole-stream | generated | hand | 833.7 | 926.7 | 1.11x | 947.9 | 1.14x | +2.3% | 1352 | 1352 |
| fix/Order.whole-reader | generated | hand | 845.1 | 961.4 | 1.14x | 933.5 | 1.10x | -2.9% | 1320 | 1320 |
