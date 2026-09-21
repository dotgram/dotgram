# Paired stand, 2026-09-21 14:03

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 918012.5 | 297000.0 | 0.32x | 314300.0 | 0.34x | +5.8% | 424449 | 424449 | 1,477 % |  |
| sql/select20.at | generated | hand | 7159.0 | 17536.9 | 2.45x | 17563.3 | 2.45x | +0.2% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 6869.2 | 17977.7 | 2.62x | 18050.8 | 2.63x | +0.4% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 373.8 | 373.7 | 1.00x | 377.3 | 1.01x | +1.0% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 380.8 | 374.0 | 0.98x | 372.7 | 0.98x | -0.3% | 0 | 0 | 30 % |  |
| sql/select20.bool | generated | hand | 6975.8 | 18236.2 | 2.61x | 18251.2 | 2.62x | +0.1% | 21448 | 21392 | 12 % |  |
| sql/literal | generated | hand | 59.9 | 145.2 | 2.42x | 146.6 | 2.45x | +0.9% | 160 | 160 | 21 % |  |
| sql/conditions1000 | generated | hand | 491880.1 | 1207319.1 | 2.45x | 1191091.0 | 2.42x | -1.3% | 1616203 | 1616203 | 1 % |  |
| sql/select1 | generated | hand | 651.9 | 1541.8 | 2.36x | 1508.9 | 2.31x | -2.1% | 1688 | 1688 | 7 % |  |
| sql/select20 | generated | hand | 6898.2 | 18181.3 | 2.64x | 17950.4 | 2.60x | -1.3% | 21448 | 21448 | 5 % |  |
