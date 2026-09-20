# Paired stand, 2026-09-20 09:23

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/One.text | generated | hand | 73.8 | 78.6 | 1.06x | 80.0 | 1.08x | +1.8% | 192 | 192 | 1 % |  |
| fix/Order.text | generated | hand | 673.3 | 640.7 | 0.95x | 631.7 | 0.94x | -1.4% | 1096 | 1096 | 4 % |  |
| web/json.object10000 | generated | hand | 938009.4 | 1545333.6 | 1.65x | 1665671.1 | 1.78x | +7.8% | 1599242 | 1599241 | 40 % |  |
| feeds/stock-count.good.text | generated | hand | 36061.9 | 28609.0 | 0.79x | 28321.7 | 0.79x | -1.0% | 111312 | 111312 | 5 % |  |
| web/url.full | generated | hand | 152.6 | 278.6 | 1.83x | 269.0 | 1.76x | -3.4% | 536 | 536 | 14 % |  |
| web/json.object | generated | hand | 582.5 | 942.5 | 1.62x | 926.3 | 1.59x | -1.7% | 2584 | 2584 | 6 % |  |
| web/cookie.short | generated | control | 50.1 | 79.0 | 1.58x | 79.3 | 1.58x | +0.4% | 336 | 336 | 3 % |  |
| web/media-type.plain | generated | control | 160.5 | 187.2 | 1.17x | 207.3 | 1.29x | +10.7% | 448 | 448 | 4 % |  |
