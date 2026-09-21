# Paired stand, 2026-09-21 00:43

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.1 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e1c2e8f1, framework net10.0, no properties, emitted edc6933b61ef4954). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 248675.0 | 225312.5 | 0.91x | 225312.5 | 0.91x | 0.0% | 403288 | 403288 | 21 % |  |
| tsql/columns1000 | generated | scriptdom | 2144850.0 | 221462.5 | 0.10x | 193562.5 | 0.09x | -12.6% | 200489 | 200489 | 51 % |  |
| web/json.array10000 | generated | hand | 197614.8 | 190071.1 | 0.96x | 190055.5 | 0.96x | 0.0% | 720048 | 720048 | 20 % |  |
| sql/select20.at | generated | hand | 7040.6 | 17811.4 | 2.53x | 18103.0 | 2.57x | +1.6% | 21416 | 21416 | 14 % |  |
| sql/select20.window | generated | hand | 7288.8 | 19003.4 | 2.61x | 19397.5 | 2.66x | +2.1% | 21416 | 21416 | 20 % |  |
| sql/select20.scan | generated | control | 399.2 | 395.0 | 0.99x | 389.0 | 0.97x | -1.5% | 0 | 0 | 34 % |  |
| tsql/select20.scan | generated | control | 400.4 | 384.4 | 0.96x | 398.1 | 0.99x | +3.6% | 0 | 0 | 21 % |  |
| el/ladder.scan | generated | control | 146.4 | 145.5 | 0.99x | 147.3 | 1.01x | +1.3% | 0 | 0 | 16 % |  |
| el/ladder.bool | generated | hand | 1045.6 | 1795.3 | 1.72x | 1886.7 | 1.80x | +5.1% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2787.1 | 12646.4 | 4.54x | 6300.7 | 2.26x | -50.2% | 13552 | 6616 | 24 % |  |
| sql/select20.bool | generated | hand | 7077.0 | 18553.0 | 2.62x | 18822.7 | 2.66x | +1.5% | 21448 | 21392 | 29 % |  |
| sql/refused-cliff-case-1391 | generated | control | 5008437.5 | 4981637.5 | 0.99x | 4979481.2 | 0.99x | 0.0% | 6276660 | 6276488 | 39 % |  |
| sql/refused-cliff-case-1738 | generated | control | 17333412.5 | 23107537.5 | 1.33x | 5965562.5 | 0.34x | -74.2% | 74362760 | 7842219 | 145 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2738787.5 | 2784059.4 | 1.02x | 2760562.5 | 1.01x | -0.8% | 2994123 | 2994056 | 8 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 3794081.2 | 4819237.5 | 1.27x | 3291275.0 | 0.87x | -31.7% | 12948549 | 3740107 | 84 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 8255681.2 | 8268168.8 | 1.00x | 5299225.0 | 0.64x | -35.9% | 21143712 | 5840130 | 49 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 15983850.0 | 11873650.0 | 0.74x | 7339975.0 | 0.46x | -38.2% | 94725087 | 7298240 | 235 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 8542156.2 | 8586500.0 | 1.01x | 8125825.0 | 0.95x | -5.4% | 8778600 | 8778619 | 34 % |  |
| sql/refused-cliff-and-2715 | generated | control | 7529737.5 | 7583237.5 | 1.01x | 7979687.5 | 1.06x | +5.2% | 8775208 | 8775208 | 28 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 17755275.0 | 14203900.0 | 0.80x | 8804650.0 | 0.50x | -38.0% | 94858013 | 10969699 | 269 % |  |
| sql/refused-cliff-and-3393 | generated | control | 32105250.0 | 35333887.5 | 1.10x | 9726662.5 | 0.30x | -72.5% | 94854770 | 10966528 | 115 % |  |
| el/parse-200k-tokens | generated | control | 16280650.0 | 16720150.0 | 1.03x | 16718725.0 | 1.03x | 0.0% | 16801507 | 16801617 | 30 % |  |
| el/parse-500k-tokens | generated | control | 43930350.0 | 45650250.0 | 1.04x | 41327350.0 | 0.94x | -9.5% | 49503178 | 42002327 | 58 % |  |
| sql/worst-columns-100k | generated | control | 481663150.0 | 467349250.0 | 0.97x | 288402800.0 | 0.60x | -38.3% | 999698739 | 95920972 | 27 % |  |
| fix/Orders128.yield-string | generated | hand | 82916.6 | 267162.9 | 3.22x | 273501.0 | 3.30x | +2.4% | 117936 | 117936 | 21 % |  |
| web/media-type.quoted | generated | control | 247.6 | 298.5 | 1.21x | 292.2 | 1.18x | -2.1% | 816 | 816 | 13 % |  |
| el/ladder | generated | hand | 1072.1 | 1917.2 | 1.79x | 1897.7 | 1.77x | -1.0% | 1776 | 1776 | 26 % |  |
| el/ladder | immediate | hand | 1072.1 | 1274.8 | 1.19x | 1261.4 | 1.18x | -1.0% | 1784 | 1784 | 26 % |  |
| el/terms1000 | generated | hand | 110032.6 | 151550.6 | 1.38x | 149703.5 | 1.36x | -1.2% | 169104 | 169104 | 64 % |  |
| el/terms1000 | immediate | hand | 110032.6 | 126682.6 | 1.15x | 128143.2 | 1.16x | +1.2% | 177120 | 177123 | 64 % |  |
| sql/select20 | generated | hand | 7158.9 | 19202.2 | 2.68x | 20346.8 | 2.84x | +6.0% | 21448 | 21448 | 15 % |  |
| sql/refused-late | generated | hand | 2727.4 | 13014.2 | 4.77x | 13273.4 | 4.87x | +2.0% | 13552 | 13552 | 21 % |  |
