# Paired stand, 2026-09-21 01:41

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 246157.8 | 223787.5 | 0.91x | 216464.1 | 0.88x | -3.3% | 403288 | 403288 | 39 % |  |
| tsql/columns1000 | generated | scriptdom | 2170637.5 | 213781.2 | 0.10x | 175075.0 | 0.08x | -18.1% | 200489 | 200489 | 53 % |  |
| web/json.array10000 | generated | hand | 186360.9 | 186843.0 | 1.00x | 183491.4 | 0.98x | -1.8% | 720048 | 720048 | 15 % |  |
| sql/select20.at | generated | hand | 6797.8 | 17480.2 | 2.57x | 19402.0 | 2.85x | +11.0% | 21416 | 21416 | 8 % |  |
| sql/select20.window | generated | hand | 6807.9 | 17820.7 | 2.62x | 19652.9 | 2.89x | +10.3% | 21416 | 21416 | 10 % |  |
| sql/select20.scan | generated | control | 392.8 | 373.8 | 0.95x | 374.6 | 0.95x | +0.2% | 0 | 0 | 8 % |  |
| tsql/select20.scan | generated | control | 371.3 | 376.9 | 1.02x | 373.9 | 1.01x | -0.8% | 0 | 0 | 14 % |  |
| el/ladder.scan | generated | control | 141.1 | 137.0 | 0.97x | 137.8 | 0.98x | +0.6% | 0 | 0 | 22 % |  |
| el/ladder.bool | generated | hand | 974.1 | 1692.9 | 1.74x | 1698.5 | 1.74x | +0.3% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2619.2 | 12171.0 | 4.65x | 6234.3 | 2.38x | -48.8% | 13552 | 6616 | 11 % |  |
| sql/select20.bool | generated | hand | 6734.2 | 17865.4 | 2.65x | 19966.3 | 2.96x | +11.8% | 21448 | 21392 | 8 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4506193.8 | 4541450.0 | 1.01x | 4529662.5 | 1.01x | -0.3% | 6276488 | 6276488 | 14 % |  |
| sql/refused-cliff-case-1738 | generated | control | 8879475.0 | 8863750.0 | 1.00x | 6124650.0 | 0.69x | -30.9% | 74362514 | 7842220 | 352 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2650837.5 | 2694359.4 | 1.02x | 2618153.1 | 0.99x | -2.8% | 2994166 | 2994056 | 15 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 4909156.2 | 5426109.4 | 1.11x | 3291653.1 | 0.67x | -39.3% | 12948488 | 3740043 | 88 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 8616393.8 | 9212893.8 | 1.07x | 5327018.8 | 0.62x | -42.2% | 21143693 | 5840150 | 41 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 14760637.5 | 20313100.0 | 1.38x | 6353950.0 | 0.43x | -68.7% | 94725116 | 7298370 | 157 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6384406.2 | 6375106.2 | 1.00x | 6528868.8 | 1.02x | +2.4% | 8778600 | 8778600 | 7 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6340218.8 | 6375093.8 | 1.01x | 6572956.2 | 1.04x | +3.1% | 8775448 | 8775424 | 12 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 30822700.0 | 31791737.5 | 1.03x | 8435550.0 | 0.27x | -73.5% | 94857915 | 10969724 | 106 % |  |
| sql/refused-cliff-and-3393 | generated | control | 12896800.0 | 11636950.0 | 0.90x | 8398775.0 | 0.65x | -27.8% | 94854754 | 10966571 | 286 % |  |
| el/parse-200k-tokens | generated | control | 15285437.5 | 15731737.5 | 1.03x | 15902575.0 | 1.04x | +1.1% | 16801593 | 16801485 | 35 % |  |
| el/parse-500k-tokens | generated | control | 40309800.0 | 41086900.0 | 1.02x | 39398250.0 | 0.98x | -4.1% | 49503178 | 42002327 | 43 % |  |
| sql/worst-columns-100k | generated | control | 465016950.0 | 449358700.0 | 0.97x | 285668900.0 | 0.61x | -36.4% | 999698544 | 95920877 | 25 % |  |
| fix/Orders128.yield-string | generated | hand | 79918.4 | 246387.3 | 3.08x | 259182.0 | 3.24x | +5.2% | 117936 | 117936 | 8 % |  |
| web/media-type.quoted | generated | control | 246.0 | 309.5 | 1.26x | 287.0 | 1.17x | -7.3% | 816 | 816 | 12 % |  |
| el/ladder | generated | hand | 979.9 | 1712.2 | 1.75x | 1708.0 | 1.74x | -0.3% | 1776 | 1776 | 7 % |  |
| el/ladder | immediate | hand | 979.9 | 1145.0 | 1.17x | 1147.5 | 1.17x | +0.2% | 1784 | 1784 | 7 % |  |
| el/terms1000 | generated | hand | 104952.4 | 143452.0 | 1.37x | 142728.5 | 1.36x | -0.5% | 169104 | 169104 | 18 % |  |
| el/terms1000 | immediate | hand | 104952.4 | 116190.2 | 1.11x | 116306.1 | 1.11x | +0.1% | 177120 | 177120 | 18 % |  |
| sql/select20 | generated | hand | 6665.8 | 17892.5 | 2.68x | 18714.4 | 2.81x | +4.6% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2601.2 | 12076.0 | 4.64x | 12357.6 | 4.75x | +2.3% | 13552 | 13552 | 7 % |  |
