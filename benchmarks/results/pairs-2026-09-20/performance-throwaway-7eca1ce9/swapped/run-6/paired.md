# Paired stand, 2026-09-21 03:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6708.5 | 17047.3 | 2.54x | 17670.2 | 2.63x | +3.7% | 21416 | 21441 | 431 % |  |
| sql/select20.window | generated | hand | 6686.2 | 17367.5 | 2.60x | 18226.8 | 2.73x | +4.9% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 375.1 | 373.1 | 0.99x | 370.2 | 0.99x | -0.8% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 368.7 | 371.1 | 1.01x | 376.0 | 1.02x | +1.3% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 135.6 | 134.4 | 0.99x | 134.6 | 0.99x | +0.1% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 947.6 | 1665.7 | 1.76x | 1652.0 | 1.74x | -0.8% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2509.8 | 11805.6 | 4.70x | 5876.3 | 2.34x | -50.2% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6651.3 | 17420.3 | 2.62x | 18063.7 | 2.72x | +3.7% | 21448 | 21392 | 8 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6148500.0 | 7143400.0 | 1.16x | 6516800.0 | 1.06x | -8.8% | 8778600 | 8778600 | 41 % |  |
| el/ladder | generated | hand | 947.8 | 1657.9 | 1.75x | 1634.4 | 1.72x | -1.4% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 947.8 | 1127.2 | 1.19x | 1125.7 | 1.19x | -0.1% | 1784 | 1784 | 4 % |  |
| sql/select20 | generated | hand | 6670.9 | 17251.2 | 2.59x | 18313.2 | 2.75x | +6.2% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2521.6 | 11788.3 | 4.67x | 12256.4 | 4.86x | +4.0% | 13552 | 13552 | 2 % |  |
