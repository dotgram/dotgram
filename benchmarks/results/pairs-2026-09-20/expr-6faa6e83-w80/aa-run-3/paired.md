# Paired stand, 2026-09-20 10:02

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.9 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 657800.0 | 117434.4 | 0.18x | 121434.4 | 0.18x | +3.4% | 113600 | 105600 | 17 % |  |
| tsql/script100.boolboth | generated | scriptdom | 654536.7 | 121084.4 | 0.18x | 122089.8 | 0.19x | +0.8% | 105600 | 105600 | 23 % |  |
| tsql/script100 | generated | scriptdom | 653730.5 | 122556.2 | 0.19x | 121052.3 | 0.19x | -1.2% | 113600 | 113600 | 23 % |  |
| tsql/columns1000 | generated | scriptdom | 1731128.1 | 189014.1 | 0.11x | 188226.6 | 0.11x | -0.4% | 200464 | 200464 | 3 % |  |
| sql/select20.at | generated | hand | 6768.3 | 17345.3 | 2.56x | 17503.5 | 2.59x | +0.9% | 21416 | 21416 | 19 % |  |
| sql/select20.window | generated | hand | 6764.2 | 17931.1 | 2.65x | 18148.8 | 2.68x | +1.2% | 21416 | 21440 | 3 % |  |
| sql/select20.scan | generated | control | 372.4 | 374.6 | 1.01x | 374.4 | 1.01x | -0.1% | 0 | 0 | 20 % |  |
| tsql/select20.scan | generated | control | 376.2 | 375.0 | 1.00x | 379.2 | 1.01x | +1.1% | 0 | 0 | 20 % |  |
| el/ladder.scan | generated | control | 136.5 | 136.3 | 1.00x | 136.3 | 1.00x | 0.0% | 0 | 0 | 20 % |  |
| el/ladder.bool | generated | hand | 959.3 | 1669.8 | 1.74x | 1654.6 | 1.72x | -0.9% | 1776 | 1720 | 25 % |  |
| sql/refused-late.bool | generated | hand | 2523.0 | 12038.2 | 4.77x | 5798.6 | 2.30x | -51.8% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6704.3 | 17856.7 | 2.66x | 17966.0 | 2.68x | +0.6% | 21448 | 21392 | 4 % |  |
| el/ladder | generated | hand | 957.7 | 1673.5 | 1.75x | 1678.4 | 1.75x | +0.3% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 957.7 | 1124.4 | 1.17x | 1132.8 | 1.18x | +0.7% | 1784 | 1784 | 3 % |  |
| el/terms100 | generated | hand | 11403.7 | 16309.4 | 1.43x | 15934.3 | 1.40x | -2.3% | 17904 | 17904 | 17 % |  |
| el/terms100 | immediate | hand | 11403.7 | 12871.7 | 1.13x | 12887.8 | 1.13x | +0.1% | 18720 | 18720 | 17 % |  |
| el/terms1000 | generated | hand | 111451.8 | 153875.3 | 1.38x | 151572.8 | 1.36x | -1.5% | 169150 | 169104 | 13 % |  |
| el/terms1000 | immediate | hand | 111451.8 | 124125.4 | 1.11x | 124650.3 | 1.12x | +0.4% | 177120 | 177120 | 13 % |  |
| sql/select20 | generated | hand | 6741.3 | 17818.2 | 2.64x | 17933.0 | 2.66x | +0.6% | 21448 | 21448 | 6 % |  |
| sql/refused-late | generated | hand | 2551.3 | 12007.0 | 4.71x | 12145.3 | 4.76x | +1.2% | 13552 | 13552 | 18 % |  |
