# Paired stand, 2026-09-20 20:55

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.2 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137); after (commit 6faa6e83, framework net10.0, no properties, emitted 5aaa4eb426ed9d99). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Order.text | generated | hand | 653.1 | 616.1 | 0.94x | 614.9 | 0.94x | -0.2% | 1096 | 1096 | 2 % |  |
| tsql/columns1000 | generated | scriptdom | 2164912.5 | 192931.2 | 0.09x | 183262.5 | 0.08x | -5.0% | 200464 | 200464 | 59 % |  |
| config/dense | generated | control | 12291.3 | 13914.9 | 1.13x | 11274.2 | 0.92x | -19.0% | 47256 | 47256 | 10 % |  |
| config/spaced | generated | control | 14743.8 | 17020.1 | 1.15x | 15016.7 | 1.02x | -11.8% | 47256 | 47256 | 16 % |  |
| config/commented | generated | control | 19407.2 | 23540.0 | 1.21x | 21602.3 | 1.11x | -8.2% | 47256 | 47256 | 16 % |  |
