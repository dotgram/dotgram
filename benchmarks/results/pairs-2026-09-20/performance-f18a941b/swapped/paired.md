Median of 10 of 10 runs, each in a process of its own; control 30.5 ns (the runs' controls: 30.3, 30.5, 30.5, 30.5, 30.3, 30.6, 30.5, 30.5, 30.5, 30.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 05:55

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| web/json.array10000 | generated | hand | 164723.0 | 181521.9 | 1.10x | 182452.0 | 1.11x | +0.5% | 720048 | 720048 | 2 % | [-0.2%..+1.1%], 9 of 10 positive | +0.8% [-0.2%..+4.1%], 8 of 10 positive |
| sql/select20.at | generated | hand | 6618.4 | 17137.5 | 2.59x | 17080.0 | 2.58x | -0.3% | 21416 | 21416 | 2 % | [-4.1%..+0.8%], 4 of 10 positive | +0.2% [-7.8%..+4.7%], 7 of 10 positive |
| sql/select20.window | generated | hand | 6626.8 | 17505.1 | 2.64x | 17470.7 | 2.64x | -0.2% | 21416 | 21416 | 2 % | [-6.3%..+3.4%], 4 of 10 positive | +0.5% [-4.6%..+3.6%], 5 of 10 positive |
| sql/select20.scan | generated | control | 369.4 | 370.8 | 1.00x | 370.4 | 1.00x | -0.1% | 0 | 0 | 4 % | [-1.2%..+1.3%], 6 of 10 positive | -0.3% [-0.8%..+2.7%], 4 of 10 positive |
| tsql/select20.scan | generated | control | 371.0 | 371.6 | 1.00x | 372.4 | 1.00x | +0.2% | 0 | 0 | 5 % | [-10.8%..+3.6%], 7 of 10 positive | -0.5% [-9.3%..+1.0%], 4 of 10 positive |
| el/ladder.scan | generated | control | 135.9 | 133.8 | 0.98x | 134.1 | 0.99x | +0.2% | 0 | 0 | 3 % | [-1.3%..+2.7%], 3 of 10 positive | -0.3% [-1.2%..+0.4%], 4 of 10 positive |
| el/ladder.bool | generated | hand | 935.2 | 1663.3 | 1.78x | 1635.9 | 1.75x | -1.6% | 1776 | 1720 | 5 % | [-3.1%..+1.1%], 1 of 10 positive | -0.4% [-2.8%..+2.2%], 5 of 10 positive |
| sql/refused-late.bool | generated | hand | 2496.2 | 11851.7 | 4.75x | 5646.8 | 2.26x | -52.4% | 13552 | 6616 | 3 % | [-53.2%..-51.4%], 0 of 10 positive | -51.9% [-56.0%..-50.0%], 0 of 10 positive |
| sql/select20.bool | generated | hand | 6644.8 | 17648.9 | 2.66x | 17577.9 | 2.65x | -0.4% | 21448 | 21392 | 3 % | [-3.1%..+0.9%], 6 of 10 positive | +0.6% [-11.9%..+3.2%], 7 of 10 positive |
| sql/refused-cliff-case-1391 | generated | control | 4324978.1 | 4372768.8 | 1.01x | 4382171.9 | 1.01x | +0.2% | 6276652 | 6276660 | 6 % | [-5.6%..+2.5%], 6 of 10 positive | -0.5% [-4.7%..+2.7%], 5 of 10 positive |
| sql/refused-cliff-paren-2715 | generated | control | 6286843.8 | 6298618.8 | 1.00x | 6303428.1 | 1.00x | +0.1% | 8778556 | 8778600 | 6 % | [-1.6%..+3.8%], 8 of 10 positive | -0.8% [-4.7%..+6.0%], 3 of 10 positive |
| el/ladder | generated | hand | 939.6 | 1655.2 | 1.76x | 1656.7 | 1.76x | +0.1% | 1776 | 1776 | 3 % | [-2.9%..+1.9%], 4 of 10 positive | +0.6% [-1.5%..+3.3%], 6 of 10 positive |
| el/ladder | immediate | hand | 939.6 | 1125.6 | 1.20x | 1133.1 | 1.21x | +0.7% | 1784 | 1784 | 3 % | [-1.9%..+1.8%], 4 of 10 positive | -0.6% [-3.0%..+6.1%], 2 of 10 positive |
| el/terms1000 | generated | hand | 108102.3 | 147390.6 | 1.36x | 147111.9 | 1.36x | -0.2% | 169104 | 169116 | 3 % | [-1.7%..+1.4%], 5 of 10 positive | -0.5% [-3.2%..+3.1%], 5 of 10 positive |
| el/terms1000 | immediate | hand | 108102.3 | 120917.6 | 1.12x | 121350.9 | 1.12x | +0.4% | 177120 | 177123 | 3 % | [-2.1%..+3.6%], 5 of 10 positive | -0.3% [-2.3%..+3.8%], 5 of 10 positive |
| sql/select20 | generated | hand | 6652.5 | 17552.2 | 2.64x | 17576.9 | 2.64x | +0.1% | 21448 | 21448 | 2 % | [-3.6%..+1.9%], 5 of 10 positive | +0.6% [-12.4%..+3.7%], 7 of 10 positive |
| sql/refused-late | generated | hand | 2497.1 | 11848.8 | 4.75x | 11765.0 | 4.71x | -0.7% | 13552 | 13552 | 3 % | [-1.1%..+2.6%], 3 of 10 positive | +0.3% [-7.8%..+4.1%], 6 of 10 positive |
