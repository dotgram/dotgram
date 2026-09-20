# Paired stand, 2026-09-19 23:02

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 645.2 | 597.9 | 0.93x | 627.6 | 0.97x | +5.0% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 679.8 | 742.9 | 1.09x | 757.0 | 1.11x | +1.9% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 818.1 | 1097.5 | 1.34x | 1102.2 | 1.35x | +0.4% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 752.0 | 1252.0 | 1.66x | 1299.0 | 1.73x | +3.8% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 763.4 | 1447.8 | 1.90x | 1498.9 | 1.96x | +3.5% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 761.1 | 2202.4 | 2.89x | 2233.4 | 2.93x | +1.4% | 1872 | 1872 |
