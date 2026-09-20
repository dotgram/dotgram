# Paired stand, 2026-09-20 02:48

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 86.1 | 204.4 | 2.37x | 207.0 | 2.40x | +1.2% | 360 | 360 | 56 % |  |
| web/url.refused | generated | hand | 59.0 | 251.3 | 4.26x | 194.4 | 3.29x | -22.7% | 64 | 64 | 41 % |  |
| web/url.relative | generated | hand | 88.6 | 178.4 | 2.01x | 181.0 | 2.04x | +1.5% | 312 | 312 | 3 % |  |
| web/url.relative-dot-colon | generated | hand | 75.0 | 155.2 | 2.07x | 160.1 | 2.13x | +3.1% | 280 | 280 | 2 % |  |
| web/url.relative-letters | generated | hand | 115.3 | 210.2 | 1.82x | 225.3 | 1.95x | +7.2% | 312 | 312 | 2 % |  |
