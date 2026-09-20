Median of 7 of 7 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.5, 31.4, 31.6, 31.2, 31.5, 32.2, 31.7).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 23:11

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 6715.9 | 17384.3 | 2.59x | 17056.0 | 2.54x | -1.9% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7063.2 | 18414.1 | 2.61x | 18711.8 | 2.65x | +1.6% | 21416 | 21416 |
| sql/select20.scan | generated | control | 386.4 | 390.0 | 1.01x | 387.2 | 1.00x | -0.7% | 0 | 0 |
| tsql/select20.scan | generated | control | 388.3 | 391.5 | 1.01x | 389.4 | 1.00x | -0.5% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2626.9 | 12578.1 | 4.79x | 5974.1 | 2.27x | -52.5% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 6947.7 | 18172.2 | 2.62x | 18484.0 | 2.66x | +1.7% | 21448 | 21392 |
| sql/select20 | generated | hand | 7029.9 | 18092.0 | 2.57x | 18480.7 | 2.63x | +2.1% | 21448 | 21448 |
| sql/refused-late | generated | hand | 2625.2 | 12459.9 | 4.75x | 12490.2 | 4.76x | +0.2% | 13552 | 13552 |
