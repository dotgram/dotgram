# Paired stand, 2026-09-20 02:48

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 85.8 | 211.9 | 2.47x | 205.7 | 2.40x | -2.9% | 360 | 360 | 3 % |  |
| web/url.refused | generated | hand | 59.9 | 243.8 | 4.07x | 196.5 | 3.28x | -19.4% | 64 | 64 | 4 % |  |
| web/url.relative | generated | hand | 94.1 | 187.6 | 1.99x | 188.0 | 2.00x | +0.3% | 312 | 312 | 11 % |  |
| web/url.relative-dot-colon | generated | hand | 75.1 | 156.9 | 2.09x | 159.6 | 2.13x | +1.7% | 280 | 280 | 5 % |  |
| web/url.relative-letters | generated | hand | 122.3 | 215.1 | 1.76x | 231.6 | 1.89x | +7.7% | 312 | 312 | 7 % |  |
