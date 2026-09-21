# Paired stand, 2026-09-21 05:23

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164918.0 | 184159.4 | 1.12x | 183263.3 | 1.11x | -0.5% | 720048 | 720048 | 21 % |  |
| sql/select20.at | generated | hand | 6769.7 | 16965.2 | 2.51x | 17750.8 | 2.62x | +4.6% | 21416 | 21416 | 21 % |  |
| sql/select20.window | generated | hand | 6590.0 | 17382.1 | 2.64x | 18247.7 | 2.77x | +5.0% | 21416 | 21416 | 7 % |  |
| sql/select20.scan | generated | control | 366.5 | 367.5 | 1.00x | 369.7 | 1.01x | +0.6% | 0 | 0 | 1 % |  |
| tsql/select20.scan | generated | control | 372.9 | 365.9 | 0.98x | 368.5 | 0.99x | +0.7% | 0 | 0 | 6 % |  |
| el/ladder.scan | generated | control | 137.2 | 134.6 | 0.98x | 133.1 | 0.97x | -1.1% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 973.2 | 1661.9 | 1.71x | 1698.4 | 1.75x | +2.2% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2474.0 | 11734.0 | 4.74x | 5754.3 | 2.33x | -51.0% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6594.8 | 17306.8 | 2.62x | 18404.3 | 2.79x | +6.3% | 21448 | 21392 | 10 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4436837.5 | 4402037.5 | 0.99x | 4500287.5 | 1.01x | +2.2% | 6276555 | 6276598 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6437450.0 | 6283012.5 | 0.98x | 6424250.0 | 1.00x | +2.2% | 8778619 | 8778600 | 5 % |  |
| el/ladder | generated | hand | 972.0 | 1665.6 | 1.71x | 1687.7 | 1.74x | +1.3% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 972.0 | 1138.3 | 1.17x | 1121.8 | 1.15x | -1.5% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 108295.1 | 147348.0 | 1.36x | 148511.7 | 1.37x | +0.8% | 169104 | 169104 | 13 % |  |
| el/terms1000 | immediate | hand | 108295.1 | 126138.0 | 1.16x | 123023.6 | 1.14x | -2.5% | 177166 | 177144 | 13 % |  |
| sql/select20 | generated | hand | 6575.3 | 17295.0 | 2.63x | 18189.4 | 2.77x | +5.2% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2481.4 | 11699.4 | 4.71x | 12066.9 | 4.86x | +3.1% | 13552 | 13552 | 15 % |  |
