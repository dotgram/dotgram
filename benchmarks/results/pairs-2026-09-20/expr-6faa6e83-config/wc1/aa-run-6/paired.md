# Paired stand, 2026-09-20 20:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137); after (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Order.text | generated | hand | 655.2 | 599.4 | 0.91x | 625.1 | 0.95x | +4.3% | 1096 | 1096 | 15 % |  |
| tsql/columns1000 | generated | scriptdom | 2335006.2 | 185831.2 | 0.08x | 180287.5 | 0.08x | -3.0% | 200464 | 200464 | 46 % |  |
| config/dense | generated | control | 11654.2 | 13749.3 | 1.18x | 10801.3 | 0.93x | -21.4% | 47256 | 47256 | 18 % |  |
| config/spaced | generated | control | 13106.8 | 15631.5 | 1.19x | 13993.5 | 1.07x | -10.5% | 47256 | 47256 | 9 % |  |
| config/commented | generated | control | 18115.6 | 22027.7 | 1.22x | 20676.3 | 1.14x | -6.1% | 47256 | 47256 | 13 % |  |
