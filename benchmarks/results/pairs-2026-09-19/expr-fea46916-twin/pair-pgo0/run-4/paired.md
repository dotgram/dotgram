# Paired stand, 2026-09-19 23:13

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 8383.2 | 22416.5 | 2.67x | 21809.7 | 2.60x | -2.7% | 21416 | 21416 |
| sql/select20.window | generated | hand | 8671.1 | 23399.2 | 2.70x | 23751.1 | 2.74x | +1.5% | 21416 | 21416 |
| sql/select20.scan | generated | control | 395.4 | 402.7 | 1.02x | 399.4 | 1.01x | -0.8% | 0 | 0 |
| tsql/select20.scan | generated | control | 396.9 | 400.7 | 1.01x | 395.4 | 1.00x | -1.3% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 3397.1 | 15935.9 | 4.69x | 7708.8 | 2.27x | -51.6% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 8625.6 | 23091.4 | 2.68x | 23789.9 | 2.76x | +3.0% | 21448 | 21392 |
| sql/select20 | generated | hand | 8729.5 | 23138.9 | 2.65x | 23109.6 | 2.65x | -0.1% | 21448 | 21448 |
| sql/refused-late | generated | hand | 3337.4 | 15621.7 | 4.68x | 15441.7 | 4.63x | -1.2% | 13552 | 13552 |
