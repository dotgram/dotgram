# Paired stand, 2026-09-20 08:24

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/One.text | generated | hand | 88.2 | 82.3 | 0.93x | 83.0 | 0.94x | +0.8% | 192 | 192 | 23 % |  |
| fix/Order.text | generated | hand | 693.1 | 681.8 | 0.98x | 665.5 | 0.96x | -2.4% | 1096 | 1096 | 33 % |  |
| web/json.object10000 | generated | hand | 731759.4 | 1383993.0 | 1.89x | 1383412.5 | 1.89x | 0.0% | 1599241 | 1599241 | 36 % |  |
| feeds/stock-count.good.text | generated | hand | 36236.9 | 28716.3 | 0.79x | 28654.4 | 0.79x | -0.2% | 111312 | 111312 | 20 % |  |
| web/url.full | generated | hand | 156.7 | 279.3 | 1.78x | 280.1 | 1.79x | +0.3% | 536 | 536 | 9 % |  |
| web/json.object | generated | hand | 579.4 | 942.7 | 1.63x | 929.2 | 1.60x | -1.4% | 2584 | 2584 | 19 % |  |
| web/cookie.short | generated | control | 50.5 | 78.6 | 1.56x | 78.8 | 1.56x | +0.2% | 336 | 336 | 3 % |  |
| web/media-type.plain | generated | control | 160.7 | 190.9 | 1.19x | 189.4 | 1.18x | -0.8% | 448 | 448 | 5 % |  |
