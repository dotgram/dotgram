# Paired stand, 2026-09-21 14:16

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 859593.8 | 300268.8 | 0.35x | 293925.0 | 0.34x | -2.1% | 424449 | 424449 | 15 % |  |
| sql/select20.at | generated | hand | 6726.3 | 17229.3 | 2.56x | 17258.6 | 2.57x | +0.2% | 21416 | 21416 | 8 % |  |
| sql/select20.window | generated | hand | 6794.3 | 17577.9 | 2.59x | 18608.3 | 2.74x | +5.9% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 377.6 | 378.1 | 1.00x | 379.2 | 1.00x | +0.3% | 0 | 0 | 27 % |  |
| tsql/select20.scan | generated | control | 376.0 | 381.7 | 1.01x | 386.5 | 1.03x | +1.3% | 0 | 0 | 4 % |  |
| sql/select20.bool | generated | hand | 6797.0 | 17553.5 | 2.58x | 17817.0 | 2.62x | +1.5% | 21448 | 21392 | 3 % |  |
| sql/literal | generated | hand | 54.7 | 136.2 | 2.49x | 183.8 | 3.36x | +35.0% | 160 | 160 | 14 % |  |
| sql/conditions1000 | generated | hand | 480893.0 | 1167917.6 | 2.43x | 1183914.8 | 2.46x | +1.4% | 1616203 | 1616160 | 1 % |  |
| sql/select1 | generated | hand | 649.1 | 1476.4 | 2.27x | 1521.0 | 2.34x | +3.0% | 1688 | 1688 | 2 % |  |
| sql/select20 | generated | hand | 6793.3 | 17556.4 | 2.58x | 17620.5 | 2.59x | +0.4% | 21448 | 21448 | 6 % |  |
