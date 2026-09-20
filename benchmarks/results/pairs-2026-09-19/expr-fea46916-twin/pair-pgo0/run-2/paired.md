# Paired stand, 2026-09-19 23:12

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 8099.7 | 21714.1 | 2.68x | 21212.8 | 2.62x | -2.3% | 21416 | 21416 |
| sql/select20.window | generated | hand | 8500.7 | 23340.6 | 2.75x | 23661.4 | 2.78x | +1.4% | 21416 | 21416 |
| sql/select20.scan | generated | control | 399.7 | 392.4 | 0.98x | 385.9 | 0.97x | -1.7% | 0 | 0 |
| tsql/select20.scan | generated | control | 411.8 | 404.8 | 0.98x | 397.3 | 0.96x | -1.8% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 3319.1 | 15465.5 | 4.66x | 7591.3 | 2.29x | -50.9% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 8471.2 | 23279.7 | 2.75x | 23033.3 | 2.72x | -1.1% | 21448 | 21392 |
| sql/select20 | generated | hand | 8537.5 | 23070.7 | 2.70x | 22899.6 | 2.68x | -0.7% | 21448 | 21448 |
| sql/refused-late | generated | hand | 3237.6 | 15296.0 | 4.72x | 15293.4 | 4.72x | 0.0% | 13552 | 13552 |
