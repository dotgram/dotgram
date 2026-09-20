# Paired stand, 2026-09-20 08:52

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/One.text | generated | hand | 73.7 | 78.9 | 1.07x | 79.2 | 1.07x | +0.3% | 192 | 192 | 1 % |  |
| fix/Order.text | generated | hand | 916.6 | 663.4 | 0.72x | 631.3 | 0.69x | -4.8% | 1096 | 1096 | 8 % |  |
| web/json.object10000 | generated | hand | 742132.8 | 1296460.2 | 1.75x | 1408607.8 | 1.90x | +8.7% | 1599241 | 1599241 | 42 % |  |
| feeds/stock-count.good.text | generated | hand | 36081.0 | 28369.3 | 0.79x | 28375.3 | 0.79x | 0.0% | 111312 | 111312 | 9 % |  |
| web/url.full | generated | hand | 153.1 | 283.1 | 1.85x | 273.2 | 1.78x | -3.5% | 536 | 536 | 4 % |  |
| web/json.object | generated | hand | 580.1 | 915.9 | 1.58x | 922.6 | 1.59x | +0.7% | 2584 | 2584 | 3 % |  |
| web/cookie.short | generated | control | 50.2 | 78.7 | 1.57x | 78.9 | 1.57x | +0.3% | 336 | 336 | 14 % |  |
| web/media-type.plain | generated | control | 157.3 | 190.2 | 1.21x | 186.7 | 1.19x | -1.8% | 448 | 448 | 25 % |  |
