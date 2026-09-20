# Paired stand, 2026-09-19 23:12

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 8206.3 | 22075.9 | 2.69x | 21284.5 | 2.59x | -3.6% | 21416 | 21416 |
| sql/select20.window | generated | hand | 8371.3 | 23251.5 | 2.78x | 23060.1 | 2.75x | -0.8% | 21416 | 21416 |
| sql/select20.scan | generated | control | 406.5 | 389.7 | 0.96x | 392.1 | 0.96x | +0.6% | 0 | 0 |
| tsql/select20.scan | generated | control | 399.0 | 394.8 | 0.99x | 395.4 | 0.99x | +0.1% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 3265.2 | 15366.3 | 4.71x | 7534.1 | 2.31x | -51.0% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 8534.3 | 22977.4 | 2.69x | 22834.6 | 2.68x | -0.6% | 21448 | 21392 |
| sql/select20 | generated | hand | 8760.3 | 23638.2 | 2.70x | 23289.6 | 2.66x | -1.5% | 21448 | 21448 |
| sql/refused-late | generated | hand | 3511.6 | 16033.9 | 4.57x | 16068.0 | 4.58x | +0.2% | 13552 | 13552 |
