# Paired stand, 2026-09-21 06:47

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 171863.3 | 181475.0 | 1.06x | 183101.6 | 1.07x | +0.9% | 720048 | 720048 | 160 % |  |
| sql/select20.at | generated | hand | 6756.5 | 16972.0 | 2.51x | 17075.6 | 2.53x | +0.6% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6606.7 | 17306.6 | 2.62x | 17464.9 | 2.64x | +0.9% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 368.1 | 371.1 | 1.01x | 370.6 | 1.01x | -0.2% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 365.5 | 379.6 | 1.04x | 372.2 | 1.02x | -1.9% | 0 | 0 | 10 % |  |
| el/ladder.scan | generated | control | 135.5 | 134.5 | 0.99x | 135.9 | 1.00x | +1.1% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 938.0 | 1688.4 | 1.80x | 1663.0 | 1.77x | -1.5% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2497.7 | 11701.3 | 4.68x | 5645.0 | 2.26x | -51.8% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6652.7 | 17401.9 | 2.62x | 17522.6 | 2.63x | +0.7% | 21448 | 21392 | 10 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4339756.2 | 4582293.8 | 1.06x | 4412250.0 | 1.02x | -3.7% | 6276598 | 6276684 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6296125.0 | 6411275.0 | 1.02x | 6315787.5 | 1.00x | -1.5% | 8778600 | 8778600 | 7 % |  |
| el/ladder | generated | hand | 931.8 | 1693.4 | 1.82x | 1685.9 | 1.81x | -0.4% | 1776 | 1776 | 14 % |  |
| el/ladder | immediate | hand | 931.8 | 1139.2 | 1.22x | 1124.4 | 1.21x | -1.3% | 1784 | 1784 | 14 % |  |
| el/terms1000 | generated | hand | 108359.9 | 146662.9 | 1.35x | 145733.6 | 1.34x | -0.6% | 169104 | 169104 | 3 % |  |
| el/terms1000 | immediate | hand | 108359.9 | 120200.7 | 1.11x | 119893.5 | 1.11x | -0.3% | 177123 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6610.2 | 17361.0 | 2.63x | 17353.6 | 2.63x | 0.0% | 21448 | 21472 | 3 % |  |
| sql/refused-late | generated | hand | 2486.5 | 11696.9 | 4.70x | 11829.4 | 4.76x | +1.1% | 13552 | 13552 | 2 % |  |
