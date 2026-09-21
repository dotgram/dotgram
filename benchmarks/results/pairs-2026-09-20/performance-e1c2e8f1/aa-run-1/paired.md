# Paired stand, 2026-09-21 00:45

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 257829.7 | 217875.0 | 0.85x | 226946.9 | 0.88x | +4.2% | 403288 | 403288 | 719 % |  |
| tsql/columns1000 | generated | scriptdom | 1854787.5 | 173381.2 | 0.09x | 171931.2 | 0.09x | -0.8% | 200489 | 200489 | 58 % |  |
| web/json.array10000 | generated | hand | 170052.3 | 198318.8 | 1.17x | 205400.0 | 1.21x | +3.6% | 720048 | 720048 | 15 % |  |
| sql/select20.at | generated | hand | 6777.3 | 18068.5 | 2.67x | 17796.4 | 2.63x | -1.5% | 21416 | 21416 | 7 % |  |
| sql/select20.window | generated | hand | 6872.9 | 18490.1 | 2.69x | 18170.8 | 2.64x | -1.7% | 21416 | 21416 | 10 % |  |
| sql/select20.scan | generated | control | 389.3 | 386.8 | 0.99x | 388.4 | 1.00x | +0.4% | 0 | 0 | 10 % |  |
| tsql/select20.scan | generated | control | 380.8 | 391.6 | 1.03x | 386.2 | 1.01x | -1.4% | 0 | 0 | 12 % |  |
| el/ladder.scan | generated | control | 140.0 | 147.7 | 1.05x | 140.4 | 1.00x | -4.9% | 0 | 0 | 12 % |  |
| el/ladder.bool | generated | hand | 1011.9 | 1812.9 | 1.79x | 1721.0 | 1.70x | -5.1% | 1776 | 1720 | 50 % |  |
| sql/refused-late.bool | generated | hand | 2641.1 | 12364.5 | 4.68x | 5817.7 | 2.20x | -52.9% | 13552 | 6616 | 28 % |  |
| sql/select20.bool | generated | hand | 6904.9 | 18917.8 | 2.74x | 18584.3 | 2.69x | -1.8% | 21448 | 21392 | 16 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4900037.5 | 5056825.0 | 1.03x | 4848137.5 | 0.99x | -4.1% | 6276555 | 6276641 | 46 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2579437.5 | 2570721.9 | 1.00x | 2535450.0 | 0.98x | -1.4% | 2994166 | 2994123 | 22 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6485150.0 | 6453250.0 | 1.00x | 6346668.8 | 0.98x | -1.7% | 8778556 | 8778600 | 47 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6853575.0 | 6999237.5 | 1.02x | 6935487.5 | 1.01x | -0.9% | 8775448 | 8775448 | 40 % |  |
| fix/Orders128.yield-string | generated | hand | 87989.9 | 303007.2 | 3.44x | 260907.1 | 2.97x | -13.9% | 117936 | 117936 | 30 % |  |
| web/media-type.quoted | generated | control | 266.4 | 322.3 | 1.21x | 337.8 | 1.27x | +4.8% | 816 | 816 | 38 % |  |
| el/ladder | generated | hand | 1038.5 | 1812.5 | 1.75x | 1796.9 | 1.73x | -0.9% | 1776 | 1776 | 20 % |  |
| el/ladder | immediate | hand | 1038.5 | 1206.4 | 1.16x | 1240.4 | 1.19x | +2.8% | 1784 | 1784 | 20 % |  |
| el/terms1000 | generated | hand | 113435.8 | 162297.5 | 1.43x | 153278.1 | 1.35x | -5.6% | 169107 | 169104 | 46 % |  |
| el/terms1000 | immediate | hand | 113435.8 | 125983.4 | 1.11x | 125284.1 | 1.10x | -0.6% | 177120 | 177120 | 46 % |  |
| sql/select20 | generated | hand | 7023.6 | 19023.0 | 2.71x | 18443.9 | 2.63x | -3.0% | 21448 | 21448 | 78 % |  |
| sql/refused-late | generated | hand | 2725.6 | 12707.3 | 4.66x | 12606.1 | 4.63x | -0.8% | 13552 | 13552 | 19 % |  |
