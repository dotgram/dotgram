# Paired stand, 2026-09-20 07:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/One.text | generated | hand | 74.7 | 79.4 | 1.06x | 79.7 | 1.07x | +0.3% | 192 | 192 | 1 % |  |
| fix/Order.text | generated | hand | 667.6 | 665.8 | 1.00x | 646.1 | 0.97x | -3.0% | 1096 | 1096 | 3 % |  |
| web/json.object10000 | generated | hand | 744997.7 | 1325146.9 | 1.78x | 1334635.9 | 1.79x | +0.7% | 1599241 | 1599240 | 29 % |  |
| feeds/stock-count.good.text | generated | hand | 36340.0 | 28662.1 | 0.79x | 28460.1 | 0.78x | -0.7% | 111312 | 111312 | 10 % |  |
| web/url.full | generated | hand | 152.4 | 272.8 | 1.79x | 284.1 | 1.86x | +4.1% | 536 | 536 | 31 % |  |
| web/json.object | generated | hand | 585.1 | 921.8 | 1.58x | 946.0 | 1.62x | +2.6% | 2584 | 2584 | 5 % |  |
| web/cookie.short | generated | control | 51.5 | 79.6 | 1.55x | 80.1 | 1.56x | +0.6% | 336 | 336 | 8 % |  |
| web/media-type.plain | generated | control | 160.3 | 190.8 | 1.19x | 192.6 | 1.20x | +0.9% | 448 | 448 | 5 % |  |
