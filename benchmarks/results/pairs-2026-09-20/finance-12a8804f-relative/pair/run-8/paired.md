# Paired stand, 2026-09-20 02:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 86.1 | 206.1 | 2.39x | 206.4 | 2.40x | +0.2% | 360 | 360 | 3 % |  |
| web/url.refused | generated | hand | 56.4 | 249.7 | 4.43x | 192.9 | 3.42x | -22.8% | 64 | 64 | 15 % |  |
| web/url.relative | generated | hand | 94.1 | 181.5 | 1.93x | 183.1 | 1.95x | +0.9% | 312 | 312 | 4 % |  |
| web/url.relative-dot-colon | generated | hand | 78.1 | 156.0 | 2.00x | 159.6 | 2.04x | +2.3% | 280 | 280 | 10 % |  |
| web/url.relative-letters | generated | hand | 129.2 | 216.1 | 1.67x | 223.7 | 1.73x | +3.5% | 312 | 312 | 2 % |  |
