# Paired stand, 2026-09-20 02:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 46.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 88.7 | 212.2 | 2.39x | 212.6 | 2.40x | +0.2% | 360 | 360 | 3 % |  |
| web/url.refused | generated | hand | 78.0 | 324.5 | 4.16x | 353.0 | 4.52x | +8.8% | 64 | 64 | 55 % |  |
| web/url.relative | generated | hand | 161.3 | 319.5 | 1.98x | 317.5 | 1.97x | -0.6% | 312 | 312 | 43 % |  |
| web/url.relative-dot-colon | generated | hand | 139.3 | 267.0 | 1.92x | 264.3 | 1.90x | -1.0% | 280 | 280 | 46 % |  |
| web/url.relative-letters | generated | hand | 202.6 | 379.6 | 1.87x | 372.2 | 1.84x | -2.0% | 312 | 312 | 19 % |  |
