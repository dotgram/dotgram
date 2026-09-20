# Paired stand, 2026-09-20 03:52

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 15952.9 | 12924.0 | 0.81x | 13021.7 | 0.82x | +0.8% | 8384 | 8384 | 3 % |  |
| web/url.plain | generated | hand | 182.2 | 259.7 | 1.43x | 267.4 | 1.47x | +2.9% | 360 | 360 | 14 % |  |
| web/url.full | generated | hand | 297.0 | 369.4 | 1.24x | 371.9 | 1.25x | +0.7% | 536 | 536 | 7 % |  |
| web/url.ipv4 | generated | hand | 197.5 | 273.8 | 1.39x | 271.3 | 1.37x | -0.9% | 384 | 384 | 3 % |  |
| web/url.long-path | generated | hand | 434.6 | 483.7 | 1.11x | 485.3 | 1.12x | +0.3% | 512 | 512 | 7 % |  |
| web/url.refused | generated | hand | 112.5 | 287.8 | 2.56x | 296.4 | 2.63x | +3.0% | 64 | 64 | 3 % |  |
| web/url.relative | generated | hand | 142.5 | 227.3 | 1.60x | 228.8 | 1.61x | +0.7% | 312 | 312 | 5 % |  |
| web/url.relative-dot-colon | generated | hand | 110.0 | 201.0 | 1.83x | 201.2 | 1.83x | +0.1% | 280 | 280 | 5 % |  |
| web/url.relative-letters | generated | hand | 239.7 | 252.1 | 1.05x | 255.1 | 1.06x | +1.2% | 312 | 312 | 7 % |  |
