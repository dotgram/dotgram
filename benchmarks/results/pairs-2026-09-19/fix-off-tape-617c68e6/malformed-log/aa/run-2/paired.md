# Paired stand, 2026-09-19 23:02

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 652.1 | 612.1 | 0.94x | 599.9 | 0.92x | -2.0% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 673.9 | 768.9 | 1.14x | 734.1 | 1.09x | -4.5% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 810.8 | 1076.1 | 1.33x | 1080.3 | 1.33x | +0.4% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 763.0 | 1273.0 | 1.67x | 1296.3 | 1.70x | +1.8% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 783.6 | 1452.7 | 1.85x | 1468.9 | 1.87x | +1.1% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 783.8 | 2217.8 | 2.83x | 2285.9 | 2.92x | +3.1% | 1872 | 1872 |
