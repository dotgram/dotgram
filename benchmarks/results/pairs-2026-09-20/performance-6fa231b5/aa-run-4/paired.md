# Paired stand, 2026-09-20 06:54

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/One.text | generated | hand | 74.1 | 79.6 | 1.07x | 80.5 | 1.09x | +1.0% | 192 | 192 | 1 % |  |
| fix/Order.text | generated | hand | 657.9 | 657.8 | 1.00x | 665.2 | 1.01x | +1.1% | 1096 | 1096 | 3 % |  |
| web/json.object10000 | generated | hand | 791003.9 | 1339501.6 | 1.69x | 1225971.1 | 1.55x | -8.5% | 1599241 | 1599241 | 73 % |  |
| sql/select20.at | generated | hand | 6912.2 | 18116.4 | 2.62x | 17325.6 | 2.51x | -4.4% | 21416 | 21416 | 30 % |  |
| sql/select20.window | generated | hand | 6889.7 | 18577.9 | 2.70x | 17920.5 | 2.60x | -3.5% | 21416 | 21416 | 16 % |  |
| sql/select20.scan | generated | control | 376.1 | 372.8 | 0.99x | 377.8 | 1.00x | +1.3% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 375.8 | 387.1 | 1.03x | 377.5 | 1.00x | -2.5% | 0 | 0 | 6 % |  |
| el/ladder.scan | generated | control | 135.9 | 135.8 | 1.00x | 137.2 | 1.01x | +1.0% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 968.7 | 1708.6 | 1.76x | 1676.2 | 1.73x | -1.9% | 1776 | 1720 | 11 % |  |
| sql/select20.bool | generated | hand | 6861.6 | 17917.3 | 2.61x | 18231.8 | 2.66x | +1.8% | 21448 | 21392 | 2 % |  |
| web/url.full | generated | hand | 155.0 | 284.6 | 1.84x | 285.3 | 1.84x | +0.3% | 536 | 536 | 13 % |  |
| web/url.refused | generated | hand | 59.6 | 196.0 | 3.29x | 194.9 | 3.27x | -0.5% | 64 | 64 | 3 % |  |
| web/json.object | generated | hand | 591.8 | 932.8 | 1.58x | 937.0 | 1.58x | +0.5% | 2584 | 2584 | 8 % |  |
| web/media-type.plain | generated | control | 154.8 | 201.1 | 1.30x | 186.9 | 1.21x | -7.1% | 448 | 448 | 2 % |  |
| web/media-type.refused | generated | control | 61.7 | 88.9 | 1.44x | 89.9 | 1.46x | +1.1% | 64 | 64 | 2 % |  |
| web/addr-spec.refused | generated | control | 42.4 | 71.1 | 1.68x | 71.3 | 1.68x | +0.3% | 64 | 64 | 3 % |  |
| web/language-tag.refused | generated | control | 99.6 | 133.8 | 1.34x | 132.8 | 1.33x | -0.7% | 64 | 64 | 1 % |  |
| web/date-time.refused | generated | control | 35.4 | 48.4 | 1.37x | 48.6 | 1.37x | +0.5% | 96 | 96 | 6 % |  |
| el/ladder | generated | hand | 969.2 | 1701.8 | 1.76x | 1718.4 | 1.77x | +1.0% | 1776 | 1776 | 13 % |  |
| el/ladder | immediate | hand | 969.2 | 1156.2 | 1.19x | 1156.1 | 1.19x | 0.0% | 1784 | 1784 | 13 % |  |
| sql/select20 | generated | hand | 6856.9 | 17932.0 | 2.62x | 17753.8 | 2.59x | -1.0% | 21448 | 21448 | 20 % |  |
