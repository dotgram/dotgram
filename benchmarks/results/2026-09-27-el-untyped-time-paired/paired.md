Median of 5 of 5 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.5, 31.5, 31.5, 31.5, 31.4).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-27 06:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

This binary, and the libraries it holds as the control, was built from 23d95ef7. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 46dec223, framework net10.0, no properties, emitted 119770ab5acdde58); after (commit f84e3f4a, framework net10.0, no properties, emitted 2231273af56eadc8). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| el/ladder.scan | generated | control | 141.4 | 142.8 | 1.01x | 142.6 | 1.01x | -0.1% | 0 | 0 | 8 % | [-1.4%..+1.2%], 2 of 5 positive | +0.7% [-2.2%..+0.7%], 2 of 5 positive |
| el/refused-early.bool | match-vs-bool | hand | 333.5 | 1229.0 | 3.69x | 640.0 | 1.92x | -47.9% | 1096 | 584 | 9 % | [-52.3%..-45.7%], 0 of 5 positive | -49.2% [-50.4%..-47.2%], 0 of 5 positive |
| el/refused-late.bool | match-vs-bool | hand | 1242.3 | 2151.1 | 1.73x | 1097.7 | 0.88x | -49.0% | 1096 | 584 | 3 % | [-50.7%..-46.1%], 0 of 5 positive | -49.1% [-50.7%..-47.9%], 0 of 5 positive |
| el/ladder.bool | match-vs-bool | hand | 972.3 | 2039.3 | 2.10x | 2016.4 | 2.07x | -1.1% | 2000 | 1584 | 5 % | [-2.8%..+1.6%], 1 of 5 positive | -2.5% [-8.1%..+0.8%], 2 of 5 positive |
| el/nested-100.match | generated | control | 16269.5 | 16220.3 | 1.00x | 17676.4 | 1.09x | +9.0% | 8408 | 8272 | 1 % | [+5.9%..+10.4%], 5 of 5 positive | +0.7% [-10.4%..+4.5%], 2 of 5 positive |
| el/nested-100.match | immediate | control | 16269.5 | 15068.9 | 0.93x | 15946.9 | 0.98x | +5.8% | 8408 | 8272 | 1 % | [-0.2%..+8.8%], 4 of 5 positive | +2.3% [-0.9%..+4.1%], 4 of 5 positive |
| el/nested-refused-100.match | generated | control | 19339.5 | 19550.4 | 1.01x | 21271.5 | 1.10x | +8.8% | 1192 | 960 | 7 % | [+6.2%..+10.3%], 5 of 5 positive | -0.2% [-2.7%..+0.4%], 2 of 5 positive |
| el/nested-refused-100.match | immediate | control | 19339.5 | 17174.0 | 0.89x | 18524.5 | 0.96x | +7.9% | 1192 | 960 | 7 % | [+7.1%..+13.1%], 5 of 5 positive | +0.7% [-0.5%..+1.6%], 4 of 5 positive |
| el/nested-400.match | generated | control | 63405.5 | 62515.2 | 0.99x | 68490.6 | 1.08x | +9.6% | 30008 | 29872 | 2 % | [+7.0%..+10.5%], 5 of 5 positive | -0.6% [-10.3%..+3.9%], 2 of 5 positive |
| el/nested-400.match | immediate | control | 63405.5 | 57244.8 | 0.90x | 61309.5 | 0.97x | +7.1% | 30008 | 29872 | 2 % | [+5.4%..+9.4%], 5 of 5 positive | +2.4% [-1.3%..+4.3%], 4 of 5 positive |
| el/nested-refused-400.match | generated | control | 73722.1 | 74196.7 | 1.01x | 80745.1 | 1.10x | +8.8% | 1192 | 960 | 7 % | [+6.9%..+10.7%], 5 of 5 positive | -0.6% [-3.0%..+3.8%], 2 of 5 positive |
| el/nested-refused-400.match | immediate | control | 73722.1 | 64378.6 | 0.87x | 70148.5 | 0.95x | +9.0% | 1192 | 960 | 7 % | [+8.8%..+13.2%], 5 of 5 positive | +0.2% [-8.0%..+2.4%], 2 of 5 positive |
| el/parse-200k-tokens | generated | control | 17818075.0 | 19764075.0 | 1.11x | 18498250.0 | 1.04x | -6.4% | 19201611 | 9601295 | 13 % | [-8.4%..+1.3%], 1 of 5 positive | +1.1% [-2.2%..+6.6%], 2 of 5 positive |
| el/parse-500k-tokens | generated | control | 46613850.0 | 47221150.0 | 1.01x | 48410400.0 | 1.04x | +2.5% | 48002501 | 24001709 | 5 % | [-12.2%..+4.8%], 4 of 5 positive | +19.0% [-2.8%..+21.8%], 4 of 5 positive |
| el/floor | generated | hand | 315.5 | 902.3 | 2.86x | 926.2 | 2.94x | +2.6% | 1160 | 1024 | 6 % | [-1.6%..+4.8%], 3 of 5 positive | -2.8% [-6.6%..+0.0%], 1 of 5 positive |
| el/floor | immediate | hand | 315.5 | 895.5 | 2.84x | 901.2 | 2.86x | +0.6% | 1160 | 1024 | 6 % | [-3.7%..+7.0%], 4 of 5 positive | -1.8% [-2.5%..+0.6%], 1 of 5 positive |
| el/ladder | generated | hand | 973.7 | 2052.9 | 2.11x | 2060.0 | 2.12x | +0.3% | 2000 | 1640 | 5 % | [-1.3%..+2.7%], 4 of 5 positive | -0.8% [-8.7%..+2.8%], 2 of 5 positive |
| el/ladder | immediate | hand | 973.7 | 2042.2 | 2.10x | 2039.3 | 2.09x | -0.1% | 2000 | 1640 | 5 % | [-6.6%..+1.2%], 2 of 5 positive | +0.2% [-1.2%..+3.1%], 3 of 5 positive |
| el/nest7 | generated | hand | 701.1 | 2074.7 | 2.96x | 2164.2 | 3.09x | +4.3% | 1712 | 1576 | 3 % | [+0.6%..+6.8%], 5 of 5 positive | -1.0% [-6.3%..+1.8%], 2 of 5 positive |
| el/nest7 | immediate | hand | 701.1 | 2061.4 | 2.94x | 2111.4 | 3.01x | +2.4% | 1712 | 1576 | 3 % | [-3.5%..+7.6%], 4 of 5 positive | +0.5% [-1.6%..+4.0%], 3 of 5 positive |
| el/block | generated | hand | 1025.8 | 2043.7 | 1.99x | 2025.3 | 1.97x | -0.9% | 2448 | 2184 | 2 % | [-1.2%..+1.9%], 2 of 5 positive | -1.0% [-4.6%..+7.6%], 1 of 5 positive |
| el/block | immediate | hand | 1025.8 | 2019.0 | 1.97x | 1995.2 | 1.94x | -1.2% | 2448 | 2184 | 2 % | [-3.0%..+3.0%], 3 of 5 positive | -0.2% [-3.8%..+4.8%], 3 of 5 positive |
| el/try | generated | hand | 4209.0 | 5083.7 | 1.21x | 5723.8 | 1.36x | +12.6% | 4632 | 4296 | 3 % | [+9.4%..+14.5%], 5 of 5 positive | +0.7% [-0.3%..+5.5%], 2 of 5 positive |
| el/try | immediate | hand | 4209.0 | 5093.7 | 1.21x | 5626.3 | 1.34x | +10.5% | 4632 | 4296 | 3 % | [+7.7%..+12.5%], 5 of 5 positive | -1.0% [-3.2%..+3.4%], 3 of 5 positive |
| el/loop | generated | hand | 2257.8 | 4162.0 | 1.84x | 4211.8 | 1.87x | +1.2% | 4976 | 4744 | 3 % | [-0.5%..+3.7%], 4 of 5 positive | +0.2% [-2.1%..+2.2%], 3 of 5 positive |
| el/loop | immediate | hand | 2257.8 | 4092.4 | 1.81x | 4150.8 | 1.84x | +1.4% | 4976 | 4744 | 3 % | [-3.3%..+3.4%], 3 of 5 positive | -0.6% [-1.2%..+5.2%], 1 of 5 positive |
| el/terms100 | generated | hand | 10004.6 | 17494.6 | 1.75x | 17777.0 | 1.78x | +1.6% | 20360 | 10624 | 2 % | [+0.2%..+6.2%], 5 of 5 positive | -0.7% [-5.7%..+2.2%], 2 of 5 positive |
| el/terms100 | immediate | hand | 10004.6 | 18837.1 | 1.88x | 18392.9 | 1.84x | -2.4% | 20360 | 10624 | 2 % | [-3.7%..-1.8%], 0 of 5 positive | +0.5% [-0.6%..+5.2%], 4 of 5 positive |
| el/terms1000 | generated | hand | 96547.1 | 165146.6 | 1.71x | 170585.9 | 1.77x | +3.3% | 193160 | 97024 | 5 % | [+0.7%..+6.8%], 5 of 5 positive | +0.3% [-7.2%..+3.1%], 3 of 5 positive |
| el/terms1000 | immediate | hand | 96547.1 | 178759.8 | 1.85x | 173883.2 | 1.80x | -2.7% | 193160 | 97024 | 5 % | [-3.9%..-1.3%], 0 of 5 positive | -0.1% [-2.7%..+4.5%], 3 of 5 positive |
| el/overloads | generated | hand | 1898.0 | 2862.1 | 1.51x | 2891.3 | 1.52x | +1.0% | 4352 | 4176 | 5 % | [-1.8%..+2.7%], 3 of 5 positive | -0.7% [-2.2%..+0.1%], 2 of 5 positive |
| el/overloads | immediate | hand | 1898.0 | 2864.3 | 1.51x | 2861.7 | 1.51x | -0.1% | 4352 | 4176 | 5 % | [-0.8%..+2.0%], 1 of 5 positive | -1.1% [-2.8%..+1.6%], 3 of 5 positive |
| el/string | generated | hand | 308.7 | 1078.6 | 3.49x | 1103.3 | 3.57x | +2.3% | 1112 | 1168 | 16 % | [-1.9%..+3.8%], 4 of 5 positive | -2.4% [-5.1%..+0.7%], 1 of 5 positive |
| el/string | immediate | hand | 308.7 | 1065.2 | 3.45x | 1101.5 | 3.57x | +3.4% | 1112 | 1168 | 16 % | [+0.7%..+4.8%], 5 of 5 positive | -0.7% [-3.2%..+0.9%], 1 of 5 positive |
| el/interpolation | generated | hand | 1881.3 | 4994.3 | 2.65x | 4857.4 | 2.58x | -2.7% | 2496 | 2168 | 39 % | [-10.5%..+15.8%], 1 of 5 positive | -0.4% [-7.2%..+17.0%], 3 of 5 positive |
| el/interpolation | immediate | hand | 1881.3 | 5264.0 | 2.80x | 4694.7 | 2.50x | -10.8% | 2496 | 2168 | 39 % | [-15.6%..+10.0%], 1 of 5 positive | +3.9% [-11.7%..+22.6%], 3 of 5 positive |
| el/untyped | generated | hand | 34210.9 | 36040.6 | 1.05x | 38218.8 | 1.12x | +6.0% | 24832 | 24440 | 27 % | [+3.1%..+12.2%], 5 of 5 positive | -0.7% [-0.7%..+10.0%], 2 of 5 positive |
| el/refused-early | generated | hand | 443.6 | 1395.1 | 3.14x | 1417.0 | 3.19x | +1.6% | 1096 | 864 | 18 % | [-6.2%..+2.3%], 3 of 5 positive | +0.6% [-0.5%..+4.5%], 4 of 5 positive |
| el/refused-early | immediate | hand | 443.6 | 1406.1 | 3.17x | 1385.5 | 3.12x | -1.5% | 1096 | 864 | 18 % | [-2.9%..+0.5%], 1 of 5 positive | -0.2% [-1.1%..+1.3%], 2 of 5 positive |
| el/refused-late | generated | hand | 1511.6 | 2342.0 | 1.55x | 2346.0 | 1.55x | +0.2% | 1096 | 864 | 10 % | [-3.0%..+4.4%], 2 of 5 positive | +0.6% [-0.7%..+5.2%], 3 of 5 positive |
| el/refused-late | immediate | hand | 1511.6 | 2284.7 | 1.51x | 2323.9 | 1.54x | +1.7% | 1096 | 864 | 10 % | [-2.9%..+2.2%], 4 of 5 positive | +0.7% [-1.8%..+2.3%], 3 of 5 positive |
