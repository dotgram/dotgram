# Paired stand, 2026-09-21 14:23

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 893318.8 | 289637.5 | 0.32x | 291925.0 | 0.33x | +0.8% | 424449 | 424449 | 1,539 % |  |
| sql/select20.at | generated | hand | 6706.1 | 17570.7 | 2.62x | 17207.6 | 2.57x | -2.1% | 21416 | 21416 | 14 % |  |
| sql/select20.window | generated | hand | 6692.9 | 17788.0 | 2.66x | 17503.8 | 2.62x | -1.6% | 21416 | 21416 | 10 % |  |
| sql/select20.scan | generated | control | 375.0 | 376.1 | 1.00x | 379.6 | 1.01x | +0.9% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 375.5 | 376.7 | 1.00x | 375.6 | 1.00x | -0.3% | 0 | 0 | 12 % |  |
| sql/select20.bool | generated | hand | 6787.3 | 18028.9 | 2.66x | 18086.4 | 2.66x | +0.3% | 21448 | 21392 | 7 % |  |
| sql/literal | generated | hand | 55.1 | 187.4 | 3.40x | 142.0 | 2.58x | -24.2% | 160 | 160 | 13 % |  |
| sql/conditions1000 | generated | hand | 488909.0 | 1187748.4 | 2.43x | 1249928.5 | 2.56x | +5.2% | 1616203 | 1616203 | 6 % |  |
| sql/select1 | generated | hand | 643.7 | 1611.3 | 2.50x | 1479.7 | 2.30x | -8.2% | 1688 | 1688 | 5 % |  |
| sql/select20 | generated | hand | 6831.0 | 17997.9 | 2.63x | 17573.1 | 2.57x | -2.4% | 21448 | 21448 | 2 % |  |
