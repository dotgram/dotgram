# Paired stand, 2026-09-20 23:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.2 ns.

This binary, and the libraries it holds as the control, was built from b746f489. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 240762.5 | 229765.6 | 0.95x | 221100.0 | 0.92x | -3.8% | 403288 | 403288 | 16 % |  |
| tsql/columns1000 | generated | scriptdom | 2036387.5 | 183612.5 | 0.09x | 201568.8 | 0.10x | +9.8% | 200489 | 200489 | 54 % |  |
| web/json.array10000 | generated | hand | 180321.1 | 192432.0 | 1.07x | 207005.5 | 1.15x | +7.6% | 720048 | 720048 | 43 % |  |
| sql/select20.at | generated | hand | 7400.3 | 19122.9 | 2.58x | 18968.8 | 2.56x | -0.8% | 21416 | 21416 | 17 % |  |
| sql/select20.window | generated | hand | 7098.6 | 19362.3 | 2.73x | 18959.0 | 2.67x | -2.1% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 424.9 | 437.4 | 1.03x | 446.5 | 1.05x | +2.1% | 0 | 0 | 21 % |  |
| tsql/select20.scan | generated | control | 446.0 | 460.1 | 1.03x | 466.6 | 1.05x | +1.4% | 0 | 0 | 38 % |  |
| el/ladder.scan | generated | control | 154.0 | 152.1 | 0.99x | 152.3 | 0.99x | +0.1% | 0 | 0 | 28 % |  |
| el/ladder.bool | generated | hand | 1152.1 | 1932.1 | 1.68x | 1830.6 | 1.59x | -5.3% | 1776 | 1720 | 43 % |  |
| sql/refused-late.bool | generated | hand | 2859.5 | 13227.5 | 4.63x | 6261.1 | 2.19x | -52.7% | 13552 | 6616 | 37 % |  |
| sql/select20.bool | generated | hand | 6984.0 | 19290.9 | 2.76x | 19680.8 | 2.82x | +2.0% | 21448 | 21392 | 6 % |  |
| fix/Orders128.yield-string | generated | hand | 81477.1 | 266004.2 | 3.26x | 278496.2 | 3.42x | +4.7% | 117936 | 117936 | 15 % |  |
| web/media-type.quoted | generated | control | 254.2 | 302.6 | 1.19x | 304.4 | 1.20x | +0.6% | 816 | 816 | 9 % |  |
| el/ladder | generated | hand | 1042.6 | 1740.1 | 1.67x | 1743.0 | 1.67x | +0.2% | 1776 | 1776 | 10 % |  |
| el/ladder | immediate | hand | 1042.6 | 1222.9 | 1.17x | 1243.1 | 1.19x | +1.7% | 1784 | 1784 | 10 % |  |
| el/terms1000 | generated | hand | 117807.5 | 155891.9 | 1.32x | 157553.5 | 1.34x | +1.1% | 169104 | 169104 | 10 % |  |
| el/terms1000 | immediate | hand | 117807.5 | 129570.8 | 1.10x | 130806.0 | 1.11x | +1.0% | 177120 | 177120 | 10 % |  |
| sql/select20 | generated | hand | 6899.4 | 18998.9 | 2.75x | 18783.6 | 2.72x | -1.1% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2590.7 | 12756.8 | 4.92x | 12662.5 | 4.89x | -0.7% | 13552 | 13552 | 2 % |  |
