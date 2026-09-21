# Paired stand, 2026-09-21 14:31

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.8 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 895306.2 | 297750.0 | 0.33x | 305200.0 | 0.34x | +2.5% | 424449 | 424449 | 1,538 % |  |
| sql/select20.at | generated | hand | 6853.2 | 17517.6 | 2.56x | 17532.4 | 2.56x | +0.1% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 7345.9 | 17714.1 | 2.41x | 17805.0 | 2.42x | +0.5% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 432.4 | 372.2 | 0.86x | 372.7 | 0.86x | +0.2% | 0 | 0 | 18 % |  |
| tsql/select20.scan | generated | control | 374.5 | 373.0 | 1.00x | 371.9 | 0.99x | -0.3% | 0 | 0 | 1 % |  |
| sql/select20.bool | generated | hand | 6911.7 | 17662.5 | 2.56x | 17996.8 | 2.60x | +1.9% | 21448 | 21392 | 15 % |  |
| sql/literal | generated | hand | 55.2 | 198.4 | 3.60x | 181.9 | 3.30x | -8.3% | 160 | 160 | 17 % |  |
| sql/conditions1000 | generated | hand | 484080.5 | 1189641.0 | 2.46x | 1186801.2 | 2.45x | -0.2% | 1616179 | 1616203 | 43 % |  |
| sql/select1 | generated | hand | 644.4 | 1500.8 | 2.33x | 1502.1 | 2.33x | +0.1% | 1688 | 1688 | 2 % |  |
| sql/select20 | generated | hand | 6875.7 | 17693.8 | 2.57x | 17847.6 | 2.60x | +0.9% | 21448 | 21448 | 5 % |  |
