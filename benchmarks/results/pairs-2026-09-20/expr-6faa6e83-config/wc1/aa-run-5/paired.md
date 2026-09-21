# Paired stand, 2026-09-20 20:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.4 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137); after (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Order.text | generated | hand | 643.5 | 636.8 | 0.99x | 635.1 | 0.99x | -0.3% | 1096 | 1096 | 15 % |  |
| tsql/columns1000 | generated | scriptdom | 2100000.0 | 183718.8 | 0.09x | 178562.5 | 0.09x | -2.8% | 200464 | 200464 | 52 % |  |
| config/dense | generated | control | 11761.4 | 13423.5 | 1.14x | 11397.7 | 0.97x | -15.1% | 47256 | 47256 | 19 % |  |
| config/spaced | generated | control | 13910.2 | 15552.3 | 1.12x | 14151.8 | 1.02x | -9.0% | 47256 | 47256 | 41 % |  |
| config/commented | generated | control | 19382.2 | 22623.6 | 1.17x | 21743.1 | 1.12x | -3.9% | 47256 | 47256 | 11 % |  |
