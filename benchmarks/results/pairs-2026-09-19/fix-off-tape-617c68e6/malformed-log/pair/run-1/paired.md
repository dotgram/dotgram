# Paired stand, 2026-09-19 23:03

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 642.8 | 637.8 | 0.99x | 640.3 | 1.00x | +0.4% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 681.4 | 755.9 | 1.11x | 760.0 | 1.12x | +0.5% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 814.3 | 1105.0 | 1.36x | 1097.0 | 1.35x | -0.7% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 764.9 | 1278.3 | 1.67x | 691.0 | 0.90x | -45.9% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 757.4 | 1561.5 | 2.06x | 870.4 | 1.15x | -44.3% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 754.0 | 2246.4 | 2.98x | 1190.8 | 1.58x | -47.0% | 1872 | 1872 |
