# Paired stand, 2026-09-21 01:15

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.9 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e1c2e8f1, framework net10.0, no properties, emitted edc6933b61ef4954). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 251112.5 | 224000.0 | 0.89x | 212668.8 | 0.85x | -5.1% | 403288 | 403288 | 9 % |  |
| tsql/columns1000 | generated | scriptdom | 1686381.2 | 170787.5 | 0.10x | 169256.2 | 0.10x | -0.9% | 200489 | 200489 | 4 % |  |
| web/json.array10000 | generated | hand | 190244.5 | 185581.2 | 0.98x | 184416.4 | 0.97x | -0.6% | 720048 | 720048 | 11 % |  |
| sql/select20.at | generated | hand | 6830.9 | 17389.3 | 2.55x | 17671.2 | 2.59x | +1.6% | 21416 | 21416 | 7 % |  |
| sql/select20.window | generated | hand | 6778.4 | 17662.2 | 2.61x | 18129.8 | 2.67x | +2.6% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 393.1 | 411.2 | 1.05x | 377.0 | 0.96x | -8.3% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 376.5 | 380.2 | 1.01x | 379.4 | 1.01x | -0.2% | 0 | 0 | 9 % |  |
| el/ladder.scan | generated | control | 139.9 | 138.9 | 0.99x | 143.2 | 1.02x | +3.1% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 1006.7 | 1720.3 | 1.71x | 1702.0 | 1.69x | -1.1% | 1776 | 1720 | 7 % |  |
| sql/refused-late.bool | generated | hand | 2594.7 | 11793.1 | 4.55x | 5885.2 | 2.27x | -50.1% | 13552 | 6616 | 14 % |  |
| sql/select20.bool | generated | hand | 6786.1 | 18108.0 | 2.67x | 18149.9 | 2.67x | +0.2% | 21448 | 21392 | 19 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4509275.0 | 4441893.8 | 0.99x | 4429037.5 | 0.98x | -0.3% | 6276684 | 6276488 | 17 % |  |
| sql/refused-cliff-case-1738 | generated | control | 11067950.0 | 9067125.0 | 0.82x | 6080875.0 | 0.55x | -32.9% | 74362722 | 7842195 | 387 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2627343.8 | 2690209.4 | 1.02x | 2593212.5 | 0.99x | -3.6% | 2994123 | 2994056 | 11 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 4428931.2 | 4032768.8 | 0.91x | 3280137.5 | 0.74x | -18.7% | 12948549 | 3740107 | 91 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 9734981.2 | 7881793.8 | 0.81x | 5006593.8 | 0.51x | -36.5% | 21143833 | 5840086 | 44 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 18251650.0 | 18102637.5 | 0.99x | 6385962.5 | 0.35x | -64.7% | 94724968 | 7298326 | 55 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7965468.8 | 7876762.5 | 0.99x | 7364706.2 | 0.92x | -6.5% | 8778600 | 8778600 | 13 % |  |
| sql/refused-cliff-and-2715 | generated | control | 7181537.5 | 7456693.8 | 1.04x | 8062056.2 | 1.12x | +8.1% | 8775448 | 8775275 | 19 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 26619250.0 | 32055425.0 | 1.20x | 8309112.5 | 0.31x | -74.1% | 94857940 | 10969699 | 91 % |  |
| sql/refused-cliff-and-3393 | generated | control | 30063350.0 | 30781000.0 | 1.02x | 8889287.5 | 0.30x | -71.1% | 94854748 | 10966528 | 99 % |  |
| el/parse-200k-tokens | generated | control | 15314037.5 | 16276625.0 | 1.06x | 15867575.0 | 1.04x | -2.5% | 16801507 | 16801574 | 14 % |  |
| el/parse-500k-tokens | generated | control | 40742250.0 | 39545950.0 | 0.97x | 38625750.0 | 0.95x | -2.3% | 49503175 | 42002327 | 39 % |  |
| sql/worst-columns-100k | generated | control | 429804300.0 | 419085100.0 | 0.98x | 292113300.0 | 0.68x | -30.3% | 999698385 | 95920898 | 15 % |  |
| fix/Orders128.yield-string | generated | hand | 79919.3 | 249660.7 | 3.12x | 246386.1 | 3.08x | -1.3% | 117936 | 117936 | 14 % |  |
| web/media-type.quoted | generated | control | 242.9 | 312.1 | 1.28x | 282.2 | 1.16x | -9.6% | 816 | 816 | 7 % |  |
| el/ladder | generated | hand | 1001.9 | 1703.2 | 1.70x | 1717.8 | 1.71x | +0.9% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 1001.9 | 1177.0 | 1.17x | 1154.8 | 1.15x | -1.9% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 103084.3 | 142914.0 | 1.39x | 140462.7 | 1.36x | -1.7% | 169107 | 169104 | 8 % |  |
| el/terms1000 | immediate | hand | 103084.3 | 115565.7 | 1.12x | 116960.0 | 1.13x | +1.2% | 177120 | 177120 | 8 % |  |
| sql/select20 | generated | hand | 6678.5 | 17698.4 | 2.65x | 18007.5 | 2.70x | +1.7% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2565.8 | 11719.4 | 4.57x | 11878.7 | 4.63x | +1.4% | 13552 | 13552 | 12 % |  |
