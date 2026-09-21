# Paired stand, 2026-09-20 20:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.9 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137); after (commit 6faa6e83, framework net10.0, no properties, emitted 5aaa4eb426ed9d99). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Order.text | generated | hand | 641.6 | 599.6 | 0.93x | 636.1 | 0.99x | +6.1% | 1096 | 1096 | 15 % |  |
| tsql/columns1000 | generated | scriptdom | 2078912.5 | 183475.0 | 0.09x | 189900.0 | 0.09x | +3.5% | 200464 | 200464 | 52 % |  |
| config/dense | generated | control | 12077.6 | 13350.7 | 1.11x | 10987.0 | 0.91x | -17.7% | 47256 | 47256 | 7 % |  |
| config/spaced | generated | control | 13882.3 | 15720.9 | 1.13x | 14209.6 | 1.02x | -9.6% | 47256 | 47256 | 13 % |  |
| config/commented | generated | control | 19123.2 | 22016.8 | 1.15x | 20713.8 | 1.08x | -5.9% | 47256 | 47256 | 13 % |  |
