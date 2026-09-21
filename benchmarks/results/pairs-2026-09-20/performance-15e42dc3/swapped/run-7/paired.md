# Paired stand, 2026-09-21 07:23

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165317.2 | 184162.5 | 1.11x | 182030.5 | 1.10x | -1.2% | 720048 | 720048 | 361 % |  |
| sql/select20.at | generated | hand | 6718.7 | 17321.9 | 2.58x | 17441.7 | 2.60x | +0.7% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6672.0 | 17774.0 | 2.66x | 17989.3 | 2.70x | +1.2% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 365.4 | 369.7 | 1.01x | 369.1 | 1.01x | -0.2% | 0 | 0 | 13 % |  |
| tsql/select20.scan | generated | control | 368.3 | 370.1 | 1.00x | 369.8 | 1.00x | -0.1% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 134.6 | 133.1 | 0.99x | 134.0 | 1.00x | +0.6% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 938.1 | 1671.8 | 1.78x | 1667.1 | 1.78x | -0.3% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2515.0 | 11735.3 | 4.67x | 5797.0 | 2.30x | -50.6% | 13552 | 6616 | 10 % |  |
| sql/select20.bool | generated | hand | 6746.9 | 17636.9 | 2.61x | 17850.9 | 2.65x | +1.2% | 21448 | 21392 | 2 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4420637.5 | 4364550.0 | 0.99x | 4579050.0 | 1.04x | +4.9% | 6276688 | 6276688 | 11 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6447881.2 | 6268750.0 | 0.97x | 6473975.0 | 1.00x | +3.3% | 8778600 | 8778600 | 4 % |  |
| el/ladder | generated | hand | 944.9 | 1666.5 | 1.76x | 1682.4 | 1.78x | +1.0% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 944.9 | 1135.9 | 1.20x | 1132.3 | 1.20x | -0.3% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 107837.5 | 149130.4 | 1.38x | 146147.0 | 1.36x | -2.0% | 169107 | 169104 | 3 % |  |
| el/terms1000 | immediate | hand | 107837.5 | 123114.9 | 1.14x | 120877.9 | 1.12x | -1.8% | 177120 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6770.0 | 17553.7 | 2.59x | 17945.1 | 2.65x | +2.2% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2504.8 | 11795.8 | 4.71x | 11951.9 | 4.77x | +1.3% | 13552 | 13552 | 2 % |  |
