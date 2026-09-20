# Paired stand, 2026-09-19 23:09

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 6697.7 | 17384.3 | 2.60x | 16564.9 | 2.47x | -4.7% | 21416 | 21416 |
| sql/select20.window | generated | hand | 6915.5 | 18649.9 | 2.70x | 18109.1 | 2.62x | -2.9% | 21416 | 21416 |
| sql/select20.scan | generated | control | 400.8 | 390.0 | 0.97x | 386.0 | 0.96x | -1.0% | 0 | 0 |
| tsql/select20.scan | generated | control | 386.3 | 397.3 | 1.03x | 385.0 | 1.00x | -3.1% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2590.3 | 12498.6 | 4.83x | 5835.2 | 2.25x | -53.3% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 6806.4 | 19023.7 | 2.79x | 17714.4 | 2.60x | -6.9% | 21448 | 21392 |
| sql/select20 | generated | hand | 6804.2 | 18936.1 | 2.78x | 18079.8 | 2.66x | -4.5% | 21448 | 21448 |
| sql/refused-late | generated | hand | 2627.0 | 12574.8 | 4.79x | 12474.0 | 4.75x | -0.8% | 13552 | 13552 |
