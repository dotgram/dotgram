# Paired stand, 2026-09-21 03:59

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6822.5 | 17364.5 | 2.55x | 17298.1 | 2.54x | -0.4% | 21416 | 21416 | 430 % |  |
| sql/select20.window | generated | hand | 6695.2 | 17702.0 | 2.64x | 17640.9 | 2.63x | -0.3% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 377.9 | 374.0 | 0.99x | 372.8 | 0.99x | -0.3% | 0 | 0 | 88 % |  |
| tsql/select20.scan | generated | control | 366.9 | 375.9 | 1.02x | 384.2 | 1.05x | +2.2% | 0 | 0 | 15 % |  |
| el/ladder.scan | generated | control | 139.9 | 134.6 | 0.96x | 135.1 | 0.97x | +0.4% | 0 | 0 | 16 % |  |
| el/ladder.bool | generated | hand | 941.8 | 1645.5 | 1.75x | 1624.3 | 1.72x | -1.3% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2545.5 | 11839.3 | 4.65x | 5685.4 | 2.23x | -52.0% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6795.2 | 17681.5 | 2.60x | 17861.4 | 2.63x | +1.0% | 21448 | 21392 | 13 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6332075.0 | 6399125.0 | 1.01x | 6555662.5 | 1.04x | +2.4% | 8778619 | 8778600 | 15 % |  |
| el/ladder | generated | hand | 940.3 | 1644.5 | 1.75x | 1631.5 | 1.74x | -0.8% | 1776 | 1776 | 28 % |  |
| el/ladder | immediate | hand | 940.3 | 1146.2 | 1.22x | 1110.8 | 1.18x | -3.1% | 1784 | 1784 | 28 % |  |
| sql/select20 | generated | hand | 6739.6 | 17642.7 | 2.62x | 17649.3 | 2.62x | 0.0% | 21448 | 21448 | 16 % |  |
| sql/refused-late | generated | hand | 2552.7 | 11847.3 | 4.64x | 11939.2 | 4.68x | +0.8% | 13552 | 13552 | 15 % |  |
