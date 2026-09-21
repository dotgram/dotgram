# Paired stand, 2026-09-21 14:24

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1224743.8 | 301043.8 | 0.25x | 312412.5 | 0.26x | +3.8% | 424449 | 424449 | 1,093 % |  |
| sql/select20.at | generated | hand | 6730.8 | 17653.4 | 2.62x | 17475.0 | 2.60x | -1.0% | 21416 | 21416 | 3 % |  |
| sql/select20.window | generated | hand | 6732.9 | 18152.5 | 2.70x | 17793.4 | 2.64x | -2.0% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 377.2 | 380.6 | 1.01x | 379.0 | 1.00x | -0.4% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 379.7 | 381.9 | 1.01x | 379.5 | 1.00x | -0.6% | 0 | 0 | 1 % |  |
| sql/select20.bool | generated | hand | 6780.4 | 18006.8 | 2.66x | 18046.2 | 2.66x | +0.2% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 54.7 | 166.4 | 3.04x | 196.6 | 3.59x | +18.1% | 160 | 160 | 6 % |  |
| sql/conditions1000 | generated | hand | 498501.2 | 1203622.7 | 2.41x | 1213718.4 | 2.43x | +0.8% | 1616203 | 1616203 | 5 % |  |
| sql/select1 | generated | hand | 645.7 | 1599.4 | 2.48x | 1542.9 | 2.39x | -3.5% | 1688 | 1688 | 3 % |  |
| sql/select20 | generated | hand | 6793.2 | 17936.2 | 2.64x | 17978.1 | 2.65x | +0.2% | 21448 | 21448 | 3 % |  |
