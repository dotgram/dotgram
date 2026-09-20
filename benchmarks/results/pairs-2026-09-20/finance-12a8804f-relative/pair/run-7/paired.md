# Paired stand, 2026-09-20 02:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 86.6 | 206.8 | 2.39x | 207.1 | 2.39x | +0.1% | 360 | 360 | 2 % |  |
| web/url.refused | generated | hand | 57.4 | 265.6 | 4.63x | 191.3 | 3.34x | -28.0% | 64 | 64 | 17 % |  |
| web/url.relative | generated | hand | 89.8 | 183.7 | 2.05x | 194.2 | 2.16x | +5.7% | 312 | 312 | 6 % |  |
| web/url.relative-dot-colon | generated | hand | 75.8 | 156.6 | 2.07x | 168.9 | 2.23x | +7.8% | 280 | 280 | 50 % |  |
| web/url.relative-letters | generated | hand | 117.1 | 219.5 | 1.87x | 233.7 | 2.00x | +6.5% | 312 | 312 | 43 % |  |
