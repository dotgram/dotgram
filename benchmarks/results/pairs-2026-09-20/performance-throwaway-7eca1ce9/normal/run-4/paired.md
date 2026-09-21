# Paired stand, 2026-09-21 03:04

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6707.6 | 17039.3 | 2.54x | 17325.1 | 2.58x | +1.7% | 21416 | 21441 | 433 % |  |
| sql/select20.window | generated | hand | 6675.5 | 17564.9 | 2.63x | 17626.2 | 2.64x | +0.3% | 21416 | 21440 | 5 % |  |
| sql/select20.scan | generated | control | 376.7 | 382.4 | 1.01x | 374.0 | 0.99x | -2.2% | 0 | 0 | 17 % |  |
| tsql/select20.scan | generated | control | 371.4 | 367.9 | 0.99x | 368.7 | 0.99x | +0.2% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 135.5 | 133.0 | 0.98x | 135.1 | 1.00x | +1.6% | 0 | 0 | 21 % |  |
| el/ladder.bool | generated | hand | 935.7 | 1670.9 | 1.79x | 1619.8 | 1.73x | -3.1% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2486.7 | 11761.2 | 4.73x | 5767.6 | 2.32x | -51.0% | 13552 | 6616 | 8 % |  |
| sql/select20.bool | generated | hand | 6669.5 | 17400.8 | 2.61x | 17689.3 | 2.65x | +1.7% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6188750.0 | 6463450.0 | 1.04x | 6276950.0 | 1.01x | -2.9% | 8778600 | 8778600 | 48 % |  |
| el/ladder | generated | hand | 940.0 | 1675.6 | 1.78x | 1638.8 | 1.74x | -2.2% | 1776 | 1776 | 14 % |  |
| el/ladder | immediate | hand | 940.0 | 1152.5 | 1.23x | 1129.0 | 1.20x | -2.0% | 1784 | 1784 | 14 % |  |
| sql/select20 | generated | hand | 6677.3 | 17494.5 | 2.62x | 17826.7 | 2.67x | +1.9% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2498.5 | 11693.4 | 4.68x | 11893.0 | 4.76x | +1.7% | 13552 | 13552 | 3 % |  |
