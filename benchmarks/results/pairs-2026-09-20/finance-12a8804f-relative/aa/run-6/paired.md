# Paired stand, 2026-09-20 02:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 46.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 85.6 | 204.5 | 2.39x | 204.7 | 2.39x | +0.1% | 360 | 360 | 3 % |  |
| web/url.refused | generated | hand | 99.4 | 416.1 | 4.19x | 440.2 | 4.43x | +5.8% | 64 | 64 | 44 % |  |
| web/url.relative | generated | hand | 138.8 | 263.4 | 1.90x | 275.1 | 1.98x | +4.5% | 312 | 312 | 31 % |  |
| web/url.relative-dot-colon | generated | hand | 142.0 | 278.8 | 1.96x | 295.8 | 2.08x | +6.1% | 280 | 280 | 49 % |  |
| web/url.relative-letters | generated | hand | 215.6 | 422.3 | 1.96x | 417.7 | 1.94x | -1.1% | 312 | 312 | 51 % |  |
