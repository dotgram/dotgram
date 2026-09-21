# Paired stand, 2026-09-21 14:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 886812.5 | 289031.2 | 0.33x | 285775.0 | 0.32x | -1.1% | 424449 | 424449 | 1,524 % |  |
| sql/select20.at | generated | hand | 6805.0 | 17813.3 | 2.62x | 17988.4 | 2.64x | +1.0% | 21416 | 21416 | 23 % |  |
| sql/select20.window | generated | hand | 6747.7 | 18056.8 | 2.68x | 18069.8 | 2.68x | +0.1% | 21416 | 21416 | 7 % |  |
| sql/select20.scan | generated | control | 378.8 | 382.9 | 1.01x | 381.7 | 1.01x | -0.3% | 0 | 0 | 12 % |  |
| tsql/select20.scan | generated | control | 374.3 | 379.1 | 1.01x | 378.3 | 1.01x | -0.2% | 0 | 0 | 4 % |  |
| sql/select20.bool | generated | hand | 6806.3 | 18348.5 | 2.70x | 18377.5 | 2.70x | +0.2% | 21448 | 21392 | 11 % |  |
| sql/literal | generated | hand | 57.4 | 187.0 | 3.26x | 196.2 | 3.42x | +4.9% | 160 | 160 | 8 % |  |
| sql/conditions1000 | generated | hand | 483537.1 | 1205821.5 | 2.49x | 1260622.7 | 2.61x | +4.5% | 1616160 | 1616203 | 9 % |  |
| sql/select1 | generated | hand | 653.9 | 1540.1 | 2.36x | 1525.6 | 2.33x | -0.9% | 1688 | 1688 | 3 % |  |
| sql/select20 | generated | hand | 6749.3 | 17897.6 | 2.65x | 17984.0 | 2.66x | +0.5% | 21448 | 21448 | 20 % |  |
