# Paired stand, 2026-09-21 03:02

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6690.4 | 17453.9 | 2.61x | 17083.7 | 2.55x | -2.1% | 21416 | 21416 | 2 % |  |
| sql/select20.window | generated | hand | 6631.0 | 17773.2 | 2.68x | 17422.7 | 2.63x | -2.0% | 21416 | 21416 | 1 % |  |
| sql/select20.scan | generated | control | 383.4 | 372.1 | 0.97x | 370.5 | 0.97x | -0.4% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 371.0 | 371.3 | 1.00x | 371.1 | 1.00x | -0.1% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 137.3 | 134.5 | 0.98x | 135.1 | 0.98x | +0.5% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 963.1 | 1687.6 | 1.75x | 1678.0 | 1.74x | -0.6% | 1776 | 1720 | 39 % |  |
| sql/refused-late.bool | generated | hand | 2514.0 | 11762.8 | 4.68x | 5612.8 | 2.23x | -52.3% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6697.9 | 17909.3 | 2.67x | 17376.1 | 2.59x | -3.0% | 21448 | 21392 | 17 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6291750.0 | 6348250.0 | 1.01x | 6282550.0 | 1.00x | -1.0% | 8778600 | 8778619 | 59 % |  |
| el/ladder | generated | hand | 940.3 | 1671.4 | 1.78x | 1686.6 | 1.79x | +0.9% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 940.3 | 1134.0 | 1.21x | 1151.5 | 1.22x | +1.5% | 1784 | 1784 | 2 % |  |
| sql/select20 | generated | hand | 6680.9 | 17958.4 | 2.69x | 17558.0 | 2.63x | -2.2% | 21448 | 21448 | 16 % |  |
| sql/refused-late | generated | hand | 2516.8 | 11765.7 | 4.67x | 11825.1 | 4.70x | +0.5% | 13552 | 13552 | 7 % |  |
