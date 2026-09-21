Median of 10 of 10 runs, each in a process of its own; control 30.3 ns (the runs' controls: 30.6, 30.5, 30.2, 30.2, 30.2, 30.5, 30.5, 30.3, 30.2, 30.3).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 07:06

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| web/json.array10000 | generated | hand | 169387.9 | 183304.3 | 1.08x | 184478.5 | 1.09x | +0.6% | 720048 | 720048 | 7 % | [-0.5%..+3.8%], 8 of 10 positive | +1.4% [+0.0%..+3.6%], 10 of 10 positive |
| sql/select20.at | generated | hand | 6647.1 | 17351.0 | 2.61x | 17083.8 | 2.57x | -1.5% | 21416 | 21416 | 3 % | [-8.4%..+1.7%], 2 of 10 positive | +1.1% [-2.0%..+3.4%], 7 of 10 positive |
| sql/select20.window | generated | hand | 6606.3 | 17674.8 | 2.68x | 17472.9 | 2.64x | -1.1% | 21416 | 21416 | 3 % | [-8.3%..+1.9%], 4 of 10 positive | +0.7% [-1.5%..+5.4%], 7 of 10 positive |
| sql/select20.scan | generated | control | 371.5 | 370.9 | 1.00x | 370.0 | 1.00x | -0.2% | 0 | 0 | 6 % | [-2.0%..+1.7%], 5 of 10 positive | 0.0% [-1.4%..+0.6%], 6 of 10 positive |
| tsql/select20.scan | generated | control | 369.6 | 372.2 | 1.01x | 371.2 | 1.00x | -0.3% | 0 | 0 | 8 % | [-2.2%..+1.1%], 2 of 10 positive | +0.2% [-3.5%..+4.3%], 6 of 10 positive |
| el/ladder.scan | generated | control | 134.5 | 134.2 | 1.00x | 133.9 | 1.00x | -0.3% | 0 | 0 | 4 % | [-2.4%..+1.1%], 3 of 10 positive | +0.1% [-3.9%..+1.0%], 4 of 10 positive |
| el/ladder.bool | generated | hand | 945.5 | 1683.1 | 1.78x | 1664.2 | 1.76x | -1.1% | 1776 | 1720 | 3 % | [-4.3%..+3.1%], 6 of 10 positive | -1.4% [-3.6%..+1.2%], 1 of 10 positive |
| sql/refused-late.bool | generated | hand | 2500.7 | 11882.3 | 4.75x | 5639.6 | 2.26x | -52.5% | 13552 | 6616 | 3 % | [-54.7%..-51.1%], 0 of 10 positive | -52.1% [-53.6%..-50.6%], 0 of 10 positive |
| sql/select20.bool | generated | hand | 6647.9 | 17671.2 | 2.66x | 17530.8 | 2.64x | -0.8% | 21448 | 21392 | 4 % | [-7.7%..+2.5%], 3 of 10 positive | 0.0% [-7.8%..+4.1%], 6 of 10 positive |
| sql/refused-cliff-case-1391 | generated | control | 4353950.0 | 4499793.8 | 1.03x | 4391631.2 | 1.01x | -2.4% | 6276672 | 6276684 | 18 % | [-8.1%..+6.9%], 3 of 10 positive | +1.3% [-4.2%..+10.4%], 7 of 10 positive |
| sql/refused-cliff-paren-2715 | generated | control | 6289034.4 | 6362687.5 | 1.01x | 6303900.0 | 1.00x | -0.9% | 8778600 | 8778600 | 5 % | [-3.8%..+1.4%], 2 of 10 positive | +0.3% [-3.9%..+5.2%], 7 of 10 positive |
| el/ladder | generated | hand | 939.5 | 1679.8 | 1.79x | 1687.8 | 1.80x | +0.5% | 1776 | 1776 | 5 % | [-2.6%..+4.4%], 7 of 10 positive | +0.1% [-2.2%..+4.1%], 4 of 10 positive |
| el/ladder | immediate | hand | 939.5 | 1133.5 | 1.21x | 1134.2 | 1.21x | +0.1% | 1784 | 1784 | 5 % | [-3.3%..+2.4%], 6 of 10 positive | +0.8% [-1.2%..+6.1%], 6 of 10 positive |
| el/terms1000 | generated | hand | 107953.4 | 147287.0 | 1.36x | 146136.5 | 1.35x | -0.8% | 169104 | 169107 | 3 % | [-2.2%..+1.6%], 3 of 10 positive | -0.1% [-1.8%..+2.2%], 3 of 10 positive |
| el/terms1000 | immediate | hand | 107953.4 | 120468.1 | 1.12x | 120100.2 | 1.11x | -0.3% | 177120 | 177120 | 3 % | [-4.7%..+2.0%], 4 of 10 positive | +0.5% [-1.9%..+14.8%], 5 of 10 positive |
| sql/select20 | generated | hand | 6632.2 | 17654.6 | 2.66x | 17431.4 | 2.63x | -1.3% | 21448 | 21448 | 4 % | [-7.7%..+1.9%], 3 of 10 positive | +0.5% [-8.0%..+3.2%], 6 of 10 positive |
| sql/refused-late | generated | hand | 2500.7 | 11844.9 | 4.74x | 11773.8 | 4.71x | -0.6% | 13552 | 13552 | 2 % | [-5.4%..+1.3%], 4 of 10 positive | -0.2% [-3.1%..+2.5%], 4 of 10 positive |
