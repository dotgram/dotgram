# Paired stand, 2026-09-20 22:36

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

This binary, and the libraries it holds as the control, was built from 7dea2aca. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 181431.2 | 191009.4 | 1.05x | 215975.0 | 1.19x | +13.1% | 720048 | 720048 | 6 % |  |
| fix/Orders128.yield-string | generated | hand | 85462.1 | 253554.7 | 2.97x | 260690.6 | 3.05x | +2.8% | 117936 | 117936 | 13 % |  |
| web/media-type.quoted | generated | control | 253.3 | 317.6 | 1.25x | 297.1 | 1.17x | -6.5% | 816 | 816 | 6 % |  |
