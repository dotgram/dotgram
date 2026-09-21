# Paired stand, 2026-09-21 03:44

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6631.7 | 17024.6 | 2.57x | 17783.5 | 2.68x | +4.5% | 21416 | 21416 | 442 % |  |
| sql/select20.window | generated | hand | 6597.8 | 17317.1 | 2.62x | 18239.7 | 2.76x | +5.3% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 365.3 | 376.4 | 1.03x | 372.5 | 1.02x | -1.0% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 367.0 | 374.1 | 1.02x | 377.3 | 1.03x | +0.8% | 0 | 0 | 18 % |  |
| el/ladder.scan | generated | control | 135.4 | 137.1 | 1.01x | 136.4 | 1.01x | -0.5% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 946.3 | 1696.7 | 1.79x | 1624.5 | 1.72x | -4.3% | 1776 | 1720 | 11 % |  |
| sql/refused-late.bool | generated | hand | 2496.1 | 11785.0 | 4.72x | 5869.1 | 2.35x | -50.2% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6690.2 | 17493.9 | 2.61x | 18379.9 | 2.75x | +5.1% | 21448 | 21392 | 9 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6121050.0 | 6505750.0 | 1.06x | 6631650.0 | 1.08x | +1.9% | 8778600 | 8778576 | 44 % |  |
| el/ladder | generated | hand | 947.2 | 1686.9 | 1.78x | 1616.5 | 1.71x | -4.2% | 1776 | 1776 | 12 % |  |
| el/ladder | immediate | hand | 947.2 | 1133.2 | 1.20x | 1116.0 | 1.18x | -1.5% | 1784 | 1784 | 12 % |  |
| sql/select20 | generated | hand | 6709.3 | 17543.4 | 2.61x | 18180.9 | 2.71x | +3.6% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2509.8 | 11811.9 | 4.71x | 12199.4 | 4.86x | +3.3% | 13552 | 13552 | 6 % |  |
