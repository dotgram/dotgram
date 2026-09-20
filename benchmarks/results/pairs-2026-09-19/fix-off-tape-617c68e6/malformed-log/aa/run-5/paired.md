# Paired stand, 2026-09-19 23:03

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 674.1 | 619.3 | 0.92x | 612.3 | 0.91x | -1.1% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 675.1 | 751.4 | 1.11x | 744.4 | 1.10x | -0.9% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 821.4 | 1107.3 | 1.35x | 1081.7 | 1.32x | -2.3% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 792.7 | 1275.6 | 1.61x | 1252.9 | 1.58x | -1.8% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 782.8 | 1454.9 | 1.86x | 1440.5 | 1.84x | -1.0% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 808.9 | 2210.7 | 2.73x | 2193.9 | 2.71x | -0.8% | 1872 | 1872 |
