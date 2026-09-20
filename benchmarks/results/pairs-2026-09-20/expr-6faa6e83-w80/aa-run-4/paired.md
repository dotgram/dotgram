# Paired stand, 2026-09-20 10:09

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.9 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 653059.4 | 121931.2 | 0.19x | 123468.8 | 0.19x | +1.3% | 113600 | 105603 | 11 % |  |
| tsql/script100.boolboth | generated | scriptdom | 657415.6 | 123307.8 | 0.19x | 123351.6 | 0.19x | 0.0% | 105600 | 105600 | 3 % |  |
| tsql/script100 | generated | scriptdom | 658617.2 | 123978.1 | 0.19x | 122680.5 | 0.19x | -1.0% | 113600 | 113600 | 26 % |  |
| tsql/columns1000 | generated | scriptdom | 1735271.9 | 187357.8 | 0.11x | 187620.3 | 0.11x | +0.1% | 200464 | 200464 | 8 % |  |
| sql/select20.at | generated | hand | 6866.9 | 17494.9 | 2.55x | 17742.8 | 2.58x | +1.4% | 21416 | 21441 | 4 % |  |
| sql/select20.window | generated | hand | 6838.0 | 18189.8 | 2.66x | 18326.3 | 2.68x | +0.8% | 21416 | 21440 | 8 % |  |
| sql/select20.scan | generated | control | 379.1 | 377.2 | 0.99x | 378.8 | 1.00x | +0.4% | 0 | 0 | 14 % |  |
| tsql/select20.scan | generated | control | 375.2 | 373.4 | 1.00x | 377.8 | 1.01x | +1.2% | 0 | 0 | 20 % |  |
| el/ladder.scan | generated | control | 141.3 | 135.8 | 0.96x | 136.5 | 0.97x | +0.5% | 0 | 0 | 12 % |  |
| el/ladder.bool | generated | hand | 979.7 | 1681.3 | 1.72x | 1673.1 | 1.71x | -0.5% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2539.6 | 12262.8 | 4.83x | 5860.5 | 2.31x | -52.2% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6731.2 | 17976.6 | 2.67x | 18019.4 | 2.68x | +0.2% | 21448 | 21392 | 2 % |  |
| el/ladder | generated | hand | 973.9 | 1692.8 | 1.74x | 1680.2 | 1.73x | -0.7% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 973.9 | 1185.4 | 1.22x | 1152.3 | 1.18x | -2.8% | 1784 | 1784 | 3 % |  |
| el/terms100 | generated | hand | 11427.0 | 15773.5 | 1.38x | 15844.5 | 1.39x | +0.4% | 17904 | 17928 | 2 % |  |
| el/terms100 | immediate | hand | 11427.0 | 12841.3 | 1.12x | 13005.1 | 1.14x | +1.3% | 18766 | 18720 | 2 % |  |
| el/terms1000 | generated | hand | 110434.1 | 149772.4 | 1.36x | 151183.2 | 1.37x | +0.9% | 169128 | 169128 | 4 % |  |
| el/terms1000 | immediate | hand | 110434.1 | 123104.7 | 1.11x | 124976.8 | 1.13x | +1.5% | 177120 | 177123 | 4 % |  |
| sql/select20 | generated | hand | 6730.0 | 17985.1 | 2.67x | 18065.7 | 2.68x | +0.4% | 21448 | 21472 | 2 % |  |
| sql/refused-late | generated | hand | 2545.7 | 12234.7 | 4.81x | 12225.8 | 4.80x | -0.1% | 13552 | 13576 | 2 % |  |
