# Paired stand, 2026-09-21 14:41

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1513125.0 | 340556.2 | 0.23x | 459150.0 | 0.30x | +34.8% | 424449 | 424449 | 911 % |  |
| sql/select20.at | generated | hand | 6725.4 | 17609.4 | 2.62x | 17427.1 | 2.59x | -1.0% | 21416 | 21416 | 3 % |  |
| sql/select20.window | generated | hand | 6743.2 | 17950.3 | 2.66x | 17958.1 | 2.66x | 0.0% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 394.6 | 404.8 | 1.03x | 384.7 | 0.97x | -5.0% | 0 | 0 | 27 % |  |
| tsql/select20.scan | generated | control | 372.7 | 374.0 | 1.00x | 378.4 | 1.02x | +1.2% | 0 | 0 | 20 % |  |
| sql/select20.bool | generated | hand | 6958.8 | 18176.7 | 2.61x | 18340.9 | 2.64x | +0.9% | 21448 | 21392 | 19 % |  |
| sql/literal | generated | hand | 56.7 | 197.3 | 3.48x | 198.8 | 3.51x | +0.7% | 160 | 160 | 19 % |  |
| sql/conditions1000 | generated | hand | 490156.6 | 1230413.3 | 2.51x | 1255403.1 | 2.56x | +2.0% | 1616203 | 1616203 | 5 % |  |
| sql/select1 | generated | hand | 660.0 | 1550.1 | 2.35x | 1538.2 | 2.33x | -0.8% | 1688 | 1688 | 5 % |  |
| sql/select20 | generated | hand | 6742.0 | 18049.1 | 2.68x | 17950.0 | 2.66x | -0.5% | 21448 | 21448 | 19 % |  |
