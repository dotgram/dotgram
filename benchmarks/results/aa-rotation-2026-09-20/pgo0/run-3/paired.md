# Paired stand, 2026-09-20 03:52

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 15630.4 | 12798.3 | 0.82x | 12699.3 | 0.81x | -0.8% | 8384 | 8384 | 1 % |  |
| web/url.plain | generated | hand | 185.2 | 278.9 | 1.51x | 266.9 | 1.44x | -4.3% | 360 | 360 | 9 % |  |
| web/url.full | generated | hand | 292.4 | 386.4 | 1.32x | 368.8 | 1.26x | -4.5% | 536 | 536 | 14 % |  |
| web/url.ipv4 | generated | hand | 197.9 | 284.7 | 1.44x | 269.5 | 1.36x | -5.3% | 384 | 384 | 3 % |  |
| web/url.long-path | generated | hand | 576.4 | 773.1 | 1.34x | 768.2 | 1.33x | -0.6% | 512 | 512 | 29 % |  |
| web/url.refused | generated | hand | 112.8 | 291.1 | 2.58x | 291.7 | 2.59x | +0.2% | 64 | 64 | 3 % |  |
| web/url.relative | generated | hand | 146.9 | 243.0 | 1.65x | 237.0 | 1.61x | -2.5% | 312 | 312 | 15 % |  |
| web/url.relative-dot-colon | generated | hand | 112.6 | 213.2 | 1.89x | 206.4 | 1.83x | -3.2% | 280 | 280 | 3 % |  |
| web/url.relative-letters | generated | hand | 241.4 | 260.3 | 1.08x | 257.8 | 1.07x | -1.0% | 312 | 312 | 4 % |  |
