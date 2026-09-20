# Paired stand, 2026-09-19 23:11

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 6734.0 | 17629.4 | 2.62x | 17242.5 | 2.56x | -2.2% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7181.0 | 18819.9 | 2.62x | 18481.2 | 2.57x | -1.8% | 21416 | 21416 |
| sql/select20.scan | generated | control | 377.6 | 392.5 | 1.04x | 387.2 | 1.03x | -1.3% | 0 | 0 |
| tsql/select20.scan | generated | control | 388.1 | 391.5 | 1.01x | 389.4 | 1.00x | -0.5% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2624.2 | 12604.6 | 4.80x | 6049.1 | 2.31x | -52.0% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 6902.4 | 18060.2 | 2.62x | 18323.1 | 2.65x | +1.5% | 21448 | 21392 |
| sql/select20 | generated | hand | 6962.9 | 18016.0 | 2.59x | 17973.3 | 2.58x | -0.2% | 21448 | 21448 |
| sql/refused-late | generated | hand | 2556.2 | 12206.3 | 4.78x | 12452.2 | 4.87x | +2.0% | 13552 | 13552 |
