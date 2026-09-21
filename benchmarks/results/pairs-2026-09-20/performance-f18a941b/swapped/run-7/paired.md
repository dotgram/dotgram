# Paired stand, 2026-09-21 05:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164038.3 | 181778.9 | 1.11x | 183158.6 | 1.12x | +0.8% | 720048 | 720048 | 22 % |  |
| sql/select20.at | generated | hand | 6677.6 | 16898.4 | 2.53x | 17028.3 | 2.55x | +0.8% | 21416 | 21416 | 20 % |  |
| sql/select20.window | generated | hand | 6636.1 | 17291.4 | 2.61x | 17498.6 | 2.64x | +1.2% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 371.2 | 370.8 | 1.00x | 375.6 | 1.01x | +1.3% | 0 | 0 | 20 % |  |
| tsql/select20.scan | generated | control | 372.2 | 373.4 | 1.00x | 386.7 | 1.04x | +3.6% | 0 | 0 | 8 % |  |
| el/ladder.scan | generated | control | 133.9 | 135.7 | 1.01x | 133.9 | 1.00x | -1.3% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 921.5 | 1643.6 | 1.78x | 1609.7 | 1.75x | -2.1% | 1776 | 1720 | 9 % |  |
| sql/refused-late.bool | generated | hand | 2493.8 | 11638.1 | 4.67x | 5652.7 | 2.27x | -51.4% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6656.8 | 17386.9 | 2.61x | 17466.2 | 2.62x | +0.5% | 21448 | 21392 | 14 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4327168.8 | 4365981.2 | 1.01x | 4476450.0 | 1.03x | +2.5% | 6276684 | 6276684 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6212281.2 | 6196137.5 | 1.00x | 6433243.8 | 1.04x | +3.8% | 8778600 | 8778600 | 17 % |  |
| el/ladder | generated | hand | 930.2 | 1657.1 | 1.78x | 1667.6 | 1.79x | +0.6% | 1776 | 1776 | 7 % |  |
| el/ladder | immediate | hand | 930.2 | 1167.9 | 1.26x | 1151.3 | 1.24x | -1.4% | 1784 | 1784 | 7 % |  |
| el/terms1000 | generated | hand | 108831.0 | 148706.0 | 1.37x | 146320.3 | 1.34x | -1.6% | 169107 | 169104 | 2 % |  |
| el/terms1000 | immediate | hand | 108831.0 | 121120.9 | 1.11x | 120671.6 | 1.11x | -0.4% | 177120 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6673.1 | 17269.6 | 2.59x | 17603.0 | 2.64x | +1.9% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2482.3 | 11675.7 | 4.70x | 11976.8 | 4.82x | +2.6% | 13552 | 13552 | 5 % |  |
