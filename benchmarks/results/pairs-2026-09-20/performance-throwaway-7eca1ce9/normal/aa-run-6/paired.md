# Paired stand, 2026-09-21 03:13

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6618.8 | 16850.6 | 2.55x | 16990.9 | 2.57x | +0.8% | 21416 | 21416 | 398 % |  |
| sql/select20.window | generated | hand | 6563.0 | 17197.3 | 2.62x | 17242.5 | 2.63x | +0.3% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 368.1 | 374.7 | 1.02x | 372.8 | 1.01x | -0.5% | 0 | 0 | 10 % |  |
| tsql/select20.scan | generated | control | 378.2 | 375.6 | 0.99x | 372.2 | 0.98x | -0.9% | 0 | 0 | 11 % |  |
| el/ladder.scan | generated | control | 137.5 | 136.3 | 0.99x | 140.1 | 1.02x | +2.8% | 0 | 0 | 23 % |  |
| el/ladder.bool | generated | hand | 992.3 | 1645.3 | 1.66x | 1623.0 | 1.64x | -1.4% | 1776 | 1720 | 23 % |  |
| sql/refused-late.bool | generated | hand | 2505.4 | 11677.9 | 4.66x | 5731.7 | 2.29x | -50.9% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6690.4 | 17324.3 | 2.59x | 17387.1 | 2.60x | +0.4% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6188400.0 | 6281400.0 | 1.02x | 6213150.0 | 1.00x | -1.1% | 8778600 | 8778556 | 42 % |  |
| el/ladder | generated | hand | 985.6 | 1631.7 | 1.66x | 1638.2 | 1.66x | +0.4% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 985.6 | 1123.9 | 1.14x | 1146.9 | 1.16x | +2.0% | 1784 | 1784 | 1 % |  |
| sql/select20 | generated | hand | 6714.1 | 17481.2 | 2.60x | 17368.7 | 2.59x | -0.6% | 21448 | 21448 | 5 % |  |
| sql/refused-late | generated | hand | 2535.8 | 11668.0 | 4.60x | 11825.5 | 4.66x | +1.3% | 13552 | 13552 | 16 % |  |
