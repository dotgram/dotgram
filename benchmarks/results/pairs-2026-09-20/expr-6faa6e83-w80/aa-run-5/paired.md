# Paired stand, 2026-09-20 10:15

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 648896.9 | 121112.5 | 0.19x | 121153.1 | 0.19x | 0.0% | 113600 | 105600 | 11 % |  |
| tsql/script100.boolboth | generated | scriptdom | 648618.0 | 122491.4 | 0.19x | 119846.9 | 0.18x | -2.2% | 105603 | 105600 | 3 % |  |
| tsql/script100 | generated | scriptdom | 649194.5 | 122075.0 | 0.19x | 120470.3 | 0.19x | -1.3% | 113600 | 113600 | 1 % |  |
| tsql/columns1000 | generated | scriptdom | 1705340.6 | 187568.8 | 0.11x | 187407.8 | 0.11x | -0.1% | 200464 | 200464 | 4 % |  |
| sql/select20.at | generated | hand | 6827.5 | 17483.1 | 2.56x | 17791.4 | 2.61x | +1.8% | 21416 | 21441 | 17 % |  |
| sql/select20.window | generated | hand | 6823.7 | 18086.6 | 2.65x | 18334.6 | 2.69x | +1.4% | 21416 | 21440 | 15 % |  |
| sql/select20.scan | generated | control | 382.1 | 380.4 | 1.00x | 398.7 | 1.04x | +4.8% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 376.1 | 375.6 | 1.00x | 378.3 | 1.01x | +0.7% | 0 | 0 | 6 % |  |
| el/ladder.scan | generated | control | 137.8 | 136.1 | 0.99x | 136.5 | 0.99x | +0.3% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 960.8 | 1708.2 | 1.78x | 1663.5 | 1.73x | -2.6% | 1776 | 1720 | 7 % |  |
| sql/refused-late.bool | generated | hand | 2566.5 | 12212.0 | 4.76x | 5838.0 | 2.27x | -52.2% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6773.5 | 18023.4 | 2.66x | 18168.9 | 2.68x | +0.8% | 21448 | 21392 | 14 % |  |
| el/ladder | generated | hand | 954.3 | 1706.8 | 1.79x | 1682.5 | 1.76x | -1.4% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 954.3 | 1149.1 | 1.20x | 1142.8 | 1.20x | -0.6% | 1784 | 1784 | 4 % |  |
| el/terms100 | generated | hand | 11453.6 | 15856.8 | 1.38x | 15782.8 | 1.38x | -0.5% | 17904 | 17904 | 3 % |  |
| el/terms100 | immediate | hand | 11453.6 | 12742.2 | 1.11x | 12706.0 | 1.11x | -0.3% | 18720 | 18720 | 3 % |  |
| el/terms1000 | generated | hand | 110878.6 | 150960.7 | 1.36x | 148700.5 | 1.34x | -1.5% | 169104 | 169107 | 4 % |  |
| el/terms1000 | immediate | hand | 110878.6 | 122966.9 | 1.11x | 122812.3 | 1.11x | -0.1% | 177120 | 177120 | 4 % |  |
| sql/select20 | generated | hand | 6784.5 | 18029.4 | 2.66x | 18209.0 | 2.68x | +1.0% | 21448 | 21448 | 7 % |  |
| sql/refused-late | generated | hand | 2578.3 | 12108.7 | 4.70x | 12275.7 | 4.76x | +1.4% | 13552 | 13552 | 3 % |  |
