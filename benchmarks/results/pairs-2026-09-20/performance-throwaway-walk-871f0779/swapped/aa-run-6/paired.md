# Paired stand, 2026-09-21 14:36

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 885325.0 | 293956.2 | 0.33x | 287912.5 | 0.33x | -2.1% | 424449 | 424449 | 1,536 % |  |
| sql/select20.at | generated | hand | 6683.0 | 17353.2 | 2.60x | 17453.8 | 2.61x | +0.6% | 21416 | 21416 | 34 % |  |
| sql/select20.window | generated | hand | 6690.4 | 17792.2 | 2.66x | 17894.1 | 2.67x | +0.6% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 374.7 | 378.2 | 1.01x | 393.3 | 1.05x | +4.0% | 0 | 0 | 13 % |  |
| tsql/select20.scan | generated | control | 375.0 | 377.7 | 1.01x | 380.1 | 1.01x | +0.6% | 0 | 0 | 5 % |  |
| sql/select20.bool | generated | hand | 6718.6 | 17713.8 | 2.64x | 17928.5 | 2.67x | +1.2% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 54.2 | 193.6 | 3.57x | 180.2 | 3.32x | -6.9% | 160 | 160 | 3 % |  |
| sql/conditions1000 | generated | hand | 478088.7 | 1170945.7 | 2.45x | 1205754.7 | 2.52x | +3.0% | 1616160 | 1616203 | 2 % |  |
| sql/select1 | generated | hand | 646.8 | 1508.0 | 2.33x | 1537.2 | 2.38x | +1.9% | 1688 | 1688 | 23 % |  |
| sql/select20 | generated | hand | 6755.2 | 17703.3 | 2.62x | 17943.4 | 2.66x | +1.4% | 21448 | 21448 | 1 % |  |
