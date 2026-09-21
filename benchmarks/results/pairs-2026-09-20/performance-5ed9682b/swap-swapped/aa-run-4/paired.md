# Paired stand, 2026-09-20 22:35

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.2 ns.

This binary, and the libraries it holds as the control, was built from 7dea2aca. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166159.4 | 200060.2 | 1.20x | 197828.9 | 1.19x | -1.1% | 720048 | 720048 | 13 % |  |
| fix/Orders128.yield-string | generated | hand | 86458.6 | 259534.8 | 3.00x | 265292.6 | 3.07x | +2.2% | 117936 | 117936 | 22 % |  |
| web/media-type.quoted | generated | control | 260.3 | 328.4 | 1.26x | 314.3 | 1.21x | -4.3% | 816 | 816 | 13 % |  |
