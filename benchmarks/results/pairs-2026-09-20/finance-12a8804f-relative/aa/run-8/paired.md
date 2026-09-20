# Paired stand, 2026-09-20 02:47

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 86.5 | 204.7 | 2.37x | 207.9 | 2.40x | +1.6% | 360 | 360 | 9 % |  |
| web/url.refused | generated | hand | 60.1 | 246.0 | 4.10x | 244.1 | 4.06x | -0.8% | 64 | 64 | 45 % |  |
| web/url.relative | generated | hand | 89.2 | 182.6 | 2.05x | 187.2 | 2.10x | +2.6% | 312 | 312 | 11 % |  |
| web/url.relative-dot-colon | generated | hand | 74.9 | 153.7 | 2.05x | 160.9 | 2.15x | +4.7% | 280 | 280 | 6 % |  |
| web/url.relative-letters | generated | hand | 116.2 | 212.4 | 1.83x | 227.0 | 1.95x | +6.9% | 312 | 312 | 9 % |  |
