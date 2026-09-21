# Paired stand, 2026-09-21 05:32

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 163840.6 | 181856.2 | 1.11x | 182947.7 | 1.12x | +0.6% | 720048 | 720048 | 354 % |  |
| sql/select20.at | generated | hand | 6680.2 | 16997.5 | 2.54x | 17327.7 | 2.59x | +1.9% | 21416 | 21416 | 18 % |  |
| sql/select20.window | generated | hand | 6646.9 | 17300.1 | 2.60x | 17524.8 | 2.64x | +1.3% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 366.0 | 366.4 | 1.00x | 369.7 | 1.01x | +0.9% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 373.2 | 369.3 | 0.99x | 371.5 | 1.00x | +0.6% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 135.0 | 135.3 | 1.00x | 135.6 | 1.00x | +0.2% | 0 | 0 | 17 % |  |
| el/ladder.bool | generated | hand | 942.1 | 1644.8 | 1.75x | 1681.5 | 1.78x | +2.2% | 1776 | 1720 | 18 % |  |
| sql/refused-late.bool | generated | hand | 2483.3 | 11676.4 | 4.70x | 5631.5 | 2.27x | -51.8% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6596.0 | 17465.3 | 2.65x | 17604.2 | 2.67x | +0.8% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4285662.5 | 4373906.2 | 1.02x | 4473850.0 | 1.04x | +2.3% | 6276684 | 6276684 | 9 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6434750.0 | 6314743.8 | 0.98x | 6266568.8 | 0.97x | -0.8% | 8778600 | 8778600 | 20 % |  |
| el/ladder | generated | hand | 942.3 | 1645.5 | 1.75x | 1699.2 | 1.80x | +3.3% | 1776 | 1776 | 14 % |  |
| el/ladder | immediate | hand | 942.3 | 1129.9 | 1.20x | 1125.5 | 1.19x | -0.4% | 1784 | 1784 | 14 % |  |
| el/terms1000 | generated | hand | 108882.3 | 146419.4 | 1.34x | 147592.2 | 1.36x | +0.8% | 169104 | 169131 | 6 % |  |
| el/terms1000 | immediate | hand | 108882.3 | 123058.7 | 1.13x | 120350.3 | 1.11x | -2.2% | 177120 | 177120 | 6 % |  |
| sql/select20 | generated | hand | 6643.8 | 17316.3 | 2.61x | 17548.1 | 2.64x | +1.3% | 21448 | 21448 | 7 % |  |
| sql/refused-late | generated | hand | 2471.0 | 11653.2 | 4.72x | 11780.3 | 4.77x | +1.1% | 13552 | 13552 | 9 % |  |
