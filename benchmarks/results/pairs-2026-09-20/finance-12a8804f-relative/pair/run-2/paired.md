# Paired stand, 2026-09-20 02:47

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 85.8 | 206.5 | 2.41x | 208.4 | 2.43x | +0.9% | 360 | 360 | 3 % |  |
| web/url.refused | generated | hand | 57.6 | 252.2 | 4.38x | 196.6 | 3.42x | -22.0% | 64 | 64 | 16 % |  |
| web/url.relative | generated | hand | 90.2 | 180.0 | 2.00x | 188.0 | 2.08x | +4.4% | 312 | 312 | 20 % |  |
| web/url.relative-dot-colon | generated | hand | 76.0 | 152.9 | 2.01x | 164.4 | 2.16x | +7.5% | 280 | 280 | 5 % |  |
| web/url.relative-letters | generated | hand | 189.0 | 329.9 | 1.75x | 357.9 | 1.89x | +8.5% | 312 | 312 | 46 % |  |
