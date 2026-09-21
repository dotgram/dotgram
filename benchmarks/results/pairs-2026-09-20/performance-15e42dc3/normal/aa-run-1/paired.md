# Paired stand, 2026-09-21 06:43

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 171407.0 | 182247.7 | 1.06x | 188196.9 | 1.10x | +3.3% | 720048 | 720048 | 53 % |  |
| sql/select20.at | generated | hand | 6543.4 | 17160.6 | 2.62x | 17062.3 | 2.61x | -0.6% | 21416 | 21416 | 19 % |  |
| sql/select20.window | generated | hand | 6622.6 | 17502.3 | 2.64x | 18441.3 | 2.78x | +5.4% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 373.4 | 364.0 | 0.97x | 365.9 | 0.98x | +0.5% | 0 | 0 | 11 % |  |
| tsql/select20.scan | generated | control | 368.2 | 367.3 | 1.00x | 372.5 | 1.01x | +1.4% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 136.0 | 132.4 | 0.97x | 133.7 | 0.98x | +1.0% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 927.3 | 1650.8 | 1.78x | 1612.9 | 1.74x | -2.3% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2478.3 | 11810.8 | 4.77x | 5621.0 | 2.27x | -52.4% | 13552 | 6616 | 8 % |  |
| sql/select20.bool | generated | hand | 6561.0 | 17867.8 | 2.72x | 17581.6 | 2.68x | -1.6% | 21448 | 21392 | 2 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4626556.2 | 4312250.0 | 0.93x | 4416075.0 | 0.95x | +2.4% | 6276684 | 6276684 | 7 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6504387.5 | 6592137.5 | 1.01x | 6332143.8 | 0.97x | -3.9% | 8778600 | 8778600 | 6 % |  |
| el/ladder | generated | hand | 923.4 | 1655.7 | 1.79x | 1644.8 | 1.78x | -0.7% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 923.4 | 1127.7 | 1.22x | 1165.5 | 1.26x | +3.4% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 108605.4 | 147656.6 | 1.36x | 146743.4 | 1.35x | -0.6% | 169104 | 169104 | 2 % |  |
| el/terms1000 | immediate | hand | 108605.4 | 120884.9 | 1.11x | 118532.1 | 1.09x | -1.9% | 177120 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6632.2 | 17869.6 | 2.69x | 17663.2 | 2.66x | -1.2% | 21448 | 21448 | 14 % |  |
| sql/refused-late | generated | hand | 2501.2 | 11845.2 | 4.74x | 11792.8 | 4.71x | -0.4% | 13552 | 13552 | 11 % |  |
