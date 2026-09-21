# Paired stand, 2026-09-21 05:38

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 163587.5 | 180578.1 | 1.10x | 182422.7 | 1.12x | +1.0% | 720048 | 720048 | 353 % |  |
| sql/select20.at | generated | hand | 6663.7 | 17328.7 | 2.60x | 16958.1 | 2.54x | -2.1% | 21416 | 21416 | 10 % |  |
| sql/select20.window | generated | hand | 6589.5 | 17811.8 | 2.70x | 17481.2 | 2.65x | -1.9% | 21416 | 21416 | 1 % |  |
| sql/select20.scan | generated | control | 365.6 | 365.0 | 1.00x | 369.4 | 1.01x | +1.2% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 367.5 | 370.9 | 1.01x | 368.9 | 1.00x | -0.5% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 138.2 | 133.0 | 0.96x | 132.6 | 0.96x | -0.3% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 934.9 | 1663.9 | 1.78x | 1634.3 | 1.75x | -1.8% | 1776 | 1720 | 12 % |  |
| sql/refused-late.bool | generated | hand | 2557.5 | 11984.2 | 4.69x | 5608.6 | 2.19x | -53.2% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6632.9 | 17807.3 | 2.68x | 17470.9 | 2.63x | -1.9% | 21448 | 21392 | 11 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4322787.5 | 4488550.0 | 1.04x | 4464662.5 | 1.03x | -0.5% | 6276664 | 6276685 | 34 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6240756.2 | 6303225.0 | 1.01x | 6304306.2 | 1.01x | 0.0% | 8778556 | 8778600 | 12 % |  |
| el/ladder | generated | hand | 937.3 | 1653.3 | 1.76x | 1646.9 | 1.76x | -0.4% | 1776 | 1776 | 17 % |  |
| el/ladder | immediate | hand | 937.3 | 1113.5 | 1.19x | 1118.2 | 1.19x | +0.4% | 1784 | 1784 | 17 % |  |
| el/terms1000 | generated | hand | 106994.5 | 146806.1 | 1.37x | 146053.4 | 1.37x | -0.5% | 169104 | 169131 | 13 % |  |
| el/terms1000 | immediate | hand | 106994.5 | 119130.8 | 1.11x | 121724.9 | 1.14x | +2.2% | 177120 | 177120 | 13 % |  |
| sql/select20 | generated | hand | 6651.4 | 17666.7 | 2.66x | 17555.4 | 2.64x | -0.6% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2566.0 | 11949.0 | 4.66x | 11856.9 | 4.62x | -0.8% | 13552 | 13552 | 7 % |  |
