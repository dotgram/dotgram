# Paired stand, 2026-09-20 02:47

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 86.1 | 207.0 | 2.40x | 223.7 | 2.60x | +8.1% | 360 | 360 | 3 % |  |
| web/url.refused | generated | hand | 59.2 | 249.1 | 4.21x | 197.5 | 3.34x | -20.7% | 64 | 64 | 33 % |  |
| web/url.relative | generated | hand | 90.3 | 175.8 | 1.95x | 183.8 | 2.04x | +4.5% | 312 | 312 | 3 % |  |
| web/url.relative-dot-colon | generated | hand | 74.9 | 153.1 | 2.04x | 157.7 | 2.11x | +3.0% | 280 | 280 | 4 % |  |
| web/url.relative-letters | generated | hand | 120.3 | 207.7 | 1.73x | 227.8 | 1.89x | +9.7% | 312 | 312 | 4 % |  |
