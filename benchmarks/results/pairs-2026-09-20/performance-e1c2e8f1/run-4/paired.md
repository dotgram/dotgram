# Paired stand, 2026-09-21 01:07

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e1c2e8f1, framework net10.0, no properties, emitted edc6933b61ef4954). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 244287.5 | 215025.0 | 0.88x | 218281.2 | 0.89x | +1.5% | 403288 | 403288 | 42 % |  |
| tsql/columns1000 | generated | scriptdom | 1711512.5 | 175556.2 | 0.10x | 171681.2 | 0.10x | -2.2% | 200489 | 200489 | 16 % |  |
| web/json.array10000 | generated | hand | 188035.9 | 185637.5 | 0.99x | 186175.8 | 0.99x | +0.3% | 720048 | 720048 | 16 % |  |
| sql/select20.at | generated | hand | 7000.8 | 17570.6 | 2.51x | 17955.0 | 2.56x | +2.2% | 21416 | 21416 | 21 % |  |
| sql/select20.window | generated | hand | 6845.8 | 17716.2 | 2.59x | 19108.5 | 2.79x | +7.9% | 21416 | 21416 | 12 % |  |
| sql/select20.scan | generated | control | 376.6 | 376.0 | 1.00x | 373.9 | 0.99x | -0.5% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 375.4 | 373.3 | 0.99x | 372.9 | 0.99x | -0.1% | 0 | 0 | 8 % |  |
| el/ladder.scan | generated | control | 137.9 | 136.9 | 0.99x | 137.4 | 1.00x | +0.4% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 980.0 | 1700.1 | 1.73x | 1756.9 | 1.79x | +3.3% | 1776 | 1720 | 44 % |  |
| sql/refused-late.bool | generated | hand | 2605.3 | 12136.5 | 4.66x | 5939.2 | 2.28x | -51.1% | 13552 | 6616 | 10 % |  |
| sql/select20.bool | generated | hand | 6877.2 | 17690.7 | 2.57x | 18688.0 | 2.72x | +5.6% | 21448 | 21392 | 8 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4530225.0 | 4507868.8 | 1.00x | 4517256.2 | 1.00x | +0.2% | 6276488 | 6276488 | 7 % |  |
| sql/refused-cliff-case-1738 | generated | control | 14965837.5 | 14316962.5 | 0.96x | 5440850.0 | 0.36x | -62.0% | 74362527 | 7842219 | 152 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2628606.2 | 2646784.4 | 1.01x | 2736343.8 | 1.04x | +3.4% | 2994166 | 2994056 | 5 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 4261225.0 | 5280218.8 | 1.24x | 4518393.8 | 1.06x | -14.4% | 12948549 | 3740106 | 104 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 9207093.8 | 9383875.0 | 1.02x | 5376525.0 | 0.58x | -42.7% | 21143731 | 5840088 | 41 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 22127162.5 | 23125625.0 | 1.05x | 6823037.5 | 0.31x | -70.5% | 94725047 | 7298259 | 107 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7345181.2 | 7330831.2 | 1.00x | 7841562.5 | 1.07x | +7.0% | 8778600 | 8778600 | 13 % |  |
| sql/refused-cliff-and-2715 | generated | control | 7446943.8 | 7667418.8 | 1.03x | 7768975.0 | 1.04x | +1.3% | 8775467 | 8775448 | 17 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 13876750.0 | 15101425.0 | 1.09x | 8921050.0 | 0.64x | -40.9% | 94857729 | 10969958 | 54 % |  |
| sql/refused-cliff-and-3393 | generated | control | 28343275.0 | 29482800.0 | 1.04x | 10132825.0 | 0.36x | -65.6% | 94854593 | 10966528 | 157 % |  |
| el/parse-200k-tokens | generated | control | 14937937.5 | 15416300.0 | 1.03x | 15954675.0 | 1.07x | +3.5% | 16801636 | 16801593 | 67 % |  |
| el/parse-500k-tokens | generated | control | 40623600.0 | 39461750.0 | 0.97x | 38593200.0 | 0.95x | -2.2% | 49503175 | 42002310 | 37 % |  |
| sql/worst-columns-100k | generated | control | 429243150.0 | 429797650.0 | 1.00x | 276706350.0 | 0.64x | -35.6% | 999698406 | 95921044 | 21 % |  |
| fix/Orders128.yield-string | generated | hand | 77665.3 | 265678.1 | 3.42x | 282333.2 | 3.64x | +6.3% | 117936 | 117936 | 14 % |  |
| web/media-type.quoted | generated | control | 244.0 | 304.1 | 1.25x | 294.8 | 1.21x | -3.1% | 816 | 816 | 8 % |  |
| el/ladder | generated | hand | 975.4 | 1696.3 | 1.74x | 1751.2 | 1.80x | +3.2% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 975.4 | 1142.9 | 1.17x | 1146.3 | 1.18x | +0.3% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 103680.2 | 141543.3 | 1.37x | 143918.1 | 1.39x | +1.7% | 169104 | 169104 | 12 % |  |
| el/terms1000 | immediate | hand | 103680.2 | 117202.1 | 1.13x | 115391.5 | 1.11x | -1.5% | 177120 | 177123 | 12 % |  |
| sql/select20 | generated | hand | 6835.5 | 17603.0 | 2.58x | 18380.8 | 2.69x | +4.4% | 21448 | 21448 | 9 % |  |
| sql/refused-late | generated | hand | 2568.2 | 11717.8 | 4.56x | 12217.2 | 4.76x | +4.3% | 13552 | 13552 | 14 % |  |
