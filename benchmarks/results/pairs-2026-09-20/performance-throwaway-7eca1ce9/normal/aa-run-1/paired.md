# Paired stand, 2026-09-21 02:55

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6652.1 | 16982.3 | 2.55x | 17044.4 | 2.56x | +0.4% | 21416 | 21416 | 21 % |  |
| sql/select20.window | generated | hand | 6599.4 | 17276.8 | 2.62x | 17276.9 | 2.62x | 0.0% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 369.3 | 375.5 | 1.02x | 375.4 | 1.02x | 0.0% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 379.9 | 374.7 | 0.99x | 380.8 | 1.00x | +1.6% | 0 | 0 | 13 % |  |
| el/ladder.scan | generated | control | 135.9 | 139.6 | 1.03x | 137.9 | 1.01x | -1.2% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 945.9 | 1632.5 | 1.73x | 1606.9 | 1.70x | -1.6% | 1776 | 1720 | 8 % |  |
| sql/refused-late.bool | generated | hand | 2513.9 | 11752.6 | 4.68x | 5654.6 | 2.25x | -51.9% | 13552 | 6616 | 14 % |  |
| sql/select20.bool | generated | hand | 6640.8 | 17369.0 | 2.62x | 17399.4 | 2.62x | +0.2% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6243450.0 | 6227350.0 | 1.00x | 6145850.0 | 0.98x | -1.3% | 8778600 | 8778600 | 40 % |  |
| el/ladder | generated | hand | 953.3 | 1627.8 | 1.71x | 1659.2 | 1.74x | +1.9% | 1776 | 1776 | 9 % |  |
| el/ladder | immediate | hand | 953.3 | 1122.9 | 1.18x | 1125.2 | 1.18x | +0.2% | 1784 | 1784 | 9 % |  |
| sql/select20 | generated | hand | 6663.4 | 17438.9 | 2.62x | 17299.7 | 2.60x | -0.8% | 21448 | 21448 | 5 % |  |
| sql/refused-late | generated | hand | 2530.5 | 11757.9 | 4.65x | 11860.4 | 4.69x | +0.9% | 13552 | 13552 | 3 % |  |
