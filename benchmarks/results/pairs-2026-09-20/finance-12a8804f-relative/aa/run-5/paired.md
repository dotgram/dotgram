# Paired stand, 2026-09-20 02:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 45.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 89.2 | 214.8 | 2.41x | 216.2 | 2.42x | +0.6% | 360 | 360 | 4 % |  |
| web/url.refused | generated | hand | 102.0 | 427.9 | 4.19x | 420.8 | 4.12x | -1.7% | 64 | 64 | 47 % |  |
| web/url.relative | generated | hand | 149.1 | 290.9 | 1.95x | 292.6 | 1.96x | +0.6% | 312 | 312 | 44 % |  |
| web/url.relative-dot-colon | generated | hand | 117.1 | 236.0 | 2.01x | 249.6 | 2.13x | +5.8% | 280 | 280 | 43 % |  |
| web/url.relative-letters | generated | hand | 173.8 | 325.6 | 1.87x | 306.3 | 1.76x | -5.9% | 312 | 312 | 21 % |  |
