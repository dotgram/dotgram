# Paired stand, 2026-09-21 14:43

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1535956.2 | 340862.5 | 0.22x | 307906.2 | 0.20x | -9.7% | 424449 | 424449 | 924 % |  |
| sql/select20.at | generated | hand | 6770.4 | 18003.9 | 2.66x | 17827.3 | 2.63x | -1.0% | 21416 | 21416 | 62 % |  |
| sql/select20.window | generated | hand | 6723.1 | 19036.6 | 2.83x | 18156.5 | 2.70x | -4.6% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 381.0 | 396.4 | 1.04x | 384.8 | 1.01x | -2.9% | 0 | 0 | 24 % |  |
| tsql/select20.scan | generated | control | 388.9 | 390.2 | 1.00x | 383.9 | 0.99x | -1.6% | 0 | 0 | 27 % |  |
| sql/select20.bool | generated | hand | 6868.0 | 18458.1 | 2.69x | 19425.0 | 2.83x | +5.2% | 21448 | 21392 | 44 % |  |
| sql/literal | generated | hand | 58.3 | 181.7 | 3.12x | 196.4 | 3.37x | +8.1% | 160 | 160 | 34 % |  |
| sql/conditions1000 | generated | hand | 484608.2 | 1212148.4 | 2.50x | 1215616.0 | 2.51x | +0.3% | 1616203 | 1616203 | 38 % |  |
| sql/select1 | generated | hand | 644.7 | 1575.1 | 2.44x | 1532.9 | 2.38x | -2.7% | 1688 | 1688 | 2 % |  |
| sql/select20 | generated | hand | 6719.6 | 18389.7 | 2.74x | 18178.9 | 2.71x | -1.1% | 21448 | 21448 | 5 % |  |
