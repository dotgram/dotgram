# Paired stand, 2026-09-21 05:10

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164630.5 | 183404.7 | 1.11x | 183227.3 | 1.11x | -0.1% | 720048 | 720048 | 6 % |  |
| sql/select20.at | generated | hand | 6654.4 | 17007.6 | 2.56x | 16940.5 | 2.55x | -0.4% | 21416 | 21416 | 34 % |  |
| sql/select20.window | generated | hand | 6572.4 | 17431.3 | 2.65x | 18659.9 | 2.84x | +7.0% | 21416 | 21416 | 10 % |  |
| sql/select20.scan | generated | control | 367.5 | 367.7 | 1.00x | 369.8 | 1.01x | +0.6% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 369.0 | 367.2 | 1.00x | 367.0 | 0.99x | -0.1% | 0 | 0 | 27 % |  |
| el/ladder.scan | generated | control | 134.5 | 135.6 | 1.01x | 136.1 | 1.01x | +0.4% | 0 | 0 | 5 % |  |
| el/ladder.bool | generated | hand | 956.4 | 1645.6 | 1.72x | 1633.9 | 1.71x | -0.7% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2510.3 | 11788.8 | 4.70x | 5642.0 | 2.25x | -52.1% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6704.7 | 17492.1 | 2.61x | 17581.2 | 2.62x | +0.5% | 21448 | 21392 | 2 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4347700.0 | 4370343.8 | 1.01x | 4400350.0 | 1.01x | +0.7% | 6276619 | 6276598 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6317193.8 | 6407650.0 | 1.01x | 6376568.8 | 1.01x | -0.5% | 8778556 | 8778600 | 18 % |  |
| el/ladder | generated | hand | 971.3 | 1643.6 | 1.69x | 1653.1 | 1.70x | +0.6% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 971.3 | 1152.3 | 1.19x | 1150.6 | 1.18x | -0.1% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 110363.8 | 149274.0 | 1.35x | 149624.3 | 1.36x | +0.2% | 169150 | 169128 | 4 % |  |
| el/terms1000 | immediate | hand | 110363.8 | 122664.0 | 1.11x | 123752.1 | 1.12x | +0.9% | 177120 | 177120 | 4 % |  |
| sql/select20 | generated | hand | 6636.9 | 17558.1 | 2.65x | 17306.2 | 2.61x | -1.4% | 21448 | 21448 | 17 % |  |
| sql/refused-late | generated | hand | 2513.2 | 11733.5 | 4.67x | 11840.2 | 4.71x | +0.9% | 13552 | 13552 | 14 % |  |
