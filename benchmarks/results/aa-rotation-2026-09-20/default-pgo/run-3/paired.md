# Paired stand, 2026-09-20 03:45

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 5280.7 | 10582.2 | 2.00x | 10525.9 | 1.99x | -0.5% | 8384 | 8384 | 6 % |  |
| web/url.plain | generated | hand | 96.1 | 224.7 | 2.34x | 219.8 | 2.29x | -2.2% | 360 | 360 | 42 % |  |
| web/url.full | generated | hand | 172.7 | 301.0 | 1.74x | 291.3 | 1.69x | -3.2% | 536 | 536 | 74 % |  |
| web/url.ipv4 | generated | hand | 113.0 | 224.4 | 1.99x | 226.0 | 2.00x | +0.7% | 384 | 384 | 17 % |  |
| web/url.long-path | generated | hand | 176.5 | 435.9 | 2.47x | 409.5 | 2.32x | -6.1% | 512 | 512 | 2 % |  |
| web/url.refused | generated | hand | 68.1 | 268.2 | 3.94x | 260.7 | 3.83x | -2.8% | 64 | 64 | 73 % |  |
| web/url.relative | generated | hand | 75.9 | 184.3 | 2.43x | 192.0 | 2.53x | +4.2% | 312 | 312 | 16 % |  |
| web/url.relative-dot-colon | generated | hand | 63.3 | 156.8 | 2.48x | 164.0 | 2.59x | +4.6% | 280 | 280 | 8 % |  |
| web/url.relative-letters | generated | hand | 104.3 | 216.5 | 2.08x | 227.3 | 2.18x | +5.0% | 312 | 312 | 3 % |  |
