# Paired stand, 2026-09-21 14:08

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 852293.8 | 293081.2 | 0.34x | 294662.5 | 0.35x | +0.5% | 424449 | 424449 | 22 % |  |
| sql/select20.at | generated | hand | 6824.5 | 17326.6 | 2.54x | 18731.3 | 2.74x | +8.1% | 21416 | 21416 | 2 % |  |
| sql/select20.window | generated | hand | 6803.3 | 17726.2 | 2.61x | 17609.4 | 2.59x | -0.7% | 21416 | 21416 | 6 % |  |
| sql/select20.scan | generated | control | 380.6 | 380.3 | 1.00x | 381.8 | 1.00x | +0.4% | 0 | 0 | 12 % |  |
| tsql/select20.scan | generated | control | 371.8 | 384.4 | 1.03x | 381.1 | 1.03x | -0.9% | 0 | 0 | 1 % |  |
| sql/select20.bool | generated | hand | 6782.2 | 17840.3 | 2.63x | 17822.9 | 2.63x | -0.1% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 54.9 | 135.0 | 2.46x | 144.2 | 2.62x | +6.8% | 160 | 160 | 2 % |  |
| sql/conditions1000 | generated | hand | 490134.4 | 1172250.4 | 2.39x | 1178746.9 | 2.40x | +0.6% | 1616160 | 1616203 | 5 % |  |
| sql/select1 | generated | hand | 650.5 | 1470.6 | 2.26x | 1466.2 | 2.25x | -0.3% | 1688 | 1688 | 11 % |  |
| sql/select20 | generated | hand | 6787.2 | 17909.5 | 2.64x | 17892.5 | 2.64x | -0.1% | 21448 | 21448 | 2 % |  |
