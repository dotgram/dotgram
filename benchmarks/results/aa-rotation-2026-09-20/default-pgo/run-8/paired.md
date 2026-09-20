# Paired stand, 2026-09-20 03:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 5325.9 | 10616.7 | 1.99x | 10626.5 | 2.00x | +0.1% | 8384 | 8384 | 6 % |  |
| web/url.plain | generated | hand | 95.5 | 216.2 | 2.26x | 214.2 | 2.24x | -0.9% | 360 | 360 | 4 % |  |
| web/url.full | generated | hand | 171.3 | 306.3 | 1.79x | 287.5 | 1.68x | -6.1% | 536 | 536 | 4 % |  |
| web/url.ipv4 | generated | hand | 111.7 | 224.3 | 2.01x | 243.8 | 2.18x | +8.7% | 384 | 384 | 14 % |  |
| web/url.long-path | generated | hand | 175.4 | 421.6 | 2.40x | 444.1 | 2.53x | +5.3% | 512 | 512 | 9 % |  |
| web/url.refused | generated | hand | 67.9 | 262.0 | 3.86x | 248.7 | 3.66x | -5.1% | 64 | 64 | 5 % |  |
| web/url.relative | generated | hand | 76.1 | 183.4 | 2.41x | 190.3 | 2.50x | +3.8% | 312 | 312 | 19 % |  |
| web/url.relative-dot-colon | generated | hand | 63.6 | 158.7 | 2.50x | 161.1 | 2.53x | +1.5% | 280 | 280 | 3 % |  |
| web/url.relative-letters | generated | hand | 106.8 | 216.8 | 2.03x | 228.7 | 2.14x | +5.5% | 312 | 312 | 51 % |  |
