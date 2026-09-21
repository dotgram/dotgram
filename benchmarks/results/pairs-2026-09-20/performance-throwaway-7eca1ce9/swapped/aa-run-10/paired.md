# Paired stand, 2026-09-21 04:01

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6575.8 | 17225.1 | 2.62x | 16977.7 | 2.58x | -1.4% | 21416 | 21441 | 7 % |  |
| sql/select20.window | generated | hand | 6617.4 | 17353.2 | 2.62x | 17335.9 | 2.62x | -0.1% | 21416 | 21440 | 10 % |  |
| sql/select20.scan | generated | control | 372.0 | 369.6 | 0.99x | 371.0 | 1.00x | +0.4% | 0 | 0 | 7 % |  |
| tsql/select20.scan | generated | control | 365.8 | 367.9 | 1.01x | 367.8 | 1.01x | 0.0% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 138.5 | 134.9 | 0.97x | 135.6 | 0.98x | +0.5% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 926.8 | 1641.0 | 1.77x | 1626.1 | 1.75x | -0.9% | 1776 | 1720 | 7 % |  |
| sql/refused-late.bool | generated | hand | 2476.4 | 11774.2 | 4.75x | 5590.4 | 2.26x | -52.5% | 13552 | 6616 | 13 % |  |
| sql/select20.bool | generated | hand | 6561.6 | 17292.8 | 2.64x | 17357.6 | 2.65x | +0.4% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6071400.0 | 6253750.0 | 1.03x | 6253600.0 | 1.03x | 0.0% | 8778600 | 8778576 | 45 % |  |
| el/ladder | generated | hand | 930.9 | 1636.7 | 1.76x | 1642.8 | 1.76x | +0.4% | 1776 | 1776 | 16 % |  |
| el/ladder | immediate | hand | 930.9 | 1122.0 | 1.21x | 1115.5 | 1.20x | -0.6% | 1784 | 1784 | 16 % |  |
| sql/select20 | generated | hand | 6618.2 | 17238.8 | 2.60x | 17402.7 | 2.63x | +1.0% | 21448 | 21448 | 9 % |  |
| sql/refused-late | generated | hand | 2483.8 | 11757.0 | 4.73x | 11689.6 | 4.71x | -0.6% | 13552 | 13552 | 5 % |  |
