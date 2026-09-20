# Paired stand, 2026-09-20 02:44

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.9 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 85.7 | 204.6 | 2.39x | 206.7 | 2.41x | +1.0% | 360 | 360 | 2 % |  |
| web/url.refused | generated | hand | 62.0 | 245.4 | 3.95x | 252.4 | 4.07x | +2.9% | 64 | 64 | 59 % |  |
| web/url.relative | generated | hand | 96.4 | 191.3 | 1.99x | 198.6 | 2.06x | +3.8% | 312 | 312 | 16 % |  |
| web/url.relative-dot-colon | generated | hand | 80.0 | 162.0 | 2.03x | 163.3 | 2.04x | +0.8% | 280 | 280 | 38 % |  |
| web/url.relative-letters | generated | hand | 120.0 | 214.7 | 1.79x | 226.0 | 1.88x | +5.2% | 312 | 312 | 9 % |  |
