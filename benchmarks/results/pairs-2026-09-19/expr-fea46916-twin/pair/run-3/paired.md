# Paired stand, 2026-09-19 23:09

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 6715.9 | 16974.2 | 2.53x | 17056.0 | 2.54x | +0.5% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7137.1 | 18225.3 | 2.55x | 18763.6 | 2.63x | +3.0% | 21416 | 21416 |
| sql/select20.scan | generated | control | 387.3 | 388.2 | 1.00x | 389.5 | 1.01x | +0.3% | 0 | 0 |
| tsql/select20.scan | generated | control | 391.5 | 389.0 | 0.99x | 392.3 | 1.00x | +0.8% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2669.6 | 12578.1 | 4.71x | 6002.3 | 2.25x | -52.3% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 6947.7 | 18141.9 | 2.61x | 18809.9 | 2.71x | +3.7% | 21448 | 21392 |
| sql/select20 | generated | hand | 7090.8 | 18092.0 | 2.55x | 18725.1 | 2.64x | +3.5% | 21448 | 21448 |
| sql/refused-late | generated | hand | 2637.9 | 12459.9 | 4.72x | 12490.2 | 4.73x | +0.2% | 13552 | 13552 |
