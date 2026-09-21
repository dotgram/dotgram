# Paired stand, 2026-09-21 02:56

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6679.4 | 17174.2 | 2.57x | 17074.8 | 2.56x | -0.6% | 21416 | 21416 | 380 % |  |
| sql/select20.window | generated | hand | 6620.4 | 17514.0 | 2.65x | 17492.7 | 2.64x | -0.1% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 366.5 | 371.9 | 1.01x | 376.4 | 1.03x | +1.2% | 0 | 0 | 7 % |  |
| tsql/select20.scan | generated | control | 368.4 | 374.0 | 1.02x | 374.9 | 1.02x | +0.2% | 0 | 0 | 6 % |  |
| el/ladder.scan | generated | control | 134.7 | 134.9 | 1.00x | 134.1 | 1.00x | -0.6% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 945.9 | 1647.4 | 1.74x | 1612.7 | 1.70x | -2.1% | 1776 | 1720 | 2 % |  |
| sql/refused-late.bool | generated | hand | 2504.6 | 11712.7 | 4.68x | 5678.8 | 2.27x | -51.5% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6676.6 | 17393.0 | 2.61x | 17392.5 | 2.60x | 0.0% | 21448 | 21392 | 1 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6182600.0 | 6184800.0 | 1.00x | 6254400.0 | 1.01x | +1.1% | 8778600 | 8778600 | 42 % |  |
| el/ladder | generated | hand | 945.9 | 1647.7 | 1.74x | 1627.8 | 1.72x | -1.2% | 1776 | 1776 | 11 % |  |
| el/ladder | immediate | hand | 945.9 | 1124.8 | 1.19x | 1118.2 | 1.18x | -0.6% | 1784 | 1784 | 11 % |  |
| sql/select20 | generated | hand | 6720.5 | 17418.6 | 2.59x | 17461.5 | 2.60x | +0.2% | 21448 | 21448 | 15 % |  |
| sql/refused-late | generated | hand | 2531.7 | 11790.4 | 4.66x | 11795.9 | 4.66x | 0.0% | 13552 | 13552 | 9 % |  |
