What the expression language's immediate carrier reads once ExpressionParser.Immediate is carried immediately again (before: 56768831, where the copy was the tape; after: e49e58cd, with empty repeated captures handed over as Array.Empty too). Window 09:06-09:42, processors 0-7,16-23, `--stand-paired before after --only el/ --repeat 5`.

Median of 5 of 5 runs, each in a process of its own; control 41.9 ns (the runs' controls: 42.6, 41.9, 41.7, 42.5, 41.9).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-10-04 09:42

Ubuntu, NOT pinned, control 41.9 ns.

This binary, and the libraries it holds as the control, was built from 8cb80edf. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 56768831, framework net10.0, no properties, emitted 8d096dd712cc86a5); after (commit e49e58cd, framework net10.0, no properties, emitted 24a5f12df58cf975). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| el/ladder.scan | generated | control | 156.7 | 167.8 | 1.07x | 165.4 | 1.06x | -1.4% | 0 | 0 | 5 % | [-3.7%..+0.4%], 1 of 5 positive | -0.8% [-0.9%..+0.4%], 2 of 5 positive |
| el/refused-early.bool | match-vs-bool | hand | 492.3 | 1857.0 | 3.77x | 948.7 | 1.93x | -48.9% | 784 | 568 | 13 % | [-49.5%..-47.8%], 0 of 5 positive | -47.4% [-49.9%..-46.5%], 0 of 5 positive |
| el/refused-late.bool | match-vs-bool | hand | 1550.9 | 3227.9 | 2.08x | 1691.5 | 1.09x | -47.6% | 784 | 568 | 7 % | [-47.7%..-45.7%], 0 of 5 positive | -47.3% [-49.9%..-45.7%], 0 of 5 positive |
| el/ladder.bool | match-vs-bool | hand | 1375.9 | 3629.7 | 2.64x | 3659.5 | 2.66x | +0.8% | 1792 | 1280 | 44 % | [-6.6%..+1.3%], 3 of 5 positive | 0.0% [-1.3%..+9.1%], 3 of 5 positive |
| el/nested-100.match | generated | control | 29246.5 | 31699.7 | 1.08x | 30753.6 | 1.05x | -3.0% | 10704 | 960 | 4 % | [-4.4%..-0.8%], 0 of 5 positive | -0.3% [-1.5%..+1.4%], 2 of 5 positive |
| el/nested-100.match | immediate | control | 29246.5 | 29372.1 | 1.00x | 9587.0 | 0.33x | -67.4% | 10704 | 960 | 4 % | [-68.2%..-67.1%], 0 of 5 positive | +0.6% [-0.9%..+2.7%], 4 of 5 positive |
| el/nested-refused-100.match | generated | control | 26619.2 | 27279.7 | 1.02x | 28777.5 | 1.08x | +5.5% | 968 | 824 | 36 % | [+5.5%..+6.7%], 5 of 5 positive | -0.2% [-1.5%..+0.9%], 2 of 5 positive |
| el/nested-refused-100.match | immediate | control | 26619.2 | 24757.4 | 0.93x | 17779.5 | 0.67x | -28.2% | 968 | 824 | 36 % | [-30.6%..-26.7%], 0 of 5 positive | 0.0% [-2.2%..+2.9%], 2 of 5 positive |
| el/nested-400.match | generated | control | 114281.3 | 122126.7 | 1.07x | 119349.7 | 1.04x | -2.3% | 39504 | 960 | 6 % | [-3.1%..-0.4%], 0 of 5 positive | -0.3% [-3.8%..+1.7%], 2 of 5 positive |
| el/nested-400.match | immediate | control | 114281.3 | 113450.4 | 0.99x | 37638.8 | 0.33x | -66.8% | 39504 | 960 | 6 % | [-67.5%..-66.1%], 0 of 5 positive | +0.2% [+0.0%..+3.1%], 4 of 5 positive |
| el/nested-refused-400.match | generated | control | 112454.0 | 116483.3 | 1.04x | 120964.7 | 1.08x | +3.8% | 968 | 824 | 20 % | [+1.5%..+4.4%], 5 of 5 positive | -0.9% [-1.8%..+1.1%], 3 of 5 positive |
| el/nested-refused-400.match | immediate | control | 112454.0 | 96011.4 | 0.85x | 71308.2 | 0.63x | -25.7% | 968 | 824 | 20 % | [-32.3%..-25.5%], 0 of 5 positive | +3.6% [-3.0%..+5.2%], 4 of 5 positive |
| el/parse-200k-tokens | generated | control | 34648450.0 | 39419650.0 | 1.14x | 35512100.0 | 1.02x | -9.9% | 12002147 | 4801438 | 55 % | [-18.5%..-7.9%], 0 of 5 positive | +1.3% [-2.8%..+7.0%], 3 of 5 positive |
| el/parse-500k-tokens | generated | control | 87583350.0 | 123988600.0 | 1.42x | 90246200.0 | 1.03x | -27.2% | 30003830 | 12002071 | 15 % | [-36.9%..-20.5%], 0 of 5 positive | -14.7% [-15.6%..+5.3%], 1 of 5 positive |
| el/floor | generated | hand | 464.2 | 1503.4 | 3.24x | 1555.0 | 3.35x | +3.4% | 1056 | 960 | 9 % | [+1.2%..+4.0%], 5 of 5 positive | -1.0% [-2.8%..+1.6%], 1 of 5 positive |
| el/floor | immediate | hand | 464.2 | 1506.9 | 3.25x | 768.8 | 1.66x | -49.0% | 1056 | 960 | 9 % | [-51.1%..-47.4%], 0 of 5 positive | -1.6% [-5.2%..-0.2%], 0 of 5 positive |
| el/ladder | generated | hand | 1355.7 | 3661.5 | 2.70x | 3681.7 | 2.72x | +0.6% | 1792 | 1336 | 4 % | [-0.8%..+2.2%], 3 of 5 positive | -0.4% [-0.9%..+0.7%], 3 of 5 positive |
| el/ladder | immediate | hand | 1355.7 | 3653.7 | 2.70x | 1846.8 | 1.36x | -49.5% | 1792 | 1336 | 4 % | [-50.5%..-48.7%], 0 of 5 positive | -0.1% [-5.6%..+1.5%], 2 of 5 positive |
| el/nest7 | generated | hand | 852.6 | 3846.1 | 4.51x | 3798.8 | 4.46x | -1.2% | 1776 | 960 | 2 % | [-2.6%..-0.2%], 0 of 5 positive | -1.8% [-3.4%..-0.2%], 0 of 5 positive |
| el/nest7 | immediate | hand | 852.6 | 3748.4 | 4.40x | 1484.7 | 1.74x | -60.4% | 1776 | 960 | 2 % | [-61.4%..-59.0%], 0 of 5 positive | -0.7% [-3.8%..+0.7%], 1 of 5 positive |
| el/block | generated | hand | 1482.7 | 3493.4 | 2.36x | 3539.6 | 2.39x | +1.3% | 2264 | 2024 | 9 % | [+0.3%..+2.7%], 5 of 5 positive | -1.9% [-2.6%..+1.9%], 1 of 5 positive |
| el/block | immediate | hand | 1482.7 | 3477.0 | 2.35x | 1846.5 | 1.25x | -46.9% | 2264 | 2024 | 9 % | [-49.8%..-44.6%], 0 of 5 positive | -0.8% [-7.7%..+2.7%], 2 of 5 positive |
| el/try | generated | hand | 6246.5 | 9582.0 | 1.53x | 9470.5 | 1.52x | -1.2% | 4448 | 3800 | 3 % | [-1.6%..+0.1%], 1 of 5 positive | -3.1% [-4.8%..-0.3%], 0 of 5 positive |
| el/try | immediate | hand | 6246.5 | 9495.2 | 1.52x | 6049.6 | 0.97x | -36.3% | 4448 | 3824 | 3 % | [-37.4%..-34.6%], 0 of 5 positive | -2.8% [-5.5%..+0.0%], 1 of 5 positive |
| el/loop | generated | hand | 3425.2 | 7406.6 | 2.16x | 7396.8 | 2.16x | -0.1% | 4824 | 4272 | 5 % | [-1.9%..+2.6%], 3 of 5 positive | -1.4% [-3.3%..+0.0%], 0 of 5 positive |
| el/loop | immediate | hand | 3425.2 | 7431.8 | 2.17x | 4169.1 | 1.22x | -43.9% | 4824 | 4192 | 5 % | [-45.1%..-41.4%], 0 of 5 positive | -0.8% [-4.4%..+2.2%], 1 of 5 positive |
| el/terms100 | generated | hand | 12523.3 | 31297.2 | 2.50x | 30814.8 | 2.46x | -1.5% | 13056 | 5760 | 6 % | [-4.7%..+1.5%], 2 of 5 positive | -0.5% [-4.0%..+1.1%], 3 of 5 positive |
| el/terms100 | immediate | hand | 12523.3 | 33553.0 | 2.68x | 16705.2 | 1.33x | -50.2% | 13056 | 5760 | 6 % | [-51.5%..-48.0%], 0 of 5 positive | -2.1% [-5.7%..+2.0%], 1 of 5 positive |
| el/terms1000 | generated | hand | 120152.2 | 292671.7 | 2.44x | 287534.6 | 2.39x | -1.8% | 121056 | 48960 | 6 % | [-6.1%..+2.3%], 2 of 5 positive | +0.4% [-3.9%..+2.3%], 2 of 5 positive |
| el/terms1000 | immediate | hand | 120152.2 | 318802.1 | 2.65x | 158779.8 | 1.32x | -50.2% | 121059 | 48960 | 6 % | [-51.2%..-48.6%], 0 of 5 positive | -3.4% [-7.2%..+1.3%], 1 of 5 positive |
| el/overloads | generated | hand | 2888.9 | 4933.3 | 1.71x | 4889.2 | 1.69x | -0.9% | 4112 | 3896 | 4 % | [-1.8%..+0.6%], 2 of 5 positive | -1.8% [-2.2%..-1.1%], 0 of 5 positive |
| el/overloads | immediate | hand | 2888.9 | 4914.7 | 1.70x | 3386.8 | 1.17x | -31.1% | 4112 | 3856 | 4 % | [-33.0%..-29.8%], 0 of 5 positive | -2.3% [-5.7%..+0.8%], 1 of 5 positive |
| el/string | generated | hand | 490.4 | 1817.5 | 3.71x | 1839.1 | 3.75x | +1.2% | 1200 | 1128 | 5 % | [-0.8%..+2.7%], 3 of 5 positive | -1.8% [-2.9%..+0.4%], 1 of 5 positive |
| el/string | immediate | hand | 490.4 | 1782.8 | 3.64x | 1091.8 | 2.23x | -38.8% | 1200 | 1128 | 5 % | [-40.3%..-36.3%], 0 of 5 positive | -2.7% [-2.9%..-0.5%], 0 of 5 positive |
| el/interpolation | generated | hand | 3118.3 | 8292.3 | 2.66x | 8085.0 | 2.59x | -2.5% | 2272 | 2008 | 64 % | [-6.1%..+7.3%], 2 of 5 positive | -5.8% [-9.1%..+1.2%], 1 of 5 positive |
| el/interpolation | immediate | hand | 3118.3 | 8131.4 | 2.61x | 6514.0 | 2.09x | -19.9% | 2272 | 2008 | 64 % | [-33.5%..-19.4%], 0 of 5 positive | +2.5% [-7.4%..+5.7%], 4 of 5 positive |
| el/untyped | generated | hand | 58667.0 | 65068.8 | 1.11x | 64780.4 | 1.10x | -0.4% | 24448 | 24040 | 9 % | [-0.9%..-0.1%], 0 of 5 positive | -8.8% [-22.8%..+3.1%], 2 of 5 positive |
| el/refused-early | generated | hand | 775.0 | 2108.7 | 2.72x | 2151.6 | 2.78x | +2.0% | 784 | 736 | 17 % | [-0.5%..+4.0%], 4 of 5 positive | +2.9% [-0.7%..+2.9%], 4 of 5 positive |
| el/refused-early | immediate | hand | 775.0 | 2090.2 | 2.70x | 1905.3 | 2.46x | -8.8% | 784 | 1408 | 17 % | [-8.8%..-4.3%], 0 of 5 positive | -1.5% [-2.4%..+1.7%], 1 of 5 positive |
| el/refused-late | generated | hand | 2114.8 | 3571.7 | 1.69x | 3527.0 | 1.67x | -1.3% | 784 | 736 | 21 % | [-2.7%..+3.9%], 2 of 5 positive | +0.1% [-4.0%..+2.8%], 3 of 5 positive |
| el/refused-late | immediate | hand | 2114.8 | 3657.3 | 1.73x | 4721.6 | 2.23x | +29.1% | 784 | 3392 | 21 % | [+21.2%..+32.3%], 5 of 5 positive | -0.7% [-4.7%..+3.5%], 2 of 5 positive |
