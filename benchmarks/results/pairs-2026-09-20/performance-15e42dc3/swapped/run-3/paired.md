# Paired stand, 2026-09-21 07:13

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166851.6 | 184551.6 | 1.11x | 185802.3 | 1.11x | +0.7% | 720048 | 720048 | 166 % |  |
| sql/select20.at | generated | hand | 6626.7 | 17156.8 | 2.59x | 17063.8 | 2.58x | -0.5% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 6638.8 | 17697.7 | 2.67x | 17369.7 | 2.62x | -1.9% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 368.5 | 366.9 | 1.00x | 367.9 | 1.00x | +0.3% | 0 | 0 | 13 % |  |
| tsql/select20.scan | generated | control | 369.9 | 371.3 | 1.00x | 368.0 | 0.99x | -0.9% | 0 | 0 | 7 % |  |
| el/ladder.scan | generated | control | 136.6 | 133.8 | 0.98x | 133.1 | 0.97x | -0.6% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 927.5 | 1650.3 | 1.78x | 1687.9 | 1.82x | +2.3% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2502.2 | 11926.1 | 4.77x | 5600.4 | 2.24x | -53.0% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6577.0 | 17553.0 | 2.67x | 17432.9 | 2.65x | -0.7% | 21448 | 21392 | 1 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4364687.5 | 4454081.2 | 1.02x | 4465206.2 | 1.02x | +0.2% | 6276555 | 6276598 | 7 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6493687.5 | 6603625.0 | 1.02x | 6370725.0 | 0.98x | -3.5% | 8778600 | 8778600 | 10 % |  |
| el/ladder | generated | hand | 941.2 | 1670.0 | 1.77x | 1702.4 | 1.81x | +1.9% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 941.2 | 1191.5 | 1.27x | 1147.6 | 1.22x | -3.7% | 1784 | 1784 | 1 % |  |
| el/terms1000 | generated | hand | 107050.8 | 146105.6 | 1.36x | 149922.8 | 1.40x | +2.6% | 169104 | 169107 | 3 % |  |
| el/terms1000 | immediate | hand | 107050.8 | 129123.3 | 1.21x | 124529.7 | 1.16x | -3.6% | 177120 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6568.6 | 17593.8 | 2.68x | 17402.8 | 2.65x | -1.1% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2494.0 | 11948.9 | 4.79x | 11773.7 | 4.72x | -1.5% | 13552 | 13552 | 5 % |  |
