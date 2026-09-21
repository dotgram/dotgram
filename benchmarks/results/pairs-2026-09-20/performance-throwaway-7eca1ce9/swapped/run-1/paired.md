# Paired stand, 2026-09-21 03:28

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.4 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6820.1 | 17097.3 | 2.51x | 17577.8 | 2.58x | +2.8% | 21416 | 21441 | 432 % |  |
| sql/select20.window | generated | hand | 6680.0 | 17473.9 | 2.62x | 17820.0 | 2.67x | +2.0% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 367.7 | 374.7 | 1.02x | 373.2 | 1.01x | -0.4% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 368.9 | 372.0 | 1.01x | 377.0 | 1.02x | +1.3% | 0 | 0 | 1 % |  |
| el/ladder.scan | generated | control | 136.1 | 138.0 | 1.01x | 138.3 | 1.02x | +0.2% | 0 | 0 | 16 % |  |
| el/ladder.bool | generated | hand | 953.6 | 1676.5 | 1.76x | 1655.9 | 1.74x | -1.2% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2496.2 | 11526.5 | 4.62x | 5729.2 | 2.30x | -50.3% | 13552 | 6616 | 9 % |  |
| sql/select20.bool | generated | hand | 6626.7 | 17275.5 | 2.61x | 17880.1 | 2.70x | +3.5% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6238250.0 | 6294100.0 | 1.01x | 6426750.0 | 1.03x | +2.1% | 8778600 | 8778600 | 11 % |  |
| el/ladder | generated | hand | 941.0 | 1652.7 | 1.76x | 1702.3 | 1.81x | +3.0% | 1776 | 1776 | 11 % |  |
| el/ladder | immediate | hand | 941.0 | 1125.8 | 1.20x | 1098.5 | 1.17x | -2.4% | 1784 | 1787 | 11 % |  |
| sql/select20 | generated | hand | 6661.5 | 17338.2 | 2.60x | 17815.9 | 2.67x | +2.8% | 21448 | 21448 | 11 % |  |
| sql/refused-late | generated | hand | 2521.2 | 11536.8 | 4.58x | 12044.6 | 4.78x | +4.4% | 13552 | 13552 | 5 % |  |
