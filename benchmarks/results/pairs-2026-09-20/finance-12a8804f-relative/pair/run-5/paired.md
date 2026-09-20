# Paired stand, 2026-09-20 02:48

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 86.5 | 207.2 | 2.39x | 207.9 | 2.40x | +0.4% | 360 | 360 | 4 % |  |
| web/url.refused | generated | hand | 56.7 | 241.9 | 4.27x | 191.3 | 3.37x | -20.9% | 64 | 64 | 17 % |  |
| web/url.relative | generated | hand | 88.0 | 178.0 | 2.02x | 184.2 | 2.09x | +3.5% | 312 | 312 | 1 % |  |
| web/url.relative-dot-colon | generated | hand | 74.5 | 154.7 | 2.08x | 161.3 | 2.17x | +4.3% | 280 | 280 | 3 % |  |
| web/url.relative-letters | generated | hand | 114.3 | 212.2 | 1.86x | 227.4 | 1.99x | +7.2% | 312 | 312 | 1 % |  |
