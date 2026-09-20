# Paired stand, 2026-09-20 03:21

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 73.6 | 78.2 | 1.06x | 78.2 | 1.06x | 0.0% | 192 | 192 |
| fix/One.bytes | generated | hand | 76.0 | 98.3 | 1.29x | 100.0 | 1.32x | +1.7% | 248 | 248 |
| fix/One.stream | generated | hand | 146.3 | 325.4 | 2.22x | 347.9 | 2.38x | +6.9% | 888 | 888 |
| fix/Order.text | generated | hand | 674.0 | 666.8 | 0.99x | 649.3 | 0.96x | -2.6% | 1096 | 1096 |
| fix/Order.bytes | generated | hand | 686.3 | 751.8 | 1.10x | 737.3 | 1.07x | -1.9% | 1152 | 1152 |
| fix/Order.stream | generated | hand | 809.3 | 1101.5 | 1.36x | 1085.2 | 1.34x | -1.5% | 1712 | 1712 |
| fix/slope-16.text | generated | hand | 899.2 | 804.5 | 0.89x | 779.7 | 0.87x | -3.1% | 1752 | 1752 |
| fix/Order.span | generated | hand | 665.0 | 521.6 | 0.78x | 500.4 | 0.75x | -4.1% | 1008 | 1008 |
| fix/One.log-text | generated | hand | 81.4 | 81.7 | 1.00x | 82.5 | 1.01x | +1.0% | 192 | 192 |
| fix/One.log-bytes | generated | hand | 81.6 | 110.0 | 1.35x | 106.6 | 1.31x | -3.0% | 248 | 248 |
| fix/One.log-stream | generated | hand | 81.0 | 371.9 | 4.59x | 366.8 | 4.53x | -1.4% | 888 | 888 |
| fix/Order.log-text | generated | hand | 724.0 | 677.9 | 0.94x | 661.7 | 0.91x | -2.4% | 1096 | 1096 |
| fix/Order.log-bytes | generated | hand | 733.9 | 871.1 | 1.19x | 828.4 | 1.13x | -4.9% | 1152 | 1152 |
| fix/Order.log-stream | generated | hand | 725.9 | 1293.8 | 1.78x | 1179.6 | 1.62x | -8.8% | 1712 | 1712 |
| fix/One.yield-reader | generated | hand | 192.3 | 324.6 | 1.69x | 339.0 | 1.76x | +4.4% | 856 | 856 |
| fix/One.yield-string | generated | hand | 76.7 | 231.3 | 3.01x | 225.0 | 2.93x | -2.7% | 272 | 272 |
| fix/One.yield-memory | generated | hand | 77.0 | 120.8 | 1.57x | 118.4 | 1.54x | -2.0% | 352 | 352 |
| fix/One.whole-stream | generated | hand | 147.0 | 220.6 | 1.50x | 221.7 | 1.51x | +0.5% | 448 | 448 |
| fix/One.whole-reader | generated | hand | 149.1 | 235.1 | 1.58x | 225.3 | 1.51x | -4.2% | 416 | 416 |
| fix/Order.yield-reader | generated | hand | 880.3 | 1096.0 | 1.25x | 1093.8 | 1.24x | -0.2% | 1680 | 1680 |
| fix/Order.yield-string | generated | hand | 695.8 | 2279.2 | 3.28x | 2320.5 | 3.33x | +1.8% | 1096 | 1096 |
| fix/Order.yield-memory | generated | hand | 670.5 | 813.9 | 1.21x | 815.8 | 1.22x | +0.2% | 1176 | 1176 |
| fix/Order.whole-stream | generated | hand | 824.2 | 928.6 | 1.13x | 912.0 | 1.11x | -1.8% | 1352 | 1352 |
| fix/Order.whole-reader | generated | hand | 826.4 | 947.8 | 1.15x | 934.6 | 1.13x | -1.4% | 1320 | 1320 |
