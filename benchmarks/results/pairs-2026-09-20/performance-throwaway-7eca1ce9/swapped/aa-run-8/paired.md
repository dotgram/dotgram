# Paired stand, 2026-09-21 03:55

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6692.3 | 17730.3 | 2.65x | 17284.9 | 2.58x | -2.5% | 21416 | 21416 | 396 % |  |
| sql/select20.window | generated | hand | 6681.8 | 18302.1 | 2.74x | 17728.6 | 2.65x | -3.1% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 365.3 | 373.2 | 1.02x | 373.0 | 1.02x | -0.1% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 374.8 | 372.0 | 0.99x | 372.1 | 0.99x | 0.0% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 135.9 | 137.5 | 1.01x | 135.9 | 1.00x | -1.2% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 942.0 | 1660.9 | 1.76x | 1623.3 | 1.72x | -2.3% | 1776 | 1720 | 2 % |  |
| sql/refused-late.bool | generated | hand | 2500.9 | 12267.3 | 4.91x | 5739.6 | 2.30x | -53.2% | 13552 | 6616 | 7 % |  |
| sql/select20.bool | generated | hand | 6675.5 | 18061.8 | 2.71x | 17584.6 | 2.63x | -2.6% | 21448 | 21392 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6158500.0 | 6678400.0 | 1.08x | 6266450.0 | 1.02x | -6.2% | 8778559 | 8778619 | 36 % |  |
| el/ladder | generated | hand | 960.8 | 1663.3 | 1.73x | 1639.3 | 1.71x | -1.4% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 960.8 | 1144.6 | 1.19x | 1135.8 | 1.18x | -0.8% | 1784 | 1784 | 2 % |  |
| sql/select20 | generated | hand | 6689.7 | 18039.6 | 2.70x | 17567.0 | 2.63x | -2.6% | 21448 | 21448 | 10 % |  |
| sql/refused-late | generated | hand | 2521.7 | 12250.8 | 4.86x | 11932.8 | 4.73x | -2.6% | 13552 | 13552 | 5 % |  |
