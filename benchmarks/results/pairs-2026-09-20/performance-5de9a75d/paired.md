Median of 5 of 5 runs, each in a process of its own; control 31.8 ns (the runs' controls: 32.0, 31.8, 31.2, 31.7, 31.8).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 00:02

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

This binary, and the libraries it holds as the control, was built from b746f489. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5de9a75d, framework net10.0, no properties, emitted 194b78b7193dfafe). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| fix/orders400.text | generated | hand | 254656.2 | 226050.0 | 0.89x | 227806.2 | 0.89x | +0.8% | 403288 | 403288 | 10 % | [-3.0%..+5.6%], 3 of 5 positive | -0.7% [-5.7%..+3.7%], 2 of 5 positive |
| tsql/columns1000 | generated | scriptdom | 1774156.2 | 175006.2 | 0.10x | 178537.5 | 0.10x | +2.0% | 200489 | 200489 | 19 % | [-0.6%..+3.5%], 3 of 5 positive | +0.5% [-23.4%..+9.8%], 4 of 5 positive |
| web/json.array10000 | generated | hand | 172110.2 | 191087.5 | 1.11x | 201262.5 | 1.17x | +5.3% | 720048 | 720048 | 2 % | [+1.5%..+8.2%], 5 of 5 positive | +0.8% [-4.8%..+7.6%], 2 of 5 positive |
| sql/select20.at | generated | hand | 7009.0 | 18193.4 | 2.60x | 18196.9 | 2.60x | 0.0% | 21416 | 21416 | 10 % | [-0.2%..+2.0%], 4 of 5 positive | -6.1% [-8.3%..+0.6%], 2 of 5 positive |
| sql/select20.window | generated | hand | 6956.9 | 18307.1 | 2.63x | 18491.0 | 2.66x | +1.0% | 21416 | 21416 | 4 % | [-0.5%..+3.6%], 4 of 5 positive | -1.7% [-2.1%..-0.6%], 0 of 5 positive |
| sql/select20.scan | generated | control | 387.9 | 384.6 | 0.99x | 385.4 | 0.99x | +0.2% | 0 | 0 | 5 % | [-0.8%..+3.1%], 4 of 5 positive | -0.6% [-1.7%..+2.1%], 1 of 5 positive |
| tsql/select20.scan | generated | control | 389.2 | 382.8 | 0.98x | 383.3 | 0.98x | +0.1% | 0 | 0 | 8 % | [-1.3%..+2.0%], 3 of 5 positive | -3.7% [-3.7%..+24.0%], 3 of 5 positive |
| el/ladder.scan | generated | control | 139.4 | 139.2 | 1.00x | 140.2 | 1.01x | +0.7% | 0 | 0 | 12 % | [+0.4%..+3.1%], 5 of 5 positive | -0.6% [-1.5%..+3.5%], 2 of 5 positive |
| el/ladder.bool | generated | hand | 1006.5 | 1782.6 | 1.77x | 1732.9 | 1.72x | -2.8% | 1776 | 1720 | 5 % | [-4.5%..+7.7%], 2 of 5 positive | -2.1% [-5.3%..+3.6%], 2 of 5 positive |
| sql/refused-late.bool | generated | hand | 2608.0 | 12119.6 | 4.65x | 5949.2 | 2.28x | -50.9% | 13552 | 6616 | 4 % | [-51.1%..-50.6%], 0 of 5 positive | -52.7% [-53.7%..-50.3%], 0 of 5 positive |
| sql/select20.bool | generated | hand | 6860.3 | 18678.3 | 2.72x | 18845.9 | 2.75x | +0.9% | 21448 | 21392 | 7 % | [+0.4%..+3.0%], 5 of 5 positive | -1.4% [-1.4%..+4.0%], 3 of 5 positive |
| sql/refused-cliff-case-1391 | generated | control | 4707087.5 | 4753543.8 | 1.01x | 4740012.5 | 1.01x | -0.3% | 6276684 | 6276488 | 8 % | [-4.9%..+2.7%], 1 of 5 positive |  |
| sql/refused-cliff-case-1738 | generated | control | 18878437.5 | 13768525.0 | 0.73x | 5853900.0 | 0.31x | -57.5% | 74362362 | 7842262 | 32 % | [-65.3%..-50.7%], 0 of 5 positive |  |
| sql/refused-cliff-joins-891 | generated | control | 2685962.5 | 2707034.4 | 1.01x | 2688928.1 | 1.00x | -0.7% | 2994166 | 2994056 | 6 % | [-2.5%..+1.1%], 2 of 5 positive |  |
| sql/refused-cliff-joins-1113 | generated | control | 4595362.5 | 5501618.8 | 1.20x | 3475137.5 | 0.76x | -36.8% | 12948525 | 3740046 | 29 % | [-39.6%..-29.1%], 0 of 5 positive |  |
| sql/refused-cliff-joins-1738 | generated | control | 8863168.8 | 8877662.5 | 1.00x | 5386137.5 | 0.61x | -39.3% | 21143712 | 5840086 | 35 % | [-47.1%..-29.0%], 0 of 5 positive |  |
| sql/refused-cliff-joins-2172 | generated | control | 17907737.5 | 15967012.5 | 0.89x | 6690200.0 | 0.37x | -58.1% | 94724831 | 7298302 | 72 % | [-71.3%..-37.4%], 0 of 5 positive |  |
| sql/refused-cliff-paren-2715 | generated | control | 7657718.8 | 7729693.8 | 1.01x | 8370987.5 | 1.09x | +8.3% | 8778600 | 8778600 | 18 % | [-5.2%..+12.9%], 4 of 5 positive |  |
| sql/refused-cliff-and-2715 | generated | control | 7696100.0 | 7649831.2 | 0.99x | 7796750.0 | 1.01x | +1.9% | 8775318 | 8775448 | 26 % | [-5.6%..+11.2%], 3 of 5 positive |  |
| sql/refused-cliff-paren-3393 | generated | control | 29365550.0 | 30567037.5 | 1.04x | 8427125.0 | 0.29x | -72.4% | 94857750 | 10969699 | 89 % | [-77.6%..-32.1%], 0 of 5 positive |  |
| sql/refused-cliff-and-3393 | generated | control | 30414362.5 | 29100037.5 | 0.96x | 8483262.5 | 0.28x | -70.8% | 94854594 | 10966806 | 79 % | [-78.4%..-30.7%], 0 of 5 positive |  |
| el/parse-200k-tokens | generated | control | 16473412.5 | 15964225.0 | 0.97x | 16172987.5 | 0.98x | +1.3% | 16801507 | 16801571 | 9 % | [-2.9%..+4.0%], 2 of 5 positive |  |
| el/parse-500k-tokens | generated | control | 43573950.0 | 44236350.0 | 1.02x | 40688100.0 | 0.93x | -8.0% | 49503192 | 42002310 | 24 % | [-15.8%..+1.4%], 1 of 5 positive |  |
| sql/worst-columns-100k | generated | control | 456148000.0 | 456449400.0 | 1.00x | 287563600.0 | 0.63x | -37.0% | 999698410 | 95920900 | 6 % | [-38.2%..-32.4%], 0 of 5 positive |  |
| fix/Orders128.yield-string | generated | hand | 81639.5 | 269694.3 | 3.30x | 271889.6 | 3.33x | +0.8% | 117936 | 117936 | 13 % | [-3.1%..+7.4%], 3 of 5 positive | +1.0% [-2.8%..+4.7%], 2 of 5 positive |
| web/media-type.quoted | generated | control | 254.2 | 309.3 | 1.22x | 295.7 | 1.16x | -4.4% | 816 | 816 | 7 % | [-12.0%..+0.3%], 1 of 5 positive | +1.8% [-7.4%..+15.9%], 3 of 5 positive |
| el/ladder | generated | hand | 987.4 | 1763.3 | 1.79x | 1772.6 | 1.80x | +0.5% | 1776 | 1776 | 8 % | [-6.6%..+4.2%], 2 of 5 positive | +0.2% [-1.2%..+1.3%], 3 of 5 positive |
| el/ladder | immediate | hand | 987.4 | 1192.0 | 1.21x | 1186.0 | 1.20x | -0.5% | 1784 | 1784 | 8 % | [-1.3%..+1.6%], 2 of 5 positive | -1.1% [-8.0%..+1.7%], 3 of 5 positive |
| el/terms1000 | generated | hand | 107395.6 | 147499.3 | 1.37x | 146321.9 | 1.36x | -0.8% | 169104 | 169104 | 4 % | [-2.4%..+4.4%], 2 of 5 positive | +2.6% [-0.1%..+3.0%], 4 of 5 positive |
| el/terms1000 | immediate | hand | 107395.6 | 120432.0 | 1.12x | 120117.3 | 1.12x | -0.3% | 177120 | 177120 | 4 % | [-3.1%..+1.2%], 1 of 5 positive | +0.2% [-13.7%..+4.6%], 3 of 5 positive |
| sql/select20 | generated | hand | 6911.9 | 18593.9 | 2.69x | 18523.6 | 2.68x | -0.4% | 21448 | 21448 | 13 % | [-0.4%..+2.1%], 3 of 5 positive | -0.3% [-2.6%..+0.5%], 1 of 5 positive |
| sql/refused-late | generated | hand | 2655.8 | 12317.5 | 4.64x | 12561.7 | 4.73x | +2.0% | 13552 | 13552 | 25 % | [-0.4%..+2.5%], 4 of 5 positive | -0.6% [-2.1%..+1.0%], 3 of 5 positive |
