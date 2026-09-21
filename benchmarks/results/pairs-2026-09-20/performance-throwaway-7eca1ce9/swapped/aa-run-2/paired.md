# Paired stand, 2026-09-21 03:33

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6686.4 | 17019.9 | 2.55x | 17139.6 | 2.56x | +0.7% | 21416 | 21441 | 437 % |  |
| sql/select20.window | generated | hand | 6584.4 | 17254.1 | 2.62x | 17426.0 | 2.65x | +1.0% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 370.6 | 366.0 | 0.99x | 371.8 | 1.00x | +1.6% | 0 | 0 | 14 % |  |
| tsql/select20.scan | generated | control | 367.7 | 371.8 | 1.01x | 370.2 | 1.01x | -0.4% | 0 | 0 | 7 % |  |
| el/ladder.scan | generated | control | 135.3 | 135.2 | 1.00x | 134.6 | 0.99x | -0.5% | 0 | 0 | 13 % |  |
| el/ladder.bool | generated | hand | 930.5 | 1657.4 | 1.78x | 1622.0 | 1.74x | -2.1% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2492.0 | 11748.3 | 4.71x | 5693.3 | 2.28x | -51.5% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6616.2 | 17363.6 | 2.62x | 17591.8 | 2.66x | +1.3% | 21448 | 21392 | 9 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6215600.0 | 6574150.0 | 1.06x | 6312550.0 | 1.02x | -4.0% | 8778576 | 8778600 | 49 % |  |
| el/ladder | generated | hand | 933.9 | 1663.2 | 1.78x | 1668.9 | 1.79x | +0.3% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 933.9 | 1110.2 | 1.19x | 1113.8 | 1.19x | +0.3% | 1784 | 1784 | 2 % |  |
| sql/select20 | generated | hand | 6685.6 | 17388.5 | 2.60x | 17414.7 | 2.60x | +0.2% | 21448 | 21448 | 9 % |  |
| sql/refused-late | generated | hand | 2525.5 | 11766.8 | 4.66x | 11793.1 | 4.67x | +0.2% | 13552 | 13552 | 9 % |  |
