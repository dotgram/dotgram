# Paired stand, 2026-09-21 14:02

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 873712.5 | 305156.2 | 0.35x | 298918.8 | 0.34x | -2.0% | 424449 | 424449 | 12 % |  |
| sql/select20.at | generated | hand | 6896.2 | 17901.1 | 2.60x | 17612.4 | 2.55x | -1.6% | 21416 | 21416 | 2 % |  |
| sql/select20.window | generated | hand | 6805.4 | 18191.1 | 2.67x | 18080.8 | 2.66x | -0.6% | 21416 | 21441 | 10 % |  |
| sql/select20.scan | generated | control | 385.3 | 378.0 | 0.98x | 375.2 | 0.97x | -0.7% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 378.1 | 375.0 | 0.99x | 378.2 | 1.00x | +0.9% | 0 | 0 | 2 % |  |
| sql/select20.bool | generated | hand | 6728.9 | 18141.0 | 2.70x | 18215.8 | 2.71x | +0.4% | 21448 | 21392 | 14 % |  |
| sql/literal | generated | hand | 54.7 | 137.9 | 2.52x | 188.9 | 3.46x | +37.0% | 160 | 160 | 3 % |  |
| sql/conditions1000 | generated | hand | 488471.9 | 1184594.1 | 2.43x | 1179198.4 | 2.41x | -0.5% | 1616203 | 1616160 | 2 % |  |
| sql/select1 | generated | hand | 641.1 | 1497.7 | 2.34x | 1536.1 | 2.40x | +2.6% | 1688 | 1688 | 2 % |  |
| sql/select20 | generated | hand | 6665.1 | 18022.9 | 2.70x | 18087.1 | 2.71x | +0.4% | 21448 | 21448 | 3 % |  |
