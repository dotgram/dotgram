# Paired stand, 2026-09-21 14:42

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1540606.2 | 353243.8 | 0.23x | 410993.8 | 0.27x | +16.3% | 424449 | 424449 | 907 % |  |
| sql/select20.at | generated | hand | 6838.3 | 17796.9 | 2.60x | 17595.8 | 2.57x | -1.1% | 21416 | 21416 | 61 % |  |
| sql/select20.window | generated | hand | 6845.5 | 18324.0 | 2.68x | 18150.0 | 2.65x | -0.9% | 21416 | 21416 | 7 % |  |
| sql/select20.scan | generated | control | 373.8 | 387.2 | 1.04x | 377.4 | 1.01x | -2.5% | 0 | 0 | 9 % |  |
| tsql/select20.scan | generated | control | 386.0 | 379.1 | 0.98x | 387.5 | 1.00x | +2.2% | 0 | 0 | 18 % |  |
| sql/select20.bool | generated | hand | 7010.4 | 18784.9 | 2.68x | 18288.2 | 2.61x | -2.6% | 21448 | 21392 | 12 % |  |
| sql/literal | generated | hand | 56.8 | 195.5 | 3.45x | 140.6 | 2.48x | -28.1% | 160 | 160 | 17 % |  |
| sql/conditions1000 | generated | hand | 483605.5 | 1242366.8 | 2.57x | 1183398.4 | 2.45x | -4.7% | 1616203 | 1616203 | 8 % |  |
| sql/select1 | generated | hand | 641.9 | 1529.3 | 2.38x | 1477.7 | 2.30x | -3.4% | 1688 | 1688 | 21 % |  |
| sql/select20 | generated | hand | 6784.7 | 18259.4 | 2.69x | 18302.8 | 2.70x | +0.2% | 21448 | 21448 | 8 % |  |
