# Paired stand, 2026-09-21 03:41

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6608.9 | 16794.4 | 2.54x | 16959.5 | 2.57x | +1.0% | 21416 | 21441 | 439 % |  |
| sql/select20.window | generated | hand | 6596.9 | 17409.5 | 2.64x | 18565.6 | 2.81x | +6.6% | 21416 | 21440 | 4 % |  |
| sql/select20.scan | generated | control | 369.1 | 368.4 | 1.00x | 369.9 | 1.00x | +0.4% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 368.6 | 370.3 | 1.00x | 374.9 | 1.02x | +1.2% | 0 | 0 | 9 % |  |
| el/ladder.scan | generated | control | 136.3 | 134.8 | 0.99x | 135.8 | 1.00x | +0.7% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 935.8 | 1670.0 | 1.78x | 1593.1 | 1.70x | -4.6% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2505.1 | 11576.4 | 4.62x | 5638.8 | 2.25x | -51.3% | 13552 | 6616 | 19 % |  |
| sql/select20.bool | generated | hand | 6639.2 | 17364.1 | 2.62x | 17462.8 | 2.63x | +0.6% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6077800.0 | 6146250.0 | 1.01x | 6261500.0 | 1.03x | +1.9% | 8778600 | 8778600 | 45 % |  |
| el/ladder | generated | hand | 936.6 | 1659.7 | 1.77x | 1630.4 | 1.74x | -1.8% | 1776 | 1776 | 10 % |  |
| el/ladder | immediate | hand | 936.6 | 1139.7 | 1.22x | 1124.8 | 1.20x | -1.3% | 1784 | 1784 | 10 % |  |
| sql/select20 | generated | hand | 6651.9 | 17243.0 | 2.59x | 17340.4 | 2.61x | +0.6% | 21448 | 21448 | 9 % |  |
| sql/refused-late | generated | hand | 2509.9 | 11524.0 | 4.59x | 11700.1 | 4.66x | +1.5% | 13552 | 13552 | 2 % |  |
