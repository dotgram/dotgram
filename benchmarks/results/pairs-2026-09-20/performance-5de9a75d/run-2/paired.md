# Paired stand, 2026-09-20 23:37

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

This binary, and the libraries it holds as the control, was built from b746f489. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5de9a75d, framework net10.0, no properties, emitted 194b78b7193dfafe). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 256031.2 | 228162.5 | 0.89x | 221237.5 | 0.86x | -3.0% | 403288 | 403288 | 14 % |  |
| tsql/columns1000 | generated | scriptdom | 1823906.2 | 179681.2 | 0.10x | 178537.5 | 0.10x | -0.6% | 200489 | 200489 | 55 % |  |
| web/json.array10000 | generated | hand | 172110.2 | 191087.5 | 1.11x | 199467.2 | 1.16x | +4.4% | 720048 | 720048 | 7 % |  |
| sql/select20.at | generated | hand | 6962.2 | 18238.5 | 2.62x | 18485.6 | 2.66x | +1.4% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6879.0 | 18164.9 | 2.64x | 18823.8 | 2.74x | +3.6% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 378.0 | 384.3 | 1.02x | 385.4 | 1.02x | +0.3% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 389.2 | 385.4 | 0.99x | 391.2 | 1.01x | +1.5% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 137.8 | 139.4 | 1.01x | 140.0 | 1.02x | +0.4% | 0 | 0 | 15 % |  |
| el/ladder.bool | generated | hand | 1006.5 | 1782.6 | 1.77x | 1732.9 | 1.72x | -2.8% | 1776 | 1720 | 10 % |  |
| sql/refused-late.bool | generated | hand | 2608.0 | 12021.1 | 4.61x | 5898.1 | 2.26x | -50.9% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6852.8 | 18774.0 | 2.74x | 18845.9 | 2.75x | +0.4% | 21448 | 21392 | 8 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4707087.5 | 4740800.0 | 1.01x | 4740012.5 | 1.01x | 0.0% | 6276488 | 6276488 | 11 % |  |
| sql/refused-cliff-case-1738 | generated | control | 19107287.5 | 13768525.0 | 0.72x | 5863325.0 | 0.31x | -57.4% | 74362360 | 7842262 | 104 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2705968.8 | 2707034.4 | 1.00x | 2709006.2 | 1.00x | +0.1% | 2994166 | 2994056 | 5 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 4517618.8 | 5658506.2 | 1.25x | 3475137.5 | 0.77x | -38.6% | 12948525 | 3740046 | 103 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 8863168.8 | 8870218.8 | 1.00x | 5422756.2 | 0.61x | -38.9% | 21143712 | 5840125 | 47 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 17347187.5 | 14312575.0 | 0.83x | 6648100.0 | 0.38x | -53.6% | 94724827 | 7298283 | 185 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7657718.8 | 7729693.8 | 1.01x | 8370987.5 | 1.09x | +8.3% | 8778600 | 8778600 | 21 % |  |
| sql/refused-cliff-and-2715 | generated | control | 7696100.0 | 7649831.2 | 0.99x | 7796750.0 | 1.01x | +1.9% | 8775318 | 8775448 | 19 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 12821075.0 | 12115925.0 | 0.95x | 8224950.0 | 0.64x | -32.1% | 94857744 | 10969681 | 270 % |  |
| sql/refused-cliff-and-3393 | generated | control | 17925700.0 | 12057700.0 | 0.67x | 8358150.0 | 0.47x | -30.7% | 94854633 | 10966806 | 228 % |  |
| el/parse-200k-tokens | generated | control | 16994975.0 | 16559787.5 | 0.97x | 16341675.0 | 0.96x | -1.3% | 16801507 | 16801531 | 20 % |  |
| el/parse-500k-tokens | generated | control | 43573950.0 | 44236350.0 | 1.02x | 41169700.0 | 0.94x | -6.9% | 49503192 | 42002310 | 48 % |  |
| sql/worst-columns-100k | generated | control | 468259450.0 | 456449400.0 | 0.97x | 290360300.0 | 0.62x | -36.4% | 999698393 | 95921089 | 39 % |  |
| fix/Orders128.yield-string | generated | hand | 81639.5 | 257324.2 | 3.15x | 276363.9 | 3.39x | +7.4% | 117936 | 117936 | 22 % |  |
| web/media-type.quoted | generated | control | 254.2 | 309.3 | 1.22x | 303.0 | 1.19x | -2.0% | 816 | 816 | 9 % |  |
| el/ladder | generated | hand | 1052.3 | 1896.8 | 1.80x | 1772.6 | 1.68x | -6.6% | 1776 | 1776 | 44 % |  |
| el/ladder | immediate | hand | 1052.3 | 1223.6 | 1.16x | 1242.9 | 1.18x | +1.6% | 1784 | 1784 | 44 % |  |
| el/terms1000 | generated | hand | 108533.5 | 147499.3 | 1.36x | 153991.8 | 1.42x | +4.4% | 169104 | 169104 | 17 % |  |
| el/terms1000 | immediate | hand | 108533.5 | 118681.3 | 1.09x | 120117.3 | 1.11x | +1.2% | 177166 | 177144 | 17 % |  |
| sql/select20 | generated | hand | 6977.4 | 19232.1 | 2.76x | 19156.1 | 2.75x | -0.4% | 21448 | 21448 | 14 % |  |
| sql/refused-late | generated | hand | 2655.8 | 12594.0 | 4.74x | 12622.1 | 4.75x | +0.2% | 13552 | 13552 | 20 % |  |
