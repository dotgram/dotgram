# Paired stand, 2026-09-19 23:10

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 7166.1 | 18131.8 | 2.53x | 17854.8 | 2.49x | -1.5% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7063.2 | 18216.8 | 2.58x | 18473.8 | 2.62x | +1.4% | 21416 | 21416 |
| sql/select20.scan | generated | control | 384.7 | 386.3 | 1.00x | 387.1 | 1.01x | +0.2% | 0 | 0 |
| tsql/select20.scan | generated | control | 386.5 | 396.9 | 1.03x | 400.5 | 1.04x | +0.9% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2623.2 | 12093.4 | 4.61x | 5823.7 | 2.22x | -51.8% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 6972.8 | 18511.1 | 2.65x | 18484.0 | 2.65x | -0.1% | 21448 | 21392 |
| sql/select20 | generated | hand | 7074.5 | 18355.2 | 2.59x | 18345.9 | 2.59x | -0.1% | 21448 | 21448 |
| sql/refused-late | generated | hand | 2651.6 | 12064.9 | 4.55x | 12242.0 | 4.62x | +1.5% | 13552 | 13552 |
