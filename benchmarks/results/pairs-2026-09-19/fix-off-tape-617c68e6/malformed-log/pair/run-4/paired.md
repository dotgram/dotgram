# Paired stand, 2026-09-19 23:04

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 655.4 | 611.3 | 0.93x | 629.5 | 0.96x | +3.0% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 676.6 | 750.1 | 1.11x | 749.1 | 1.11x | -0.1% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 824.8 | 1112.3 | 1.35x | 1101.8 | 1.34x | -0.9% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 782.4 | 1238.1 | 1.58x | 679.9 | 0.87x | -45.1% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 782.0 | 1455.1 | 1.86x | 877.9 | 1.12x | -39.7% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 762.0 | 2411.6 | 3.16x | 1208.5 | 1.59x | -49.9% | 1872 | 1872 |
