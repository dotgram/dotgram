# Paired stand, 2026-09-21 14:04

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1537962.5 | 313831.2 | 0.20x | 302912.5 | 0.20x | -3.5% | 424449 | 424449 | 907 % |  |
| sql/select20.at | generated | hand | 6728.6 | 18340.4 | 2.73x | 17967.7 | 2.67x | -2.0% | 21416 | 21416 | 10 % |  |
| sql/select20.window | generated | hand | 6754.4 | 18725.8 | 2.77x | 18137.1 | 2.69x | -3.1% | 21416 | 21416 | 19 % |  |
| sql/select20.scan | generated | control | 370.6 | 386.7 | 1.04x | 380.7 | 1.03x | -1.6% | 0 | 0 | 1 % |  |
| tsql/select20.scan | generated | control | 373.2 | 372.6 | 1.00x | 372.3 | 1.00x | -0.1% | 0 | 0 | 2 % |  |
| sql/select20.bool | generated | hand | 6769.1 | 18740.2 | 2.77x | 18468.0 | 2.73x | -1.5% | 21448 | 21392 | 3 % |  |
| sql/literal | generated | hand | 54.4 | 138.8 | 2.55x | 186.0 | 3.42x | +34.0% | 160 | 160 | 3 % |  |
| sql/conditions1000 | generated | hand | 479768.0 | 1218231.6 | 2.54x | 1192013.7 | 2.48x | -2.2% | 1616203 | 1616203 | 5 % |  |
| sql/select1 | generated | hand | 645.5 | 1531.2 | 2.37x | 1526.4 | 2.36x | -0.3% | 1688 | 1688 | 2 % |  |
| sql/select20 | generated | hand | 6753.8 | 18734.4 | 2.77x | 18214.1 | 2.70x | -2.8% | 21448 | 21448 | 2 % |  |
