# Paired stand, 2026-09-20 02:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 85.9 | 212.5 | 2.47x | 209.6 | 2.44x | -1.4% | 360 | 360 | 2 % |  |
| web/url.refused | generated | hand | 61.6 | 250.9 | 4.08x | 194.9 | 3.17x | -22.3% | 64 | 64 | 9 % |  |
| web/url.relative | generated | hand | 90.7 | 177.3 | 1.96x | 183.1 | 2.02x | +3.3% | 312 | 312 | 2 % |  |
| web/url.relative-dot-colon | generated | hand | 76.3 | 153.4 | 2.01x | 161.7 | 2.12x | +5.4% | 280 | 280 | 3 % |  |
| web/url.relative-letters | generated | hand | 127.5 | 214.0 | 1.68x | 227.6 | 1.79x | +6.3% | 312 | 312 | 5 % |  |
