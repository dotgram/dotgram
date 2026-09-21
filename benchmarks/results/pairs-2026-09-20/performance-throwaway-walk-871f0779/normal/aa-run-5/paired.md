# Paired stand, 2026-09-21 14:11

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 857750.0 | 303281.2 | 0.35x | 292143.8 | 0.34x | -3.7% | 424449 | 424449 | 74 % |  |
| sql/select20.at | generated | hand | 6817.4 | 17509.6 | 2.57x | 17401.4 | 2.55x | -0.6% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6735.2 | 17853.4 | 2.65x | 18096.8 | 2.69x | +1.4% | 21416 | 21416 | 22 % |  |
| sql/select20.scan | generated | control | 370.5 | 373.2 | 1.01x | 372.7 | 1.01x | -0.1% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 375.2 | 372.6 | 0.99x | 373.7 | 1.00x | +0.3% | 0 | 0 | 3 % |  |
| sql/select20.bool | generated | hand | 6791.4 | 17849.2 | 2.63x | 17768.8 | 2.62x | -0.5% | 21448 | 21392 | 1 % |  |
| sql/literal | generated | hand | 57.1 | 131.7 | 2.31x | 140.3 | 2.46x | +6.5% | 160 | 160 | 24 % |  |
| sql/conditions1000 | generated | hand | 482768.8 | 1171887.1 | 2.43x | 1181158.6 | 2.45x | +0.8% | 1616203 | 1616203 | 8 % |  |
| sql/select1 | generated | hand | 637.9 | 1473.7 | 2.31x | 1473.7 | 2.31x | 0.0% | 1688 | 1688 | 1 % |  |
| sql/select20 | generated | hand | 6797.5 | 17906.9 | 2.63x | 17930.6 | 2.64x | +0.1% | 21448 | 21448 | 18 % |  |
