# Paired stand, 2026-09-20 03:47

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 5219.8 | 10718.8 | 2.05x | 10510.4 | 2.01x | -1.9% | 8384 | 8384 | 4 % |  |
| web/url.plain | generated | hand | 95.3 | 216.3 | 2.27x | 214.2 | 2.25x | -1.0% | 360 | 360 | 20 % |  |
| web/url.full | generated | hand | 169.3 | 285.8 | 1.69x | 284.8 | 1.68x | -0.4% | 536 | 536 | 5 % |  |
| web/url.ipv4 | generated | hand | 113.4 | 220.8 | 1.95x | 229.8 | 2.03x | +4.1% | 384 | 384 | 8 % |  |
| web/url.long-path | generated | hand | 178.6 | 410.2 | 2.30x | 440.5 | 2.47x | +7.4% | 512 | 512 | 4 % |  |
| web/url.refused | generated | hand | 68.5 | 253.8 | 3.71x | 256.2 | 3.74x | +0.9% | 64 | 64 | 19 % |  |
| web/url.relative | generated | hand | 76.3 | 179.3 | 2.35x | 183.5 | 2.40x | +2.3% | 312 | 312 | 32 % |  |
| web/url.relative-dot-colon | generated | hand | 63.4 | 155.0 | 2.45x | 158.9 | 2.51x | +2.6% | 280 | 280 | 6 % |  |
| web/url.relative-letters | generated | hand | 104.9 | 207.3 | 1.98x | 205.1 | 1.95x | -1.1% | 312 | 312 | 15 % |  |
