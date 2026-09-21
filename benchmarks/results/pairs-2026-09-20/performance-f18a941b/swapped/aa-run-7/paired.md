# Paired stand, 2026-09-21 05:47

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165640.6 | 180978.1 | 1.09x | 188339.1 | 1.14x | +4.1% | 720048 | 720048 | 21 % |  |
| sql/select20.at | generated | hand | 6588.0 | 17070.1 | 2.59x | 16972.9 | 2.58x | -0.6% | 21416 | 21416 | 29 % |  |
| sql/select20.window | generated | hand | 6610.4 | 17359.7 | 2.63x | 17307.8 | 2.62x | -0.3% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 364.0 | 374.2 | 1.03x | 371.0 | 1.02x | -0.8% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 367.6 | 372.9 | 1.01x | 376.7 | 1.02x | +1.0% | 0 | 0 | 11 % |  |
| el/ladder.scan | generated | control | 136.2 | 135.3 | 0.99x | 135.2 | 0.99x | 0.0% | 0 | 0 | 5 % |  |
| el/ladder.bool | generated | hand | 938.1 | 1645.1 | 1.75x | 1627.7 | 1.74x | -1.1% | 1776 | 1720 | 14 % |  |
| sql/refused-late.bool | generated | hand | 2486.9 | 11926.0 | 4.80x | 5589.7 | 2.25x | -53.1% | 13552 | 6616 | 9 % |  |
| sql/select20.bool | generated | hand | 6650.3 | 17343.7 | 2.61x | 17462.4 | 2.63x | +0.7% | 21448 | 21392 | 6 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4371856.2 | 4621812.5 | 1.06x | 4402343.8 | 1.01x | -4.7% | 6276598 | 6276684 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6255025.0 | 6553212.5 | 1.05x | 6267550.0 | 1.00x | -4.4% | 8778600 | 8778600 | 6 % |  |
| el/ladder | generated | hand | 931.0 | 1653.5 | 1.78x | 1644.1 | 1.77x | -0.6% | 1776 | 1776 | 7 % |  |
| el/ladder | immediate | hand | 931.0 | 1139.3 | 1.22x | 1125.7 | 1.21x | -1.2% | 1784 | 1784 | 7 % |  |
| el/terms1000 | generated | hand | 106958.4 | 147278.8 | 1.38x | 145834.0 | 1.36x | -1.0% | 169104 | 169128 | 2 % |  |
| el/terms1000 | immediate | hand | 106958.4 | 121232.6 | 1.13x | 120388.2 | 1.13x | -0.7% | 177123 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6673.5 | 17291.4 | 2.59x | 17315.8 | 2.59x | +0.1% | 21448 | 21448 | 10 % |  |
| sql/refused-late | generated | hand | 2485.4 | 11835.5 | 4.76x | 11687.6 | 4.70x | -1.2% | 13552 | 13552 | 3 % |  |
