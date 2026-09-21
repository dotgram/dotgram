# Paired stand, 2026-09-21 14:18

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 924118.8 | 314737.5 | 0.34x | 316762.5 | 0.34x | +0.6% | 424449 | 424449 | 70 % |  |
| sql/select20.at | generated | hand | 6843.0 | 18021.5 | 2.63x | 17849.5 | 2.61x | -1.0% | 21416 | 21416 | 7 % |  |
| sql/select20.window | generated | hand | 6748.8 | 18446.9 | 2.73x | 18225.7 | 2.70x | -1.2% | 21416 | 21441 | 2 % |  |
| sql/select20.scan | generated | control | 386.2 | 378.3 | 0.98x | 376.1 | 0.97x | -0.6% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 372.5 | 374.8 | 1.01x | 377.7 | 1.01x | +0.8% | 0 | 0 | 2 % |  |
| sql/select20.bool | generated | hand | 6807.5 | 18301.4 | 2.69x | 18445.6 | 2.71x | +0.8% | 21448 | 21392 | 1 % |  |
| sql/literal | generated | hand | 58.3 | 133.6 | 2.29x | 191.1 | 3.28x | +43.0% | 160 | 160 | 1 % |  |
| sql/conditions1000 | generated | hand | 489297.3 | 1208548.4 | 2.47x | 1183234.8 | 2.42x | -2.1% | 1616179 | 1616203 | 3 % |  |
| sql/select1 | generated | hand | 643.4 | 1492.8 | 2.32x | 1528.5 | 2.38x | +2.4% | 1688 | 1688 | 4 % |  |
| sql/select20 | generated | hand | 6803.9 | 18280.9 | 2.69x | 18104.7 | 2.66x | -1.0% | 21448 | 21448 | 1 % |  |
