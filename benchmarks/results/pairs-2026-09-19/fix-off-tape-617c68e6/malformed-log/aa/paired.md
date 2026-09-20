Median of 5 of 5 runs, each in a process of its own; control 31.4 ns (the runs' controls: 31.2, 31.3, 31.4, 31.5, 31.5).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 23:03

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 656.7 | 612.1 | 0.93x | 612.3 | 0.93x | 0.0% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 673.9 | 754.5 | 1.12x | 745.1 | 1.11x | -1.3% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 810.8 | 1086.9 | 1.34x | 1102.2 | 1.36x | +1.4% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 783.8 | 1273.0 | 1.62x | 1278.8 | 1.63x | +0.5% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 782.8 | 1452.7 | 1.86x | 1468.9 | 1.88x | +1.1% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 782.3 | 2210.7 | 2.83x | 2205.4 | 2.82x | -0.2% | 1872 | 1872 |
