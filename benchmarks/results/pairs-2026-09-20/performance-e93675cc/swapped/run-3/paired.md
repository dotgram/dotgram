# Paired stand, 2026-09-21 02:19

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 247935.9 | 220554.7 | 0.89x | 223250.0 | 0.90x | +1.2% | 403288 | 403288 | 11 % |  |
| tsql/columns1000 | generated | scriptdom | 1924300.0 | 173475.0 | 0.09x | 171943.8 | 0.09x | -0.9% | 200489 | 200489 | 58 % |  |
| web/json.array10000 | generated | hand | 188284.4 | 188882.0 | 1.00x | 192060.2 | 1.02x | +1.7% | 720048 | 720048 | 6 % |  |
| sql/select20.at | generated | hand | 6636.7 | 17209.0 | 2.59x | 17189.2 | 2.59x | -0.1% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6635.1 | 17768.3 | 2.68x | 17518.3 | 2.64x | -1.4% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 371.7 | 372.8 | 1.00x | 370.1 | 1.00x | -0.7% | 0 | 0 | 8 % |  |
| tsql/select20.scan | generated | control | 367.4 | 386.7 | 1.05x | 368.9 | 1.00x | -4.6% | 0 | 0 | 7 % |  |
| el/ladder.scan | generated | control | 135.1 | 134.1 | 0.99x | 133.9 | 0.99x | -0.1% | 0 | 0 | 27 % |  |
| el/ladder.bool | generated | hand | 954.8 | 1695.0 | 1.78x | 1697.8 | 1.78x | +0.2% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2545.1 | 11811.7 | 4.64x | 5712.9 | 2.24x | -51.6% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6739.5 | 17565.1 | 2.61x | 17645.6 | 2.62x | +0.5% | 21448 | 21392 | 13 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4382743.8 | 4356018.8 | 0.99x | 4401618.8 | 1.00x | +1.0% | 6276488 | 6276488 | 3 % |  |
| sql/refused-cliff-case-1738 | generated | control | 20288787.5 | 5365725.0 | 0.26x | 11705112.5 | 0.58x | +118.1% | 7842176 | 74362470 | 139 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2422200.0 | 2396750.0 | 0.99x | 2624600.0 | 1.08x | +9.5% | 2994056 | 2994166 | 57 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 3722400.0 | 3242412.5 | 0.87x | 4066781.2 | 1.09x | +25.4% | 3740019 | 12948506 | 109 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 5616150.0 | 5182825.0 | 0.92x | 6491525.0 | 1.16x | +25.3% | 5840044 | 21143715 | 226 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 19064475.0 | 6126100.0 | 0.32x | 25178487.5 | 1.32x | +311.0% | 7298259 | 94725052 | 177 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6641175.0 | 6692787.5 | 1.01x | 6719837.5 | 1.01x | +0.4% | 8778619 | 8778600 | 18 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6393418.8 | 6440856.2 | 1.01x | 6586575.0 | 1.03x | +2.3% | 8775448 | 8775448 | 8 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 33181100.0 | 8422887.5 | 0.25x | 34004137.5 | 1.02x | +303.7% | 10969896 | 94857969 | 81 % |  |
| sql/refused-cliff-and-3393 | generated | control | 15897600.0 | 8426450.0 | 0.53x | 16294900.0 | 1.02x | +93.4% | 10966745 | 94854689 | 188 % |  |
| el/parse-200k-tokens | generated | control | 15319500.0 | 15220450.0 | 0.99x | 14943750.0 | 0.98x | -1.8% | 16801614 | 16801485 | 39 % |  |
| el/parse-500k-tokens | generated | control | 39921400.0 | 44258450.0 | 1.11x | 45164375.0 | 1.13x | +2.0% | 42002327 | 49503178 | 45 % |  |
| sql/worst-columns-100k | generated | control | 443484200.0 | 274670350.0 | 0.62x | 436617600.0 | 0.98x | +59.0% | 95920865 | 999698470 | 17 % |  |
| fix/Orders128.yield-string | generated | hand | 78792.6 | 262380.5 | 3.33x | 267495.7 | 3.39x | +1.9% | 117936 | 117936 | 4 % |  |
| web/media-type.quoted | generated | control | 235.1 | 277.1 | 1.18x | 303.8 | 1.29x | +9.7% | 816 | 816 | 5 % |  |
| el/ladder | generated | hand | 953.4 | 1693.4 | 1.78x | 1694.8 | 1.78x | +0.1% | 1776 | 1776 | 17 % |  |
| el/ladder | immediate | hand | 953.4 | 1156.2 | 1.21x | 1147.1 | 1.20x | -0.8% | 1784 | 1784 | 17 % |  |
| el/terms1000 | generated | hand | 104336.0 | 142767.4 | 1.37x | 140797.7 | 1.35x | -1.4% | 169104 | 169104 | 2 % |  |
| el/terms1000 | immediate | hand | 104336.0 | 120901.5 | 1.16x | 113738.5 | 1.09x | -5.9% | 177120 | 177166 | 2 % |  |
| sql/select20 | generated | hand | 6774.3 | 19094.3 | 2.82x | 17561.9 | 2.59x | -8.0% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2556.2 | 12645.0 | 4.95x | 11637.7 | 4.55x | -8.0% | 13552 | 13552 | 5 % |  |
