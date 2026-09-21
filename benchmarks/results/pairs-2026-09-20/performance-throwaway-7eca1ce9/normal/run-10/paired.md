# Paired stand, 2026-09-21 03:25

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6723.3 | 17119.9 | 2.55x | 17157.9 | 2.55x | +0.2% | 21416 | 21441 | 430 % |  |
| sql/select20.window | generated | hand | 6674.2 | 17541.1 | 2.63x | 17717.2 | 2.65x | +1.0% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 371.2 | 368.5 | 0.99x | 373.3 | 1.01x | +1.3% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 380.0 | 368.2 | 0.97x | 370.0 | 0.97x | +0.5% | 0 | 0 | 11 % |  |
| el/ladder.scan | generated | control | 138.3 | 133.7 | 0.97x | 135.8 | 0.98x | +1.6% | 0 | 0 | 12 % |  |
| el/ladder.bool | generated | hand | 986.8 | 1638.2 | 1.66x | 1628.8 | 1.65x | -0.6% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2480.5 | 11787.5 | 4.75x | 5716.4 | 2.30x | -51.5% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6578.5 | 17493.8 | 2.66x | 17592.6 | 2.67x | +0.6% | 21448 | 21392 | 9 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6398800.0 | 6261000.0 | 0.98x | 6236550.0 | 0.97x | -0.4% | 8778600 | 8778600 | 40 % |  |
| el/ladder | generated | hand | 973.9 | 1645.9 | 1.69x | 1636.7 | 1.68x | -0.6% | 1776 | 1776 | 7 % |  |
| el/ladder | immediate | hand | 973.9 | 1140.9 | 1.17x | 1122.2 | 1.15x | -1.6% | 1784 | 1784 | 7 % |  |
| sql/select20 | generated | hand | 6604.5 | 17458.9 | 2.64x | 17641.2 | 2.67x | +1.0% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2491.8 | 11827.2 | 4.75x | 11843.8 | 4.75x | +0.1% | 13552 | 13552 | 4 % |  |
