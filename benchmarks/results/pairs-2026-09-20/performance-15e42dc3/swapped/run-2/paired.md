# Paired stand, 2026-09-21 07:10

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165593.0 | 182326.6 | 1.10x | 183054.7 | 1.11x | +0.4% | 720048 | 720048 | 6 % |  |
| sql/select20.at | generated | hand | 6626.2 | 17550.4 | 2.65x | 17115.9 | 2.58x | -2.5% | 21416 | 21416 | 22 % |  |
| sql/select20.window | generated | hand | 6610.1 | 18120.1 | 2.74x | 18165.8 | 2.75x | +0.3% | 21416 | 21416 | 6 % |  |
| sql/select20.scan | generated | control | 362.5 | 370.4 | 1.02x | 369.2 | 1.02x | -0.3% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 397.3 | 369.8 | 0.93x | 368.7 | 0.93x | -0.3% | 0 | 0 | 10 % |  |
| el/ladder.scan | generated | control | 133.2 | 134.3 | 1.01x | 133.9 | 1.00x | -0.3% | 0 | 0 | 2 % |  |
| el/ladder.bool | generated | hand | 946.3 | 1696.5 | 1.79x | 1657.5 | 1.75x | -2.3% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2539.5 | 12072.4 | 4.75x | 5679.3 | 2.24x | -53.0% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6678.9 | 18052.3 | 2.70x | 17486.2 | 2.62x | -3.1% | 21448 | 21392 | 32 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4344375.0 | 4504893.8 | 1.04x | 4454487.5 | 1.03x | -1.1% | 6276555 | 6276641 | 31 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6248875.0 | 6453156.2 | 1.03x | 6290900.0 | 1.01x | -2.5% | 8778600 | 8778600 | 6 % |  |
| el/ladder | generated | hand | 953.4 | 1706.9 | 1.79x | 1693.3 | 1.78x | -0.8% | 1776 | 1776 | 9 % |  |
| el/ladder | immediate | hand | 953.4 | 1163.3 | 1.22x | 1137.8 | 1.19x | -2.2% | 1784 | 1784 | 9 % |  |
| el/terms1000 | generated | hand | 105768.9 | 148771.2 | 1.41x | 148300.0 | 1.40x | -0.3% | 169107 | 169104 | 9 % |  |
| el/terms1000 | immediate | hand | 105768.9 | 123204.8 | 1.16x | 124209.2 | 1.17x | +0.8% | 177120 | 177120 | 9 % |  |
| sql/select20 | generated | hand | 6670.7 | 18045.5 | 2.71x | 17651.5 | 2.65x | -2.2% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2543.1 | 12056.8 | 4.74x | 11852.7 | 4.66x | -1.7% | 13552 | 13552 | 14 % |  |
