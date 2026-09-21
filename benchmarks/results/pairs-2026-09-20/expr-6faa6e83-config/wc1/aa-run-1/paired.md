# Paired stand, 2026-09-20 20:55

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.1 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137); after (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Order.text | generated | hand | 643.8 | 635.0 | 0.99x | 635.2 | 0.99x | 0.0% | 1096 | 1096 | 16 % |  |
| tsql/columns1000 | generated | scriptdom | 2202018.8 | 177168.8 | 0.08x | 195812.5 | 0.09x | +10.5% | 200464 | 200464 | 50 % |  |
| config/dense | generated | control | 12078.8 | 13666.6 | 1.13x | 11573.4 | 0.96x | -15.3% | 47256 | 47256 | 21 % |  |
| config/spaced | generated | control | 13564.2 | 15481.2 | 1.14x | 14096.3 | 1.04x | -8.9% | 47256 | 47256 | 8 % |  |
| config/commented | generated | control | 19722.8 | 22733.3 | 1.15x | 21174.4 | 1.07x | -6.9% | 47256 | 47256 | 13 % |  |
