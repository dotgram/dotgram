# Paired stand, 2026-09-21 00:00

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

This binary, and the libraries it holds as the control, was built from b746f489. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5de9a75d, framework net10.0, no properties, emitted 194b78b7193dfafe). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 251200.0 | 230637.5 | 0.92x | 227806.2 | 0.91x | -1.2% | 403288 | 403288 | 88 % |  |
| tsql/columns1000 | generated | scriptdom | 2026706.2 | 183037.5 | 0.09x | 182787.5 | 0.09x | -0.1% | 200489 | 200489 | 53 % |  |
| web/json.array10000 | generated | hand | 171864.8 | 190128.1 | 1.11x | 201262.5 | 1.17x | +5.9% | 720048 | 720048 | 13 % |  |
| sql/select20.at | generated | hand | 7016.8 | 17944.5 | 2.56x | 18061.0 | 2.57x | +0.6% | 21416 | 21416 | 18 % |  |
| sql/select20.window | generated | hand | 6956.9 | 18447.2 | 2.65x | 18491.0 | 2.66x | +0.2% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 389.5 | 384.6 | 0.99x | 381.4 | 0.98x | -0.8% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 393.3 | 381.9 | 0.97x | 383.3 | 0.97x | +0.4% | 0 | 0 | 10 % |  |
| el/ladder.scan | generated | control | 139.6 | 138.6 | 0.99x | 140.2 | 1.00x | +1.2% | 0 | 0 | 16 % |  |
| el/ladder.bool | generated | hand | 991.7 | 1803.6 | 1.82x | 1721.8 | 1.74x | -4.5% | 1776 | 1720 | 15 % |  |
| sql/refused-late.bool | generated | hand | 2667.8 | 12174.6 | 4.56x | 5949.2 | 2.23x | -51.1% | 13552 | 6616 | 13 % |  |
| sql/select20.bool | generated | hand | 7220.5 | 18678.3 | 2.59x | 19239.6 | 2.66x | +3.0% | 21448 | 21392 | 11 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4892237.5 | 5044093.8 | 1.03x | 4794787.5 | 0.98x | -4.9% | 6276488 | 6276488 | 16 % |  |
| sql/refused-cliff-case-1738 | generated | control | 23540700.0 | 15952062.5 | 0.68x | 6019850.0 | 0.26x | -62.3% | 74362362 | 7842222 | 97 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2622071.9 | 2689546.9 | 1.03x | 2677121.9 | 1.02x | -0.5% | 2994166 | 2994166 | 9 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 5767043.8 | 5082450.0 | 0.88x | 3603756.2 | 0.62x | -29.1% | 12948549 | 3740086 | 92 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 10158475.0 | 10130625.0 | 1.00x | 5386137.5 | 0.53x | -46.8% | 21143715 | 5840044 | 57 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 17907737.5 | 16536125.0 | 0.92x | 10358200.0 | 0.58x | -37.4% | 94724831 | 7298327 | 148 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7857700.0 | 7702606.2 | 0.98x | 8693225.0 | 1.11x | +12.9% | 8778600 | 8778600 | 16 % |  |
| sql/refused-cliff-and-2715 | generated | control | 8776612.5 | 8270800.0 | 0.94x | 8315362.5 | 0.95x | +0.5% | 8775208 | 8775448 | 20 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 39016162.5 | 31471562.5 | 0.81x | 9129800.0 | 0.23x | -71.0% | 94857728 | 10969977 | 98 % |  |
| sql/refused-cliff-and-3393 | generated | control | 29403887.5 | 29063887.5 | 0.99x | 8878162.5 | 0.30x | -69.5% | 94854594 | 10966572 | 104 % |  |
| el/parse-200k-tokens | generated | control | 15512250.0 | 15629512.5 | 1.01x | 16172987.5 | 1.04x | +3.5% | 16801485 | 16801528 | 20 % |  |
| el/parse-500k-tokens | generated | control | 52159900.0 | 46922150.0 | 0.90x | 41729750.0 | 0.80x | -11.1% | 49503175 | 42002310 | 43 % |  |
| sql/worst-columns-100k | generated | control | 439647300.0 | 417800050.0 | 0.95x | 282409850.0 | 0.64x | -32.4% | 999698523 | 95920900 | 21 % |  |
| fix/Orders128.yield-string | generated | hand | 80223.8 | 254561.7 | 3.17x | 267831.6 | 3.34x | +5.2% | 117936 | 117936 | 4 % |  |
| web/media-type.quoted | generated | control | 252.9 | 320.6 | 1.27x | 282.1 | 1.12x | -12.0% | 816 | 816 | 8 % |  |
| el/ladder | generated | hand | 986.0 | 1781.4 | 1.81x | 1733.8 | 1.76x | -2.7% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 986.0 | 1178.1 | 1.19x | 1182.4 | 1.20x | +0.4% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 104174.4 | 144968.3 | 1.39x | 144537.6 | 1.39x | -0.3% | 169107 | 169104 | 5 % |  |
| el/terms1000 | immediate | hand | 104174.4 | 119125.9 | 1.14x | 118680.6 | 1.14x | -0.4% | 177120 | 177120 | 5 % |  |
| sql/select20 | generated | hand | 6911.9 | 18321.8 | 2.65x | 18436.4 | 2.67x | +0.6% | 21448 | 21448 | 13 % |  |
| sql/refused-late | generated | hand | 2618.9 | 12085.9 | 4.61x | 12383.4 | 4.73x | +2.5% | 13552 | 13552 | 7 % |  |
