# Paired stand, 2026-09-21 14:22

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1192918.8 | 296687.5 | 0.25x | 295431.2 | 0.25x | -0.4% | 424449 | 424449 | 1,122 % |  |
| sql/select20.at | generated | hand | 6801.7 | 17730.9 | 2.61x | 17345.1 | 2.55x | -2.2% | 21416 | 21416 | 16 % |  |
| sql/select20.window | generated | hand | 6743.9 | 17946.9 | 2.66x | 17847.8 | 2.65x | -0.6% | 21416 | 21416 | 1 % |  |
| sql/select20.scan | generated | control | 371.0 | 379.0 | 1.02x | 378.8 | 1.02x | 0.0% | 0 | 0 | 1 % |  |
| tsql/select20.scan | generated | control | 371.8 | 378.1 | 1.02x | 378.1 | 1.02x | 0.0% | 0 | 0 | 27 % |  |
| sql/select20.bool | generated | hand | 6939.7 | 17836.8 | 2.57x | 17876.9 | 2.58x | +0.2% | 21448 | 21392 | 5 % |  |
| sql/literal | generated | hand | 54.6 | 136.5 | 2.50x | 131.3 | 2.41x | -3.8% | 160 | 160 | 2 % |  |
| sql/conditions1000 | generated | hand | 485741.0 | 1213523.4 | 2.50x | 1175635.2 | 2.42x | -3.1% | 1616203 | 1616160 | 11 % |  |
| sql/select1 | generated | hand | 664.0 | 1462.0 | 2.20x | 1463.1 | 2.20x | +0.1% | 1688 | 1688 | 4 % |  |
| sql/select20 | generated | hand | 6910.1 | 17830.8 | 2.58x | 17789.5 | 2.57x | -0.2% | 21448 | 21448 | 17 % |  |
