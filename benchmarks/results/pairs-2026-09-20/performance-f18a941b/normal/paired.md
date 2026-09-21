Median of 10 of 10 runs, each in a process of its own; control 30.5 ns (the runs' controls: 30.5, 30.6, 30.6, 30.5, 30.5, 30.5, 30.5, 30.6, 30.5, 30.5).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 05:29

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| web/json.array10000 | generated | hand | 164975.0 | 182326.2 | 1.11x | 183366.4 | 1.11x | +0.6% | 720048 | 720048 | 3 % | [-2.4%..+4.9%], 7 of 10 positive | +0.6% [-0.8%..+1.1%], 8 of 10 positive |
| sql/select20.at | generated | hand | 6663.3 | 17031.7 | 2.56x | 17107.3 | 2.57x | +0.4% | 21416 | 21416 | 3 % | [-0.8%..+4.6%], 5 of 10 positive | -1.2% [-8.0%..+1.4%], 3 of 10 positive |
| sql/select20.window | generated | hand | 6606.0 | 17439.7 | 2.64x | 17502.6 | 2.65x | +0.4% | 21416 | 21416 | 2 % | [-1.7%..+7.0%], 5 of 10 positive | -1.4% [-7.5%..+1.0%], 2 of 10 positive |
| sql/select20.scan | generated | control | 367.4 | 368.9 | 1.00x | 370.0 | 1.01x | +0.3% | 0 | 0 | 2 % | [-3.5%..+3.4%], 7 of 10 positive | +0.1% [-2.2%..+1.7%], 5 of 10 positive |
| tsql/select20.scan | generated | control | 370.8 | 373.9 | 1.01x | 370.5 | 1.00x | -0.9% | 0 | 0 | 9 % | [-6.6%..+0.8%], 3 of 10 positive | 0.0% [-1.7%..+3.0%], 4 of 10 positive |
| el/ladder.scan | generated | control | 134.3 | 134.4 | 1.00x | 134.5 | 1.00x | 0.0% | 0 | 0 | 4 % | [-2.6%..+1.9%], 7 of 10 positive | -0.5% [-1.1%..+0.2%], 2 of 10 positive |
| el/ladder.bool | generated | hand | 947.7 | 1647.8 | 1.74x | 1638.7 | 1.73x | -0.5% | 1776 | 1720 | 5 % | [-4.6%..+4.8%], 5 of 10 positive | -1.3% [-2.7%..+2.5%], 3 of 10 positive |
| sql/refused-late.bool | generated | hand | 2490.9 | 11772.8 | 4.73x | 5649.1 | 2.27x | -52.0% | 13552 | 6616 | 1 % | [-52.5%..-51.0%], 0 of 10 positive | -52.3% [-54.9%..-51.2%], 0 of 10 positive |
| sql/select20.bool | generated | hand | 6630.7 | 17506.0 | 2.64x | 17534.2 | 2.64x | +0.2% | 21448 | 21392 | 2 % | [-4.0%..+6.3%], 7 of 10 positive | -0.9% [-6.8%..+0.4%], 2 of 10 positive |
| sql/refused-cliff-case-1391 | generated | control | 4415293.8 | 4369659.4 | 0.99x | 4356968.8 | 0.99x | -0.3% | 6276664 | 6276684 | 9 % | [-4.2%..+2.2%], 6 of 10 positive | -1.0% [-5.4%..+4.4%], 3 of 10 positive |
| sql/refused-cliff-paren-2715 | generated | control | 6311071.9 | 6281325.0 | 1.00x | 6319378.1 | 1.00x | +0.6% | 8778566 | 8778600 | 4 % | [-2.3%..+2.2%], 5 of 10 positive | +0.1% [-4.2%..+5.1%], 4 of 10 positive |
| el/ladder | generated | hand | 948.2 | 1656.8 | 1.75x | 1671.4 | 1.76x | +0.9% | 1776 | 1776 | 6 % | [-2.6%..+5.7%], 6 of 10 positive | +0.3% [-1.2%..+8.1%], 7 of 10 positive |
| el/ladder | immediate | hand | 948.2 | 1131.8 | 1.19x | 1125.6 | 1.19x | -0.5% | 1784 | 1784 | 6 % | [-2.5%..+1.9%], 3 of 10 positive | -1.4% [-4.5%..+0.0%], 1 of 10 positive |
| el/terms1000 | generated | hand | 107836.2 | 148434.9 | 1.38x | 147441.7 | 1.37x | -0.7% | 169104 | 169104 | 3 % | [-1.7%..+1.8%], 4 of 10 positive | +0.6% [-2.9%..+2.3%], 6 of 10 positive |
| el/terms1000 | immediate | hand | 107836.2 | 122558.0 | 1.14x | 121333.4 | 1.13x | -1.0% | 177120 | 177121 | 3 % | [-2.5%..+1.8%], 4 of 10 positive | -0.3% [-17.6%..+1.5%], 5 of 10 positive |
| sql/select20 | generated | hand | 6612.1 | 17466.5 | 2.64x | 17447.1 | 2.64x | -0.1% | 21448 | 21448 | 2 % | [-3.7%..+5.2%], 5 of 10 positive | -1.1% [-7.2%..+0.2%], 1 of 10 positive |
| sql/refused-late | generated | hand | 2496.8 | 11730.7 | 4.70x | 11789.1 | 4.72x | +0.5% | 13552 | 13552 | 1 % | [-0.9%..+3.1%], 7 of 10 positive | -0.1% [-5.4%..+3.3%], 4 of 10 positive |
