# Paired stand, 2026-09-20 03:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 5359.3 | 10627.5 | 1.98x | 10597.5 | 1.98x | -0.3% | 8384 | 8384 | 11 % |  |
| web/url.plain | generated | hand | 96.1 | 218.2 | 2.27x | 215.3 | 2.24x | -1.4% | 360 | 360 | 70 % |  |
| web/url.full | generated | hand | 168.0 | 295.4 | 1.76x | 283.6 | 1.69x | -4.0% | 536 | 536 | 5 % |  |
| web/url.ipv4 | generated | hand | 112.7 | 222.0 | 1.97x | 228.8 | 2.03x | +3.1% | 384 | 384 | 3 % |  |
| web/url.long-path | generated | hand | 176.6 | 419.0 | 2.37x | 408.5 | 2.31x | -2.5% | 512 | 512 | 7 % |  |
| web/url.refused | generated | hand | 68.9 | 254.9 | 3.70x | 242.5 | 3.52x | -4.9% | 64 | 64 | 11 % |  |
| web/url.relative | generated | hand | 76.0 | 182.2 | 2.40x | 185.4 | 2.44x | +1.8% | 312 | 312 | 4 % |  |
| web/url.relative-dot-colon | generated | hand | 63.2 | 155.8 | 2.46x | 160.9 | 2.55x | +3.3% | 280 | 280 | 12 % |  |
| web/url.relative-letters | generated | hand | 109.0 | 218.1 | 2.00x | 216.2 | 1.98x | -0.9% | 312 | 312 | 9 % |  |
