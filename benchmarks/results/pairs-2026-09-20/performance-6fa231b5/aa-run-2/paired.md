# Paired stand, 2026-09-20 05:52

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/One.text | generated | hand | 82.6 | 77.6 | 0.94x | 80.6 | 0.98x | +3.9% | 192 | 192 | 24 % |  |
| fix/Order.text | generated | hand | 666.5 | 637.8 | 0.96x | 651.7 | 0.98x | +2.2% | 1096 | 1096 | 20 % |  |
| web/json.object10000 | generated | hand | 862242.2 | 1184553.9 | 1.37x | 1238676.6 | 1.44x | +4.6% | 1599241 | 1599241 | 35 % |  |
| sql/select20.at | generated | hand | 6956.5 | 17515.0 | 2.52x | 17624.2 | 2.53x | +0.6% | 21416 | 21416 | 15 % |  |
| sql/select20.window | generated | hand | 6807.5 | 18137.3 | 2.66x | 18228.7 | 2.68x | +0.5% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 372.9 | 378.1 | 1.01x | 377.3 | 1.01x | -0.2% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 377.7 | 372.9 | 0.99x | 372.5 | 0.99x | -0.1% | 0 | 0 | 26 % |  |
| el/ladder.scan | generated | control | 136.8 | 136.8 | 1.00x | 135.7 | 0.99x | -0.8% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 958.3 | 1677.6 | 1.75x | 1701.6 | 1.78x | +1.4% | 1776 | 1720 | 6 % |  |
| sql/select20.bool | generated | hand | 6773.4 | 18039.9 | 2.66x | 18206.7 | 2.69x | +0.9% | 21448 | 21392 | 8 % |  |
| web/url.full | generated | hand | 153.8 | 269.2 | 1.75x | 280.1 | 1.82x | +4.0% | 536 | 536 | 15 % |  |
| web/url.refused | generated | hand | 59.5 | 195.7 | 3.29x | 194.8 | 3.28x | -0.4% | 64 | 64 | 18 % |  |
| web/json.object | generated | hand | 585.0 | 929.3 | 1.59x | 917.8 | 1.57x | -1.2% | 2584 | 2584 | 11 % |  |
| web/media-type.plain | generated | control | 165.7 | 185.5 | 1.12x | 193.1 | 1.17x | +4.0% | 448 | 448 | 25 % |  |
| web/media-type.refused | generated | control | 61.8 | 89.0 | 1.44x | 88.5 | 1.43x | -0.6% | 64 | 64 | 2 % |  |
| web/addr-spec.refused | generated | control | 42.4 | 70.5 | 1.66x | 71.2 | 1.68x | +1.0% | 64 | 64 | 5 % |  |
| web/language-tag.refused | generated | control | 99.8 | 135.4 | 1.36x | 132.5 | 1.33x | -2.2% | 64 | 64 | 2 % |  |
| web/date-time.refused | generated | control | 34.9 | 48.5 | 1.39x | 48.1 | 1.38x | -0.6% | 96 | 96 | 1 % |  |
| el/ladder | generated | hand | 953.8 | 1680.5 | 1.76x | 1686.7 | 1.77x | +0.4% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 953.8 | 1145.9 | 1.20x | 1140.1 | 1.20x | -0.5% | 1784 | 1784 | 2 % |  |
| sql/select20 | generated | hand | 6755.8 | 18008.6 | 2.67x | 18168.0 | 2.69x | +0.9% | 21448 | 21448 | 16 % |  |
