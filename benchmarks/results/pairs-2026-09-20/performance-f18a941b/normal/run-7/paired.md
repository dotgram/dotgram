# Paired stand, 2026-09-21 05:20

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165032.0 | 182288.3 | 1.10x | 182911.7 | 1.11x | +0.3% | 720048 | 720048 | 20 % |  |
| sql/select20.at | generated | hand | 6670.0 | 17204.9 | 2.58x | 17356.8 | 2.60x | +0.9% | 21416 | 21416 | 10 % |  |
| sql/select20.window | generated | hand | 6642.8 | 17483.1 | 2.63x | 17843.3 | 2.69x | +2.1% | 21416 | 21416 | 6 % |  |
| sql/select20.scan | generated | control | 365.5 | 385.2 | 1.05x | 371.7 | 1.02x | -3.5% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 366.9 | 374.2 | 1.02x | 377.3 | 1.03x | +0.8% | 0 | 0 | 1 % |  |
| el/ladder.scan | generated | control | 135.2 | 136.3 | 1.01x | 138.9 | 1.03x | +1.9% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 952.8 | 1654.1 | 1.74x | 1643.6 | 1.72x | -0.6% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2489.3 | 11934.7 | 4.79x | 5758.9 | 2.31x | -51.7% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6608.6 | 17645.2 | 2.67x | 17904.6 | 2.71x | +1.5% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4363931.2 | 4365362.5 | 1.00x | 4408412.5 | 1.01x | +1.0% | 6276660 | 6276684 | 7 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6307675.0 | 6279637.5 | 1.00x | 6385481.2 | 1.01x | +1.7% | 8778556 | 8778600 | 17 % |  |
| el/ladder | generated | hand | 947.2 | 1666.6 | 1.76x | 1657.0 | 1.75x | -0.6% | 1776 | 1776 | 15 % |  |
| el/ladder | immediate | hand | 947.2 | 1134.0 | 1.20x | 1125.1 | 1.19x | -0.8% | 1784 | 1784 | 15 % |  |
| el/terms1000 | generated | hand | 107218.5 | 147619.2 | 1.38x | 145153.7 | 1.35x | -1.7% | 169104 | 169104 | 13 % |  |
| el/terms1000 | immediate | hand | 107218.5 | 119313.4 | 1.11x | 120355.6 | 1.12x | +0.9% | 177166 | 177144 | 13 % |  |
| sql/select20 | generated | hand | 6618.1 | 17512.3 | 2.65x | 17772.8 | 2.69x | +1.5% | 21448 | 21448 | 9 % |  |
| sql/refused-late | generated | hand | 2496.1 | 11905.8 | 4.77x | 12029.7 | 4.82x | +1.0% | 13552 | 13552 | 14 % |  |
