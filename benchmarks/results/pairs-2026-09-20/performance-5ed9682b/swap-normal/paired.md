Median of 6 of 6 runs, each in a process of its own; control 31.8 ns (the runs' controls: 32.2, 31.7, 31.7, 32.0, 32.0, 31.5).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 22:34

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

This binary, and the libraries it holds as the control, was built from 7dea2aca. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| web/json.array10000 | generated | hand | 163382.8 | 180894.1 | 1.11x | 186196.5 | 1.14x | +2.9% | 720048 | 720048 | 4 % | [-1.4%..+7.1%], 4 of 6 positive | +2.0% [-1.0%..+3.4%], 4 of 6 positive |
| fix/Orders128.yield-string | generated | hand | 82346.1 | 260631.6 | 3.17x | 263588.1 | 3.20x | +1.1% | 117936 | 117936 | 6 % | [-3.2%..+5.4%], 3 of 6 positive | +0.4% [-6.6%..+3.3%], 3 of 6 positive |
| web/media-type.quoted | generated | control | 253.9 | 283.0 | 1.11x | 285.3 | 1.12x | +0.8% | 816 | 816 | 7 % | [-2.3%..+3.0%], 4 of 6 positive | +1.3% [-4.6%..+3.6%], 3 of 6 positive |
