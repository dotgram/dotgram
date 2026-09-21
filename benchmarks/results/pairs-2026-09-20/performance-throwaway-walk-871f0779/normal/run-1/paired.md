# Paired stand, 2026-09-21 13:59

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1534668.8 | 334787.5 | 0.22x | 353150.0 | 0.23x | +5.5% | 424449 | 424449 | 555 % |  |
| sql/select20.at | generated | hand | 6887.6 | 17602.0 | 2.56x | 18628.8 | 2.70x | +5.8% | 21416 | 21416 | 29 % |  |
| sql/select20.window | generated | hand | 6787.5 | 18051.9 | 2.66x | 18083.7 | 2.66x | +0.2% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 376.7 | 380.9 | 1.01x | 379.6 | 1.01x | -0.4% | 0 | 0 | 25 % |  |
| tsql/select20.scan | generated | control | 372.7 | 380.5 | 1.02x | 379.4 | 1.02x | -0.3% | 0 | 0 | 14 % |  |
| sql/select20.bool | generated | hand | 6765.9 | 17887.9 | 2.64x | 18296.5 | 2.70x | +2.3% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 59.4 | 132.8 | 2.24x | 186.9 | 3.15x | +40.8% | 160 | 160 | 22 % |  |
| sql/conditions1000 | generated | hand | 492194.1 | 1197552.0 | 2.43x | 1217571.1 | 2.47x | +1.7% | 1616203 | 1616203 | 2 % |  |
| sql/select1 | generated | hand | 658.0 | 1536.0 | 2.33x | 1578.4 | 2.40x | +2.8% | 1688 | 1688 | 8 % |  |
| sql/select20 | generated | hand | 6888.1 | 18018.4 | 2.62x | 18376.3 | 2.67x | +2.0% | 21448 | 21448 | 11 % |  |
