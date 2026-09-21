Median of 6 of 6 runs, each in a process of its own; control 31.8 ns (the runs' controls: 31.7, 31.7, 32.2, 32.4, 31.6, 31.9).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 20:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 475e6864, framework net10.0, no properties, emitted e40ed39227dae137); after (commit 6faa6e83, framework net10.0, no properties, emitted 5aaa4eb426ed9d99). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| fix/Order.text | generated | hand | 642.7 | 616.5 | 0.96x | 629.5 | 0.98x | +2.1% | 1096 | 1096 | 7 % | [-0.5%..+6.1%], 4 of 6 positive | +1.7% [-0.3%..+4.3%], 5 of 6 positive |
| tsql/columns1000 | generated | scriptdom | 2142031.2 | 194375.0 | 0.09x | 186581.2 | 0.09x | -4.0% | 200464 | 200464 | 15 % | [-10.1%..+3.5%], 3 of 6 positive | +0.5% [-5.1%..+10.5%], 3 of 6 positive |
| config/dense | generated | control | 12192.7 | 13590.7 | 1.11x | 11060.3 | 0.91x | -18.6% | 47256 | 47256 | 13 % | [-19.0%..-0.8%], 0 of 6 positive | -17.3% [-21.4%..-14.7%], 0 of 6 positive |
| config/spaced | generated | control | 13804.2 | 15691.3 | 1.14x | 14033.2 | 1.02x | -10.6% | 47256 | 47256 | 9 % | [-12.0%..-1.8%], 0 of 6 positive | -10.2% [-12.6%..-8.9%], 0 of 6 positive |
| config/commented | generated | control | 18505.9 | 22064.3 | 1.19x | 20754.2 | 1.12x | -5.9% | 47256 | 47256 | 6 % | [-8.2%..+0.2%], 1 of 6 positive | -7.4% [-8.4%..-3.9%], 0 of 6 positive |
