# Paired stand, 2026-09-19 23:14

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 8018.5 | 21696.7 | 2.71x | 21078.6 | 2.63x | -2.8% | 21416 | 21416 |
| sql/select20.window | generated | hand | 8455.2 | 22880.3 | 2.71x | 23140.4 | 2.74x | +1.1% | 21416 | 21416 |
| sql/select20.scan | generated | control | 404.4 | 397.0 | 0.98x | 397.7 | 0.98x | +0.2% | 0 | 0 |
| tsql/select20.scan | generated | control | 401.6 | 392.6 | 0.98x | 417.7 | 1.04x | +6.4% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 3245.1 | 15152.0 | 4.67x | 7517.1 | 2.32x | -50.4% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 8428.3 | 22868.4 | 2.71x | 23296.5 | 2.76x | +1.9% | 21448 | 21392 |
| sql/select20 | generated | hand | 8356.1 | 22826.8 | 2.73x | 23039.2 | 2.76x | +0.9% | 21448 | 21448 |
| sql/refused-late | generated | hand | 3216.3 | 15185.4 | 4.72x | 15482.8 | 4.81x | +2.0% | 13552 | 13552 |
