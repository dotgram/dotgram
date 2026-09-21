# Paired stand, 2026-09-21 06:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 167764.1 | 181337.5 | 1.08x | 184757.0 | 1.10x | +1.9% | 720048 | 720048 | 164 % |  |
| sql/select20.at | generated | hand | 6646.8 | 17590.6 | 2.65x | 17092.0 | 2.57x | -2.8% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6554.1 | 17653.4 | 2.69x | 17449.8 | 2.66x | -1.2% | 21416 | 21416 | 7 % |  |
| sql/select20.scan | generated | control | 368.3 | 371.2 | 1.01x | 367.1 | 1.00x | -1.1% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 373.1 | 369.5 | 0.99x | 373.6 | 1.00x | +1.1% | 0 | 0 | 6 % |  |
| el/ladder.scan | generated | control | 133.7 | 134.6 | 1.01x | 134.1 | 1.00x | -0.3% | 0 | 0 | 6 % |  |
| el/ladder.bool | generated | hand | 929.1 | 1642.0 | 1.77x | 1652.5 | 1.78x | +0.6% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2513.8 | 12087.5 | 4.81x | 5610.8 | 2.23x | -53.6% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6643.1 | 17689.8 | 2.66x | 17475.6 | 2.63x | -1.2% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4202787.5 | 4340587.5 | 1.03x | 4353737.5 | 1.04x | +0.3% | 6276641 | 6276684 | 5 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6271962.5 | 6343193.8 | 1.01x | 6309750.0 | 1.01x | -0.5% | 8778556 | 8778600 | 16 % |  |
| el/ladder | generated | hand | 924.0 | 1640.3 | 1.78x | 1655.6 | 1.79x | +0.9% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 924.0 | 1135.2 | 1.23x | 1130.5 | 1.22x | -0.4% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 106499.8 | 147736.9 | 1.39x | 146055.5 | 1.37x | -1.1% | 169107 | 169104 | 22 % |  |
| el/terms1000 | immediate | hand | 106499.8 | 125884.5 | 1.18x | 119960.4 | 1.13x | -4.7% | 177120 | 177120 | 22 % |  |
| sql/select20 | generated | hand | 6635.5 | 17615.9 | 2.65x | 17414.0 | 2.62x | -1.1% | 21448 | 21448 | 8 % |  |
| sql/refused-late | generated | hand | 2518.3 | 12092.2 | 4.80x | 11683.2 | 4.64x | -3.4% | 13552 | 13576 | 2 % |  |
