# Paired stand, 2026-09-21 14:26

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 853443.8 | 306893.8 | 0.36x | 300937.5 | 0.35x | -1.9% | 424449 | 424449 | 23 % |  |
| sql/select20.at | generated | hand | 6717.7 | 17718.6 | 2.64x | 17546.5 | 2.61x | -1.0% | 21416 | 21416 | 2 % |  |
| sql/select20.window | generated | hand | 6854.6 | 18090.9 | 2.64x | 17997.1 | 2.63x | -0.5% | 21416 | 21416 | 10 % |  |
| sql/select20.scan | generated | control | 376.4 | 384.6 | 1.02x | 380.2 | 1.01x | -1.1% | 0 | 0 | 15 % |  |
| tsql/select20.scan | generated | control | 388.8 | 397.4 | 1.02x | 380.1 | 0.98x | -4.3% | 0 | 0 | 3 % |  |
| sql/select20.bool | generated | hand | 6799.4 | 18063.9 | 2.66x | 18080.2 | 2.66x | +0.1% | 21448 | 21392 | 3 % |  |
| sql/literal | generated | hand | 54.3 | 197.1 | 3.63x | 184.6 | 3.40x | -6.3% | 160 | 160 | 1 % |  |
| sql/conditions1000 | generated | hand | 484815.2 | 1186650.0 | 2.45x | 1180343.0 | 2.43x | -0.5% | 1616203 | 1616203 | 2 % |  |
| sql/select1 | generated | hand | 655.3 | 1546.5 | 2.36x | 1524.9 | 2.33x | -1.4% | 1688 | 1688 | 4 % |  |
| sql/select20 | generated | hand | 6788.3 | 18023.1 | 2.66x | 18185.0 | 2.68x | +0.9% | 21448 | 21448 | 2 % |  |
