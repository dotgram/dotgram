# Paired stand, 2026-09-20 20:56

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.4 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137); after (commit 6faa6e83, framework net10.0, no properties, emitted 5aaa4eb426ed9d99). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Order.text | generated | hand | 643.5 | 616.8 | 0.96x | 635.5 | 0.99x | +3.0% | 1096 | 1096 | 16 % |  |
| tsql/columns1000 | generated | scriptdom | 2119150.0 | 205206.2 | 0.10x | 206393.8 | 0.10x | +0.6% | 200464 | 200464 | 52 % |  |
| config/dense | generated | control | 13580.7 | 15638.0 | 1.15x | 12754.4 | 0.94x | -18.4% | 47256 | 47256 | 36 % |  |
| config/spaced | generated | control | 13726.2 | 15661.7 | 1.14x | 13787.9 | 1.00x | -12.0% | 47256 | 47256 | 12 % |  |
| config/commented | generated | control | 18483.0 | 21416.9 | 1.16x | 20794.7 | 1.13x | -2.9% | 47256 | 47256 | 6 % |  |
