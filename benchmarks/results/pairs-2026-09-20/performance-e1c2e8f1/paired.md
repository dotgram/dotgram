Median of 5 of 5 runs, each in a process of its own; control 31.0 ns (the runs' controls: 32.1, 31.6, 30.9, 31.0, 30.9).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 01:17

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e1c2e8f1, framework net10.0, no properties, emitted edc6933b61ef4954). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| fix/orders400.text | generated | hand | 248675.0 | 224000.0 | 0.90x | 218281.2 | 0.88x | -2.6% | 403288 | 403288 | 60 % | [-5.1%..+15.3%], 2 of 5 positive | -1.3% [-5.1%..+4.2%], 1 of 5 positive |
| tsql/columns1000 | generated | scriptdom | 1933050.0 | 175556.2 | 0.09x | 179393.8 | 0.09x | +2.2% | 200489 | 200489 | 32 % | [-12.6%..+3.7%], 1 of 5 positive | +22.5% [-13.5%..+22.7%], 2 of 5 positive |
| web/json.array10000 | generated | hand | 189887.5 | 190004.7 | 1.00x | 186281.2 | 0.98x | -2.0% | 720048 | 720048 | 13 % | [-2.0%..+0.3%], 2 of 5 positive | -3.6% [-3.6%..+3.6%], 1 of 5 positive |
| sql/select20.at | generated | hand | 6991.5 | 17811.4 | 2.55x | 18103.0 | 2.59x | +1.6% | 21416 | 21416 | 3 % | [+0.3%..+5.8%], 5 of 5 positive | 0.0% [-1.5%..+3.9%], 2 of 5 positive |
| sql/select20.window | generated | hand | 6863.6 | 18367.0 | 2.68x | 19108.5 | 2.78x | +4.0% | 21416 | 21416 | 9 % | [+2.1%..+7.9%], 5 of 5 positive | -0.3% [-1.7%..+2.9%], 3 of 5 positive |
| sql/select20.scan | generated | control | 393.1 | 389.3 | 0.99x | 386.0 | 0.98x | -0.9% | 0 | 0 | 6 % | [-8.3%..+1.5%], 1 of 5 positive | -1.2% [-1.5%..+0.4%], 2 of 5 positive |
| tsql/select20.scan | generated | control | 376.5 | 383.7 | 1.02x | 380.6 | 1.01x | -0.8% | 0 | 0 | 15 % | [-3.4%..+3.6%], 1 of 5 positive | +1.2% [-1.4%..+1.7%], 2 of 5 positive |
| el/ladder.scan | generated | control | 139.9 | 140.1 | 1.00x | 142.3 | 1.02x | +1.6% | 0 | 0 | 7 % | [-8.1%..+3.1%], 4 of 5 positive | -0.1% [-4.9%..+0.0%], 1 of 5 positive |
| el/ladder.bool | generated | hand | 991.1 | 1740.9 | 1.76x | 1737.3 | 1.75x | -0.2% | 1776 | 1720 | 7 % | [-2.0%..+5.1%], 2 of 5 positive | +0.9% [-5.1%..+3.8%], 1 of 5 positive |
| sql/refused-late.bool | generated | hand | 2635.5 | 12136.5 | 4.60x | 5991.1 | 2.27x | -50.6% | 13552 | 6616 | 7 % | [-51.1%..-49.2%], 0 of 5 positive | -51.2% [-52.9%..-50.4%], 0 of 5 positive |
| sql/select20.bool | generated | hand | 6883.8 | 18184.6 | 2.64x | 18822.7 | 2.73x | +3.5% | 21448 | 21392 | 4 % | [+0.2%..+7.5%], 5 of 5 positive | +2.8% [-1.8%..+3.3%], 3 of 5 positive |
| sql/refused-cliff-case-1391 | generated | control | 4561843.8 | 4507868.8 | 0.99x | 4573006.2 | 1.00x | +1.4% | 6276488 | 6276488 | 11 % | [-0.3%..+4.0%], 3 of 5 positive | +6.3% [-4.1%..+14.6%], 3 of 5 positive |
| sql/refused-cliff-case-1738 | generated | control | 14965837.5 | 14316962.5 | 0.96x | 6078362.5 | 0.41x | -57.5% | 74362718 | 7842219 | 53 % | [-74.2%..-32.9%], 0 of 5 positive |  |
| sql/refused-cliff-joins-891 | generated | control | 2738787.5 | 2703875.0 | 0.99x | 2760562.5 | 1.01x | +2.1% | 2994166 | 2994056 | 6 % | [-3.6%..+9.2%], 3 of 5 positive | +0.4% [-1.7%..+3.6%], 3 of 5 positive |
| sql/refused-cliff-joins-1113 | generated | control | 4261225.0 | 4032768.8 | 0.95x | 3291275.0 | 0.77x | -18.4% | 12948549 | 3740106 | 28 % | [-31.7%..-3.4%], 0 of 5 positive |  |
| sql/refused-cliff-joins-1738 | generated | control | 9207093.8 | 8430968.8 | 0.92x | 5299225.0 | 0.58x | -37.1% | 21143712 | 5840129 | 17 % | [-45.3%..-35.9%], 0 of 5 positive |  |
| sql/refused-cliff-joins-2172 | generated | control | 15983850.0 | 12741525.0 | 0.80x | 7033325.0 | 0.44x | -44.8% | 94725047 | 7298283 | 73 % | [-70.5%..-37.1%], 0 of 5 positive |  |
| sql/refused-cliff-paren-2715 | generated | control | 7965468.8 | 7797068.8 | 0.98x | 8019006.2 | 1.01x | +2.8% | 8778600 | 8778600 | 16 % | [-6.5%..+8.1%], 3 of 5 positive | -0.1% [-4.9%..+2.0%], 3 of 5 positive |
| sql/refused-cliff-and-2715 | generated | control | 7529737.5 | 7667418.8 | 1.02x | 7768975.0 | 1.03x | +1.3% | 8775448 | 8775275 | 11 % | [-7.3%..+8.1%], 3 of 5 positive | -2.5% [-6.1%..+5.1%], 3 of 5 positive |
| sql/refused-cliff-paren-3393 | generated | control | 16562800.0 | 15101425.0 | 0.91x | 8804650.0 | 0.53x | -41.7% | 94857940 | 10969699 | 77 % | [-74.1%..-38.0%], 0 of 5 positive |  |
| sql/refused-cliff-and-3393 | generated | control | 30063350.0 | 29482800.0 | 0.98x | 9726662.5 | 0.32x | -67.0% | 94854770 | 10966528 | 55 % | [-72.5%..-43.4%], 0 of 5 positive |  |
| el/parse-200k-tokens | generated | control | 15540437.5 | 16276625.0 | 1.05x | 16070662.5 | 1.03x | -1.3% | 16801614 | 16801574 | 11 % | [-2.5%..+7.0%], 2 of 5 positive |  |
| el/parse-500k-tokens | generated | control | 42220600.0 | 41600950.0 | 0.99x | 38625750.0 | 0.91x | -7.2% | 49503176 | 42002327 | 27 % | [-9.5%..-0.6%], 0 of 5 positive |  |
| sql/worst-columns-100k | generated | control | 429804300.0 | 429797650.0 | 1.00x | 283231350.0 | 0.66x | -34.1% | 999698406 | 95920958 | 15 % | [-38.3%..-30.3%], 0 of 5 positive |  |
| fix/Orders128.yield-string | generated | hand | 79677.5 | 259507.6 | 3.26x | 273501.0 | 3.43x | +5.4% | 117936 | 117936 | 7 % | [-1.3%..+12.7%], 4 of 5 positive | -3.0% [-13.9%..+3.0%], 2 of 5 positive |
| web/media-type.quoted | generated | control | 244.0 | 304.1 | 1.25x | 289.8 | 1.19x | -4.7% | 816 | 816 | 3 % | [-9.6%..-2.1%], 0 of 5 positive | +1.9% [+0.2%..+6.1%], 5 of 5 positive |
| el/ladder | generated | hand | 995.4 | 1703.2 | 1.71x | 1751.2 | 1.76x | +2.8% | 1776 | 1776 | 10 % | [-1.0%..+3.2%], 3 of 5 positive | +2.6% [-0.9%..+3.0%], 4 of 5 positive |
| el/ladder | immediate | hand | 995.4 | 1171.1 | 1.18x | 1154.8 | 1.16x | -1.4% | 1784 | 1784 | 10 % | [-1.9%..+0.3%], 2 of 5 positive | -0.2% [-1.9%..+2.8%], 3 of 5 positive |
| el/terms1000 | generated | hand | 105971.2 | 144777.4 | 1.37x | 144873.3 | 1.37x | +0.1% | 169104 | 169104 | 7 % | [-1.7%..+1.7%], 3 of 5 positive | +0.8% [-5.6%..+1.7%], 3 of 5 positive |
| el/terms1000 | immediate | hand | 105971.2 | 119114.0 | 1.12x | 119254.4 | 1.13x | +0.1% | 177120 | 177120 | 7 % | [-1.5%..+1.2%], 3 of 5 positive | -0.6% [-4.3%..+4.7%], 2 of 5 positive |
| sql/select20 | generated | hand | 6891.3 | 17805.1 | 2.58x | 18385.9 | 2.67x | +3.3% | 21448 | 21448 | 32 % | [+1.7%..+10.9%], 5 of 5 positive | +2.2% [-3.0%..+4.6%], 3 of 5 positive |
| sql/refused-late | generated | hand | 2600.1 | 11889.6 | 4.57x | 12217.2 | 4.70x | +2.8% | 13552 | 13552 | 6 % | [+1.4%..+4.6%], 5 of 5 positive | +0.4% [-0.9%..+1.4%], 3 of 5 positive |
