# Paired stand, 2026-09-21 07:05

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 168500.0 | 184271.1 | 1.09x | 184711.7 | 1.10x | +0.2% | 720048 | 720048 | 163 % |  |
| sql/select20.at | generated | hand | 6699.0 | 17283.5 | 2.58x | 17579.6 | 2.62x | +1.7% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6700.8 | 17667.9 | 2.64x | 17999.4 | 2.69x | +1.9% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 380.4 | 364.3 | 0.96x | 370.3 | 0.97x | +1.7% | 0 | 0 | 20 % |  |
| tsql/select20.scan | generated | control | 367.1 | 372.4 | 1.01x | 369.6 | 1.01x | -0.7% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 136.7 | 132.8 | 0.97x | 132.6 | 0.97x | -0.1% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 960.0 | 1677.9 | 1.75x | 1665.3 | 1.73x | -0.8% | 1776 | 1720 | 10 % |  |
| sql/refused-late.bool | generated | hand | 2539.4 | 11886.5 | 4.68x | 5811.5 | 2.29x | -51.1% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6838.5 | 17932.9 | 2.62x | 18386.9 | 2.69x | +2.5% | 21448 | 21392 | 21 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4427725.0 | 4455812.5 | 1.01x | 4489325.0 | 1.01x | +0.8% | 6276555 | 6276660 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6475800.0 | 6312850.0 | 0.97x | 6326387.5 | 0.98x | +0.2% | 8778556 | 8778595 | 12 % |  |
| el/ladder | generated | hand | 970.7 | 1690.1 | 1.74x | 1692.5 | 1.74x | +0.1% | 1776 | 1776 | 5 % |  |
| el/ladder | immediate | hand | 970.7 | 1135.2 | 1.17x | 1136.0 | 1.17x | +0.1% | 1784 | 1784 | 5 % |  |
| el/terms1000 | generated | hand | 107891.9 | 146554.5 | 1.36x | 147269.1 | 1.36x | +0.5% | 169104 | 169107 | 3 % |  |
| el/terms1000 | immediate | hand | 107891.9 | 119932.1 | 1.11x | 121610.9 | 1.13x | +1.4% | 177120 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6768.3 | 17718.1 | 2.62x | 18048.1 | 2.67x | +1.9% | 21448 | 21448 | 17 % |  |
| sql/refused-late | generated | hand | 2528.9 | 11877.4 | 4.70x | 12027.8 | 4.76x | +1.3% | 13552 | 13552 | 3 % |  |
