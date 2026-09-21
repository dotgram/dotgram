# Paired stand, 2026-09-21 14:33

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 892025.0 | 285868.8 | 0.32x | 291312.5 | 0.33x | +1.9% | 424449 | 424449 | 1,261 % |  |
| sql/select20.at | generated | hand | 6851.6 | 17523.8 | 2.56x | 17821.4 | 2.60x | +1.7% | 21416 | 21416 | 16 % |  |
| sql/select20.window | generated | hand | 6845.6 | 17919.9 | 2.62x | 18309.6 | 2.67x | +2.2% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 374.4 | 379.0 | 1.01x | 379.1 | 1.01x | 0.0% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 373.0 | 378.1 | 1.01x | 378.3 | 1.01x | +0.1% | 0 | 0 | 12 % |  |
| sql/select20.bool | generated | hand | 6818.9 | 17923.9 | 2.63x | 18394.5 | 2.70x | +2.6% | 21448 | 21392 | 20 % |  |
| sql/literal | generated | hand | 59.0 | 202.2 | 3.43x | 179.4 | 3.04x | -11.3% | 160 | 160 | 4 % |  |
| sql/conditions1000 | generated | hand | 487002.0 | 1177174.2 | 2.42x | 1209154.3 | 2.48x | +2.7% | 1616203 | 1616203 | 8 % |  |
| sql/select1 | generated | hand | 650.7 | 1539.9 | 2.37x | 1595.0 | 2.45x | +3.6% | 1688 | 1688 | 8 % |  |
| sql/select20 | generated | hand | 6775.3 | 18062.3 | 2.67x | 18575.7 | 2.74x | +2.8% | 21448 | 21448 | 2 % |  |
