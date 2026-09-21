# Paired stand, 2026-09-21 03:18

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6706.9 | 17054.0 | 2.54x | 17376.4 | 2.59x | +1.9% | 21416 | 21416 | 412 % |  |
| sql/select20.window | generated | hand | 6645.6 | 17426.6 | 2.62x | 17602.3 | 2.65x | +1.0% | 21416 | 21416 | 6 % |  |
| sql/select20.scan | generated | control | 371.2 | 369.1 | 0.99x | 377.1 | 1.02x | +2.2% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 367.8 | 374.9 | 1.02x | 371.1 | 1.01x | -1.0% | 0 | 0 | 10 % |  |
| el/ladder.scan | generated | control | 135.1 | 134.8 | 1.00x | 134.1 | 0.99x | -0.5% | 0 | 0 | 2 % |  |
| el/ladder.bool | generated | hand | 938.4 | 1669.9 | 1.78x | 1648.1 | 1.76x | -1.3% | 1776 | 1720 | 2 % |  |
| sql/refused-late.bool | generated | hand | 2540.1 | 11697.4 | 4.61x | 5700.3 | 2.24x | -51.3% | 13552 | 6616 | 14 % |  |
| sql/select20.bool | generated | hand | 6782.4 | 17493.7 | 2.58x | 17738.1 | 2.62x | +1.4% | 21448 | 21392 | 11 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6532550.0 | 6318500.0 | 0.97x | 6275150.0 | 0.96x | -0.7% | 8778600 | 8778600 | 46 % |  |
| el/ladder | generated | hand | 941.5 | 1671.0 | 1.77x | 1677.3 | 1.78x | +0.4% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 941.5 | 1125.3 | 1.20x | 1139.6 | 1.21x | +1.3% | 1784 | 1784 | 3 % |  |
| sql/select20 | generated | hand | 6808.0 | 17464.2 | 2.57x | 17697.3 | 2.60x | +1.3% | 21448 | 21448 | 26 % |  |
| sql/refused-late | generated | hand | 2546.6 | 11706.5 | 4.60x | 11849.2 | 4.65x | +1.2% | 13552 | 13552 | 2 % |  |
