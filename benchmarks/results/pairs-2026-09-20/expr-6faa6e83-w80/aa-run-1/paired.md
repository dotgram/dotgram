# Paired stand, 2026-09-20 09:48

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 2621287.5 | 159462.5 | 0.06x | 184518.8 | 0.07x | +15.7% | 113600 | 105603 | 79 % |  |
| tsql/script100.boolboth | generated | scriptdom | 670264.8 | 123678.1 | 0.18x | 119942.2 | 0.18x | -3.0% | 105600 | 105600 | 12 % |  |
| tsql/script100 | generated | scriptdom | 662063.3 | 122514.1 | 0.19x | 122272.7 | 0.18x | -0.2% | 113600 | 113600 | 8 % |  |
| tsql/columns1000 | generated | scriptdom | 1805460.9 | 192476.6 | 0.11x | 194725.0 | 0.11x | +1.2% | 200464 | 200464 | 21 % |  |
| sql/select20.at | generated | hand | 7334.0 | 18112.3 | 2.47x | 18265.3 | 2.49x | +0.8% | 21416 | 21416 | 22 % |  |
| sql/select20.window | generated | hand | 7187.5 | 19066.2 | 2.65x | 19256.8 | 2.68x | +1.0% | 21416 | 21440 | 15 % |  |
| sql/select20.scan | generated | control | 377.0 | 475.2 | 1.26x | 376.3 | 1.00x | -20.8% | 0 | 0 | 10 % |  |
| tsql/select20.scan | generated | control | 380.1 | 389.1 | 1.02x | 382.7 | 1.01x | -1.6% | 0 | 0 | 16 % |  |
| el/ladder.scan | generated | control | 138.1 | 137.2 | 0.99x | 139.1 | 1.01x | +1.4% | 0 | 0 | 14 % |  |
| el/ladder.bool | generated | hand | 977.1 | 1701.8 | 1.74x | 1718.5 | 1.76x | +1.0% | 1776 | 1720 | 12 % |  |
| sql/refused-late.bool | generated | hand | 2622.2 | 12914.0 | 4.92x | 6043.5 | 2.30x | -53.2% | 13552 | 6616 | 18 % |  |
| sql/select20.bool | generated | hand | 7185.7 | 18951.9 | 2.64x | 18935.8 | 2.64x | -0.1% | 21448 | 21392 | 37 % |  |
| el/ladder | generated | hand | 1028.2 | 1783.0 | 1.73x | 1774.6 | 1.73x | -0.5% | 1776 | 1776 | 16 % |  |
| el/ladder | immediate | hand | 1028.2 | 1223.1 | 1.19x | 1201.1 | 1.17x | -1.8% | 1784 | 1784 | 16 % |  |
| el/terms100 | generated | hand | 12128.6 | 16033.5 | 1.32x | 16207.7 | 1.34x | +1.1% | 17904 | 17904 | 21 % |  |
| el/terms100 | immediate | hand | 12128.6 | 13080.5 | 1.08x | 13406.3 | 1.11x | +2.5% | 18720 | 18720 | 21 % |  |
| el/terms1000 | generated | hand | 114789.6 | 152341.8 | 1.33x | 152281.3 | 1.33x | 0.0% | 169104 | 169107 | 12 % |  |
| el/terms1000 | immediate | hand | 114789.6 | 125686.9 | 1.09x | 125077.6 | 1.09x | -0.5% | 177120 | 177120 | 12 % |  |
| sql/select20 | generated | hand | 7190.2 | 19224.8 | 2.67x | 19590.5 | 2.72x | +1.9% | 21448 | 21472 | 21 % |  |
| sql/refused-late | generated | hand | 2628.4 | 12702.1 | 4.83x | 12662.0 | 4.82x | -0.3% | 13552 | 13576 | 26 % |  |
