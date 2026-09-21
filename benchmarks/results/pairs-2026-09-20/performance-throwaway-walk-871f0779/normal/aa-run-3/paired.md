# Paired stand, 2026-09-21 14:06

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1533031.2 | 340837.5 | 0.22x | 311537.5 | 0.20x | -8.6% | 424449 | 424449 | 910 % |  |
| sql/select20.at | generated | hand | 6780.5 | 17560.4 | 2.59x | 17345.7 | 2.56x | -1.2% | 21416 | 21416 | 3 % |  |
| sql/select20.window | generated | hand | 6759.9 | 18012.2 | 2.66x | 17736.0 | 2.62x | -1.5% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 371.4 | 377.9 | 1.02x | 379.3 | 1.02x | +0.4% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 380.8 | 373.8 | 0.98x | 372.6 | 0.98x | -0.3% | 0 | 0 | 6 % |  |
| sql/select20.bool | generated | hand | 6673.0 | 18001.8 | 2.70x | 17838.4 | 2.67x | -0.9% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 54.4 | 146.0 | 2.69x | 145.8 | 2.68x | -0.1% | 160 | 160 | 3 % |  |
| sql/conditions1000 | generated | hand | 483932.4 | 1191824.2 | 2.46x | 1197217.6 | 2.47x | +0.5% | 1616179 | 1616203 | 5 % |  |
| sql/select1 | generated | hand | 644.0 | 1487.9 | 2.31x | 1570.8 | 2.44x | +5.6% | 1688 | 1688 | 4 % |  |
| sql/select20 | generated | hand | 6681.0 | 18056.6 | 2.70x | 18678.5 | 2.80x | +3.4% | 21448 | 21448 | 2 % |  |
