# Paired stand, 2026-09-20 03:53

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 15887.1 | 12909.8 | 0.81x | 12888.5 | 0.81x | -0.2% | 8384 | 8384 | 2 % |  |
| web/url.plain | generated | hand | 181.9 | 256.4 | 1.41x | 261.9 | 1.44x | +2.1% | 360 | 360 | 12 % |  |
| web/url.full | generated | hand | 290.1 | 367.3 | 1.27x | 360.3 | 1.24x | -1.9% | 536 | 536 | 4 % |  |
| web/url.ipv4 | generated | hand | 198.3 | 265.3 | 1.34x | 266.5 | 1.34x | +0.4% | 384 | 384 | 10 % |  |
| web/url.long-path | generated | hand | 436.4 | 490.5 | 1.12x | 481.5 | 1.10x | -1.8% | 512 | 512 | 28 % |  |
| web/url.refused | generated | hand | 112.5 | 283.6 | 2.52x | 294.3 | 2.62x | +3.7% | 64 | 64 | 2 % |  |
| web/url.relative | generated | hand | 143.5 | 226.4 | 1.58x | 227.0 | 1.58x | +0.3% | 312 | 312 | 4 % |  |
| web/url.relative-dot-colon | generated | hand | 110.1 | 196.8 | 1.79x | 199.4 | 1.81x | +1.3% | 280 | 280 | 9 % |  |
| web/url.relative-letters | generated | hand | 243.1 | 250.3 | 1.03x | 259.7 | 1.07x | +3.8% | 312 | 312 | 10 % |  |
