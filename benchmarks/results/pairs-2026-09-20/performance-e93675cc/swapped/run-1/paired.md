# Paired stand, 2026-09-21 02:04

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 241503.1 | 214900.0 | 0.89x | 218926.6 | 0.91x | +1.9% | 403288 | 403288 | 5 % |  |
| tsql/columns1000 | generated | scriptdom | 1688931.2 | 169225.0 | 0.10x | 169512.5 | 0.10x | +0.2% | 200489 | 200489 | 27 % |  |
| web/json.array10000 | generated | hand | 189425.8 | 182650.8 | 0.96x | 186570.3 | 0.98x | +2.1% | 720048 | 720048 | 6 % |  |
| sql/select20.at | generated | hand | 6893.5 | 18109.7 | 2.63x | 17649.0 | 2.56x | -2.5% | 21416 | 21416 | 8 % |  |
| sql/select20.window | generated | hand | 6763.6 | 18462.2 | 2.73x | 17971.0 | 2.66x | -2.7% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 376.1 | 369.6 | 0.98x | 370.0 | 0.98x | +0.1% | 0 | 0 | 13 % |  |
| tsql/select20.scan | generated | control | 370.9 | 373.1 | 1.01x | 378.3 | 1.02x | +1.4% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 137.9 | 136.2 | 0.99x | 134.5 | 0.98x | -1.2% | 0 | 0 | 12 % |  |
| el/ladder.bool | generated | hand | 984.2 | 1726.5 | 1.75x | 1674.7 | 1.70x | -3.0% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2553.4 | 12335.0 | 4.83x | 5736.2 | 2.25x | -53.5% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6636.6 | 18221.6 | 2.75x | 17824.7 | 2.69x | -2.2% | 21448 | 21392 | 16 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4915925.0 | 4830181.2 | 0.98x | 4921518.8 | 1.00x | +1.9% | 6276488 | 6276688 | 9 % |  |
| sql/refused-cliff-case-1738 | generated | control | 7880625.0 | 5794275.0 | 0.74x | 9186950.0 | 1.17x | +58.6% | 7842195 | 74362339 | 492 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2405600.0 | 2578400.0 | 1.07x | 2471475.0 | 1.03x | -4.1% | 2994056 | 2994142 | 21 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 4233256.2 | 3762175.0 | 0.89x | 4210950.0 | 0.99x | +11.9% | 3740044 | 12948549 | 103 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 6617312.5 | 5496537.5 | 0.83x | 7030387.5 | 1.06x | +27.9% | 5840019 | 21143832 | 124 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 12361175.0 | 7424200.0 | 0.60x | 13229350.0 | 1.07x | +78.2% | 7298259 | 94724975 | 131 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6508631.2 | 6755050.0 | 1.04x | 6592318.8 | 1.01x | -2.4% | 8778600 | 8778600 | 12 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6423650.0 | 6889275.0 | 1.07x | 6719837.5 | 1.05x | -2.5% | 8775467 | 8775448 | 8 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 13216575.0 | 8236200.0 | 0.62x | 13312425.0 | 1.01x | +61.6% | 10969785 | 94857894 | 279 % |  |
| sql/refused-cliff-and-3393 | generated | control | 11095200.0 | 8103175.0 | 0.73x | 15945125.0 | 1.44x | +96.8% | 10966547 | 94854889 | 561 % |  |
| el/parse-200k-tokens | generated | control | 15415025.0 | 14876200.0 | 0.97x | 14915925.0 | 0.97x | +0.3% | 16801550 | 16801636 | 42 % |  |
| el/parse-500k-tokens | generated | control | 40412375.0 | 44026150.0 | 1.09x | 44491500.0 | 1.10x | +1.1% | 42002327 | 49503176 | 21 % |  |
| sql/worst-columns-100k | generated | control | 422788500.0 | 275606850.0 | 0.65x | 432753750.0 | 1.02x | +57.0% | 95920879 | 999698552 | 27 % |  |
| fix/Orders128.yield-string | generated | hand | 76338.3 | 246056.2 | 3.22x | 248592.2 | 3.26x | +1.0% | 117936 | 117936 | 8 % |  |
| web/media-type.quoted | generated | control | 239.1 | 280.2 | 1.17x | 296.3 | 1.24x | +5.7% | 816 | 816 | 4 % |  |
| el/ladder | generated | hand | 975.0 | 1707.5 | 1.75x | 1670.9 | 1.71x | -2.1% | 1776 | 1776 | 11 % |  |
| el/ladder | immediate | hand | 975.0 | 1128.2 | 1.16x | 1129.0 | 1.16x | +0.1% | 1784 | 1784 | 11 % |  |
| el/terms1000 | generated | hand | 102501.8 | 140415.2 | 1.37x | 140907.3 | 1.37x | +0.4% | 169104 | 169104 | 3 % |  |
| el/terms1000 | immediate | hand | 102501.8 | 118444.2 | 1.16x | 115386.4 | 1.13x | -2.6% | 177120 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6706.3 | 18965.9 | 2.83x | 17833.7 | 2.66x | -6.0% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2506.4 | 12760.9 | 5.09x | 11833.3 | 4.72x | -7.3% | 13552 | 13552 | 14 % |  |
