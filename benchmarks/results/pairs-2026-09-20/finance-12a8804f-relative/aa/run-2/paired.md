# Paired stand, 2026-09-20 02:44

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 86.3 | 210.5 | 2.44x | 207.9 | 2.41x | -1.3% | 360 | 360 | 2 % |  |
| web/url.refused | generated | hand | 59.6 | 251.7 | 4.22x | 259.6 | 4.35x | +3.1% | 64 | 64 | 15 % |  |
| web/url.relative | generated | hand | 90.5 | 183.3 | 2.03x | 191.4 | 2.12x | +4.4% | 312 | 312 | 3 % |  |
| web/url.relative-dot-colon | generated | hand | 77.7 | 163.6 | 2.11x | 164.5 | 2.12x | +0.5% | 280 | 280 | 12 % |  |
| web/url.relative-letters | generated | hand | 117.5 | 217.6 | 1.85x | 217.5 | 1.85x | 0.0% | 312 | 312 | 6 % |  |
