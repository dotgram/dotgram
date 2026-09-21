# Paired stand, 2026-09-21 02:29

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 253440.6 | 221962.5 | 0.88x | 217785.9 | 0.86x | -1.9% | 403288 | 403288 | 17 % |  |
| tsql/columns1000 | generated | scriptdom | 1973025.0 | 180381.2 | 0.09x | 219093.8 | 0.11x | +21.5% | 200464 | 200464 | 56 % |  |
| web/json.array10000 | generated | hand | 169468.8 | 186353.1 | 1.10x | 184313.3 | 1.09x | -1.1% | 720048 | 720048 | 51 % |  |
| sql/select20.at | generated | hand | 6652.9 | 17454.1 | 2.62x | 17652.7 | 2.65x | +1.1% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6638.6 | 17877.4 | 2.69x | 17788.4 | 2.68x | -0.5% | 21416 | 21416 | 18 % |  |
| sql/select20.scan | generated | control | 372.4 | 372.8 | 1.00x | 377.0 | 1.01x | +1.1% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 366.2 | 372.1 | 1.02x | 372.9 | 1.02x | +0.2% | 0 | 0 | 12 % |  |
| el/ladder.scan | generated | control | 135.6 | 135.5 | 1.00x | 135.5 | 1.00x | 0.0% | 0 | 0 | 15 % |  |
| el/ladder.bool | generated | hand | 955.3 | 1733.7 | 1.81x | 1652.1 | 1.73x | -4.7% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2544.3 | 11749.5 | 4.62x | 5750.9 | 2.26x | -51.1% | 13552 | 6616 | 22 % |  |
| sql/select20.bool | generated | hand | 6627.1 | 17661.6 | 2.67x | 17933.4 | 2.71x | +1.5% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4404975.0 | 4420000.0 | 1.00x | 4347850.0 | 0.99x | -1.6% | 6276686 | 6276684 | 50 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2479196.9 | 2518121.9 | 1.02x | 2508862.5 | 1.01x | -0.4% | 2994123 | 2994142 | 8 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6203200.0 | 6239043.8 | 1.01x | 6304025.0 | 1.02x | +1.0% | 8778600 | 8778600 | 27 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6218400.0 | 6337912.5 | 1.02x | 6264856.2 | 1.01x | -1.2% | 8775404 | 8775448 | 6 % |  |
| fix/Orders128.yield-string | generated | hand | 79957.5 | 266132.2 | 3.33x | 245033.0 | 3.06x | -7.9% | 117936 | 117936 | 5 % |  |
| web/media-type.quoted | generated | control | 243.8 | 281.1 | 1.15x | 279.5 | 1.15x | -0.6% | 816 | 816 | 7 % |  |
| el/ladder | generated | hand | 960.4 | 1731.9 | 1.80x | 1669.4 | 1.74x | -3.6% | 1776 | 1776 | 16 % |  |
| el/ladder | immediate | hand | 960.4 | 1134.0 | 1.18x | 1159.5 | 1.21x | +2.2% | 1784 | 1784 | 16 % |  |
| el/terms1000 | generated | hand | 107657.2 | 157646.4 | 1.46x | 151855.5 | 1.41x | -3.7% | 169104 | 169131 | 4 % |  |
| el/terms1000 | immediate | hand | 107657.2 | 125025.5 | 1.16x | 121534.5 | 1.13x | -2.8% | 177120 | 177120 | 4 % |  |
| sql/select20 | generated | hand | 6657.6 | 17702.9 | 2.66x | 17740.9 | 2.66x | +0.2% | 21448 | 21448 | 59 % |  |
| sql/refused-late | generated | hand | 2523.4 | 11761.3 | 4.66x | 11816.0 | 4.68x | +0.5% | 13552 | 13552 | 2 % |  |
