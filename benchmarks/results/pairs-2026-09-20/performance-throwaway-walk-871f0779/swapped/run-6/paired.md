# Paired stand, 2026-09-21 14:34

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1335275.0 | 472150.0 | 0.35x | 321537.5 | 0.24x | -31.9% | 424449 | 424449 | 1,054 % |  |
| sql/select20.at | generated | hand | 6853.4 | 17613.6 | 2.57x | 17672.4 | 2.58x | +0.3% | 21416 | 21416 | 13 % |  |
| sql/select20.window | generated | hand | 6741.2 | 17806.2 | 2.64x | 18029.4 | 2.67x | +1.3% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 376.7 | 384.6 | 1.02x | 380.0 | 1.01x | -1.2% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 382.6 | 378.8 | 0.99x | 376.3 | 0.98x | -0.7% | 0 | 0 | 3 % |  |
| sql/select20.bool | generated | hand | 6758.3 | 17995.2 | 2.66x | 18303.3 | 2.71x | +1.7% | 21448 | 21392 | 3 % |  |
| sql/literal | generated | hand | 55.0 | 185.2 | 3.37x | 145.7 | 2.65x | -21.3% | 160 | 160 | 6 % |  |
| sql/conditions1000 | generated | hand | 482399.6 | 1192881.2 | 2.47x | 1212978.9 | 2.51x | +1.7% | 1616203 | 1616203 | 6 % |  |
| sql/select1 | generated | hand | 655.1 | 1520.0 | 2.32x | 1566.7 | 2.39x | +3.1% | 1688 | 1688 | 2 % |  |
| sql/select20 | generated | hand | 6745.5 | 17950.9 | 2.66x | 18919.8 | 2.80x | +5.4% | 21448 | 21448 | 19 % |  |
