# Paired stand, 2026-09-20 02:47

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 86.7 | 208.7 | 2.41x | 206.4 | 2.38x | -1.1% | 360 | 360 | 49 % |  |
| web/url.refused | generated | hand | 59.5 | 249.5 | 4.19x | 244.0 | 4.10x | -2.2% | 64 | 64 | 14 % |  |
| web/url.relative | generated | hand | 90.4 | 178.4 | 1.97x | 174.5 | 1.93x | -2.2% | 312 | 312 | 10 % |  |
| web/url.relative-dot-colon | generated | hand | 76.3 | 153.5 | 2.01x | 151.1 | 1.98x | -1.6% | 280 | 280 | 2 % |  |
| web/url.relative-letters | generated | hand | 115.6 | 211.2 | 1.83x | 208.6 | 1.80x | -1.2% | 312 | 312 | 8 % |  |
