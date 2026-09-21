# Paired stand, 2026-09-21 06:53

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164472.7 | 182667.2 | 1.11x | 183711.7 | 1.12x | +0.6% | 720048 | 720048 | 166 % |  |
| sql/select20.at | generated | hand | 6684.5 | 17200.0 | 2.57x | 16919.6 | 2.53x | -1.6% | 21416 | 21416 | 29 % |  |
| sql/select20.window | generated | hand | 6632.8 | 17594.0 | 2.65x | 17522.2 | 2.64x | -0.4% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 370.6 | 371.6 | 1.00x | 371.0 | 1.00x | -0.2% | 0 | 0 | 25 % |  |
| tsql/select20.scan | generated | control | 366.7 | 372.0 | 1.01x | 369.5 | 1.01x | -0.7% | 0 | 0 | 13 % |  |
| el/ladder.scan | generated | control | 134.5 | 141.7 | 1.05x | 136.2 | 1.01x | -3.9% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 974.0 | 1695.4 | 1.74x | 1633.9 | 1.68x | -3.6% | 1776 | 1720 | 7 % |  |
| sql/refused-late.bool | generated | hand | 2501.0 | 11767.3 | 4.70x | 5619.0 | 2.25x | -52.2% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6658.7 | 17610.2 | 2.64x | 17354.1 | 2.61x | -1.5% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4360812.5 | 4416868.8 | 1.01x | 4379887.5 | 1.00x | -0.8% | 6276550 | 6276684 | 7 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6479725.0 | 6424906.2 | 0.99x | 6618012.5 | 1.02x | +3.0% | 8778600 | 8778600 | 10 % |  |
| el/ladder | generated | hand | 964.3 | 1705.8 | 1.77x | 1668.5 | 1.73x | -2.2% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 964.3 | 1128.2 | 1.17x | 1197.1 | 1.24x | +6.1% | 1784 | 1784 | 1 % |  |
| el/terms1000 | generated | hand | 108206.2 | 145238.1 | 1.34x | 148397.2 | 1.37x | +2.2% | 169107 | 169104 | 6 % |  |
| el/terms1000 | immediate | hand | 108206.2 | 118880.3 | 1.10x | 136422.9 | 1.26x | +14.8% | 177120 | 177120 | 6 % |  |
| sql/select20 | generated | hand | 6581.1 | 17598.6 | 2.67x | 17417.2 | 2.65x | -1.0% | 21448 | 21448 | 8 % |  |
| sql/refused-late | generated | hand | 2502.9 | 11783.6 | 4.71x | 11746.5 | 4.69x | -0.3% | 13552 | 13552 | 6 % |  |
