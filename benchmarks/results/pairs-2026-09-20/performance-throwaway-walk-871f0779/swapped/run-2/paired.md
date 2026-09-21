# Paired stand, 2026-09-21 14:25

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 856000.0 | 299912.5 | 0.35x | 296356.2 | 0.35x | -1.2% | 424449 | 424449 | 13 % |  |
| sql/select20.at | generated | hand | 6778.4 | 17573.8 | 2.59x | 17606.7 | 2.60x | +0.2% | 21416 | 21416 | 2 % |  |
| sql/select20.window | generated | hand | 6767.3 | 18026.4 | 2.66x | 18041.0 | 2.67x | +0.1% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 377.1 | 373.6 | 0.99x | 374.8 | 0.99x | +0.3% | 0 | 0 | 10 % |  |
| tsql/select20.scan | generated | control | 379.9 | 378.4 | 1.00x | 378.0 | 1.00x | -0.1% | 0 | 0 | 2 % |  |
| sql/select20.bool | generated | hand | 6690.8 | 18064.7 | 2.70x | 18050.4 | 2.70x | -0.1% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 54.7 | 192.5 | 3.52x | 138.8 | 2.54x | -27.9% | 160 | 160 | 9 % |  |
| sql/conditions1000 | generated | hand | 491312.1 | 1216165.6 | 2.48x | 1200811.3 | 2.44x | -1.3% | 1616160 | 1616203 | 1 % |  |
| sql/select1 | generated | hand | 648.3 | 1541.5 | 2.38x | 1536.4 | 2.37x | -0.3% | 1688 | 1688 | 3 % |  |
| sql/select20 | generated | hand | 6704.8 | 18075.5 | 2.70x | 18052.8 | 2.69x | -0.1% | 21448 | 21448 | 3 % |  |
