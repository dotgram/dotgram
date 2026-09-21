# Paired stand, 2026-09-21 05:39

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165275.8 | 181400.0 | 1.10x | 183805.5 | 1.11x | +1.3% | 720048 | 720048 | 350 % |  |
| sql/select20.at | generated | hand | 6669.9 | 17073.6 | 2.56x | 17160.3 | 2.57x | +0.5% | 21416 | 21441 | 6 % |  |
| sql/select20.window | generated | hand | 6603.1 | 17313.7 | 2.62x | 17373.6 | 2.63x | +0.3% | 21416 | 21416 | 6 % |  |
| sql/select20.scan | generated | control | 377.3 | 372.1 | 0.99x | 369.3 | 0.98x | -0.7% | 0 | 0 | 15 % |  |
| tsql/select20.scan | generated | control | 371.4 | 369.3 | 0.99x | 368.5 | 0.99x | -0.2% | 0 | 0 | 13 % |  |
| el/ladder.scan | generated | control | 134.2 | 134.2 | 1.00x | 134.1 | 1.00x | -0.1% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 943.8 | 1688.9 | 1.79x | 1641.4 | 1.74x | -2.8% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2492.1 | 11697.8 | 4.69x | 5659.3 | 2.27x | -51.6% | 13552 | 6616 | 1 % |  |
| sql/select20.bool | generated | hand | 6702.8 | 17508.1 | 2.61x | 17567.1 | 2.62x | +0.3% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4400112.5 | 4603837.5 | 1.05x | 4488125.0 | 1.02x | -2.5% | 6276641 | 6276617 | 44 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6378193.8 | 6325187.5 | 0.99x | 6301631.2 | 0.99x | -0.4% | 8778600 | 8778619 | 8 % |  |
| el/ladder | generated | hand | 941.0 | 1687.3 | 1.79x | 1688.6 | 1.79x | +0.1% | 1776 | 1776 | 7 % |  |
| el/ladder | immediate | hand | 941.0 | 1149.0 | 1.22x | 1130.1 | 1.20x | -1.6% | 1784 | 1784 | 7 % |  |
| el/terms1000 | generated | hand | 105595.6 | 148530.7 | 1.41x | 143708.7 | 1.36x | -3.2% | 169107 | 169104 | 23 % |  |
| el/terms1000 | immediate | hand | 105595.6 | 121268.3 | 1.15x | 118507.9 | 1.12x | -2.3% | 177120 | 177120 | 23 % |  |
| sql/select20 | generated | hand | 6622.4 | 17416.4 | 2.63x | 17446.8 | 2.63x | +0.2% | 21448 | 21448 | 5 % |  |
| sql/refused-late | generated | hand | 2490.1 | 11731.2 | 4.71x | 11702.1 | 4.70x | -0.2% | 13552 | 13552 | 10 % |  |
