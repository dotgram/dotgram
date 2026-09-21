# Paired stand, 2026-09-21 14:13

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1529393.8 | 356450.0 | 0.23x | 326137.5 | 0.21x | -8.5% | 424449 | 424449 | 885 % |  |
| sql/select20.at | generated | hand | 6885.4 | 17608.0 | 2.56x | 17950.2 | 2.61x | +1.9% | 21416 | 21416 | 60 % |  |
| sql/select20.window | generated | hand | 6697.2 | 17812.8 | 2.66x | 17926.5 | 2.68x | +0.6% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 387.2 | 382.1 | 0.99x | 383.2 | 0.99x | +0.3% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 377.2 | 382.2 | 1.01x | 381.6 | 1.01x | -0.2% | 0 | 0 | 12 % |  |
| sql/select20.bool | generated | hand | 6706.6 | 17680.2 | 2.64x | 17992.7 | 2.68x | +1.8% | 21448 | 21392 | 19 % |  |
| sql/literal | generated | hand | 54.9 | 135.3 | 2.46x | 138.3 | 2.52x | +2.2% | 160 | 160 | 1 % |  |
| sql/conditions1000 | generated | hand | 484037.5 | 1191878.9 | 2.46x | 1188945.3 | 2.46x | -0.2% | 1616203 | 1616160 | 7 % |  |
| sql/select1 | generated | hand | 650.3 | 1474.3 | 2.27x | 1483.0 | 2.28x | +0.6% | 1688 | 1688 | 4 % |  |
| sql/select20 | generated | hand | 6779.8 | 17729.6 | 2.62x | 18031.1 | 2.66x | +1.7% | 21448 | 21448 | 2 % |  |
