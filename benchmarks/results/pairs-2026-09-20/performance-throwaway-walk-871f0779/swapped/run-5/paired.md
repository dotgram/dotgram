# Paired stand, 2026-09-21 14:32

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1520681.2 | 309143.8 | 0.20x | 300875.0 | 0.20x | -2.7% | 424449 | 424449 | 898 % |  |
| sql/select20.at | generated | hand | 6798.9 | 17472.8 | 2.57x | 17391.7 | 2.56x | -0.5% | 21416 | 21416 | 2 % |  |
| sql/select20.window | generated | hand | 6789.2 | 17733.3 | 2.61x | 17729.2 | 2.61x | 0.0% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 372.9 | 376.9 | 1.01x | 376.0 | 1.01x | -0.2% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 382.9 | 371.1 | 0.97x | 372.7 | 0.97x | +0.4% | 0 | 0 | 2 % |  |
| sql/select20.bool | generated | hand | 6847.9 | 17929.6 | 2.62x | 17824.9 | 2.60x | -0.6% | 21448 | 21392 | 6 % |  |
| sql/literal | generated | hand | 58.9 | 195.8 | 3.32x | 134.6 | 2.28x | -31.3% | 160 | 160 | 5 % |  |
| sql/conditions1000 | generated | hand | 485592.2 | 1192411.7 | 2.46x | 1201418.0 | 2.47x | +0.8% | 1616203 | 1616160 | 2 % |  |
| sql/select1 | generated | hand | 644.7 | 1630.4 | 2.53x | 1509.6 | 2.34x | -7.4% | 1688 | 1688 | 2 % |  |
| sql/select20 | generated | hand | 6825.0 | 18007.8 | 2.64x | 17901.6 | 2.62x | -0.6% | 21448 | 21448 | 5 % |  |
