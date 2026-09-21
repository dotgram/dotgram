# Paired stand, 2026-09-20 20:59

IGOR-DESKTOP, pinned to 0-15, high priority, control 45.5 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO=0, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137); after (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| config/dense | generated | control | 37243.2 | 31265.2 | 0.84x | 31919.4 | 0.86x | +2.1% | 47256 | 47256 | 13 % |  |
| config/spaced | generated | control | 43269.7 | 35659.6 | 0.82x | 35051.0 | 0.81x | -1.7% | 47256 | 47256 | 12 % |  |
| config/commented | generated | control | 56472.2 | 49338.6 | 0.87x | 49857.4 | 0.88x | +1.1% | 47256 | 47256 | 21 % |  |
