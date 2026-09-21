# Paired stand, 2026-09-21 14:27

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1511937.5 | 396893.8 | 0.26x | 323768.8 | 0.21x | -18.4% | 424449 | 424449 | 939 % |  |
| sql/select20.at | generated | hand | 6775.4 | 17617.3 | 2.60x | 17848.4 | 2.63x | +1.3% | 21416 | 21416 | 43 % |  |
| sql/select20.window | generated | hand | 6791.6 | 17979.5 | 2.65x | 18129.3 | 2.67x | +0.8% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 378.6 | 381.5 | 1.01x | 374.6 | 0.99x | -1.8% | 0 | 0 | 30 % |  |
| tsql/select20.scan | generated | control | 373.8 | 378.7 | 1.01x | 374.7 | 1.00x | -1.1% | 0 | 0 | 3 % |  |
| sql/select20.bool | generated | hand | 6727.3 | 18001.6 | 2.68x | 18064.0 | 2.69x | +0.3% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 52.1 | 183.1 | 3.52x | 139.8 | 2.68x | -23.7% | 160 | 160 | 15 % |  |
| sql/conditions1000 | generated | hand | 483289.1 | 1203240.6 | 2.49x | 1200057.4 | 2.48x | -0.3% | 1616203 | 1616203 | 3 % |  |
| sql/select1 | generated | hand | 632.6 | 1524.6 | 2.41x | 1473.9 | 2.33x | -3.3% | 1688 | 1688 | 5 % |  |
| sql/select20 | generated | hand | 6713.7 | 17998.9 | 2.68x | 18141.0 | 2.70x | +0.8% | 21448 | 21448 | 2 % |  |
