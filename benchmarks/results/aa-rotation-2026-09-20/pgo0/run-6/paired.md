# Paired stand, 2026-09-20 03:54

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 15875.9 | 12878.6 | 0.81x | 12881.1 | 0.81x | 0.0% | 8384 | 8384 | 1 % |  |
| web/url.plain | generated | hand | 181.4 | 260.5 | 1.44x | 255.2 | 1.41x | -2.1% | 360 | 360 | 3 % |  |
| web/url.full | generated | hand | 289.7 | 369.8 | 1.28x | 373.9 | 1.29x | +1.1% | 536 | 536 | 16 % |  |
| web/url.ipv4 | generated | hand | 196.2 | 268.3 | 1.37x | 266.2 | 1.36x | -0.8% | 384 | 384 | 10 % |  |
| web/url.long-path | generated | hand | 431.5 | 479.6 | 1.11x | 482.8 | 1.12x | +0.7% | 512 | 512 | 2 % |  |
| web/url.refused | generated | hand | 114.6 | 287.0 | 2.51x | 302.9 | 2.64x | +5.5% | 64 | 64 | 8 % |  |
| web/url.relative | generated | hand | 142.6 | 234.2 | 1.64x | 230.9 | 1.62x | -1.4% | 312 | 312 | 5 % |  |
| web/url.relative-dot-colon | generated | hand | 110.3 | 203.0 | 1.84x | 202.2 | 1.83x | -0.4% | 280 | 280 | 6 % |  |
| web/url.relative-letters | generated | hand | 240.9 | 253.2 | 1.05x | 254.3 | 1.06x | +0.4% | 312 | 312 | 3 % |  |
