# Paired stand, 2026-09-21 05:45

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165695.3 | 181234.4 | 1.09x | 182821.9 | 1.10x | +0.9% | 720048 | 720048 | 19 % |  |
| sql/select20.at | generated | hand | 6598.8 | 17186.2 | 2.60x | 17569.7 | 2.66x | +2.2% | 21416 | 21416 | 18 % |  |
| sql/select20.window | generated | hand | 6641.0 | 17307.7 | 2.61x | 17928.5 | 2.70x | +3.6% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 375.5 | 376.2 | 1.00x | 374.2 | 1.00x | -0.5% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 371.4 | 378.5 | 1.02x | 379.3 | 1.02x | +0.2% | 0 | 0 | 7 % |  |
| el/ladder.scan | generated | control | 134.8 | 137.5 | 1.02x | 138.1 | 1.02x | +0.4% | 0 | 0 | 2 % |  |
| el/ladder.bool | generated | hand | 937.5 | 1681.8 | 1.79x | 1639.8 | 1.75x | -2.5% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2499.3 | 11792.5 | 4.72x | 5900.0 | 2.36x | -50.0% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6597.4 | 17441.6 | 2.64x | 17971.5 | 2.72x | +3.0% | 21448 | 21392 | 53 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4312850.0 | 4553562.5 | 1.06x | 4404843.8 | 1.02x | -3.3% | 6276617 | 6276555 | 7 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6224700.0 | 6293381.2 | 1.01x | 6669462.5 | 1.07x | +6.0% | 8778600 | 8778600 | 10 % |  |
| el/ladder | generated | hand | 947.8 | 1675.3 | 1.77x | 1662.1 | 1.75x | -0.8% | 1776 | 1776 | 18 % |  |
| el/ladder | immediate | hand | 947.8 | 1163.0 | 1.23x | 1144.8 | 1.21x | -1.6% | 1784 | 1784 | 18 % |  |
| el/terms1000 | generated | hand | 106052.1 | 147186.0 | 1.39x | 146215.6 | 1.38x | -0.7% | 169104 | 169107 | 5 % |  |
| el/terms1000 | immediate | hand | 106052.1 | 122588.6 | 1.16x | 121931.0 | 1.15x | -0.5% | 177120 | 177120 | 5 % |  |
| sql/select20 | generated | hand | 6541.5 | 17263.4 | 2.64x | 17828.3 | 2.73x | +3.3% | 21448 | 21448 | 1 % |  |
| sql/refused-late | generated | hand | 2505.5 | 11732.9 | 4.68x | 12212.0 | 4.87x | +4.1% | 13552 | 13552 | 8 % |  |
