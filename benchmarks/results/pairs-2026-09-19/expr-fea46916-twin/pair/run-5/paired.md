# Paired stand, 2026-09-19 23:10

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 7020.7 | 18870.3 | 2.69x | 19005.4 | 2.71x | +0.7% | 21416 | 21416 |
| sql/select20.window | generated | hand | 6875.4 | 18414.1 | 2.68x | 18711.8 | 2.72x | +1.6% | 21416 | 21416 |
| sql/select20.scan | generated | control | 388.2 | 380.5 | 0.98x | 387.9 | 1.00x | +1.9% | 0 | 0 |
| tsql/select20.scan | generated | control | 393.3 | 388.0 | 0.99x | 383.7 | 0.98x | -1.1% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2644.0 | 12806.7 | 4.84x | 6292.5 | 2.38x | -50.9% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 6977.0 | 18332.9 | 2.63x | 18665.8 | 2.68x | +1.8% | 21448 | 21392 |
| sql/select20 | generated | hand | 7035.9 | 18057.4 | 2.57x | 18615.2 | 2.65x | +3.1% | 21448 | 21448 |
| sql/refused-late | generated | hand | 2625.0 | 12470.3 | 4.75x | 12687.3 | 4.83x | +1.7% | 13552 | 13552 |
