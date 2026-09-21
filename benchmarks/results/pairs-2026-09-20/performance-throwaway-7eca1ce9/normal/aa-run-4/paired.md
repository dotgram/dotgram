# Paired stand, 2026-09-21 03:05

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6690.8 | 17251.6 | 2.58x | 17011.3 | 2.54x | -1.4% | 21416 | 21416 | 435 % |  |
| sql/select20.window | generated | hand | 6672.2 | 17449.2 | 2.62x | 17376.9 | 2.60x | -0.4% | 21416 | 21416 | 6 % |  |
| sql/select20.scan | generated | control | 370.6 | 379.1 | 1.02x | 377.9 | 1.02x | -0.3% | 0 | 0 | 1 % |  |
| tsql/select20.scan | generated | control | 366.8 | 373.9 | 1.02x | 375.4 | 1.02x | +0.4% | 0 | 0 | 22 % |  |
| el/ladder.scan | generated | control | 137.4 | 138.0 | 1.00x | 138.8 | 1.01x | +0.6% | 0 | 0 | 14 % |  |
| el/ladder.bool | generated | hand | 926.4 | 1672.5 | 1.81x | 1625.5 | 1.75x | -2.8% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2491.6 | 11736.2 | 4.71x | 5531.2 | 2.22x | -52.9% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6627.0 | 17799.0 | 2.69x | 17366.4 | 2.62x | -2.4% | 21448 | 21392 | 12 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6168050.0 | 6241400.0 | 1.01x | 6264300.0 | 1.02x | +0.4% | 8778603 | 8778600 | 38 % |  |
| el/ladder | generated | hand | 929.3 | 1673.7 | 1.80x | 1659.3 | 1.79x | -0.9% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 929.3 | 1157.3 | 1.25x | 1129.2 | 1.22x | -2.4% | 1784 | 1784 | 2 % |  |
| sql/select20 | generated | hand | 6642.5 | 17829.5 | 2.68x | 17500.5 | 2.63x | -1.8% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2508.2 | 11746.7 | 4.68x | 11906.1 | 4.75x | +1.4% | 13552 | 13552 | 8 % |  |
