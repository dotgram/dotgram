# Paired stand, 2026-09-20 09:55

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 670200.0 | 124259.4 | 0.19x | 123146.9 | 0.18x | -0.9% | 113600 | 105600 | 7 % |  |
| tsql/script100.boolboth | generated | scriptdom | 675795.3 | 123500.8 | 0.18x | 123330.5 | 0.18x | -0.1% | 105600 | 105603 | 11 % |  |
| tsql/script100 | generated | scriptdom | 689856.2 | 124488.3 | 0.18x | 123659.4 | 0.18x | -0.7% | 113600 | 113600 | 38 % |  |
| tsql/columns1000 | generated | scriptdom | 1750800.0 | 187689.1 | 0.11x | 190967.2 | 0.11x | +1.7% | 200464 | 200464 | 6 % |  |
| sql/select20.at | generated | hand | 6905.3 | 17916.5 | 2.59x | 17829.7 | 2.58x | -0.5% | 21416 | 21441 | 6 % |  |
| sql/select20.window | generated | hand | 6881.6 | 18407.2 | 2.67x | 18305.8 | 2.66x | -0.6% | 21416 | 21440 | 6 % |  |
| sql/select20.scan | generated | control | 373.1 | 382.5 | 1.03x | 382.2 | 1.02x | -0.1% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 379.1 | 395.4 | 1.04x | 382.0 | 1.01x | -3.4% | 0 | 0 | 17 % |  |
| el/ladder.scan | generated | control | 137.9 | 141.5 | 1.03x | 140.1 | 1.02x | -1.0% | 0 | 0 | 2 % |  |
| el/ladder.bool | generated | hand | 967.1 | 1749.9 | 1.81x | 1682.5 | 1.74x | -3.9% | 1776 | 1720 | 10 % |  |
| sql/refused-late.bool | generated | hand | 2536.4 | 12249.2 | 4.83x | 5872.9 | 2.32x | -52.1% | 13552 | 6616 | 14 % |  |
| sql/select20.bool | generated | hand | 6700.1 | 18292.2 | 2.73x | 17857.1 | 2.67x | -2.4% | 21448 | 21392 | 14 % |  |
| el/ladder | generated | hand | 957.6 | 1712.9 | 1.79x | 1822.3 | 1.90x | +6.4% | 1776 | 1776 | 10 % |  |
| el/ladder | immediate | hand | 957.6 | 1157.9 | 1.21x | 1129.2 | 1.18x | -2.5% | 1784 | 1784 | 10 % |  |
| el/terms100 | generated | hand | 11462.5 | 15813.1 | 1.38x | 15901.5 | 1.39x | +0.6% | 17904 | 17928 | 5 % |  |
| el/terms100 | immediate | hand | 11462.5 | 12836.1 | 1.12x | 12784.3 | 1.12x | -0.4% | 18720 | 18723 | 5 % |  |
| el/terms1000 | generated | hand | 111824.9 | 149245.3 | 1.33x | 150762.8 | 1.35x | +1.0% | 169128 | 169128 | 13 % |  |
| el/terms1000 | immediate | hand | 111824.9 | 122929.0 | 1.10x | 124636.0 | 1.11x | +1.4% | 177120 | 177123 | 13 % |  |
| sql/select20 | generated | hand | 6690.4 | 18041.3 | 2.70x | 17733.9 | 2.65x | -1.7% | 21448 | 21472 | 21 % |  |
| sql/refused-late | generated | hand | 2510.9 | 12122.0 | 4.83x | 12190.0 | 4.85x | +0.6% | 13552 | 13576 | 13 % |  |
