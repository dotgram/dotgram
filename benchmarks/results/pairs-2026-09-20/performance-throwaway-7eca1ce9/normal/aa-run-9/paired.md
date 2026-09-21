# Paired stand, 2026-09-21 03:23

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6632.3 | 17258.3 | 2.60x | 17240.3 | 2.60x | -0.1% | 21416 | 21416 | 393 % |  |
| sql/select20.window | generated | hand | 6593.2 | 17722.1 | 2.69x | 17333.8 | 2.63x | -2.2% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 365.1 | 371.9 | 1.02x | 375.4 | 1.03x | +1.0% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 367.2 | 372.1 | 1.01x | 375.3 | 1.02x | +0.9% | 0 | 0 | 17 % |  |
| el/ladder.scan | generated | control | 136.3 | 136.6 | 1.00x | 139.9 | 1.03x | +2.4% | 0 | 0 | 6 % |  |
| el/ladder.bool | generated | hand | 934.2 | 1680.9 | 1.80x | 1635.2 | 1.75x | -2.7% | 1776 | 1720 | 12 % |  |
| sql/refused-late.bool | generated | hand | 2489.2 | 11873.8 | 4.77x | 5655.9 | 2.27x | -52.4% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6575.8 | 17630.3 | 2.68x | 17357.0 | 2.64x | -1.5% | 21448 | 21392 | 13 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6304600.0 | 6525350.0 | 1.04x | 6344300.0 | 1.01x | -2.8% | 8778621 | 8778556 | 50 % |  |
| el/ladder | generated | hand | 941.4 | 1673.4 | 1.78x | 1665.3 | 1.77x | -0.5% | 1776 | 1776 | 16 % |  |
| el/ladder | immediate | hand | 941.4 | 1120.1 | 1.19x | 1137.7 | 1.21x | +1.6% | 1784 | 1784 | 16 % |  |
| sql/select20 | generated | hand | 6624.1 | 17801.9 | 2.69x | 17463.7 | 2.64x | -1.9% | 21448 | 21448 | 10 % |  |
| sql/refused-late | generated | hand | 2489.2 | 11913.5 | 4.79x | 11813.9 | 4.75x | -0.8% | 13552 | 13552 | 3 % |  |
