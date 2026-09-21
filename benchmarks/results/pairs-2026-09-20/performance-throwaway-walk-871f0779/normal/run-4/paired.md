# Paired stand, 2026-09-21 14:07

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1509925.0 | 305437.5 | 0.20x | 297118.8 | 0.20x | -2.7% | 424449 | 424449 | 934 % |  |
| sql/select20.at | generated | hand | 6755.1 | 17368.5 | 2.57x | 17603.9 | 2.61x | +1.4% | 21416 | 21416 | 44 % |  |
| sql/select20.window | generated | hand | 6813.3 | 17717.9 | 2.60x | 18092.9 | 2.66x | +2.1% | 21416 | 21416 | 12 % |  |
| sql/select20.scan | generated | control | 378.2 | 382.5 | 1.01x | 382.7 | 1.01x | +0.1% | 0 | 0 | 1 % |  |
| tsql/select20.scan | generated | control | 376.3 | 387.1 | 1.03x | 381.0 | 1.01x | -1.6% | 0 | 0 | 48 % |  |
| sql/select20.bool | generated | hand | 6711.1 | 17693.4 | 2.64x | 18040.8 | 2.69x | +2.0% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 54.4 | 136.3 | 2.51x | 186.3 | 3.43x | +36.7% | 160 | 160 | 4 % |  |
| sql/conditions1000 | generated | hand | 493497.7 | 1185698.0 | 2.40x | 1188553.9 | 2.41x | +0.2% | 1616203 | 1616203 | 2 % |  |
| sql/select1 | generated | hand | 642.8 | 1486.1 | 2.31x | 1610.3 | 2.51x | +8.4% | 1688 | 1688 | 1 % |  |
| sql/select20 | generated | hand | 6742.6 | 17762.6 | 2.63x | 18152.1 | 2.69x | +2.2% | 21448 | 21448 | 7 % |  |
