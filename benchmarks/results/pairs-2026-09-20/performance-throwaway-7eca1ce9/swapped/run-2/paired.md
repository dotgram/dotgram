# Paired stand, 2026-09-21 03:32

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6773.6 | 17072.7 | 2.52x | 16943.0 | 2.50x | -0.8% | 21416 | 21441 | 433 % |  |
| sql/select20.window | generated | hand | 6658.0 | 17670.7 | 2.65x | 17198.9 | 2.58x | -2.7% | 21416 | 21416 | 19 % |  |
| sql/select20.scan | generated | control | 373.4 | 372.4 | 1.00x | 372.0 | 1.00x | -0.1% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 368.2 | 373.1 | 1.01x | 376.6 | 1.02x | +1.0% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 134.7 | 135.4 | 1.01x | 134.1 | 1.00x | -1.0% | 0 | 0 | 3 % |  |
| el/ladder.bool | generated | hand | 946.3 | 1631.9 | 1.72x | 1625.7 | 1.72x | -0.4% | 1776 | 1720 | 44 % |  |
| sql/refused-late.bool | generated | hand | 2507.5 | 11831.2 | 4.72x | 5660.5 | 2.26x | -52.2% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6694.3 | 17319.8 | 2.59x | 17523.0 | 2.62x | +1.2% | 21448 | 21392 | 2 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6065300.0 | 6254500.0 | 1.03x | 6418500.0 | 1.06x | +2.6% | 8778600 | 8778556 | 38 % |  |
| el/ladder | generated | hand | 944.1 | 1628.1 | 1.72x | 1660.1 | 1.76x | +2.0% | 1776 | 1776 | 10 % |  |
| el/ladder | immediate | hand | 944.1 | 1151.6 | 1.22x | 1127.4 | 1.19x | -2.1% | 1784 | 1784 | 10 % |  |
| sql/select20 | generated | hand | 6695.0 | 17399.3 | 2.60x | 17278.9 | 2.58x | -0.7% | 21448 | 21448 | 25 % |  |
| sql/refused-late | generated | hand | 2517.8 | 11838.1 | 4.70x | 11849.2 | 4.71x | +0.1% | 13552 | 13552 | 5 % |  |
