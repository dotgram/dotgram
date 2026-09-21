# Paired stand, 2026-09-20 20:58

IGOR-DESKTOP, pinned to 0-15, high priority, control 35.3 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO=0, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137); after (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| config/dense | generated | control | 18180.8 | 18442.4 | 1.01x | 18373.2 | 1.01x | -0.4% | 47256 | 47256 | 4 % |  |
| config/spaced | generated | control | 29137.5 | 29183.6 | 1.00x | 30458.8 | 1.05x | +4.4% | 47256 | 47256 | 34 % |  |
| config/commented | generated | control | 35859.9 | 39889.2 | 1.11x | 38477.0 | 1.07x | -3.5% | 47256 | 47256 | 15 % |  |
