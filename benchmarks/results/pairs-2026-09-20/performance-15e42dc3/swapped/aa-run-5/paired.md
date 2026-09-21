# Paired stand, 2026-09-21 07:19

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 167897.7 | 183085.2 | 1.09x | 184009.4 | 1.10x | +0.5% | 720048 | 720048 | 165 % |  |
| sql/select20.at | generated | hand | 6698.4 | 18326.3 | 2.74x | 17176.8 | 2.56x | -6.3% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6678.7 | 17825.9 | 2.67x | 17791.5 | 2.66x | -0.2% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 363.5 | 366.6 | 1.01x | 365.2 | 1.00x | -0.4% | 0 | 0 | 1 % |  |
| tsql/select20.scan | generated | control | 368.3 | 369.1 | 1.00x | 374.4 | 1.02x | +1.4% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 134.0 | 133.9 | 1.00x | 133.6 | 1.00x | -0.2% | 0 | 0 | 4 % |  |
| el/ladder.bool | generated | hand | 944.9 | 1682.5 | 1.78x | 1653.8 | 1.75x | -1.7% | 1776 | 1720 | 9 % |  |
| sql/refused-late.bool | generated | hand | 2510.6 | 11740.8 | 4.68x | 5713.0 | 2.28x | -51.3% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6720.0 | 17583.5 | 2.62x | 17805.6 | 2.65x | +1.3% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4355175.0 | 4400606.2 | 1.01x | 4395700.0 | 1.01x | -0.1% | 6276555 | 6276598 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6499775.0 | 6259512.5 | 0.96x | 6414750.0 | 0.99x | +2.5% | 8778600 | 8778600 | 13 % |  |
| el/ladder | generated | hand | 943.9 | 1696.7 | 1.80x | 1685.7 | 1.79x | -0.6% | 1776 | 1776 | 46 % |  |
| el/ladder | immediate | hand | 943.9 | 1138.7 | 1.21x | 1140.4 | 1.21x | +0.2% | 1784 | 1784 | 46 % |  |
| el/terms1000 | generated | hand | 108394.3 | 146003.7 | 1.35x | 146207.6 | 1.35x | +0.1% | 169104 | 169107 | 11 % |  |
| el/terms1000 | immediate | hand | 108394.3 | 121495.0 | 1.12x | 120064.6 | 1.11x | -1.2% | 177120 | 177120 | 11 % |  |
| sql/select20 | generated | hand | 6709.1 | 17377.6 | 2.59x | 17810.8 | 2.65x | +2.5% | 21448 | 21448 | 14 % |  |
| sql/refused-late | generated | hand | 2516.9 | 11701.1 | 4.65x | 11901.4 | 4.73x | +1.7% | 13552 | 13552 | 4 % |  |
