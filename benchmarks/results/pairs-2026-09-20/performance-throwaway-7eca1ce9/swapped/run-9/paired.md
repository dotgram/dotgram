# Paired stand, 2026-09-21 03:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6658.5 | 17400.6 | 2.61x | 17144.3 | 2.57x | -1.5% | 21416 | 21416 | 438 % |  |
| sql/select20.window | generated | hand | 6587.7 | 17922.0 | 2.72x | 17410.8 | 2.64x | -2.9% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 375.0 | 378.7 | 1.01x | 380.4 | 1.01x | +0.4% | 0 | 0 | 21 % |  |
| tsql/select20.scan | generated | control | 374.5 | 376.3 | 1.00x | 380.9 | 1.02x | +1.2% | 0 | 0 | 9 % |  |
| el/ladder.scan | generated | control | 135.4 | 138.4 | 1.02x | 136.9 | 1.01x | -1.1% | 0 | 0 | 2 % |  |
| el/ladder.bool | generated | hand | 936.5 | 1648.0 | 1.76x | 1633.2 | 1.74x | -0.9% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2479.1 | 11908.1 | 4.80x | 5644.4 | 2.28x | -52.6% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6595.9 | 17727.6 | 2.69x | 17468.7 | 2.65x | -1.5% | 21448 | 21392 | 10 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6219250.0 | 6925850.0 | 1.11x | 6311400.0 | 1.01x | -8.9% | 8778604 | 8778558 | 41 % |  |
| el/ladder | generated | hand | 938.5 | 1659.0 | 1.77x | 1649.6 | 1.76x | -0.6% | 1776 | 1776 | 5 % |  |
| el/ladder | immediate | hand | 938.5 | 1134.0 | 1.21x | 1137.3 | 1.21x | +0.3% | 1784 | 1784 | 5 % |  |
| sql/select20 | generated | hand | 6591.6 | 17781.0 | 2.70x | 17448.8 | 2.65x | -1.9% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2486.7 | 11900.6 | 4.79x | 11795.0 | 4.74x | -0.9% | 13552 | 13552 | 13 % |  |
