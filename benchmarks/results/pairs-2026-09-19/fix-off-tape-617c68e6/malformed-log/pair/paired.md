Median of 5 of 5 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.5, 31.4, 31.2, 31.7, 31.6).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 23:05

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 655.4 | 618.4 | 0.94x | 629.3 | 0.96x | +1.8% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 677.8 | 755.9 | 1.12x | 750.4 | 1.11x | -0.7% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 824.8 | 1105.0 | 1.34x | 1101.8 | 1.34x | -0.3% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 777.9 | 1264.7 | 1.63x | 679.9 | 0.87x | -46.2% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 768.1 | 1463.2 | 1.91x | 869.7 | 1.13x | -40.6% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 762.0 | 2246.4 | 2.95x | 1208.5 | 1.59x | -46.2% | 1872 | 1872 |
