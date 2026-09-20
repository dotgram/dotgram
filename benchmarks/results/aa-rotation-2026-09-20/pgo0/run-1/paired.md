# Paired stand, 2026-09-20 03:50

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 15790.8 | 12833.3 | 0.81x | 12766.4 | 0.81x | -0.5% | 8384 | 8384 | 2 % |  |
| web/url.plain | generated | hand | 180.7 | 257.1 | 1.42x | 258.1 | 1.43x | +0.4% | 360 | 360 | 2 % |  |
| web/url.full | generated | hand | 293.9 | 358.7 | 1.22x | 364.2 | 1.24x | +1.5% | 536 | 536 | 10 % |  |
| web/url.ipv4 | generated | hand | 198.2 | 271.3 | 1.37x | 267.2 | 1.35x | -1.5% | 384 | 384 | 28 % |  |
| web/url.long-path | generated | hand | 430.5 | 479.3 | 1.11x | 471.0 | 1.09x | -1.7% | 512 | 512 | 3 % |  |
| web/url.refused | generated | hand | 112.5 | 287.3 | 2.55x | 290.9 | 2.59x | +1.3% | 64 | 64 | 10 % |  |
| web/url.relative | generated | hand | 143.4 | 228.5 | 1.59x | 225.5 | 1.57x | -1.3% | 312 | 312 | 21 % |  |
| web/url.relative-dot-colon | generated | hand | 112.1 | 201.7 | 1.80x | 198.4 | 1.77x | -1.6% | 280 | 280 | 30 % |  |
| web/url.relative-letters | generated | hand | 242.2 | 253.1 | 1.04x | 252.0 | 1.04x | -0.4% | 312 | 312 | 3 % |  |
