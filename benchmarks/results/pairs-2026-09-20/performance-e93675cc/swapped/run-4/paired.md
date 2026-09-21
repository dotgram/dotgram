# Paired stand, 2026-09-21 02:27

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 239650.0 | 209754.7 | 0.88x | 217039.1 | 0.91x | +3.5% | 403288 | 403288 | 11 % |  |
| tsql/columns1000 | generated | scriptdom | 1786037.5 | 170256.2 | 0.10x | 170100.0 | 0.10x | -0.1% | 200489 | 200489 | 62 % |  |
| web/json.array10000 | generated | hand | 165898.4 | 186680.5 | 1.13x | 188482.8 | 1.14x | +1.0% | 720048 | 720048 | 7 % |  |
| sql/select20.at | generated | hand | 7042.8 | 17615.1 | 2.50x | 17296.2 | 2.46x | -1.8% | 21416 | 21416 | 3 % |  |
| sql/select20.window | generated | hand | 6985.6 | 17641.3 | 2.53x | 17613.3 | 2.52x | -0.2% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 374.5 | 371.2 | 0.99x | 372.1 | 0.99x | +0.3% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 368.9 | 368.4 | 1.00x | 372.7 | 1.01x | +1.2% | 0 | 0 | 20 % |  |
| el/ladder.scan | generated | control | 134.3 | 134.3 | 1.00x | 134.9 | 1.00x | +0.4% | 0 | 0 | 3 % |  |
| el/ladder.bool | generated | hand | 963.1 | 1666.5 | 1.73x | 1695.5 | 1.76x | +1.7% | 1776 | 1720 | 2 % |  |
| sql/refused-late.bool | generated | hand | 2607.5 | 11801.3 | 4.53x | 5686.7 | 2.18x | -51.8% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 7017.1 | 17666.5 | 2.52x | 17809.0 | 2.54x | +0.8% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4428275.0 | 4535175.0 | 1.02x | 4385687.5 | 0.99x | -3.3% | 6276664 | 6276488 | 24 % |  |
| sql/refused-cliff-case-1738 | generated | control | 13905312.5 | 5659587.5 | 0.41x | 14203737.5 | 1.02x | +151.0% | 7842219 | 74362781 | 147 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2474275.0 | 2456962.5 | 0.99x | 2460150.0 | 0.99x | +0.1% | 2994056 | 2994056 | 15 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 4223156.2 | 3234412.5 | 0.77x | 5730900.0 | 1.36x | +77.2% | 3740064 | 12948530 | 109 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 8357156.2 | 5182368.8 | 0.62x | 8924437.5 | 1.07x | +72.2% | 5840043 | 21143722 | 38 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 18428437.5 | 6294250.0 | 0.34x | 18055012.5 | 0.98x | +186.8% | 7298240 | 94725013 | 120 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6179800.0 | 6363200.0 | 1.03x | 6193300.0 | 1.00x | -2.7% | 8778619 | 8778600 | 45 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6587962.5 | 6664193.8 | 1.01x | 6389906.2 | 0.97x | -4.1% | 8775448 | 8775448 | 10 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 30288725.0 | 8812712.5 | 0.29x | 28487212.5 | 0.94x | +223.3% | 10969680 | 94857999 | 98 % |  |
| sql/refused-cliff-and-3393 | generated | control | 11870400.0 | 8677050.0 | 0.73x | 15825050.0 | 1.33x | +82.4% | 10966571 | 94854761 | 277 % |  |
| el/parse-200k-tokens | generated | control | 15734925.0 | 15270075.0 | 0.97x | 14829325.0 | 0.94x | -2.9% | 16801593 | 16801636 | 33 % |  |
| el/parse-500k-tokens | generated | control | 40803250.0 | 44297725.0 | 1.09x | 44511450.0 | 1.09x | +0.5% | 42002327 | 49503158 | 21 % |  |
| sql/worst-columns-100k | generated | control | 419091750.0 | 271659600.0 | 0.65x | 425687300.0 | 1.02x | +56.7% | 95921086 | 999698512 | 19 % |  |
| fix/Orders128.yield-string | generated | hand | 76457.6 | 249835.9 | 3.27x | 276854.5 | 3.62x | +10.8% | 117936 | 117936 | 4 % |  |
| web/media-type.quoted | generated | control | 237.6 | 290.9 | 1.22x | 288.8 | 1.22x | -0.7% | 816 | 816 | 12 % |  |
| el/ladder | generated | hand | 955.3 | 1669.9 | 1.75x | 1710.1 | 1.79x | +2.4% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 955.3 | 1134.8 | 1.19x | 1137.6 | 1.19x | +0.2% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 103044.4 | 142970.1 | 1.39x | 140440.7 | 1.36x | -1.8% | 169104 | 169131 | 43 % |  |
| el/terms1000 | immediate | hand | 103044.4 | 115970.2 | 1.13x | 115277.2 | 1.12x | -0.6% | 177120 | 177120 | 43 % |  |
| sql/select20 | generated | hand | 6999.4 | 18809.9 | 2.69x | 17811.3 | 2.54x | -5.3% | 21448 | 21448 | 7 % |  |
| sql/refused-late | generated | hand | 2588.2 | 12662.0 | 4.89x | 11645.6 | 4.50x | -8.0% | 13552 | 13552 | 6 % |  |
