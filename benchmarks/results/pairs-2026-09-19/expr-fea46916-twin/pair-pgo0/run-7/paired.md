# Paired stand, 2026-09-19 23:15

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 7944.6 | 21759.9 | 2.74x | 20996.1 | 2.64x | -3.5% | 21416 | 21416 |
| sql/select20.window | generated | hand | 8357.8 | 22996.7 | 2.75x | 22767.2 | 2.72x | -1.0% | 21416 | 21416 |
| sql/select20.scan | generated | control | 388.2 | 385.2 | 0.99x | 385.0 | 0.99x | 0.0% | 0 | 0 |
| tsql/select20.scan | generated | control | 387.7 | 385.0 | 0.99x | 387.0 | 1.00x | +0.5% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 3250.8 | 15097.7 | 4.64x | 7371.0 | 2.27x | -51.2% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 8282.2 | 23044.9 | 2.78x | 23069.4 | 2.79x | +0.1% | 21448 | 21392 |
| sql/select20 | generated | hand | 8440.5 | 23131.1 | 2.74x | 22987.2 | 2.72x | -0.6% | 21448 | 21448 |
| sql/refused-late | generated | hand | 3238.0 | 15477.5 | 4.78x | 15203.8 | 4.70x | -1.8% | 13552 | 13552 |
