# Paired stand, 2026-09-21 14:12

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 863500.0 | 287806.2 | 0.33x | 297575.0 | 0.34x | +3.4% | 424449 | 424449 | 1,353 % |  |
| sql/select20.at | generated | hand | 6821.0 | 17278.8 | 2.53x | 17486.7 | 2.56x | +1.2% | 21416 | 21416 | 26 % |  |
| sql/select20.window | generated | hand | 6817.7 | 17704.4 | 2.60x | 17887.6 | 2.62x | +1.0% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 379.7 | 377.4 | 0.99x | 378.9 | 1.00x | +0.4% | 0 | 0 | 36 % |  |
| tsql/select20.scan | generated | control | 375.8 | 377.4 | 1.00x | 377.0 | 1.00x | -0.1% | 0 | 0 | 4 % |  |
| sql/select20.bool | generated | hand | 6894.7 | 17973.8 | 2.61x | 18144.9 | 2.63x | +1.0% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 59.9 | 139.5 | 2.33x | 178.4 | 2.98x | +27.9% | 160 | 160 | 2 % |  |
| sql/conditions1000 | generated | hand | 485820.7 | 1179930.9 | 2.43x | 1206858.2 | 2.48x | +2.3% | 1616203 | 1616203 | 3 % |  |
| sql/select1 | generated | hand | 657.8 | 1490.9 | 2.27x | 1536.4 | 2.34x | +3.1% | 1688 | 1688 | 3 % |  |
| sql/select20 | generated | hand | 6894.1 | 17990.5 | 2.61x | 18142.0 | 2.63x | +0.8% | 21448 | 21448 | 2 % |  |
