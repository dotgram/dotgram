# Paired stand, 2026-09-20 03:56

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 15634.2 | 12738.6 | 0.81x | 12647.7 | 0.81x | -0.7% | 8384 | 8384 | 2 % |  |
| web/url.plain | generated | hand | 182.7 | 270.1 | 1.48x | 266.5 | 1.46x | -1.3% | 360 | 360 | 13 % |  |
| web/url.full | generated | hand | 288.3 | 360.3 | 1.25x | 366.3 | 1.27x | +1.7% | 536 | 536 | 5 % |  |
| web/url.ipv4 | generated | hand | 196.5 | 270.2 | 1.37x | 257.1 | 1.31x | -4.8% | 384 | 384 | 6 % |  |
| web/url.long-path | generated | hand | 431.2 | 489.9 | 1.14x | 481.6 | 1.12x | -1.7% | 512 | 512 | 27 % |  |
| web/url.refused | generated | hand | 112.6 | 289.5 | 2.57x | 292.7 | 2.60x | +1.1% | 64 | 64 | 3 % |  |
| web/url.relative | generated | hand | 143.1 | 234.3 | 1.64x | 236.6 | 1.65x | +1.0% | 312 | 312 | 10 % |  |
| web/url.relative-dot-colon | generated | hand | 111.5 | 202.4 | 1.81x | 205.5 | 1.84x | +1.6% | 280 | 280 | 12 % |  |
| web/url.relative-letters | generated | hand | 243.0 | 253.1 | 1.04x | 260.2 | 1.07x | +2.8% | 312 | 312 | 4 % |  |
