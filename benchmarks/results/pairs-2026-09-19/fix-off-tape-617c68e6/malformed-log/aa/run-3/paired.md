# Paired stand, 2026-09-19 23:03

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 656.7 | 614.6 | 0.94x | 607.2 | 0.92x | -1.2% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 670.4 | 774.9 | 1.16x | 745.1 | 1.11x | -3.9% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 806.6 | 1086.9 | 1.35x | 1138.8 | 1.41x | +4.8% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 783.8 | 1260.1 | 1.61x | 1256.5 | 1.60x | -0.3% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 771.1 | 1443.5 | 1.87x | 1448.6 | 1.88x | +0.4% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 780.1 | 2234.6 | 2.86x | 2205.4 | 2.83x | -1.3% | 1872 | 1872 |
