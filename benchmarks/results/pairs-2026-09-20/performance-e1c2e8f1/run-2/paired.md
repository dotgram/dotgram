# Paired stand, 2026-09-21 00:51

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e1c2e8f1, framework net10.0, no properties, emitted edc6933b61ef4954). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 393037.5 | 285775.0 | 0.73x | 329568.8 | 0.84x | +15.3% | 403288 | 403288 | 69 % |  |
| tsql/columns1000 | generated | scriptdom | 2309662.5 | 257087.5 | 0.11x | 227737.5 | 0.10x | -11.4% | 200489 | 200489 | 54 % |  |
| web/json.array10000 | generated | hand | 173247.7 | 194772.7 | 1.12x | 195325.8 | 1.13x | +0.3% | 720048 | 720048 | 8 % |  |
| sql/select20.at | generated | hand | 6991.5 | 17864.6 | 2.56x | 18893.5 | 2.70x | +5.8% | 21416 | 21416 | 15 % |  |
| sql/select20.window | generated | hand | 6863.6 | 18367.0 | 2.68x | 18982.9 | 2.77x | +3.4% | 21416 | 21416 | 16 % |  |
| sql/select20.scan | generated | control | 380.2 | 386.1 | 1.02x | 386.0 | 1.02x | 0.0% | 0 | 0 | 52 % |  |
| tsql/select20.scan | generated | control | 374.4 | 383.7 | 1.02x | 380.6 | 1.02x | -0.8% | 0 | 0 | 10 % |  |
| el/ladder.scan | generated | control | 137.3 | 140.1 | 1.02x | 142.3 | 1.04x | +1.6% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 987.2 | 1753.3 | 1.78x | 1737.3 | 1.76x | -0.9% | 1776 | 1720 | 13 % |  |
| sql/refused-late.bool | generated | hand | 2694.1 | 12551.2 | 4.66x | 6381.2 | 2.37x | -49.2% | 13552 | 6616 | 43 % |  |
| sql/select20.bool | generated | hand | 6883.8 | 18184.6 | 2.64x | 19540.0 | 2.84x | +7.5% | 21448 | 21392 | 32 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4561843.8 | 4471356.2 | 0.98x | 4573006.2 | 1.00x | +2.3% | 6276488 | 6276488 | 9 % |  |
| sql/refused-cliff-case-1738 | generated | control | 18423525.0 | 16387637.5 | 0.89x | 6078362.5 | 0.33x | -62.9% | 74362358 | 7842264 | 138 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2771325.0 | 2703875.0 | 0.98x | 2905634.4 | 1.05x | +7.5% | 2994166 | 2994123 | 12 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 4939543.8 | 3958087.5 | 0.80x | 3823962.5 | 0.77x | -3.4% | 12948530 | 3740106 | 93 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 9592118.8 | 10145712.5 | 1.06x | 5548875.0 | 0.58x | -45.3% | 21143689 | 5840129 | 43 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 13597050.0 | 12741525.0 | 0.94x | 7286850.0 | 0.54x | -42.8% | 94725077 | 7298345 | 66 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 8066750.0 | 7797068.8 | 0.97x | 8188568.8 | 1.02x | +5.0% | 8778619 | 8778600 | 20 % |  |
| sql/refused-cliff-and-2715 | generated | control | 7762637.5 | 8150968.8 | 1.05x | 7676287.5 | 0.99x | -5.8% | 8775448 | 8775208 | 27 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 16562800.0 | 15580575.0 | 0.94x | 9564275.0 | 0.58x | -38.6% | 94858068 | 10969723 | 310 % |  |
| sql/refused-cliff-and-3393 | generated | control | 15623800.0 | 18083450.0 | 1.16x | 10237825.0 | 0.66x | -43.4% | 94854833 | 10966806 | 315 % |  |
| el/parse-200k-tokens | generated | control | 15540437.5 | 16731925.0 | 1.08x | 16507762.5 | 1.06x | -1.3% | 16801636 | 16801569 | 32 % |  |
| el/parse-500k-tokens | generated | control | 42220600.0 | 42912600.0 | 1.02x | 42635500.0 | 1.01x | -0.6% | 49503176 | 42002310 | 44 % |  |
| sql/worst-columns-100k | generated | control | 464885800.0 | 439372950.0 | 0.95x | 275577900.0 | 0.59x | -37.3% | 999698398 | 95920880 | 64 % |  |
| fix/Orders128.yield-string | generated | hand | 79677.5 | 250176.2 | 3.14x | 252779.7 | 3.17x | +1.0% | 117936 | 117936 | 84 % |  |
| web/media-type.quoted | generated | control | 244.4 | 311.3 | 1.27x | 289.8 | 1.19x | -6.9% | 816 | 816 | 32 % |  |
| el/ladder | generated | hand | 995.4 | 1810.5 | 1.82x | 1806.7 | 1.82x | -0.2% | 1776 | 1776 | 42 % |  |
| el/ladder | immediate | hand | 995.4 | 1171.1 | 1.18x | 1173.5 | 1.18x | +0.2% | 1784 | 1784 | 42 % |  |
| el/terms1000 | generated | hand | 107203.2 | 149093.0 | 1.39x | 149157.5 | 1.39x | 0.0% | 169150 | 169128 | 34 % |  |
| el/terms1000 | immediate | hand | 107203.2 | 119114.0 | 1.11x | 119254.4 | 1.11x | +0.1% | 177120 | 177120 | 34 % |  |
| sql/select20 | generated | hand | 8877.6 | 22325.9 | 2.51x | 24752.5 | 2.79x | +10.9% | 21448 | 21448 | 76 % |  |
| sql/refused-late | generated | hand | 2603.9 | 12403.2 | 4.76x | 12969.3 | 4.98x | +4.6% | 13552 | 13552 | 38 % |  |
