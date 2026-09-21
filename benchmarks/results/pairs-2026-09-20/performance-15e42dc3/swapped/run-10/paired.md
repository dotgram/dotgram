# Paired stand, 2026-09-21 07:31

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164421.9 | 179870.3 | 1.09x | 181376.6 | 1.10x | +0.8% | 720048 | 720048 | 14 % |  |
| sql/select20.at | generated | hand | 6645.7 | 17408.8 | 2.62x | 17389.0 | 2.62x | -0.1% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6597.3 | 17658.9 | 2.68x | 17688.1 | 2.68x | +0.2% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 371.9 | 366.3 | 0.98x | 379.3 | 1.02x | +3.6% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 365.9 | 367.1 | 1.00x | 424.8 | 1.16x | +15.7% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 134.9 | 134.2 | 0.99x | 133.7 | 0.99x | -0.4% | 0 | 0 | 14 % |  |
| el/ladder.bool | generated | hand | 972.2 | 1686.2 | 1.73x | 1664.4 | 1.71x | -1.3% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2531.9 | 11904.3 | 4.70x | 5740.2 | 2.27x | -51.8% | 13552 | 6616 | 11 % |  |
| sql/select20.bool | generated | hand | 6667.0 | 17594.7 | 2.64x | 17765.9 | 2.66x | +1.0% | 21448 | 21392 | 5 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4655775.0 | 4339675.0 | 0.93x | 4660800.0 | 1.00x | +7.4% | 6276644 | 6276643 | 22 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6378456.2 | 6469775.0 | 1.01x | 6413993.8 | 1.01x | -0.9% | 8778600 | 8778600 | 9 % |  |
| el/ladder | generated | hand | 957.5 | 1685.6 | 1.76x | 1683.0 | 1.76x | -0.2% | 1776 | 1776 | 11 % |  |
| el/ladder | immediate | hand | 957.5 | 1145.7 | 1.20x | 1135.8 | 1.19x | -0.9% | 1784 | 1784 | 11 % |  |
| el/terms1000 | generated | hand | 108779.0 | 149262.0 | 1.37x | 146360.4 | 1.35x | -1.9% | 169104 | 169128 | 2 % |  |
| el/terms1000 | immediate | hand | 108779.0 | 124466.1 | 1.14x | 121065.0 | 1.11x | -2.7% | 177123 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6698.7 | 17545.5 | 2.62x | 17807.7 | 2.66x | +1.5% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2541.6 | 11864.9 | 4.67x | 12042.2 | 4.74x | +1.5% | 13552 | 13552 | 16 % |  |
