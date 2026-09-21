# Paired stand, 2026-09-21 02:53

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6556.0 | 17433.0 | 2.66x | 17172.9 | 2.62x | -1.5% | 21416 | 21441 | 440 % |  |
| sql/select20.window | generated | hand | 6593.0 | 17605.0 | 2.67x | 17655.2 | 2.68x | +0.3% | 21416 | 21416 | 18 % |  |
| sql/select20.scan | generated | control | 366.2 | 426.8 | 1.17x | 371.4 | 1.01x | -13.0% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 374.7 | 369.6 | 0.99x | 366.3 | 0.98x | -0.9% | 0 | 0 | 31 % |  |
| el/ladder.scan | generated | control | 134.9 | 133.7 | 0.99x | 133.8 | 0.99x | +0.1% | 0 | 0 | 6 % |  |
| el/ladder.bool | generated | hand | 936.2 | 1645.5 | 1.76x | 1627.9 | 1.74x | -1.1% | 1776 | 1720 | 15 % |  |
| sql/refused-late.bool | generated | hand | 2469.1 | 12118.4 | 4.91x | 5648.3 | 2.29x | -53.4% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6490.4 | 17590.3 | 2.71x | 17478.2 | 2.69x | -0.6% | 21448 | 21392 | 2 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6106850.0 | 6702250.0 | 1.10x | 6299800.0 | 1.03x | -6.0% | 8778619 | 8778556 | 43 % |  |
| el/ladder | generated | hand | 935.4 | 1636.4 | 1.75x | 1627.1 | 1.74x | -0.6% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 935.4 | 1124.6 | 1.20x | 1108.5 | 1.19x | -1.4% | 1784 | 1784 | 3 % |  |
| sql/select20 | generated | hand | 6518.0 | 17574.6 | 2.70x | 17315.7 | 2.66x | -1.5% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2496.1 | 12211.7 | 4.89x | 11843.7 | 4.74x | -3.0% | 13552 | 13552 | 5 % |  |
