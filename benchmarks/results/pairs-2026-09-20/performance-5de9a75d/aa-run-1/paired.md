# Paired stand, 2026-09-20 23:31

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

This binary, and the libraries it holds as the control, was built from b746f489. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 242879.7 | 219604.7 | 0.90x | 224601.6 | 0.92x | +2.3% | 403288 | 403288 | 7 % |  |
| tsql/columns1000 | generated | scriptdom | 1746575.0 | 175687.5 | 0.10x | 176631.2 | 0.10x | +0.5% | 200489 | 200489 | 23 % |  |
| web/json.array10000 | generated | hand | 171886.7 | 191089.8 | 1.11x | 192697.7 | 1.12x | +0.8% | 720048 | 720048 | 40 % |  |
| sql/select20.at | generated | hand | 6719.7 | 19162.8 | 2.85x | 17578.6 | 2.62x | -8.3% | 21416 | 21416 | 2 % |  |
| sql/select20.window | generated | hand | 6756.0 | 18305.2 | 2.71x | 18061.1 | 2.67x | -1.3% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 389.1 | 388.2 | 1.00x | 386.0 | 0.99x | -0.6% | 0 | 0 | 7 % |  |
| tsql/select20.scan | generated | control | 378.2 | 380.1 | 1.00x | 382.6 | 1.01x | +0.7% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 138.8 | 140.3 | 1.01x | 138.2 | 1.00x | -1.5% | 0 | 0 | 16 % |  |
| el/ladder.bool | generated | hand | 1008.4 | 1734.3 | 1.72x | 1691.3 | 1.68x | -2.5% | 1776 | 1720 | 62 % |  |
| sql/refused-late.bool | generated | hand | 2611.5 | 12333.8 | 4.72x | 5819.2 | 2.23x | -52.8% | 13552 | 6616 | 8 % |  |
| sql/select20.bool | generated | hand | 6841.8 | 18205.8 | 2.66x | 18075.5 | 2.64x | -0.7% | 21448 | 21392 | 11 % |  |
| fix/Orders128.yield-string | generated | hand | 81077.6 | 276416.8 | 3.41x | 268756.8 | 3.31x | -2.8% | 117936 | 117936 | 20 % |  |
| web/media-type.quoted | generated | control | 255.1 | 317.8 | 1.25x | 307.9 | 1.21x | -3.1% | 816 | 816 | 14 % |  |
| el/ladder | generated | hand | 1025.2 | 1771.1 | 1.73x | 1750.6 | 1.71x | -1.2% | 1776 | 1801 | 6 % |  |
| el/ladder | immediate | hand | 1025.2 | 1205.2 | 1.18x | 1216.9 | 1.19x | +1.0% | 1784 | 1784 | 6 % |  |
| el/terms1000 | generated | hand | 114361.1 | 156670.5 | 1.37x | 156473.4 | 1.37x | -0.1% | 169107 | 169104 | 5 % |  |
| el/terms1000 | immediate | hand | 114361.1 | 127982.8 | 1.12x | 129872.6 | 1.14x | +1.5% | 177120 | 177120 | 5 % |  |
| sql/select20 | generated | hand | 7229.2 | 19538.3 | 2.70x | 19039.1 | 2.63x | -2.6% | 21448 | 21448 | 9 % |  |
| sql/refused-late | generated | hand | 2722.0 | 12635.2 | 4.64x | 12655.5 | 4.65x | +0.2% | 13552 | 13552 | 5 % |  |
