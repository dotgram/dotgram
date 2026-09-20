Median of 7 of 7 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.6, 31.5, 31.4, 31.7, 31.6, 31.2, 31.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 23:15

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 8154.5 | 21759.9 | 2.67x | 21284.5 | 2.61x | -2.2% | 21416 | 21416 |
| sql/select20.window | generated | hand | 8500.7 | 23251.5 | 2.74x | 23140.4 | 2.72x | -0.5% | 21416 | 21416 |
| sql/select20.scan | generated | control | 395.4 | 393.8 | 1.00x | 392.1 | 0.99x | -0.4% | 0 | 0 |
| tsql/select20.scan | generated | control | 396.9 | 394.8 | 0.99x | 395.4 | 1.00x | +0.1% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 3319.1 | 15366.3 | 4.63x | 7534.1 | 2.27x | -51.0% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 8461.9 | 23044.9 | 2.72x | 23069.4 | 2.73x | +0.1% | 21448 | 21392 |
| sql/select20 | generated | hand | 8537.5 | 23131.1 | 2.71x | 23109.6 | 2.71x | -0.1% | 21448 | 21448 |
| sql/refused-late | generated | hand | 3286.8 | 15477.5 | 4.71x | 15441.7 | 4.70x | -0.2% | 13552 | 13552 |
