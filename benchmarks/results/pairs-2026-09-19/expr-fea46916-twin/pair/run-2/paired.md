# Paired stand, 2026-09-19 23:08

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 6613.1 | 17168.3 | 2.60x | 17003.3 | 2.57x | -1.0% | 21416 | 21416 |
| sql/select20.window | generated | hand | 6916.5 | 18305.9 | 2.65x | 18873.1 | 2.73x | +3.1% | 21416 | 21416 |
| sql/select20.scan | generated | control | 386.4 | 405.2 | 1.05x | 390.1 | 1.01x | -3.7% | 0 | 0 |
| tsql/select20.scan | generated | control | 388.3 | 392.3 | 1.01x | 411.5 | 1.06x | +4.9% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2695.1 | 12697.7 | 4.71x | 5974.1 | 2.22x | -53.0% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7108.0 | 18131.5 | 2.55x | 18722.6 | 2.63x | +3.3% | 21448 | 21392 |
| sql/select20 | generated | hand | 7029.9 | 18038.3 | 2.57x | 18803.8 | 2.67x | +4.2% | 21448 | 21448 |
| sql/refused-late | generated | hand | 2625.2 | 12659.7 | 4.82x | 12506.9 | 4.76x | -1.2% | 13552 | 13552 |
