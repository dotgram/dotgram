# Paired stand, 2026-09-20 03:20

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 75.3 | 80.8 | 1.07x | 78.2 | 1.04x | -3.2% | 192 | 192 |
| fix/One.bytes | generated | hand | 79.7 | 97.3 | 1.22x | 98.0 | 1.23x | +0.8% | 248 | 248 |
| fix/One.stream | generated | hand | 150.2 | 333.2 | 2.22x | 340.3 | 2.27x | +2.1% | 888 | 888 |
| fix/Order.text | generated | hand | 707.9 | 645.8 | 0.91x | 641.5 | 0.91x | -0.7% | 1096 | 1096 |
| fix/Order.bytes | generated | hand | 682.4 | 766.4 | 1.12x | 772.9 | 1.13x | +0.8% | 1152 | 1152 |
| fix/Order.stream | generated | hand | 828.5 | 1113.7 | 1.34x | 1105.2 | 1.33x | -0.8% | 1712 | 1712 |
| fix/slope-16.text | generated | hand | 952.9 | 812.4 | 0.85x | 779.5 | 0.82x | -4.0% | 1752 | 1752 |
| fix/Order.span | generated | hand | 726.9 | 509.2 | 0.70x | 496.3 | 0.68x | -2.5% | 1008 | 1008 |
| fix/One.log-text | generated | hand | 83.2 | 84.4 | 1.01x | 89.6 | 1.08x | +6.2% | 192 | 192 |
| fix/One.log-bytes | generated | hand | 84.3 | 109.3 | 1.30x | 107.2 | 1.27x | -1.8% | 248 | 248 |
| fix/One.log-stream | generated | hand | 84.2 | 390.7 | 4.64x | 356.4 | 4.23x | -8.8% | 888 | 888 |
| fix/Order.log-text | generated | hand | 773.1 | 663.1 | 0.86x | 643.5 | 0.83x | -3.0% | 1096 | 1096 |
| fix/Order.log-bytes | generated | hand | 775.3 | 873.2 | 1.13x | 846.6 | 1.09x | -3.0% | 1152 | 1152 |
| fix/Order.log-stream | generated | hand | 755.9 | 1247.1 | 1.65x | 1212.8 | 1.60x | -2.7% | 1712 | 1712 |
| fix/One.yield-reader | generated | hand | 206.7 | 362.4 | 1.75x | 357.1 | 1.73x | -1.5% | 856 | 856 |
| fix/One.yield-string | generated | hand | 82.6 | 240.6 | 2.91x | 221.9 | 2.69x | -7.8% | 272 | 272 |
| fix/One.yield-memory | generated | hand | 80.5 | 124.6 | 1.55x | 122.2 | 1.52x | -2.0% | 352 | 352 |
| fix/One.whole-stream | generated | hand | 149.3 | 228.2 | 1.53x | 226.9 | 1.52x | -0.5% | 448 | 448 |
| fix/One.whole-reader | generated | hand | 150.1 | 238.0 | 1.58x | 235.4 | 1.57x | -1.1% | 416 | 416 |
| fix/Order.yield-reader | generated | hand | 908.0 | 1087.4 | 1.20x | 1093.2 | 1.20x | +0.5% | 1680 | 1680 |
| fix/Order.yield-string | generated | hand | 716.7 | 2223.0 | 3.10x | 2091.6 | 2.92x | -5.9% | 1096 | 1096 |
| fix/Order.yield-memory | generated | hand | 696.3 | 796.9 | 1.14x | 797.7 | 1.15x | +0.1% | 1176 | 1176 |
| fix/Order.whole-stream | generated | hand | 829.9 | 902.2 | 1.09x | 903.7 | 1.09x | +0.2% | 1352 | 1352 |
| fix/Order.whole-reader | generated | hand | 824.1 | 943.1 | 1.14x | 909.8 | 1.10x | -3.5% | 1320 | 1320 |
