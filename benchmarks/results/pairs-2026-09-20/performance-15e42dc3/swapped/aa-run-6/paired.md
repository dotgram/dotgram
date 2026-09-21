# Paired stand, 2026-09-21 07:22

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 173958.6 | 183974.2 | 1.06x | 184959.4 | 1.06x | +0.5% | 720048 | 720048 | 50 % |  |
| sql/select20.at | generated | hand | 6628.3 | 17513.0 | 2.64x | 17193.3 | 2.59x | -1.8% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6678.5 | 17945.2 | 2.69x | 17674.6 | 2.65x | -1.5% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 374.2 | 365.1 | 0.98x | 370.4 | 0.99x | +1.5% | 0 | 0 | 1 % |  |
| tsql/select20.scan | generated | control | 367.6 | 365.8 | 1.00x | 366.4 | 1.00x | +0.2% | 0 | 0 | 12 % |  |
| el/ladder.scan | generated | control | 135.7 | 133.0 | 0.98x | 133.2 | 0.98x | +0.1% | 0 | 0 | 13 % |  |
| el/ladder.bool | generated | hand | 984.6 | 1675.0 | 1.70x | 1639.1 | 1.66x | -2.1% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2519.0 | 11890.7 | 4.72x | 5807.0 | 2.31x | -51.2% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6711.9 | 17994.0 | 2.68x | 17737.8 | 2.64x | -1.4% | 21448 | 21392 | 32 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4317662.5 | 4442675.0 | 1.03x | 4319537.5 | 1.00x | -2.8% | 6276688 | 6276688 | 16 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6249000.0 | 6394950.0 | 1.02x | 6419575.0 | 1.03x | +0.4% | 8778600 | 8778600 | 5 % |  |
| el/ladder | generated | hand | 979.5 | 1679.0 | 1.71x | 1672.5 | 1.71x | -0.4% | 1776 | 1776 | 9 % |  |
| el/ladder | immediate | hand | 979.5 | 1223.3 | 1.25x | 1138.2 | 1.16x | -7.0% | 1784 | 1784 | 9 % |  |
| el/terms1000 | generated | hand | 107629.3 | 146889.6 | 1.36x | 146824.8 | 1.36x | 0.0% | 169104 | 169104 | 5 % |  |
| el/terms1000 | immediate | hand | 107629.3 | 142913.2 | 1.33x | 120168.4 | 1.12x | -15.9% | 177120 | 177123 | 5 % |  |
| sql/select20 | generated | hand | 6676.5 | 17948.1 | 2.69x | 17576.1 | 2.63x | -2.1% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2522.5 | 11907.7 | 4.72x | 11966.1 | 4.74x | +0.5% | 13552 | 13552 | 9 % |  |
