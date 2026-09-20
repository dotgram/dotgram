# Paired stand, 2026-09-20 02:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 86.7 | 204.9 | 2.36x | 206.9 | 2.39x | +1.0% | 360 | 360 | 3 % |  |
| web/url.refused | generated | hand | 57.8 | 243.6 | 4.21x | 192.8 | 3.33x | -20.9% | 64 | 64 | 17 % |  |
| web/url.relative | generated | hand | 91.5 | 184.0 | 2.01x | 191.8 | 2.10x | +4.3% | 312 | 312 | 15 % |  |
| web/url.relative-dot-colon | generated | hand | 76.4 | 158.7 | 2.08x | 163.6 | 2.14x | +3.1% | 280 | 280 | 2 % |  |
| web/url.relative-letters | generated | hand | 116.1 | 219.2 | 1.89x | 227.4 | 1.96x | +3.7% | 312 | 312 | 3 % |  |
