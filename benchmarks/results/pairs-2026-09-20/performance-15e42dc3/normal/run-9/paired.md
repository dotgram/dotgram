# Paired stand, 2026-09-21 07:02

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 171866.4 | 182800.0 | 1.06x | 184475.8 | 1.07x | +0.9% | 720048 | 720048 | 8 % |  |
| sql/select20.at | generated | hand | 6657.7 | 17418.4 | 2.62x | 17336.9 | 2.60x | -0.5% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6610.5 | 17731.1 | 2.68x | 17679.6 | 2.67x | -0.3% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 372.5 | 368.2 | 0.99x | 370.1 | 0.99x | +0.5% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 384.7 | 369.8 | 0.96x | 367.9 | 0.96x | -0.5% | 0 | 0 | 7 % |  |
| el/ladder.scan | generated | control | 134.1 | 133.4 | 0.99x | 133.2 | 0.99x | -0.1% | 0 | 0 | 1 % |  |
| el/ladder.bool | generated | hand | 948.8 | 1667.2 | 1.76x | 1669.3 | 1.76x | +0.1% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2495.7 | 11824.6 | 4.74x | 5759.2 | 2.31x | -51.3% | 13552 | 6616 | 8 % |  |
| sql/select20.bool | generated | hand | 6616.3 | 17779.7 | 2.69x | 17739.2 | 2.68x | -0.2% | 21448 | 21392 | 5 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4360587.5 | 4398312.5 | 1.01x | 4354206.2 | 1.00x | -1.0% | 6276684 | 6276641 | 5 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6202731.2 | 6305925.0 | 1.02x | 6392025.0 | 1.03x | +1.4% | 8778556 | 8778576 | 11 % |  |
| el/ladder | generated | hand | 941.4 | 1660.6 | 1.76x | 1689.7 | 1.79x | +1.8% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 941.4 | 1137.9 | 1.21x | 1130.5 | 1.20x | -0.6% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 109879.2 | 147191.5 | 1.34x | 146485.6 | 1.33x | -0.5% | 169104 | 169131 | 4 % |  |
| el/terms1000 | immediate | hand | 109879.2 | 120538.2 | 1.10x | 118789.0 | 1.08x | -1.5% | 177120 | 177120 | 4 % |  |
| sql/select20 | generated | hand | 6615.1 | 17751.5 | 2.68x | 17846.3 | 2.70x | +0.5% | 21448 | 21448 | 7 % |  |
| sql/refused-late | generated | hand | 2489.5 | 11812.6 | 4.74x | 11904.9 | 4.78x | +0.8% | 13552 | 13552 | 14 % |  |
