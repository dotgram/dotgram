# Paired stand, 2026-09-21 03:50

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6761.4 | 17527.6 | 2.59x | 17097.4 | 2.53x | -2.5% | 21416 | 21460 | 388 % |  |
| sql/select20.window | generated | hand | 6617.4 | 17497.0 | 2.64x | 17485.0 | 2.64x | -0.1% | 21416 | 21440 | 2 % |  |
| sql/select20.scan | generated | control | 377.0 | 373.2 | 0.99x | 371.5 | 0.99x | -0.5% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 366.6 | 376.1 | 1.03x | 378.9 | 1.03x | +0.8% | 0 | 0 | 13 % |  |
| el/ladder.scan | generated | control | 136.2 | 135.8 | 1.00x | 135.1 | 0.99x | -0.5% | 0 | 0 | 5 % |  |
| el/ladder.bool | generated | hand | 953.9 | 1642.8 | 1.72x | 1636.8 | 1.72x | -0.4% | 1776 | 1720 | 7 % |  |
| sql/refused-late.bool | generated | hand | 2490.4 | 11854.3 | 4.76x | 5700.7 | 2.29x | -51.9% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6687.7 | 17431.0 | 2.61x | 17369.8 | 2.60x | -0.4% | 21448 | 21392 | 19 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6114300.0 | 6313150.0 | 1.03x | 6637500.0 | 1.09x | +5.1% | 8778600 | 8778556 | 46 % |  |
| el/ladder | generated | hand | 943.4 | 1644.9 | 1.74x | 1664.0 | 1.76x | +1.2% | 1776 | 1776 | 12 % |  |
| el/ladder | immediate | hand | 943.4 | 1142.2 | 1.21x | 1118.8 | 1.19x | -2.1% | 1784 | 1784 | 12 % |  |
| sql/select20 | generated | hand | 6672.6 | 17390.4 | 2.61x | 17461.5 | 2.62x | +0.4% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2498.3 | 11844.9 | 4.74x | 11895.7 | 4.76x | +0.4% | 13552 | 13552 | 8 % |  |
