# Paired stand, 2026-09-19 23:03

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 669.7 | 611.7 | 0.91x | 638.4 | 0.95x | +4.4% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 668.9 | 754.5 | 1.13x | 750.6 | 1.12x | -0.5% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 810.7 | 1055.7 | 1.30x | 1108.8 | 1.37x | +5.0% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 794.0 | 1300.5 | 1.64x | 1278.8 | 1.61x | -1.7% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 783.3 | 1458.2 | 1.86x | 1472.9 | 1.88x | +1.0% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 782.3 | 2165.8 | 2.77x | 2175.6 | 2.78x | +0.5% | 1872 | 1872 |
