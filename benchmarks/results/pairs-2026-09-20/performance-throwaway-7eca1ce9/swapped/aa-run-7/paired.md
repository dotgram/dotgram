# Paired stand, 2026-09-21 03:51

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6776.9 | 17247.1 | 2.54x | 17107.4 | 2.52x | -0.8% | 21416 | 21416 | 24 % |  |
| sql/select20.window | generated | hand | 6694.3 | 17395.3 | 2.60x | 17570.4 | 2.62x | +1.0% | 21416 | 21416 | 59 % |  |
| sql/select20.scan | generated | control | 365.7 | 374.0 | 1.02x | 373.6 | 1.02x | -0.1% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 375.8 | 374.3 | 1.00x | 370.2 | 0.99x | -1.1% | 0 | 0 | 8 % |  |
| el/ladder.scan | generated | control | 138.4 | 137.4 | 0.99x | 135.9 | 0.98x | -1.1% | 0 | 0 | 17 % |  |
| el/ladder.bool | generated | hand | 948.6 | 1670.3 | 1.76x | 1654.2 | 1.74x | -1.0% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2503.1 | 11952.0 | 4.77x | 5610.2 | 2.24x | -53.1% | 13552 | 6616 | 7 % |  |
| sql/select20.bool | generated | hand | 6687.5 | 17545.8 | 2.62x | 17411.2 | 2.60x | -0.8% | 21448 | 21392 | 13 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6374150.0 | 6461000.0 | 1.01x | 6247600.0 | 0.98x | -3.3% | 8778598 | 8778559 | 43 % |  |
| el/ladder | generated | hand | 953.1 | 1682.7 | 1.77x | 1667.4 | 1.75x | -0.9% | 1776 | 1776 | 5 % |  |
| el/ladder | immediate | hand | 953.1 | 1129.4 | 1.18x | 1123.5 | 1.18x | -0.5% | 1784 | 1784 | 5 % |  |
| sql/select20 | generated | hand | 6716.4 | 17585.9 | 2.62x | 17529.1 | 2.61x | -0.3% | 21448 | 21448 | 10 % |  |
| sql/refused-late | generated | hand | 2505.8 | 11813.7 | 4.71x | 11713.8 | 4.67x | -0.8% | 13552 | 13552 | 3 % |  |
