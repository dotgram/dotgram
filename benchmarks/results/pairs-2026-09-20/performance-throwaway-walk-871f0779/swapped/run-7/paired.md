# Paired stand, 2026-09-21 14:38

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1217656.2 | 300775.0 | 0.25x | 307881.2 | 0.25x | +2.4% | 424449 | 424449 | 846 % |  |
| sql/select20.at | generated | hand | 6671.2 | 17403.1 | 2.61x | 17579.4 | 2.64x | +1.0% | 21416 | 21416 | 1 % |  |
| sql/select20.window | generated | hand | 6729.3 | 18185.3 | 2.70x | 18150.2 | 2.70x | -0.2% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 379.9 | 384.4 | 1.01x | 384.8 | 1.01x | +0.1% | 0 | 0 | 12 % |  |
| tsql/select20.scan | generated | control | 382.0 | 394.4 | 1.03x | 382.3 | 1.00x | -3.1% | 0 | 0 | 7 % |  |
| sql/select20.bool | generated | hand | 6709.7 | 17827.6 | 2.66x | 17945.0 | 2.67x | +0.7% | 21448 | 21392 | 4 % |  |
| sql/literal | generated | hand | 54.4 | 183.4 | 3.37x | 147.4 | 2.71x | -19.6% | 160 | 160 | 1 % |  |
| sql/conditions1000 | generated | hand | 488709.8 | 1204372.3 | 2.46x | 1194597.3 | 2.44x | -0.8% | 1616203 | 1616160 | 9 % |  |
| sql/select1 | generated | hand | 675.6 | 1558.9 | 2.31x | 1498.9 | 2.22x | -3.8% | 1688 | 1688 | 24 % |  |
| sql/select20 | generated | hand | 6700.4 | 17829.3 | 2.66x | 17934.8 | 2.68x | +0.6% | 21448 | 21448 | 2 % |  |
