# Paired stand, 2026-09-21 14:44

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1498400.0 | 303306.2 | 0.20x | 310837.5 | 0.21x | +2.5% | 424449 | 424449 | 918 % |  |
| sql/select20.at | generated | hand | 6690.8 | 17594.2 | 2.63x | 17817.1 | 2.66x | +1.3% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 6722.9 | 18000.4 | 2.68x | 17908.1 | 2.66x | -0.5% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 376.6 | 378.5 | 1.00x | 387.0 | 1.03x | +2.2% | 0 | 0 | 27 % |  |
| tsql/select20.scan | generated | control | 374.9 | 377.0 | 1.01x | 377.5 | 1.01x | +0.1% | 0 | 0 | 2 % |  |
| sql/select20.bool | generated | hand | 6776.7 | 18070.2 | 2.67x | 17978.4 | 2.65x | -0.5% | 21448 | 21392 | 7 % |  |
| sql/literal | generated | hand | 57.0 | 183.9 | 3.22x | 137.8 | 2.42x | -25.1% | 160 | 160 | 7 % |  |
| sql/conditions1000 | generated | hand | 486417.2 | 1187399.6 | 2.44x | 1227588.7 | 2.52x | +3.4% | 1616203 | 1616203 | 8 % |  |
| sql/select1 | generated | hand | 661.6 | 1538.5 | 2.33x | 1513.0 | 2.29x | -1.7% | 1688 | 1688 | 6 % |  |
| sql/select20 | generated | hand | 6910.5 | 18164.9 | 2.63x | 18201.0 | 2.63x | +0.2% | 21448 | 21448 | 8 % |  |
