# Paired stand, 2026-09-20 07:26

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/One.text | generated | hand | 73.6 | 80.2 | 1.09x | 80.2 | 1.09x | +0.1% | 192 | 192 | 1 % |  |
| fix/Order.text | generated | hand | 652.8 | 638.3 | 0.98x | 654.6 | 1.00x | +2.5% | 1096 | 1096 | 9 % |  |
| web/json.object10000 | generated | hand | 830495.3 | 1181295.3 | 1.42x | 1099528.1 | 1.32x | -6.9% | 1599241 | 1599241 | 39 % |  |
| sql/select20.at | generated | hand | 6895.9 | 18009.8 | 2.61x | 17538.8 | 2.54x | -2.6% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6833.3 | 17791.4 | 2.60x | 17847.7 | 2.61x | +0.3% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 378.9 | 396.6 | 1.05x | 378.3 | 1.00x | -4.6% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 373.4 | 383.9 | 1.03x | 383.2 | 1.03x | -0.2% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 140.7 | 137.4 | 0.98x | 137.8 | 0.98x | +0.3% | 0 | 0 | 25 % |  |
| el/ladder.bool | generated | hand | 983.5 | 1702.9 | 1.73x | 1728.4 | 1.76x | +1.5% | 1776 | 1720 | 5 % |  |
| sql/select20.bool | generated | hand | 6827.6 | 18421.2 | 2.70x | 17707.9 | 2.59x | -3.9% | 21448 | 21392 | 7 % |  |
| web/url.full | generated | hand | 154.5 | 264.5 | 1.71x | 275.6 | 1.78x | +4.2% | 536 | 536 | 3 % |  |
| web/url.refused | generated | hand | 59.4 | 193.1 | 3.25x | 193.7 | 3.26x | +0.3% | 64 | 64 | 2 % |  |
| web/json.object | generated | hand | 584.0 | 921.0 | 1.58x | 931.5 | 1.60x | +1.1% | 2584 | 2584 | 2 % |  |
| web/media-type.plain | generated | control | 157.7 | 183.9 | 1.17x | 185.9 | 1.18x | +1.1% | 448 | 448 | 19 % |  |
| web/media-type.refused | generated | control | 61.9 | 89.8 | 1.45x | 89.0 | 1.44x | -0.8% | 64 | 64 | 79 % |  |
| web/addr-spec.refused | generated | control | 42.3 | 70.6 | 1.67x | 70.7 | 1.67x | +0.2% | 64 | 64 | 18 % |  |
| web/language-tag.refused | generated | control | 101.3 | 134.9 | 1.33x | 135.1 | 1.33x | +0.2% | 64 | 64 | 2 % |  |
| web/date-time.refused | generated | control | 35.2 | 48.3 | 1.37x | 48.4 | 1.37x | +0.2% | 96 | 96 | 10 % |  |
| el/ladder | generated | hand | 978.2 | 1701.0 | 1.74x | 1742.7 | 1.78x | +2.5% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 978.2 | 1136.4 | 1.16x | 1127.9 | 1.15x | -0.7% | 1784 | 1784 | 4 % |  |
| sql/select20 | generated | hand | 6805.8 | 18451.2 | 2.71x | 17703.7 | 2.60x | -4.1% | 21448 | 21448 | 2 % |  |
