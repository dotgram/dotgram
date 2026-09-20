# Paired stand, 2026-09-20 03:55

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 15732.5 | 12830.0 | 0.82x | 12925.3 | 0.82x | +0.7% | 8384 | 8384 | 2 % |  |
| web/url.plain | generated | hand | 181.8 | 261.8 | 1.44x | 272.0 | 1.50x | +3.9% | 360 | 360 | 12 % |  |
| web/url.full | generated | hand | 287.8 | 356.3 | 1.24x | 389.5 | 1.35x | +9.3% | 536 | 536 | 6 % |  |
| web/url.ipv4 | generated | hand | 196.0 | 266.9 | 1.36x | 286.5 | 1.46x | +7.3% | 384 | 384 | 11 % |  |
| web/url.long-path | generated | hand | 434.1 | 483.8 | 1.11x | 507.8 | 1.17x | +5.0% | 512 | 512 | 9 % |  |
| web/url.refused | generated | hand | 114.4 | 289.9 | 2.53x | 301.2 | 2.63x | +3.9% | 64 | 64 | 7 % |  |
| web/url.relative | generated | hand | 142.7 | 228.4 | 1.60x | 233.9 | 1.64x | +2.4% | 312 | 312 | 9 % |  |
| web/url.relative-dot-colon | generated | hand | 110.6 | 201.5 | 1.82x | 203.8 | 1.84x | +1.1% | 280 | 280 | 9 % |  |
| web/url.relative-letters | generated | hand | 241.7 | 252.3 | 1.04x | 255.3 | 1.06x | +1.2% | 312 | 312 | 9 % |  |
