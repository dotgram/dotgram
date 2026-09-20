# Paired stand, 2026-09-19 23:04

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 657.0 | 618.4 | 0.94x | 597.9 | 0.91x | -3.3% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 671.5 | 736.5 | 1.10x | 746.6 | 1.11x | +1.4% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 829.1 | 1102.2 | 1.33x | 1070.3 | 1.29x | -2.9% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 777.9 | 1252.4 | 1.61x | 657.8 | 0.85x | -47.5% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 768.1 | 1464.8 | 1.91x | 866.2 | 1.13x | -40.9% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 768.0 | 2195.9 | 2.86x | 1202.3 | 1.57x | -45.2% | 1872 | 1872 |
