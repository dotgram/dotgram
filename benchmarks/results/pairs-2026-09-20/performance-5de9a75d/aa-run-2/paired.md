# Paired stand, 2026-09-20 23:38

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

This binary, and the libraries it holds as the control, was built from b746f489. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 254665.6 | 230356.2 | 0.90x | 217295.3 | 0.85x | -5.7% | 403288 | 403288 | 7 % |  |
| tsql/columns1000 | generated | scriptdom | 1885625.0 | 244025.0 | 0.13x | 186831.2 | 0.10x | -23.4% | 200489 | 200489 | 107 % |  |
| web/json.array10000 | generated | hand | 222561.7 | 239035.2 | 1.07x | 227673.4 | 1.02x | -4.8% | 720048 | 720048 | 160 % |  |
| sql/select20.at | generated | hand | 8618.5 | 24118.1 | 2.80x | 24237.7 | 2.81x | +0.5% | 21416 | 21416 | 51 % |  |
| sql/select20.window | generated | hand | 7242.3 | 20556.6 | 2.84x | 20308.1 | 2.80x | -1.2% | 21416 | 21416 | 18 % |  |
| sql/select20.scan | generated | control | 390.3 | 387.9 | 0.99x | 382.7 | 0.98x | -1.3% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 398.4 | 400.2 | 1.00x | 385.5 | 0.97x | -3.7% | 0 | 0 | 12 % |  |
| el/ladder.scan | generated | control | 150.5 | 143.5 | 0.95x | 148.4 | 0.99x | +3.5% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 1006.6 | 1721.4 | 1.71x | 1686.2 | 1.68x | -2.0% | 1776 | 1720 | 22 % |  |
| sql/refused-late.bool | generated | hand | 2581.0 | 12781.5 | 4.95x | 5915.3 | 2.29x | -53.7% | 13552 | 6616 | 23 % |  |
| sql/select20.bool | generated | hand | 6961.5 | 18813.7 | 2.70x | 18559.1 | 2.67x | -1.4% | 21448 | 21392 | 34 % |  |
| fix/Orders128.yield-string | generated | hand | 81928.0 | 271925.7 | 3.32x | 271631.8 | 3.32x | -0.1% | 117936 | 117936 | 11 % |  |
| web/media-type.quoted | generated | control | 256.9 | 297.7 | 1.16x | 313.3 | 1.22x | +5.2% | 816 | 816 | 4 % |  |
| el/ladder | generated | hand | 979.7 | 1717.4 | 1.75x | 1739.5 | 1.78x | +1.3% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 979.7 | 1155.0 | 1.18x | 1158.6 | 1.18x | +0.3% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 113112.6 | 152518.4 | 1.35x | 153352.1 | 1.36x | +0.5% | 169104 | 169104 | 7 % |  |
| el/terms1000 | immediate | hand | 113112.6 | 132497.9 | 1.17x | 125344.7 | 1.11x | -5.4% | 177123 | 177120 | 7 % |  |
| sql/select20 | generated | hand | 6832.9 | 18496.7 | 2.71x | 18385.5 | 2.69x | -0.6% | 21448 | 21448 | 24 % |  |
| sql/refused-late | generated | hand | 2594.7 | 12765.6 | 4.92x | 12502.5 | 4.82x | -2.1% | 13552 | 13552 | 12 % |  |
