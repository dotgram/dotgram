# Paired stand, 2026-09-21 02:12

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 245142.2 | 215098.4 | 0.88x | 225635.9 | 0.92x | +4.9% | 403288 | 403288 | 24 % |  |
| tsql/columns1000 | generated | scriptdom | 1872825.0 | 192612.5 | 0.10x | 185431.2 | 0.10x | -3.7% | 200489 | 200489 | 60 % |  |
| web/json.array10000 | generated | hand | 166586.7 | 185126.6 | 1.11x | 184334.4 | 1.11x | -0.4% | 720048 | 720048 | 19 % |  |
| sql/select20.at | generated | hand | 6569.0 | 17331.0 | 2.64x | 18222.9 | 2.77x | +5.1% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6572.3 | 17953.0 | 2.73x | 18681.3 | 2.84x | +4.1% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 368.2 | 386.1 | 1.05x | 374.2 | 1.02x | -3.1% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 372.4 | 373.4 | 1.00x | 374.0 | 1.00x | +0.1% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 135.5 | 138.1 | 1.02x | 139.2 | 1.03x | +0.8% | 0 | 0 | 19 % |  |
| el/ladder.bool | generated | hand | 1019.1 | 1692.7 | 1.66x | 1671.0 | 1.64x | -1.3% | 1776 | 1720 | 20 % |  |
| sql/refused-late.bool | generated | hand | 2518.5 | 11898.4 | 4.72x | 5855.3 | 2.32x | -50.8% | 13552 | 6616 | 12 % |  |
| sql/select20.bool | generated | hand | 6618.8 | 17792.6 | 2.69x | 18900.8 | 2.86x | +6.2% | 21448 | 21392 | 9 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4890068.8 | 4909025.0 | 1.00x | 4936481.2 | 1.01x | +0.6% | 6276488 | 6276664 | 16 % |  |
| sql/refused-cliff-case-1738 | generated | control | 11199837.5 | 5515975.0 | 0.49x | 9303087.5 | 0.83x | +68.7% | 7842306 | 74362340 | 200 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2470437.5 | 2520337.5 | 1.02x | 2573775.0 | 1.04x | +2.1% | 2994056 | 2994166 | 15 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 4672587.5 | 3278668.8 | 0.70x | 4402356.2 | 0.94x | +34.3% | 3740043 | 12948549 | 84 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 7331912.5 | 5179637.5 | 0.71x | 7501262.5 | 1.02x | +44.8% | 5840089 | 21143712 | 113 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 17536412.5 | 6713087.5 | 0.38x | 16492737.5 | 0.94x | +145.7% | 7298283 | 94724988 | 119 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6412637.5 | 6824512.5 | 1.06x | 6466200.0 | 1.01x | -5.3% | 8778600 | 8778600 | 22 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6326862.5 | 6461981.2 | 1.02x | 6356975.0 | 1.00x | -1.6% | 8775424 | 8775448 | 10 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 13458650.0 | 7973175.0 | 0.59x | 14616775.0 | 1.09x | +83.3% | 10969723 | 94857880 | 256 % |  |
| sql/refused-cliff-and-3393 | generated | control | 14509800.0 | 8121075.0 | 0.56x | 13040750.0 | 0.90x | +60.6% | 10966572 | 94854726 | 114 % |  |
| el/parse-200k-tokens | generated | control | 15211875.0 | 15516400.0 | 1.02x | 14777250.0 | 0.97x | -4.8% | 16801593 | 16801485 | 57 % |  |
| el/parse-500k-tokens | generated | control | 41018525.0 | 44818775.0 | 1.09x | 44502950.0 | 1.08x | -0.7% | 42002327 | 49503161 | 65 % |  |
| sql/worst-columns-100k | generated | control | 433227700.0 | 259003700.0 | 0.60x | 441749450.0 | 1.02x | +70.6% | 95920912 | 999698393 | 20 % |  |
| fix/Orders128.yield-string | generated | hand | 79024.4 | 268641.2 | 3.40x | 267572.7 | 3.39x | -0.4% | 117936 | 117936 | 16 % |  |
| web/media-type.quoted | generated | control | 238.3 | 304.9 | 1.28x | 302.1 | 1.27x | -0.9% | 816 | 816 | 3 % |  |
| el/ladder | generated | hand | 1012.6 | 1687.8 | 1.67x | 1680.7 | 1.66x | -0.4% | 1776 | 1776 | 6 % |  |
| el/ladder | immediate | hand | 1012.6 | 1136.5 | 1.12x | 1129.5 | 1.12x | -0.6% | 1784 | 1784 | 6 % |  |
| el/terms1000 | generated | hand | 130205.3 | 143660.0 | 1.10x | 139518.4 | 1.07x | -2.9% | 169104 | 169104 | 8 % |  |
| el/terms1000 | immediate | hand | 130205.3 | 115489.8 | 0.89x | 116176.9 | 0.89x | +0.6% | 177120 | 177120 | 8 % |  |
| sql/select20 | generated | hand | 6528.1 | 17711.1 | 2.71x | 18739.9 | 2.87x | +5.8% | 21448 | 21448 | 18 % |  |
| sql/refused-late | generated | hand | 2516.1 | 11762.3 | 4.67x | 11928.4 | 4.74x | +1.4% | 13552 | 13552 | 5 % |  |
