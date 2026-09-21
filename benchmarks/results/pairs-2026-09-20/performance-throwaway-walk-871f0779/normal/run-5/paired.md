# Paired stand, 2026-09-21 14:09

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1506668.8 | 324737.5 | 0.22x | 337293.8 | 0.22x | +3.9% | 424449 | 424449 | 893 % |  |
| sql/select20.at | generated | hand | 6908.3 | 17566.1 | 2.54x | 19341.2 | 2.80x | +10.1% | 21416 | 21416 | 10 % |  |
| sql/select20.window | generated | hand | 6745.8 | 18321.0 | 2.72x | 17864.2 | 2.65x | -2.5% | 21416 | 21416 | 7 % |  |
| sql/select20.scan | generated | control | 376.9 | 377.8 | 1.00x | 403.9 | 1.07x | +6.9% | 0 | 0 | 19 % |  |
| tsql/select20.scan | generated | control | 377.5 | 375.3 | 0.99x | 376.4 | 1.00x | +0.3% | 0 | 0 | 2 % |  |
| sql/select20.bool | generated | hand | 6738.3 | 17816.0 | 2.64x | 18233.3 | 2.71x | +2.3% | 21448 | 21392 | 8 % |  |
| sql/literal | generated | hand | 55.0 | 134.7 | 2.45x | 182.0 | 3.31x | +35.1% | 160 | 160 | 4 % |  |
| sql/conditions1000 | generated | hand | 483739.5 | 1184516.4 | 2.45x | 1265546.5 | 2.62x | +6.8% | 1616179 | 1616203 | 4 % |  |
| sql/select1 | generated | hand | 669.8 | 1512.3 | 2.26x | 1547.9 | 2.31x | +2.4% | 1688 | 1688 | 2 % |  |
| sql/select20 | generated | hand | 6762.2 | 17730.2 | 2.62x | 18104.9 | 2.68x | +2.1% | 21491 | 21472 | 2 % |  |
