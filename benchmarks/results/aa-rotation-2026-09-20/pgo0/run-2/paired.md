# Paired stand, 2026-09-20 03:51

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 15713.6 | 12797.7 | 0.81x | 12903.3 | 0.82x | +0.8% | 8384 | 8384 | 2 % |  |
| web/url.plain | generated | hand | 182.0 | 264.8 | 1.45x | 311.1 | 1.71x | +17.5% | 360 | 360 | 82 % |  |
| web/url.full | generated | hand | 288.4 | 361.8 | 1.25x | 409.6 | 1.42x | +13.2% | 536 | 536 | 4 % |  |
| web/url.ipv4 | generated | hand | 196.1 | 265.4 | 1.35x | 302.1 | 1.54x | +13.8% | 384 | 384 | 5 % |  |
| web/url.long-path | generated | hand | 450.3 | 489.3 | 1.09x | 527.4 | 1.17x | +7.8% | 512 | 512 | 56 % |  |
| web/url.refused | generated | hand | 112.3 | 290.4 | 2.59x | 333.5 | 2.97x | +14.9% | 64 | 64 | 3 % |  |
| web/url.relative | generated | hand | 142.5 | 230.9 | 1.62x | 232.7 | 1.63x | +0.8% | 312 | 312 | 14 % |  |
| web/url.relative-dot-colon | generated | hand | 110.5 | 201.0 | 1.82x | 201.5 | 1.82x | +0.2% | 280 | 280 | 59 % |  |
| web/url.relative-letters | generated | hand | 241.2 | 250.4 | 1.04x | 258.1 | 1.07x | +3.1% | 312 | 312 | 8 % |  |
