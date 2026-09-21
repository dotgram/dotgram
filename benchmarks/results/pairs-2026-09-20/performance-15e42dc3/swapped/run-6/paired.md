# Paired stand, 2026-09-21 07:20

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 168763.3 | 187700.0 | 1.11x | 190682.8 | 1.13x | +1.6% | 720048 | 720048 | 352 % |  |
| sql/select20.at | generated | hand | 6718.2 | 16959.8 | 2.52x | 17062.6 | 2.54x | +0.6% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 6642.9 | 17261.7 | 2.60x | 17233.2 | 2.59x | -0.2% | 21416 | 21416 | 17 % |  |
| sql/select20.scan | generated | control | 364.8 | 368.8 | 1.01x | 372.3 | 1.02x | +0.9% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 367.7 | 371.9 | 1.01x | 366.4 | 1.00x | -1.5% | 0 | 0 | 15 % |  |
| el/ladder.scan | generated | control | 133.6 | 133.6 | 1.00x | 133.3 | 1.00x | -0.2% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 952.7 | 1667.7 | 1.75x | 1623.8 | 1.70x | -2.6% | 1776 | 1720 | 16 % |  |
| sql/refused-late.bool | generated | hand | 2516.5 | 11712.2 | 4.65x | 5589.9 | 2.22x | -52.3% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6705.5 | 17566.5 | 2.62x | 17521.4 | 2.61x | -0.3% | 21448 | 21392 | 6 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4537243.8 | 4406437.5 | 0.97x | 4522050.0 | 1.00x | +2.6% | 6276687 | 6276684 | 7 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6350931.2 | 6366825.0 | 1.00x | 6270200.0 | 0.99x | -1.5% | 8778556 | 8778595 | 17 % |  |
| el/ladder | generated | hand | 935.9 | 1672.4 | 1.79x | 1652.8 | 1.77x | -1.2% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 935.9 | 1131.2 | 1.21x | 1134.8 | 1.21x | +0.3% | 1784 | 1784 | 1 % |  |
| el/terms1000 | generated | hand | 110952.8 | 148320.4 | 1.34x | 144816.1 | 1.31x | -2.4% | 169107 | 169104 | 3 % |  |
| el/terms1000 | immediate | hand | 110952.8 | 120078.7 | 1.08x | 119939.6 | 1.08x | -0.1% | 177120 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6714.0 | 17398.0 | 2.59x | 17294.2 | 2.58x | -0.6% | 21448 | 21448 | 13 % |  |
| sql/refused-late | generated | hand | 2512.3 | 11746.3 | 4.68x | 11680.1 | 4.65x | -0.6% | 13552 | 13552 | 3 % |  |
