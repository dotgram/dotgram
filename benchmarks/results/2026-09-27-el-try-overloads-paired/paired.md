Median of 5 of 5 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.7, 31.4, 31.5, 31.6, 31.5).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-27 05:22

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

This binary, and the libraries it holds as the control, was built from 23d95ef7. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5b60607c, framework net10.0, no properties, emitted 9aa7193cff0d1754); after (commit f84e3f4a, framework net10.0, no properties, emitted 2231273af56eadc8). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| el/ladder.scan | generated | control | 138.4 | 142.3 | 1.03x | 142.8 | 1.03x | +0.3% | 0 | 0 | 1 % | [-0.3%..+1.6%], 3 of 5 positive | +0.4% [-1.8%..+1.8%], 1 of 5 positive |
| el/refused-early.bool | match-vs-bool | hand | 340.9 | 1305.8 | 3.83x | 638.2 | 1.87x | -51.1% | 912 | 584 | 5 % | [-51.5%..-49.0%], 0 of 5 positive | -49.1% [-50.8%..-46.7%], 0 of 5 positive |
| el/refused-late.bool | match-vs-bool | hand | 1259.8 | 2144.3 | 1.70x | 1091.3 | 0.87x | -49.1% | 912 | 584 | 3 % | [-49.9%..-47.6%], 0 of 5 positive | -47.7% [-49.2%..-47.4%], 0 of 5 positive |
| el/ladder.bool | match-vs-bool | hand | 968.5 | 2094.7 | 2.16x | 2052.5 | 2.12x | -2.0% | 1688 | 1584 | 5 % | [-3.1%..-0.2%], 0 of 5 positive | -0.3% [-2.5%..+1.5%], 2 of 5 positive |
| el/nested-100.match | generated | control | 16462.5 | 18045.8 | 1.10x | 17978.6 | 1.09x | -0.4% | 8320 | 8272 | 1 % | [-2.9%..+4.2%], 4 of 5 positive | +1.1% [-2.3%..+2.4%], 3 of 5 positive |
| el/nested-100.match | immediate | control | 16462.5 | 16054.1 | 0.98x | 15867.1 | 0.96x | -1.2% | 8320 | 8272 | 1 % | [-7.3%..+2.3%], 2 of 5 positive | -2.5% [-8.8%..+0.1%], 1 of 5 positive |
| el/nested-refused-100.match | generated | control | 19308.6 | 21328.5 | 1.10x | 21533.3 | 1.12x | +1.0% | 1008 | 960 | 3 % | [-1.6%..+5.2%], 4 of 5 positive | +0.1% [-1.4%..+1.2%], 3 of 5 positive |
| el/nested-refused-100.match | immediate | control | 19308.6 | 18196.5 | 0.94x | 18359.3 | 0.95x | +0.9% | 1008 | 960 | 3 % | [-1.4%..+2.9%], 3 of 5 positive | -2.2% [-6.2%..+0.7%], 1 of 5 positive |
| el/nested-400.match | generated | control | 63764.8 | 69585.2 | 1.09x | 69220.8 | 1.09x | -0.5% | 29920 | 29872 | 2 % | [-4.1%..+5.0%], 3 of 5 positive | -0.8% [-4.7%..+0.8%], 3 of 5 positive |
| el/nested-400.match | immediate | control | 63764.8 | 61195.3 | 0.96x | 60694.7 | 0.95x | -0.8% | 29920 | 29872 | 2 % | [-12.1%..+5.4%], 3 of 5 positive | -2.9% [-9.3%..+1.0%], 1 of 5 positive |
| el/nested-refused-400.match | generated | control | 73373.7 | 80910.6 | 1.10x | 82639.3 | 1.13x | +2.1% | 1008 | 960 | 4 % | [-0.7%..+3.1%], 4 of 5 positive | -0.8% [-2.0%..+1.7%], 1 of 5 positive |
| el/nested-refused-400.match | immediate | control | 73373.7 | 70210.4 | 0.96x | 69425.0 | 0.95x | -1.1% | 1008 | 960 | 4 % | [-2.4%..+1.2%], 4 of 5 positive | -1.7% [-6.9%..+1.2%], 2 of 5 positive |
| el/parse-200k-tokens | generated | control | 18410275.0 | 18514475.0 | 1.01x | 18597012.5 | 1.01x | +0.4% | 9601323 | 9601314 | 10 % | [-2.1%..+5.4%], 2 of 5 positive | +1.3% [-8.3%..+7.0%], 4 of 5 positive |
| el/parse-500k-tokens | generated | control | 45905400.0 | 46741450.0 | 1.02x | 47163300.0 | 1.03x | +0.9% | 24001774 | 24001726 | 3 % | [-1.1%..+4.4%], 4 of 5 positive | -0.1% [-1.7%..+0.6%], 2 of 5 positive |
| el/floor | generated | hand | 310.8 | 929.8 | 2.99x | 905.8 | 2.91x | -2.6% | 1072 | 1024 | 7 % | [-5.7%..+2.1%], 1 of 5 positive | 0.0% [-4.3%..+1.0%], 1 of 5 positive |
| el/floor | immediate | hand | 310.8 | 930.3 | 2.99x | 911.5 | 2.93x | -2.0% | 1072 | 1024 | 7 % | [-3.4%..+3.6%], 1 of 5 positive | +0.3% [-3.9%..+1.9%], 4 of 5 positive |
| el/ladder | generated | hand | 969.3 | 2105.5 | 2.17x | 2066.6 | 2.13x | -1.8% | 1688 | 1640 | 7 % | [-3.7%..+0.8%], 1 of 5 positive | +1.1% [-8.0%..+3.4%], 3 of 5 positive |
| el/ladder | immediate | hand | 969.3 | 2075.7 | 2.14x | 2075.0 | 2.14x | 0.0% | 1688 | 1640 | 7 % | [-1.9%..+2.4%], 2 of 5 positive | -0.2% [-3.2%..+2.2%], 3 of 5 positive |
| el/nest7 | generated | hand | 682.3 | 2198.7 | 3.22x | 2195.3 | 3.22x | -0.2% | 1624 | 1576 | 6 % | [-2.7%..+1.7%], 1 of 5 positive | 0.0% [-0.7%..+1.3%], 3 of 5 positive |
| el/nest7 | immediate | hand | 682.3 | 2117.3 | 3.10x | 2086.9 | 3.06x | -1.4% | 1624 | 1576 | 6 % | [-7.3%..+1.5%], 1 of 5 positive | -2.2% [-3.7%..+0.8%], 1 of 5 positive |
| el/block | generated | hand | 1024.4 | 2063.2 | 2.01x | 2018.8 | 1.97x | -2.2% | 2232 | 2184 | 6 % | [-2.9%..-0.1%], 0 of 5 positive | -0.7% [-3.2%..+9.0%], 3 of 5 positive |
| el/block | immediate | hand | 1024.4 | 2038.2 | 1.99x | 2015.6 | 1.97x | -1.1% | 2232 | 2184 | 6 % | [-2.8%..+2.7%], 2 of 5 positive | -1.1% [-5.0%..+0.2%], 1 of 5 positive |
| el/try | generated | hand | 4147.6 | 6745.8 | 1.63x | 5673.0 | 1.37x | -15.9% | 6024 | 4296 | 3 % | [-17.1%..-14.7%], 0 of 5 positive | -3.2% [-5.2%..-2.0%], 0 of 5 positive |
| el/try | immediate | hand | 4147.6 | 6773.6 | 1.63x | 5691.4 | 1.37x | -16.0% | 6024 | 4296 | 3 % | [-17.5%..-14.6%], 0 of 5 positive | -3.3% [-3.6%..-1.5%], 0 of 5 positive |
| el/loop | generated | hand | 2277.3 | 4236.3 | 1.86x | 4239.8 | 1.86x | +0.1% | 4792 | 4744 | 4 % | [-2.2%..+1.6%], 3 of 5 positive | +0.3% [-3.6%..+1.0%], 3 of 5 positive |
| el/loop | immediate | hand | 2277.3 | 4183.3 | 1.84x | 4213.4 | 1.85x | +0.7% | 4792 | 4744 | 4 % | [-0.6%..+1.3%], 4 of 5 positive | -1.3% [-1.8%..+3.3%], 2 of 5 positive |
| el/terms100 | generated | hand | 10043.8 | 18038.0 | 1.80x | 18180.0 | 1.81x | +0.8% | 10672 | 10624 | 2 % | [-1.0%..+5.8%], 4 of 5 positive | 0.0% [-0.6%..+1.5%], 2 of 5 positive |
| el/terms100 | immediate | hand | 10043.8 | 18659.8 | 1.86x | 18864.7 | 1.88x | +1.1% | 10672 | 10624 | 2 % | [-3.3%..+6.1%], 4 of 5 positive | +2.2% [+0.5%..+4.3%], 5 of 5 positive |
| el/terms1000 | generated | hand | 97908.6 | 167186.0 | 1.71x | 172410.3 | 1.76x | +3.1% | 97072 | 97024 | 2 % | [-0.6%..+9.1%], 3 of 5 positive | -0.4% [-1.0%..+0.2%], 3 of 5 positive |
| el/terms1000 | immediate | hand | 97908.6 | 179016.9 | 1.83x | 177800.6 | 1.82x | -0.7% | 97072 | 97024 | 2 % | [-4.5%..+3.7%], 3 of 5 positive | +2.5% [-0.1%..+3.9%], 4 of 5 positive |
| el/overloads | generated | hand | 1877.5 | 4371.6 | 2.33x | 2867.0 | 1.53x | -34.4% | 5904 | 4176 | 5 % | [-36.8%..-33.0%], 0 of 5 positive | -3.6% [-4.9%..-1.1%], 0 of 5 positive |
| el/overloads | immediate | hand | 1877.5 | 4377.9 | 2.33x | 2814.0 | 1.50x | -35.7% | 5904 | 4176 | 5 % | [-36.5%..-34.2%], 0 of 5 positive | -2.3% [-4.1%..-1.5%], 0 of 5 positive |
| el/string | generated | hand | 300.9 | 1129.8 | 3.75x | 1104.2 | 3.67x | -2.3% | 1216 | 1168 | 15 % | [-6.5%..+3.0%], 2 of 5 positive | -1.9% [-4.9%..+2.0%], 2 of 5 positive |
| el/string | immediate | hand | 300.9 | 1110.0 | 3.69x | 1078.9 | 3.59x | -2.8% | 1216 | 1168 | 15 % | [-4.8%..+2.3%], 2 of 5 positive | -0.3% [-5.4%..+5.2%], 1 of 5 positive |
| el/interpolation | generated | hand | 1889.5 | 4639.9 | 2.46x | 4694.2 | 2.48x | +1.2% | 2216 | 2168 | 20 % | [-6.9%..+4.2%], 3 of 5 positive | +5.5% [-0.5%..+10.1%], 4 of 5 positive |
| el/interpolation | immediate | hand | 1889.5 | 4806.1 | 2.54x | 4548.6 | 2.41x | -5.4% | 2216 | 2168 | 20 % | [-9.4%..-0.9%], 0 of 5 positive | +3.2% [-5.1%..+5.4%], 1 of 5 positive |
| el/untyped | generated | hand | 34503.3 | 39149.7 | 1.13x | 41665.9 | 1.21x | +6.4% | 24760 | 24440 | 17 % | [+0.2%..+9.3%], 5 of 5 positive | +7.0% [-0.3%..+7.4%], 4 of 5 positive |
| el/refused-early | generated | hand | 435.3 | 1394.3 | 3.20x | 1386.5 | 3.19x | -0.6% | 912 | 864 | 6 % | [-5.6%..-0.1%], 0 of 5 positive | +2.3% [-0.8%..+3.6%], 3 of 5 positive |
| el/refused-early | immediate | hand | 435.3 | 1375.8 | 3.16x | 1401.0 | 3.22x | +1.8% | 912 | 864 | 6 % | [-0.7%..+2.4%], 4 of 5 positive | -1.7% [-5.6%..+1.5%], 1 of 5 positive |
| el/refused-late | generated | hand | 1458.0 | 2283.9 | 1.57x | 2316.5 | 1.59x | +1.4% | 912 | 864 | 13 % | [-8.8%..+9.1%], 4 of 5 positive | -2.4% [-6.6%..+0.9%], 2 of 5 positive |
| el/refused-late | immediate | hand | 1458.0 | 2310.1 | 1.58x | 2308.7 | 1.58x | -0.1% | 912 | 864 | 13 % | [-8.5%..+3.4%], 2 of 5 positive | +2.8% [-2.0%..+3.1%], 3 of 5 positive |
