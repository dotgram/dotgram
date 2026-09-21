# Paired stand, 2026-09-21 07:25

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165816.4 | 184066.4 | 1.11x | 183361.7 | 1.11x | -0.4% | 720048 | 720048 | 163 % |  |
| sql/select20.at | generated | hand | 6617.3 | 17172.1 | 2.60x | 17219.0 | 2.60x | +0.3% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6615.5 | 17843.5 | 2.70x | 17687.7 | 2.67x | -0.9% | 21416 | 21441 | 2 % |  |
| sql/select20.scan | generated | control | 366.5 | 369.9 | 1.01x | 374.4 | 1.02x | +1.2% | 0 | 0 | 23 % |  |
| tsql/select20.scan | generated | control | 403.7 | 367.1 | 0.91x | 388.2 | 0.96x | +5.7% | 0 | 0 | 6 % |  |
| el/ladder.scan | generated | control | 134.3 | 134.2 | 1.00x | 133.8 | 1.00x | -0.3% | 0 | 0 | 30 % |  |
| el/ladder.bool | generated | hand | 932.2 | 1678.9 | 1.80x | 1756.0 | 1.88x | +4.6% | 1776 | 1720 | 9 % |  |
| sql/refused-late.bool | generated | hand | 2484.3 | 11761.8 | 4.73x | 5702.4 | 2.30x | -51.5% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6615.2 | 17593.5 | 2.66x | 17636.2 | 2.67x | +0.2% | 21448 | 21392 | 22 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4605200.0 | 4428150.0 | 0.96x | 4774868.8 | 1.04x | +7.8% | 6276684 | 6276598 | 13 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6383006.2 | 6454800.0 | 1.01x | 6647850.0 | 1.04x | +3.0% | 8778556 | 8778600 | 15 % |  |
| el/ladder | generated | hand | 944.2 | 1693.9 | 1.79x | 1777.7 | 1.88x | +5.0% | 1776 | 1776 | 9 % |  |
| el/ladder | immediate | hand | 944.2 | 1194.0 | 1.26x | 1151.4 | 1.22x | -3.6% | 1784 | 1784 | 9 % |  |
| el/terms1000 | generated | hand | 107369.3 | 147455.2 | 1.37x | 147332.2 | 1.37x | -0.1% | 169104 | 169107 | 14 % |  |
| el/terms1000 | immediate | hand | 107369.3 | 122555.2 | 1.14x | 120863.5 | 1.13x | -1.4% | 177120 | 177120 | 14 % |  |
| sql/select20 | generated | hand | 6624.9 | 17562.9 | 2.65x | 17551.2 | 2.65x | -0.1% | 21448 | 21448 | 19 % |  |
| sql/refused-late | generated | hand | 2496.2 | 11836.9 | 4.74x | 11817.9 | 4.73x | -0.2% | 13552 | 13552 | 4 % |  |
