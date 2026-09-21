Median of 10 of 10 runs, each in a process of its own; control 30.5 ns (the runs' controls: 30.4, 30.3, 30.3, 30.6, 30.6, 30.3, 30.6, 30.3, 30.6, 30.6).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 04:01

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| sql/select20.at | generated | hand | 6755.7 | 17103.6 | 2.53x | 17154.6 | 2.54x | +0.3% | 21416 | 21441 | 3 % | [-2.5%..+3.7%], 5 of 10 positive | 0.0% [-2.5%..+4.5%], 6 of 10 positive |
| sql/select20.window | generated | hand | 6669.0 | 17464.4 | 2.62x | 17533.9 | 2.63x | +0.4% | 21416 | 21416 | 4 % | [-2.9%..+4.9%], 6 of 10 positive | +0.8% [-3.1%..+6.6%], 5 of 10 positive |
| sql/select20.scan | generated | control | 373.0 | 373.6 | 1.00x | 371.7 | 1.00x | -0.5% | 0 | 0 | 3 % | [-2.7%..+0.4%], 1 of 10 positive | +0.2% [-1.0%..+4.4%], 6 of 10 positive |
| tsql/select20.scan | generated | control | 368.8 | 372.5 | 1.01x | 376.8 | 1.02x | +1.1% | 0 | 0 | 6 % | [+0.1%..+2.2%], 10 of 10 positive | +0.3% [-3.1%..+12.9%], 4 of 10 positive |
| el/ladder.scan | generated | control | 136.2 | 135.0 | 0.99x | 135.0 | 0.99x | 0.0% | 0 | 0 | 4 % | [-2.6%..+2.7%], 6 of 10 positive | +0.3% [-1.2%..+3.1%], 4 of 10 positive |
| el/ladder.bool | generated | hand | 944.9 | 1656.2 | 1.75x | 1635.0 | 1.73x | -1.3% | 1776 | 1720 | 2 % | [-4.8%..-0.4%], 0 of 10 positive | -2.8% [-6.1%..-0.9%], 0 of 10 positive |
| sql/refused-late.bool | generated | hand | 2508.7 | 11833.0 | 4.72x | 5687.1 | 2.27x | -51.9% | 13552 | 6616 | 3 % | [-52.6%..-50.2%], 0 of 10 positive | -51.4% [-53.2%..-49.9%], 0 of 10 positive |
| sql/select20.bool | generated | hand | 6686.9 | 17411.6 | 2.60x | 17649.2 | 2.64x | +1.4% | 21448 | 21392 | 4 % | [-1.5%..+3.7%], 8 of 10 positive | +1.1% [-2.6%..+5.9%], 7 of 10 positive |
| sql/refused-cliff-paren-2715 | generated | control | 6231800.0 | 6335637.5 | 1.02x | 6422625.0 | 1.03x | +1.4% | 8778600 | 8778600 | 5 % | [-8.9%..+5.5%], 6 of 10 positive | -1.9% [-6.2%..+8.0%], 5 of 10 positive |
| el/ladder | generated | hand | 940.6 | 1648.8 | 1.75x | 1654.4 | 1.76x | +0.3% | 1776 | 1776 | 2 % | [-3.1%..+3.0%], 3 of 10 positive | -1.4% [-4.2%..+0.4%], 3 of 10 positive |
| el/ladder | immediate | hand | 940.6 | 1133.0 | 1.20x | 1123.4 | 1.19x | -0.9% | 1784 | 1784 | 2 % | [-3.1%..+0.3%], 3 of 10 positive | -0.4% [-2.2%..+1.3%], 2 of 10 positive |
| sql/select20 | generated | hand | 6683.8 | 17369.6 | 2.60x | 17640.8 | 2.64x | +1.6% | 21448 | 21448 | 4 % | [-1.9%..+6.2%], 8 of 10 positive | 0.0% [-2.6%..+3.6%], 6 of 10 positive |
| sql/refused-late | generated | hand | 2521.4 | 11841.5 | 4.70x | 11894.7 | 4.72x | +0.4% | 13552 | 13552 | 4 % | [-1.1%..+4.4%], 8 of 10 positive | +0.4% [-2.6%..+3.3%], 6 of 10 positive |
