# Paired stand, 2026-09-21 03:21

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6672.4 | 17159.8 | 2.57x | 17197.9 | 2.58x | +0.2% | 21416 | 21416 | 445 % |  |
| sql/select20.window | generated | hand | 6629.9 | 17538.1 | 2.65x | 17361.0 | 2.62x | -1.0% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 377.6 | 372.4 | 0.99x | 371.3 | 0.98x | -0.3% | 0 | 0 | 20 % |  |
| tsql/select20.scan | generated | control | 367.1 | 374.6 | 1.02x | 370.0 | 1.01x | -1.2% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 134.5 | 133.7 | 0.99x | 134.4 | 1.00x | +0.5% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 943.3 | 1635.0 | 1.73x | 1650.1 | 1.75x | +0.9% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2494.7 | 11762.5 | 4.71x | 5701.2 | 2.29x | -51.5% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6717.1 | 17364.4 | 2.59x | 17468.5 | 2.60x | +0.6% | 21448 | 21392 | 10 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6238250.0 | 6246400.0 | 1.00x | 6293500.0 | 1.01x | +0.8% | 8778604 | 8778600 | 43 % |  |
| el/ladder | generated | hand | 939.3 | 1626.8 | 1.73x | 1650.5 | 1.76x | +1.5% | 1776 | 1776 | 13 % |  |
| el/ladder | immediate | hand | 939.3 | 1150.0 | 1.22x | 1111.9 | 1.18x | -3.3% | 1784 | 1784 | 13 % |  |
| sql/select20 | generated | hand | 6656.1 | 17424.8 | 2.62x | 17442.6 | 2.62x | +0.1% | 21448 | 21448 | 11 % |  |
| sql/refused-late | generated | hand | 2510.9 | 11825.3 | 4.71x | 12018.0 | 4.79x | +1.6% | 13552 | 13552 | 4 % |  |
