Median of 10 of 10 runs, each in a process of its own; control 30.3 ns (the runs' controls: 30.3, 30.6, 30.3, 30.3, 30.7, 30.3, 30.3, 30.6, 30.6, 30.3).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 03:26

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| sql/select20.at | generated | hand | 6699.2 | 17136.2 | 2.56x | 17185.4 | 2.57x | +0.3% | 21416 | 21441 | 3 % | [-4.3%..+1.9%], 7 of 10 positive | -0.6% [-2.1%..+2.1%], 5 of 10 positive |
| sql/select20.window | generated | hand | 6641.7 | 17539.6 | 2.64x | 17581.0 | 2.65x | +0.2% | 21416 | 21416 | 1 % | [-5.6%..+1.0%], 6 of 10 positive | +0.3% [-2.2%..+4.9%], 6 of 10 positive |
| sql/select20.scan | generated | control | 371.6 | 372.3 | 1.00x | 372.3 | 1.00x | 0.0% | 0 | 0 | 4 % | [-13.0%..+2.2%], 3 of 10 positive | -0.7% [-1.4%..+1.0%], 3 of 10 positive |
| tsql/select20.scan | generated | control | 371.0 | 371.6 | 1.00x | 371.8 | 1.00x | +0.1% | 0 | 0 | 3 % | [-1.2%..+4.5%], 6 of 10 positive | +0.4% [-0.9%..+4.4%], 7 of 10 positive |
| el/ladder.scan | generated | control | 135.3 | 134.3 | 0.99x | 134.2 | 0.99x | -0.1% | 0 | 0 | 3 % | [-2.1%..+1.6%], 5 of 10 positive | +1.1% [-1.6%..+2.8%], 7 of 10 positive |
| el/ladder.bool | generated | hand | 942.3 | 1643.8 | 1.74x | 1623.8 | 1.72x | -1.2% | 1776 | 1720 | 6 % | [-3.1%..+0.9%], 2 of 10 positive | -1.1% [-2.8%..+2.5%], 1 of 10 positive |
| sql/refused-late.bool | generated | hand | 2494.3 | 11775.0 | 4.72x | 5680.4 | 2.28x | -51.8% | 13552 | 6616 | 3 % | [-53.5%..-51.0%], 0 of 10 positive | -51.9% [-52.9%..-50.7%], 0 of 10 positive |
| sql/select20.bool | generated | hand | 6673.0 | 17447.2 | 2.61x | 17485.5 | 2.62x | +0.2% | 21448 | 21392 | 4 % | [-5.4%..+1.7%], 6 of 10 positive | 0.0% [-3.0%..+2.9%], 6 of 10 positive |
| sql/refused-cliff-paren-2715 | generated | control | 6339162.5 | 6289750.0 | 0.99x | 6264775.0 | 0.99x | -0.4% | 8778600 | 8778600 | 7 % | [-6.9%..+3.3%], 3 of 10 positive | -0.1% [-3.5%..+2.9%], 3 of 10 positive |
| el/ladder | generated | hand | 941.7 | 1644.7 | 1.75x | 1636.9 | 1.74x | -0.5% | 1776 | 1776 | 4 % | [-2.6%..+1.5%], 3 of 10 positive | +1.3% [-0.9%..+2.9%], 7 of 10 positive |
| el/ladder | immediate | hand | 941.7 | 1127.6 | 1.20x | 1128.7 | 1.20x | +0.1% | 1784 | 1784 | 4 % | [-3.3%..+1.3%], 5 of 10 positive | +0.6% [-2.6%..+2.0%], 6 of 10 positive |
| sql/select20 | generated | hand | 6682.4 | 17461.5 | 2.61x | 17486.7 | 2.62x | +0.1% | 21448 | 21448 | 4 % | [-5.7%..+1.9%], 7 of 10 positive | +0.2% [-2.2%..+3.1%], 4 of 10 positive |
| sql/refused-late | generated | hand | 2510.8 | 11826.2 | 4.71x | 11841.2 | 4.72x | +0.1% | 13552 | 13552 | 2 % | [-3.0%..+1.7%], 6 of 10 positive | +0.6% [-0.9%..+2.5%], 7 of 10 positive |
