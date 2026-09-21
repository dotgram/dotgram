# Paired stand, 2026-09-21 03:19

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6711.8 | 17181.4 | 2.56x | 17055.9 | 2.54x | -0.7% | 21416 | 21416 | 441 % |  |
| sql/select20.window | generated | hand | 6676.2 | 17326.3 | 2.60x | 17265.9 | 2.59x | -0.3% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 365.6 | 376.8 | 1.03x | 372.2 | 1.02x | -1.2% | 0 | 0 | 12 % |  |
| tsql/select20.scan | generated | control | 366.4 | 371.9 | 1.01x | 373.8 | 1.02x | +0.5% | 0 | 0 | 25 % |  |
| el/ladder.scan | generated | control | 134.5 | 136.4 | 1.01x | 137.0 | 1.02x | +0.5% | 0 | 0 | 2 % |  |
| el/ladder.bool | generated | hand | 961.0 | 1642.9 | 1.71x | 1684.1 | 1.75x | +2.5% | 1776 | 1720 | 22 % |  |
| sql/refused-late.bool | generated | hand | 2514.2 | 11728.1 | 4.66x | 5606.0 | 2.23x | -52.2% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6724.3 | 17415.0 | 2.59x | 17347.7 | 2.58x | -0.4% | 21448 | 21392 | 10 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6225800.0 | 6487950.0 | 1.04x | 6283950.0 | 1.01x | -3.1% | 8778595 | 8778556 | 43 % |  |
| el/ladder | generated | hand | 939.7 | 1633.3 | 1.74x | 1680.2 | 1.79x | +2.9% | 1776 | 1776 | 20 % |  |
| el/ladder | immediate | hand | 939.7 | 1135.9 | 1.21x | 1137.5 | 1.21x | +0.1% | 1784 | 1784 | 20 % |  |
| sql/select20 | generated | hand | 6699.1 | 17432.5 | 2.60x | 17316.0 | 2.58x | -0.7% | 21448 | 21448 | 10 % |  |
| sql/refused-late | generated | hand | 2510.8 | 11719.2 | 4.67x | 11651.4 | 4.64x | -0.6% | 13552 | 13552 | 2 % |  |
