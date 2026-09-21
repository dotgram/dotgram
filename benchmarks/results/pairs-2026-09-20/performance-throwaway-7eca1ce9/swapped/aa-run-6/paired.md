# Paired stand, 2026-09-21 03:48

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6717.3 | 16953.6 | 2.52x | 16977.1 | 2.53x | +0.1% | 21416 | 21441 | 443 % |  |
| sql/select20.window | generated | hand | 6709.6 | 17428.9 | 2.60x | 17294.3 | 2.58x | -0.8% | 21416 | 21416 | 17 % |  |
| sql/select20.scan | generated | control | 364.4 | 367.2 | 1.01x | 370.4 | 1.02x | +0.8% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 368.0 | 369.5 | 1.00x | 417.2 | 1.13x | +12.9% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 134.2 | 134.4 | 1.00x | 133.8 | 1.00x | -0.5% | 0 | 0 | 2 % |  |
| el/ladder.bool | generated | hand | 948.2 | 1713.8 | 1.81x | 1609.4 | 1.70x | -6.1% | 1776 | 1720 | 9 % |  |
| sql/refused-late.bool | generated | hand | 2520.8 | 11655.2 | 4.62x | 5752.3 | 2.28x | -50.6% | 13552 | 6616 | 12 % |  |
| sql/select20.bool | generated | hand | 6679.6 | 17283.4 | 2.59x | 18303.0 | 2.74x | +5.9% | 21448 | 21392 | 12 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6110000.0 | 6216150.0 | 1.02x | 6247600.0 | 1.02x | +0.5% | 8778600 | 8778360 | 47 % |  |
| el/ladder | generated | hand | 946.2 | 1716.3 | 1.81x | 1645.0 | 1.74x | -4.2% | 1776 | 1776 | 22 % |  |
| el/ladder | immediate | hand | 946.2 | 1111.9 | 1.18x | 1126.1 | 1.19x | +1.3% | 1784 | 1784 | 22 % |  |
| sql/select20 | generated | hand | 6709.4 | 17380.2 | 2.59x | 17416.1 | 2.60x | +0.2% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2543.2 | 11647.0 | 4.58x | 11950.0 | 4.70x | +2.6% | 13552 | 13552 | 2 % |  |
