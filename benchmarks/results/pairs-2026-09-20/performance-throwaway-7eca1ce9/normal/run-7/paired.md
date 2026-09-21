# Paired stand, 2026-09-21 03:14

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6666.1 | 17152.4 | 2.57x | 17274.5 | 2.59x | +0.7% | 21416 | 21441 | 3 % |  |
| sql/select20.window | generated | hand | 6610.7 | 17456.4 | 2.64x | 17559.6 | 2.66x | +0.6% | 21416 | 21440 | 18 % |  |
| sql/select20.scan | generated | control | 372.0 | 368.2 | 0.99x | 367.5 | 0.99x | -0.2% | 0 | 0 | 12 % |  |
| tsql/select20.scan | generated | control | 373.5 | 369.6 | 0.99x | 376.1 | 1.01x | +1.8% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 134.7 | 134.6 | 1.00x | 133.8 | 0.99x | -0.6% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 932.1 | 1668.6 | 1.79x | 1617.0 | 1.73x | -3.1% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2503.4 | 11970.8 | 4.78x | 5644.7 | 2.25x | -52.8% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6625.2 | 17813.6 | 2.69x | 17492.8 | 2.64x | -1.8% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6279525.0 | 6050050.0 | 0.96x | 6250925.0 | 1.00x | +3.3% | 8778360 | 8778556 | 30 % |  |
| el/ladder | generated | hand | 933.8 | 1672.1 | 1.79x | 1628.2 | 1.74x | -2.6% | 1776 | 1776 | 23 % |  |
| el/ladder | immediate | hand | 933.8 | 1119.4 | 1.20x | 1128.5 | 1.21x | +0.8% | 1784 | 1784 | 23 % |  |
| sql/select20 | generated | hand | 6687.4 | 17904.2 | 2.68x | 17511.9 | 2.62x | -2.2% | 21448 | 21448 | 8 % |  |
| sql/refused-late | generated | hand | 2518.3 | 11992.7 | 4.76x | 11802.8 | 4.69x | -1.6% | 13552 | 13552 | 17 % |  |
