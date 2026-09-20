# Paired stand, 2026-09-19 23:05

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 637.2 | 631.9 | 0.99x | 629.3 | 0.99x | -0.4% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 677.8 | 759.6 | 1.12x | 750.4 | 1.11x | -1.2% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 815.1 | 1119.5 | 1.37x | 1109.7 | 1.36x | -0.9% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 769.0 | 1264.7 | 1.64x | 700.9 | 0.91x | -44.6% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 760.3 | 1463.2 | 1.92x | 869.7 | 1.14x | -40.6% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 753.4 | 2242.9 | 2.98x | 1234.7 | 1.64x | -45.0% | 1872 | 1872 |
