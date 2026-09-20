# Paired stand, 2026-09-20 02:45

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 85.8 | 213.8 | 2.49x | 213.3 | 2.49x | -0.2% | 360 | 360 | 3 % |  |
| web/url.refused | generated | hand | 60.5 | 250.0 | 4.13x | 251.4 | 4.15x | +0.6% | 64 | 64 | 4 % |  |
| web/url.relative | generated | hand | 91.5 | 194.7 | 2.13x | 192.7 | 2.11x | -1.0% | 312 | 312 | 20 % |  |
| web/url.relative-dot-colon | generated | hand | 77.9 | 166.3 | 2.13x | 167.1 | 2.14x | +0.5% | 280 | 280 | 17 % |  |
| web/url.relative-letters | generated | hand | 117.3 | 235.7 | 2.01x | 226.7 | 1.93x | -3.8% | 312 | 312 | 16 % |  |
