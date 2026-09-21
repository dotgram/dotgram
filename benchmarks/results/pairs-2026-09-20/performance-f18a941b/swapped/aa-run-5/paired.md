# Paired stand, 2026-09-21 05:42

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164014.8 | 180776.6 | 1.10x | 182350.8 | 1.11x | +0.9% | 720048 | 720048 | 351 % |  |
| sql/select20.at | generated | hand | 6574.6 | 16946.9 | 2.58x | 16937.8 | 2.58x | -0.1% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6560.3 | 17465.4 | 2.66x | 17432.0 | 2.66x | -0.2% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 369.9 | 371.1 | 1.00x | 371.4 | 1.00x | +0.1% | 0 | 0 | 14 % |  |
| tsql/select20.scan | generated | control | 366.8 | 408.1 | 1.11x | 370.2 | 1.01x | -9.3% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 132.8 | 137.9 | 1.04x | 136.2 | 1.03x | -1.2% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 943.9 | 1646.8 | 1.74x | 1647.8 | 1.75x | +0.1% | 1776 | 1720 | 18 % |  |
| sql/refused-late.bool | generated | hand | 2500.8 | 11704.5 | 4.68x | 5647.5 | 2.26x | -51.7% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6650.0 | 17485.3 | 2.63x | 17429.7 | 2.62x | -0.3% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4391543.8 | 4434387.5 | 1.01x | 4489062.5 | 1.02x | +1.2% | 6276688 | 6276641 | 3 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6361100.0 | 6357081.2 | 1.00x | 6409575.0 | 1.01x | +0.8% | 8778600 | 8778600 | 7 % |  |
| el/ladder | generated | hand | 948.9 | 1654.4 | 1.74x | 1671.1 | 1.76x | +1.0% | 1776 | 1776 | 38 % |  |
| el/ladder | immediate | hand | 948.9 | 1136.8 | 1.20x | 1127.1 | 1.19x | -0.9% | 1784 | 1784 | 38 % |  |
| el/terms1000 | generated | hand | 110697.2 | 145030.0 | 1.31x | 145892.2 | 1.32x | +0.6% | 169104 | 169107 | 4 % |  |
| el/terms1000 | immediate | hand | 110697.2 | 119628.9 | 1.08x | 119933.2 | 1.08x | +0.3% | 177120 | 177120 | 4 % |  |
| sql/select20 | generated | hand | 6742.8 | 17708.1 | 2.63x | 17661.8 | 2.62x | -0.3% | 21448 | 21448 | 27 % |  |
| sql/refused-late | generated | hand | 2572.9 | 12182.8 | 4.74x | 12469.1 | 4.85x | +2.4% | 13552 | 13552 | 17 % |  |
