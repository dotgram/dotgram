# Paired stand, 2026-09-20 03:54

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 15686.4 | 12792.3 | 0.82x | 12733.4 | 0.81x | -0.5% | 8384 | 8384 | 1 % |  |
| web/url.plain | generated | hand | 180.9 | 264.6 | 1.46x | 261.9 | 1.45x | -1.0% | 360 | 360 | 8 % |  |
| web/url.full | generated | hand | 287.3 | 361.8 | 1.26x | 364.5 | 1.27x | +0.8% | 536 | 536 | 6 % |  |
| web/url.ipv4 | generated | hand | 196.6 | 267.0 | 1.36x | 263.8 | 1.34x | -1.2% | 384 | 384 | 7 % |  |
| web/url.long-path | generated | hand | 428.3 | 477.8 | 1.12x | 477.4 | 1.11x | -0.1% | 512 | 512 | 6 % |  |
| web/url.refused | generated | hand | 112.7 | 300.7 | 2.67x | 302.5 | 2.68x | +0.6% | 64 | 64 | 15 % |  |
| web/url.relative | generated | hand | 142.6 | 233.6 | 1.64x | 231.0 | 1.62x | -1.1% | 312 | 312 | 29 % |  |
| web/url.relative-dot-colon | generated | hand | 110.1 | 206.8 | 1.88x | 203.6 | 1.85x | -1.5% | 280 | 280 | 7 % |  |
| web/url.relative-letters | generated | hand | 243.7 | 258.7 | 1.06x | 257.0 | 1.05x | -0.7% | 312 | 312 | 3 % |  |
