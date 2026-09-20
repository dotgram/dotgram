# Paired stand, 2026-09-20 03:48

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 5156.5 | 10573.2 | 2.05x | 10515.6 | 2.04x | -0.5% | 8384 | 8384 | 3 % |  |
| web/url.plain | generated | hand | 95.4 | 211.8 | 2.22x | 214.1 | 2.24x | +1.1% | 360 | 360 | 20 % |  |
| web/url.full | generated | hand | 175.3 | 290.0 | 1.65x | 283.6 | 1.62x | -2.2% | 536 | 536 | 21 % |  |
| web/url.ipv4 | generated | hand | 112.4 | 221.3 | 1.97x | 230.2 | 2.05x | +4.0% | 384 | 384 | 11 % |  |
| web/url.long-path | generated | hand | 176.2 | 412.2 | 2.34x | 414.7 | 2.35x | +0.6% | 512 | 512 | 31 % |  |
| web/url.refused | generated | hand | 68.3 | 250.8 | 3.67x | 255.0 | 3.73x | +1.7% | 64 | 64 | 12 % |  |
| web/url.relative | generated | hand | 82.0 | 181.5 | 2.21x | 182.4 | 2.22x | +0.5% | 312 | 312 | 56 % |  |
| web/url.relative-dot-colon | generated | hand | 63.4 | 153.8 | 2.43x | 156.3 | 2.46x | +1.6% | 280 | 280 | 3 % |  |
| web/url.relative-letters | generated | hand | 109.6 | 213.2 | 1.95x | 213.8 | 1.95x | +0.3% | 312 | 312 | 7 % |  |
