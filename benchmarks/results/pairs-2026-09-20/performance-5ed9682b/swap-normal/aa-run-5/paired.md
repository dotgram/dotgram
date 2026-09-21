# Paired stand, 2026-09-20 22:33

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.2 ns.

This binary, and the libraries it holds as the control, was built from 7dea2aca. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 167853.9 | 188170.3 | 1.12x | 192682.0 | 1.15x | +2.4% | 720048 | 720048 | 7 % |  |
| fix/Orders128.yield-string | generated | hand | 82365.6 | 265523.8 | 3.22x | 274364.5 | 3.33x | +3.3% | 117936 | 117936 | 8 % |  |
| web/media-type.quoted | generated | control | 253.1 | 282.6 | 1.12x | 276.4 | 1.09x | -2.2% | 816 | 816 | 19 % |  |
