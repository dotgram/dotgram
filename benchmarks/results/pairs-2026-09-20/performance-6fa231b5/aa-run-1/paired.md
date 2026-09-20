# Paired stand, 2026-09-20 05:20

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/One.text | generated | hand | 73.7 | 89.0 | 1.21x | 81.3 | 1.10x | -8.6% | 192 | 192 | 1 % |  |
| fix/Order.text | generated | hand | 662.5 | 646.3 | 0.98x | 637.1 | 0.96x | -1.4% | 1096 | 1096 | 4 % |  |
| web/json.object10000 | generated | hand | 746500.0 | 1119377.3 | 1.50x | 1145199.2 | 1.53x | +2.3% | 1599240 | 1599240 | 58 % |  |
| sql/select20.at | generated | hand | 6969.4 | 17309.3 | 2.48x | 17239.6 | 2.47x | -0.4% | 21416 | 21416 | 8 % |  |
| sql/select20.window | generated | hand | 6954.5 | 17988.1 | 2.59x | 17903.8 | 2.57x | -0.5% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 374.6 | 380.1 | 1.01x | 380.8 | 1.02x | +0.2% | 0 | 0 | 14 % |  |
| tsql/select20.scan | generated | control | 379.2 | 381.1 | 1.01x | 385.5 | 1.02x | +1.2% | 0 | 0 | 28 % |  |
| el/ladder.scan | generated | control | 140.4 | 138.0 | 0.98x | 137.7 | 0.98x | -0.2% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 964.1 | 1691.4 | 1.75x | 1669.3 | 1.73x | -1.3% | 1776 | 1720 | 68 % |  |
| sql/select20.bool | generated | hand | 6924.7 | 17826.6 | 2.57x | 17853.0 | 2.58x | +0.1% | 21448 | 21392 | 4 % |  |
| web/url.full | generated | hand | 169.8 | 276.4 | 1.63x | 280.0 | 1.65x | +1.3% | 536 | 536 | 3 % |  |
| web/url.refused | generated | hand | 75.4 | 193.8 | 2.57x | 194.7 | 2.58x | +0.5% | 64 | 64 | 9 % |  |
| web/json.object | generated | hand | 589.2 | 931.5 | 1.58x | 933.7 | 1.58x | +0.2% | 2584 | 2584 | 5 % |  |
| web/media-type.plain | generated | control | 161.7 | 184.5 | 1.14x | 197.5 | 1.22x | +7.0% | 448 | 448 | 5 % |  |
| web/media-type.refused | generated | control | 61.3 | 88.9 | 1.45x | 89.3 | 1.46x | +0.5% | 64 | 64 | 15 % |  |
| web/addr-spec.refused | generated | control | 42.8 | 70.7 | 1.65x | 80.5 | 1.88x | +13.9% | 64 | 64 | 4 % |  |
| web/language-tag.refused | generated | control | 101.2 | 133.5 | 1.32x | 132.8 | 1.31x | -0.6% | 64 | 64 | 11 % |  |
| web/date-time.refused | generated | control | 35.5 | 49.0 | 1.38x | 48.3 | 1.36x | -1.4% | 96 | 96 | 1 % |  |
| el/ladder | generated | hand | 981.0 | 1703.2 | 1.74x | 1668.0 | 1.70x | -2.1% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 981.0 | 1158.6 | 1.18x | 1137.7 | 1.16x | -1.8% | 1784 | 1784 | 2 % |  |
| sql/select20 | generated | hand | 6893.8 | 17793.6 | 2.58x | 17782.6 | 2.58x | -0.1% | 21448 | 21448 | 4 % |  |
