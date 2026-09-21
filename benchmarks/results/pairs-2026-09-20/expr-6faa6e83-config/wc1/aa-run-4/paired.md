# Paired stand, 2026-09-20 20:56

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137); after (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Order.text | generated | hand | 633.2 | 622.2 | 0.98x | 632.3 | 1.00x | +1.6% | 1096 | 1096 | 2 % |  |
| tsql/columns1000 | generated | scriptdom | 2133500.0 | 177068.8 | 0.08x | 179206.2 | 0.08x | +1.2% | 200464 | 200464 | 54 % |  |
| config/dense | generated | control | 12211.1 | 13167.6 | 1.08x | 11227.5 | 0.92x | -14.7% | 47256 | 47256 | 15 % |  |
| config/spaced | generated | control | 13563.1 | 16086.1 | 1.19x | 14051.9 | 1.04x | -12.6% | 47256 | 47256 | 16 % |  |
| config/commented | generated | control | 18746.8 | 22484.0 | 1.20x | 20592.7 | 1.10x | -8.4% | 47256 | 47256 | 7 % |  |
