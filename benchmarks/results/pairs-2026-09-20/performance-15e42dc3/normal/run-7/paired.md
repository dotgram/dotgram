# Paired stand, 2026-09-21 06:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 170769.5 | 182664.1 | 1.07x | 184481.2 | 1.08x | +1.0% | 720048 | 720048 | 341 % |  |
| sql/select20.at | generated | hand | 6643.9 | 17522.2 | 2.64x | 17139.5 | 2.58x | -2.2% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6746.9 | 17746.2 | 2.63x | 17480.9 | 2.59x | -1.5% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 384.5 | 367.6 | 0.96x | 368.1 | 0.96x | +0.1% | 0 | 0 | 11 % |  |
| tsql/select20.scan | generated | control | 368.9 | 372.9 | 1.01x | 370.3 | 1.00x | -0.7% | 0 | 0 | 12 % |  |
| el/ladder.scan | generated | control | 136.3 | 133.2 | 0.98x | 133.4 | 0.98x | +0.1% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 955.2 | 1706.3 | 1.79x | 1633.0 | 1.71x | -4.3% | 1776 | 1720 | 15 % |  |
| sql/refused-late.bool | generated | hand | 2519.6 | 11882.0 | 4.72x | 5597.2 | 2.22x | -52.9% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6699.7 | 17724.2 | 2.65x | 17684.2 | 2.64x | -0.2% | 21448 | 21392 | 11 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4356462.5 | 4781431.2 | 1.10x | 4396306.2 | 1.01x | -8.1% | 6276660 | 6276684 | 18 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6236837.5 | 6339137.5 | 1.02x | 6243700.0 | 1.00x | -1.5% | 8778556 | 8778600 | 4 % |  |
| el/ladder | generated | hand | 953.2 | 1713.1 | 1.80x | 1668.0 | 1.75x | -2.6% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 953.2 | 1180.8 | 1.24x | 1141.6 | 1.20x | -3.3% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 108014.9 | 147382.5 | 1.36x | 148796.9 | 1.38x | +1.0% | 169104 | 169131 | 4 % |  |
| el/terms1000 | immediate | hand | 108014.9 | 125349.0 | 1.16x | 122252.3 | 1.13x | -2.5% | 177120 | 177120 | 4 % |  |
| sql/select20 | generated | hand | 6706.8 | 17693.3 | 2.64x | 17448.9 | 2.60x | -1.4% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2519.3 | 11869.1 | 4.71x | 11716.7 | 4.65x | -1.3% | 13552 | 13552 | 2 % |  |
