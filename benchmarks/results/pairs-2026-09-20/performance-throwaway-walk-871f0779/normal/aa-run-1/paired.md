# Paired stand, 2026-09-21 14:00

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 872643.8 | 287381.2 | 0.33x | 289131.2 | 0.33x | +0.6% | 424449 | 424449 | 1,574 % |  |
| sql/select20.at | generated | hand | 6736.6 | 18953.1 | 2.81x | 17805.5 | 2.64x | -6.1% | 21416 | 21416 | 10 % |  |
| sql/select20.window | generated | hand | 6813.0 | 19118.4 | 2.81x | 17938.9 | 2.63x | -6.2% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 378.4 | 380.5 | 1.01x | 380.3 | 1.00x | -0.1% | 0 | 0 | 13 % |  |
| tsql/select20.scan | generated | control | 375.8 | 379.0 | 1.01x | 379.1 | 1.01x | 0.0% | 0 | 0 | 15 % |  |
| sql/select20.bool | generated | hand | 6904.2 | 18947.3 | 2.74x | 17725.5 | 2.57x | -6.4% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 59.3 | 143.8 | 2.42x | 137.9 | 2.33x | -4.1% | 160 | 160 | 3 % |  |
| sql/conditions1000 | generated | hand | 486982.0 | 1176550.0 | 2.42x | 1174228.5 | 2.41x | -0.2% | 1616203 | 1616160 | 4 % |  |
| sql/select1 | generated | hand | 656.4 | 1590.0 | 2.42x | 1488.1 | 2.27x | -6.4% | 1688 | 1688 | 29 % |  |
| sql/select20 | generated | hand | 6892.5 | 18886.2 | 2.74x | 17832.1 | 2.59x | -5.6% | 21448 | 21448 | 10 % |  |
