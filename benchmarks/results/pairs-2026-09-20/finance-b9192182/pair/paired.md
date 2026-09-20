Median of 5 of 5 runs, each in a process of its own; control 31.4 ns (the runs' controls: 31.4, 31.6, 31.4, 31.4, 31.4).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 03:22

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 74.1 | 79.5 | 1.07x | 78.2 | 1.06x | -1.6% | 192 | 192 |
| fix/One.bytes | generated | hand | 76.0 | 97.3 | 1.28x | 98.0 | 1.29x | +0.8% | 248 | 248 |
| fix/One.stream | generated | hand | 150.2 | 333.2 | 2.22x | 341.0 | 2.27x | +2.3% | 888 | 888 |
| fix/Order.text | generated | hand | 702.0 | 666.8 | 0.95x | 642.0 | 0.91x | -3.7% | 1096 | 1096 |
| fix/Order.bytes | generated | hand | 686.3 | 753.0 | 1.10x | 752.7 | 1.10x | 0.0% | 1152 | 1152 |
| fix/Order.stream | generated | hand | 828.5 | 1101.5 | 1.33x | 1081.3 | 1.31x | -1.8% | 1712 | 1712 |
| fix/slope-16.text | generated | hand | 922.4 | 812.4 | 0.88x | 779.5 | 0.85x | -4.0% | 1752 | 1752 |
| fix/Order.span | generated | hand | 690.3 | 514.1 | 0.74x | 500.4 | 0.73x | -2.7% | 1008 | 1008 |
| fix/One.log-text | generated | hand | 82.9 | 84.3 | 1.02x | 85.7 | 1.03x | +1.7% | 192 | 192 |
| fix/One.log-bytes | generated | hand | 83.2 | 110.0 | 1.32x | 106.4 | 1.28x | -3.2% | 248 | 248 |
| fix/One.log-stream | generated | hand | 84.2 | 371.9 | 4.42x | 356.4 | 4.23x | -4.2% | 888 | 888 |
| fix/Order.log-text | generated | hand | 765.4 | 676.8 | 0.88x | 653.9 | 0.85x | -3.4% | 1096 | 1096 |
| fix/Order.log-bytes | generated | hand | 742.7 | 873.2 | 1.18x | 846.6 | 1.14x | -3.0% | 1152 | 1152 |
| fix/Order.log-stream | generated | hand | 755.9 | 1245.8 | 1.65x | 1196.3 | 1.58x | -4.0% | 1712 | 1712 |
| fix/One.yield-reader | generated | hand | 206.7 | 362.4 | 1.75x | 347.7 | 1.68x | -4.1% | 856 | 856 |
| fix/One.yield-string | generated | hand | 79.1 | 231.3 | 2.92x | 225.0 | 2.84x | -2.7% | 272 | 272 |
| fix/One.yield-memory | generated | hand | 80.2 | 125.8 | 1.57x | 120.9 | 1.51x | -3.9% | 352 | 352 |
| fix/One.whole-stream | generated | hand | 149.3 | 223.7 | 1.50x | 223.6 | 1.50x | -0.1% | 448 | 448 |
| fix/One.whole-reader | generated | hand | 150.1 | 236.2 | 1.57x | 234.3 | 1.56x | -0.8% | 416 | 416 |
| fix/Order.yield-reader | generated | hand | 902.1 | 1096.0 | 1.21x | 1093.2 | 1.21x | -0.3% | 1680 | 1680 |
| fix/Order.yield-string | generated | hand | 696.1 | 2223.0 | 3.19x | 2207.2 | 3.17x | -0.7% | 1096 | 1096 |
| fix/Order.yield-memory | generated | hand | 691.4 | 802.2 | 1.16x | 797.7 | 1.15x | -0.6% | 1176 | 1176 |
| fix/Order.whole-stream | generated | hand | 829.9 | 926.7 | 1.12x | 912.0 | 1.10x | -1.6% | 1352 | 1352 |
| fix/Order.whole-reader | generated | hand | 826.4 | 946.7 | 1.15x | 933.5 | 1.13x | -1.4% | 1320 | 1320 |
