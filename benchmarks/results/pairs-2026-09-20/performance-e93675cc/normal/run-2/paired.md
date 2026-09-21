# Paired stand, 2026-09-21 01:33

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.8 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 255109.4 | 215551.6 | 0.84x | 212390.6 | 0.83x | -1.5% | 403288 | 403288 | 12 % |  |
| tsql/columns1000 | generated | scriptdom | 1790706.2 | 179437.5 | 0.10x | 169556.2 | 0.09x | -5.5% | 200489 | 200489 | 62 % |  |
| web/json.array10000 | generated | hand | 164236.7 | 196267.2 | 1.20x | 197032.8 | 1.20x | +0.4% | 720048 | 720048 | 15 % |  |
| sql/select20.at | generated | hand | 6830.6 | 17896.2 | 2.62x | 17636.4 | 2.58x | -1.5% | 21416 | 21440 | 27 % |  |
| sql/select20.window | generated | hand | 6698.7 | 17962.5 | 2.68x | 17938.2 | 2.68x | -0.1% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 388.1 | 371.5 | 0.96x | 371.5 | 0.96x | 0.0% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 374.2 | 388.6 | 1.04x | 370.4 | 0.99x | -4.7% | 0 | 0 | 14 % |  |
| el/ladder.scan | generated | control | 142.7 | 135.5 | 0.95x | 135.5 | 0.95x | 0.0% | 0 | 0 | 17 % |  |
| el/ladder.bool | generated | hand | 978.2 | 1678.4 | 1.72x | 1708.3 | 1.75x | +1.8% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2530.0 | 12019.1 | 4.75x | 5716.4 | 2.26x | -52.4% | 13552 | 6616 | 16 % |  |
| sql/select20.bool | generated | hand | 6714.4 | 18455.0 | 2.75x | 17997.8 | 2.68x | -2.5% | 21448 | 21392 | 5 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4485193.8 | 4575100.0 | 1.02x | 4454700.0 | 0.99x | -2.6% | 6276488 | 6276684 | 7 % |  |
| sql/refused-cliff-case-1738 | generated | control | 10476750.0 | 8404250.0 | 0.80x | 6047825.0 | 0.58x | -28.0% | 74362384 | 7842262 | 430 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2514450.0 | 2664875.0 | 1.06x | 2477959.4 | 0.99x | -7.0% | 2994142 | 2994166 | 4 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 5034168.8 | 4526106.2 | 0.90x | 3218618.8 | 0.64x | -28.9% | 12948531 | 3740107 | 109 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 9493612.5 | 10314093.8 | 1.09x | 4944131.2 | 0.52x | -52.1% | 21143690 | 5840148 | 34 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 11313650.0 | 10680275.0 | 0.94x | 6885025.0 | 0.61x | -35.5% | 94724913 | 7298283 | 235 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7684418.8 | 7686837.5 | 1.00x | 7178606.2 | 0.93x | -6.6% | 8778600 | 8778619 | 12 % |  |
| sql/refused-cliff-and-2715 | generated | control | 7037400.0 | 7391175.0 | 1.05x | 7835468.8 | 1.11x | +6.0% | 8775448 | 8775404 | 11 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 13017825.0 | 11291100.0 | 0.87x | 8166800.0 | 0.63x | -27.7% | 94857772 | 10969723 | 118 % |  |
| sql/refused-cliff-and-3393 | generated | control | 14270325.0 | 12905475.0 | 0.90x | 7828000.0 | 0.55x | -39.3% | 94854654 | 10966547 | 234 % |  |
| el/parse-200k-tokens | generated | control | 15002775.0 | 16551775.0 | 1.10x | 15267187.5 | 1.02x | -7.8% | 16801614 | 16801528 | 18 % |  |
| el/parse-500k-tokens | generated | control | 41456550.0 | 41539950.0 | 1.00x | 43270550.0 | 1.04x | +4.2% | 49503175 | 42002327 | 36 % |  |
| sql/worst-columns-100k | generated | control | 441747100.0 | 423675550.0 | 0.96x | 286151650.0 | 0.65x | -32.5% | 999698520 | 95920990 | 18 % |  |
| fix/Orders128.yield-string | generated | hand | 80293.9 | 250093.6 | 3.11x | 256130.5 | 3.19x | +2.4% | 117936 | 117936 | 9 % |  |
| web/media-type.quoted | generated | control | 247.6 | 289.5 | 1.17x | 282.9 | 1.14x | -2.3% | 816 | 816 | 16 % |  |
| el/ladder | generated | hand | 979.8 | 1702.6 | 1.74x | 1750.6 | 1.79x | +2.8% | 1776 | 1776 | 5 % |  |
| el/ladder | immediate | hand | 979.8 | 1144.3 | 1.17x | 1149.4 | 1.17x | +0.4% | 1784 | 1784 | 5 % |  |
| el/terms1000 | generated | hand | 103563.5 | 144931.7 | 1.40x | 143117.2 | 1.38x | -1.3% | 169104 | 169104 | 12 % |  |
| el/terms1000 | immediate | hand | 103563.5 | 116446.3 | 1.12x | 116774.9 | 1.13x | +0.3% | 177123 | 177120 | 12 % |  |
| sql/select20 | generated | hand | 6709.5 | 18528.3 | 2.76x | 17885.4 | 2.67x | -3.5% | 21448 | 21448 | 12 % |  |
| sql/refused-late | generated | hand | 2528.2 | 11944.1 | 4.72x | 11721.3 | 4.64x | -1.9% | 13552 | 13552 | 10 % |  |
