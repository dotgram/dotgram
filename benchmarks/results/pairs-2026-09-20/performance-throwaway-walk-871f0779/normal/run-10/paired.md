# Paired stand, 2026-09-21 14:21

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1528562.5 | 308068.8 | 0.20x | 377587.5 | 0.25x | +22.6% | 424449 | 424449 | 919 % |  |
| sql/select20.at | generated | hand | 6691.1 | 17662.1 | 2.64x | 17570.3 | 2.63x | -0.5% | 21416 | 21416 | 34 % |  |
| sql/select20.window | generated | hand | 6671.4 | 17731.9 | 2.66x | 17832.3 | 2.67x | +0.6% | 21416 | 21441 | 2 % |  |
| sql/select20.scan | generated | control | 373.7 | 381.5 | 1.02x | 377.5 | 1.01x | -1.0% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 375.6 | 381.7 | 1.02x | 379.3 | 1.01x | -0.6% | 0 | 0 | 3 % |  |
| sql/select20.bool | generated | hand | 6646.6 | 17848.9 | 2.69x | 17943.3 | 2.70x | +0.5% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 54.8 | 132.4 | 2.42x | 197.8 | 3.61x | +49.4% | 160 | 160 | 2 % |  |
| sql/conditions1000 | generated | hand | 487347.3 | 1175472.3 | 2.41x | 1185729.3 | 2.43x | +0.9% | 1616203 | 1616160 | 3 % |  |
| sql/select1 | generated | hand | 654.2 | 1513.3 | 2.31x | 1519.0 | 2.32x | +0.4% | 1688 | 1688 | 4 % |  |
| sql/select20 | generated | hand | 6683.5 | 17960.5 | 2.69x | 17878.3 | 2.67x | -0.5% | 21448 | 21448 | 2 % |  |
