# Paired stand, 2026-09-21 07:11

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 168159.4 | 184412.5 | 1.10x | 183724.2 | 1.09x | -0.4% | 720048 | 720048 | 9 % |  |
| sql/select20.at | generated | hand | 6660.5 | 17237.5 | 2.59x | 22457.2 | 3.37x | +30.3% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6678.6 | 17498.1 | 2.62x | 22807.2 | 3.41x | +30.3% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 373.1 | 369.8 | 0.99x | 424.8 | 1.14x | +14.9% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 376.2 | 368.2 | 0.98x | 394.7 | 1.05x | +7.2% | 0 | 0 | 9 % |  |
| el/ladder.scan | generated | control | 134.1 | 134.6 | 1.00x | 134.2 | 1.00x | -0.3% | 0 | 0 | 12 % |  |
| el/ladder.bool | generated | hand | 944.7 | 1667.0 | 1.76x | 1651.4 | 1.75x | -0.9% | 1776 | 1720 | 18 % |  |
| sql/refused-late.bool | generated | hand | 2531.2 | 11744.1 | 4.64x | 7086.0 | 2.80x | -39.7% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6674.5 | 17468.2 | 2.62x | 22779.0 | 3.41x | +30.4% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4333450.0 | 4512806.2 | 1.04x | 5646300.0 | 1.30x | +25.1% | 6276598 | 6276555 | 3 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6211743.8 | 6449887.5 | 1.04x | 7274031.2 | 1.17x | +12.8% | 8778600 | 8778600 | 13 % |  |
| el/ladder | generated | hand | 955.4 | 1680.0 | 1.76x | 1679.2 | 1.76x | -0.1% | 1776 | 1776 | 8 % |  |
| el/ladder | immediate | hand | 955.4 | 1139.5 | 1.19x | 1150.4 | 1.20x | +1.0% | 1784 | 1784 | 8 % |  |
| el/terms1000 | generated | hand | 110916.5 | 147531.7 | 1.33x | 150945.6 | 1.36x | +2.3% | 169104 | 169104 | 6 % |  |
| el/terms1000 | immediate | hand | 110916.5 | 120197.0 | 1.08x | 122837.9 | 1.11x | +2.2% | 177120 | 177123 | 6 % |  |
| sql/select20 | generated | hand | 6704.4 | 17421.3 | 2.60x | 22794.5 | 3.40x | +30.8% | 21448 | 21448 | 12 % |  |
| sql/refused-late | generated | hand | 2545.4 | 11741.2 | 4.61x | 14571.7 | 5.72x | +24.1% | 13552 | 13552 | 13 % |  |
