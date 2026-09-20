# Paired stand, 2026-09-20 02:45

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 85.4 | 206.9 | 2.42x | 210.9 | 2.47x | +1.9% | 360 | 360 | 3 % |  |
| web/url.refused | generated | hand | 59.8 | 250.0 | 4.18x | 252.3 | 4.22x | +0.9% | 64 | 64 | 11 % |  |
| web/url.relative | generated | hand | 92.7 | 182.9 | 1.97x | 188.9 | 2.04x | +3.3% | 312 | 312 | 35 % |  |
| web/url.relative-dot-colon | generated | hand | 76.0 | 159.0 | 2.09x | 163.7 | 2.15x | +2.9% | 280 | 280 | 29 % |  |
| web/url.relative-letters | generated | hand | 155.7 | 272.6 | 1.75x | 305.5 | 1.96x | +12.1% | 312 | 312 | 56 % |  |
