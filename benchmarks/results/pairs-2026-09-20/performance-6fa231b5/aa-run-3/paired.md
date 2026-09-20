# Paired stand, 2026-09-20 06:23

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/One.text | generated | hand | 74.3 | 78.2 | 1.05x | 78.6 | 1.06x | +0.5% | 192 | 192 | 2 % |  |
| fix/Order.text | generated | hand | 674.9 | 664.3 | 0.98x | 640.5 | 0.95x | -3.6% | 1096 | 1096 | 4 % |  |
| web/json.object10000 | generated | hand | 736014.8 | 1233776.6 | 1.68x | 1196753.9 | 1.63x | -3.0% | 1599241 | 1599241 | 48 % |  |
| sql/select20.at | generated | hand | 6799.2 | 17303.4 | 2.54x | 17202.3 | 2.53x | -0.6% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6802.7 | 18032.9 | 2.65x | 17879.2 | 2.63x | -0.9% | 21416 | 21416 | 1 % |  |
| sql/select20.scan | generated | control | 371.2 | 382.0 | 1.03x | 380.2 | 1.02x | -0.5% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 376.9 | 378.4 | 1.00x | 379.3 | 1.01x | +0.2% | 0 | 0 | 28 % |  |
| el/ladder.scan | generated | control | 139.0 | 137.7 | 0.99x | 136.9 | 0.98x | -0.6% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 958.9 | 1760.9 | 1.84x | 1676.8 | 1.75x | -4.8% | 1776 | 1720 | 5 % |  |
| sql/select20.bool | generated | hand | 6897.7 | 17851.1 | 2.59x | 17720.5 | 2.57x | -0.7% | 21448 | 21392 | 14 % |  |
| web/url.full | generated | hand | 153.3 | 286.3 | 1.87x | 279.1 | 1.82x | -2.5% | 536 | 536 | 4 % |  |
| web/url.refused | generated | hand | 59.8 | 206.0 | 3.44x | 195.4 | 3.27x | -5.1% | 64 | 64 | 9 % |  |
| web/json.object | generated | hand | 579.5 | 918.9 | 1.59x | 941.4 | 1.62x | +2.4% | 2584 | 2584 | 7 % |  |
| web/media-type.plain | generated | control | 161.1 | 186.4 | 1.16x | 185.6 | 1.15x | -0.4% | 448 | 448 | 4 % |  |
| web/media-type.refused | generated | control | 61.7 | 90.1 | 1.46x | 90.1 | 1.46x | +0.1% | 64 | 64 | 2 % |  |
| web/addr-spec.refused | generated | control | 42.3 | 71.8 | 1.70x | 72.0 | 1.70x | +0.3% | 64 | 64 | 1 % |  |
| web/language-tag.refused | generated | control | 99.3 | 136.9 | 1.38x | 134.7 | 1.36x | -1.6% | 64 | 64 | 2 % |  |
| web/date-time.refused | generated | control | 35.4 | 48.2 | 1.36x | 48.2 | 1.36x | 0.0% | 96 | 96 | 7 % |  |
| el/ladder | generated | hand | 952.3 | 1763.0 | 1.85x | 1674.7 | 1.76x | -5.0% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 952.3 | 1135.8 | 1.19x | 1148.2 | 1.21x | +1.1% | 1784 | 1784 | 2 % |  |
| sql/select20 | generated | hand | 6859.3 | 17895.3 | 2.61x | 17851.3 | 2.60x | -0.2% | 21448 | 21448 | 15 % |  |
