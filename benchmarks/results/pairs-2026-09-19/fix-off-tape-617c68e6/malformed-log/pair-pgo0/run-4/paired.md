# Paired stand, 2026-09-19 23:06

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 972.8 | 1030.3 | 1.06x | 1033.9 | 1.06x | +0.3% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 1035.8 | 2008.2 | 1.94x | 1976.5 | 1.91x | -1.6% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 1160.2 | 2394.1 | 2.06x | 2397.4 | 2.07x | +0.1% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 1060.7 | 1885.6 | 1.78x | 1191.8 | 1.12x | -36.8% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 1068.3 | 3078.6 | 2.88x | 2527.3 | 2.37x | -17.9% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 1071.2 | 3993.4 | 3.73x | 2917.3 | 2.72x | -26.9% | 1872 | 1872 |
