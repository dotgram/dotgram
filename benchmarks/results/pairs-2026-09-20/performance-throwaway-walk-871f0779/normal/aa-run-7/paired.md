# Paired stand, 2026-09-21 14:15

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 872331.2 | 288125.0 | 0.33x | 291237.5 | 0.33x | +1.1% | 424449 | 424449 | 1,560 % |  |
| sql/select20.at | generated | hand | 6920.4 | 17762.2 | 2.57x | 17264.1 | 2.49x | -2.8% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6815.4 | 18127.0 | 2.66x | 17575.2 | 2.58x | -3.0% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 377.1 | 375.7 | 1.00x | 371.5 | 0.98x | -1.1% | 0 | 0 | 7 % |  |
| tsql/select20.scan | generated | control | 375.8 | 374.5 | 1.00x | 374.9 | 1.00x | +0.1% | 0 | 0 | 2 % |  |
| sql/select20.bool | generated | hand | 6746.3 | 18219.8 | 2.70x | 17809.6 | 2.64x | -2.3% | 21448 | 21392 | 3 % |  |
| sql/literal | generated | hand | 58.9 | 135.1 | 2.29x | 131.5 | 2.23x | -2.6% | 160 | 160 | 20 % |  |
| sql/conditions1000 | generated | hand | 481592.6 | 1220968.4 | 2.54x | 1174557.4 | 2.44x | -3.8% | 1616203 | 1616203 | 8 % |  |
| sql/select1 | generated | hand | 637.4 | 1495.0 | 2.35x | 1468.1 | 2.30x | -1.8% | 1688 | 1688 | 11 % |  |
| sql/select20 | generated | hand | 6731.1 | 18200.6 | 2.70x | 17661.9 | 2.62x | -3.0% | 21448 | 21448 | 1 % |  |
