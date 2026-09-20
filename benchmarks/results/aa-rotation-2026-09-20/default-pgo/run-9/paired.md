# Paired stand, 2026-09-20 03:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 5208.7 | 10620.1 | 2.04x | 10728.0 | 2.06x | +1.0% | 8384 | 8384 | 6 % |  |
| web/url.plain | generated | hand | 95.6 | 217.8 | 2.28x | 219.5 | 2.30x | +0.8% | 360 | 360 | 18 % |  |
| web/url.full | generated | hand | 168.4 | 288.3 | 1.71x | 286.3 | 1.70x | -0.7% | 536 | 536 | 23 % |  |
| web/url.ipv4 | generated | hand | 117.0 | 221.8 | 1.90x | 232.6 | 1.99x | +4.9% | 384 | 384 | 3 % |  |
| web/url.long-path | generated | hand | 177.5 | 415.8 | 2.34x | 410.7 | 2.31x | -1.2% | 512 | 512 | 4 % |  |
| web/url.refused | generated | hand | 69.4 | 256.1 | 3.69x | 260.8 | 3.76x | +1.9% | 64 | 64 | 31 % |  |
| web/url.relative | generated | hand | 77.1 | 180.3 | 2.34x | 183.1 | 2.38x | +1.6% | 312 | 312 | 3 % |  |
| web/url.relative-dot-colon | generated | hand | 64.4 | 158.4 | 2.46x | 174.4 | 2.71x | +10.1% | 280 | 280 | 40 % |  |
| web/url.relative-letters | generated | hand | 107.0 | 207.2 | 1.94x | 222.1 | 2.08x | +7.2% | 312 | 312 | 15 % |  |
