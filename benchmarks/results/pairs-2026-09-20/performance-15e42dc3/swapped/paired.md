Median of 10 of 10 runs, each in a process of its own; control 30.4 ns (the runs' controls: 30.2, 30.2, 30.2, 30.4, 30.3, 30.5, 30.5, 30.3, 30.5, 30.5).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 07:32

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.4 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| web/json.array10000 | generated | hand | 166174.2 | 183926.6 | 1.11x | 183578.5 | 1.10x | -0.2% | 720048 | 720048 | 4 % | [-1.2%..+4.2%], 6 of 10 positive | +0.1% [-1.6%..+0.8%], 6 of 10 positive |
| sql/select20.at | generated | hand | 6693.6 | 17164.4 | 2.56x | 17083.3 | 2.55x | -0.5% | 21416 | 21416 | 4 % | [-2.5%..+0.7%], 4 of 10 positive | -0.2% [-6.3%..+30.3%], 4 of 10 positive |
| sql/select20.window | generated | hand | 6649.3 | 17621.0 | 2.65x | 17486.5 | 2.63x | -0.8% | 21416 | 21416 | 5 % | [-1.9%..+1.2%], 5 of 10 positive | +0.7% [-9.4%..+30.3%], 4 of 10 positive |
| sql/select20.scan | generated | control | 365.9 | 369.6 | 1.01x | 369.6 | 1.01x | 0.0% | 0 | 0 | 4 % | [-1.3%..+3.6%], 6 of 10 positive | +0.1% [-0.5%..+14.9%], 6 of 10 positive |
| tsql/select20.scan | generated | control | 369.1 | 370.0 | 1.00x | 369.2 | 1.00x | -0.2% | 0 | 0 | 10 % | [-3.6%..+15.7%], 4 of 10 positive | +0.7% [-3.8%..+7.2%], 5 of 10 positive |
| el/ladder.scan | generated | control | 134.0 | 134.1 | 1.00x | 133.9 | 1.00x | -0.2% | 0 | 0 | 3 % | [-0.6%..+0.6%], 5 of 10 positive | +0.1% [-0.8%..+1.6%], 3 of 10 positive |
| el/ladder.bool | generated | hand | 941.9 | 1679.6 | 1.78x | 1661.0 | 1.76x | -1.1% | 1776 | 1720 | 5 % | [-2.7%..+4.6%], 2 of 10 positive | -1.6% [-3.8%..-0.5%], 0 of 10 positive |
| sql/refused-late.bool | generated | hand | 2515.8 | 11767.5 | 4.68x | 5620.0 | 2.23x | -52.2% | 13552 | 6616 | 4 % | [-53.0%..-50.6%], 0 of 10 positive | -51.9% [-53.1%..-39.7%], 0 of 10 positive |
| sql/select20.bool | generated | hand | 6692.2 | 17580.0 | 2.63x | 17550.3 | 2.62x | -0.2% | 21448 | 21392 | 4 % | [-3.1%..+1.3%], 6 of 10 positive | +0.3% [-3.4%..+30.4%], 5 of 10 positive |
| sql/refused-cliff-case-1391 | generated | control | 4525587.5 | 4405015.6 | 0.97x | 4460337.5 | 0.99x | +1.3% | 6276672 | 6276652 | 9 % | [-2.0%..+7.8%], 8 of 10 positive | -0.1% [-6.2%..+25.1%], 4 of 10 positive |
| sql/refused-cliff-paren-2715 | generated | control | 6418500.0 | 6453978.1 | 1.01x | 6302825.0 | 0.98x | -2.3% | 8778600 | 8778600 | 4 % | [-6.3%..+3.3%], 2 of 10 positive | -0.4% [-3.5%..+12.8%], 6 of 10 positive |
| el/ladder | generated | hand | 944.6 | 1685.9 | 1.78x | 1685.5 | 1.78x | 0.0% | 1776 | 1776 | 4 % | [-1.3%..+5.0%], 4 of 10 positive | +0.1% [-2.2%..+1.9%], 5 of 10 positive |
| el/ladder | immediate | hand | 944.6 | 1151.5 | 1.22x | 1136.8 | 1.20x | -1.3% | 1784 | 1784 | 4 % | [-3.7%..+1.6%], 2 of 10 positive | -0.2% [-7.0%..+13.0%], 6 of 10 positive |
| el/terms1000 | generated | hand | 108003.5 | 148569.4 | 1.38x | 146253.7 | 1.35x | -1.6% | 169104 | 169107 | 5 % | [-3.2%..+2.6%], 2 of 10 positive | +0.7% [-1.7%..+7.2%], 7 of 10 positive |
| el/terms1000 | immediate | hand | 108003.5 | 122835.1 | 1.14x | 121279.2 | 1.12x | -1.3% | 177120 | 177120 | 5 % | [-3.9%..+3.5%], 3 of 10 positive | -0.5% [-15.9%..+34.7%], 4 of 10 positive |
| sql/select20 | generated | hand | 6696.1 | 17549.6 | 2.62x | 17479.9 | 2.61x | -0.4% | 21448 | 21448 | 5 % | [-2.2%..+2.2%], 4 of 10 positive | +0.1% [-2.8%..+30.8%], 5 of 10 positive |
| sql/refused-late | generated | hand | 2511.7 | 11814.9 | 4.70x | 11779.2 | 4.69x | -0.3% | 13552 | 13552 | 4 % | [-1.9%..+1.5%], 3 of 10 positive | +0.6% [-1.4%..+24.1%], 6 of 10 positive |
