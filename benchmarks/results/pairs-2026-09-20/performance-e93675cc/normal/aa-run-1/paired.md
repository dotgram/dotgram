# Paired stand, 2026-09-21 01:27

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 247551.6 | 221293.8 | 0.89x | 216400.0 | 0.87x | -2.2% | 403288 | 403288 | 739 % |  |
| tsql/columns1000 | generated | scriptdom | 1832775.0 | 185800.0 | 0.10x | 170425.0 | 0.09x | -8.3% | 200489 | 200489 | 54 % |  |
| web/json.array10000 | generated | hand | 171971.9 | 196313.3 | 1.14x | 192286.7 | 1.12x | -2.1% | 720048 | 720048 | 47 % |  |
| sql/select20.at | generated | hand | 6795.0 | 17688.2 | 2.60x | 25359.2 | 3.73x | +43.4% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 7081.1 | 18768.9 | 2.65x | 18638.7 | 2.63x | -0.7% | 21416 | 21416 | 15 % |  |
| sql/select20.scan | generated | control | 381.7 | 386.8 | 1.01x | 384.2 | 1.01x | -0.7% | 0 | 0 | 14 % |  |
| tsql/select20.scan | generated | control | 382.6 | 384.2 | 1.00x | 385.5 | 1.01x | +0.3% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 139.3 | 140.5 | 1.01x | 139.5 | 1.00x | -0.7% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 986.6 | 1751.7 | 1.78x | 1725.4 | 1.75x | -1.5% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2579.9 | 12143.2 | 4.71x | 5917.6 | 2.29x | -51.3% | 13552 | 6616 | 26 % |  |
| sql/select20.bool | generated | hand | 6803.0 | 18336.7 | 2.70x | 18380.5 | 2.70x | +0.2% | 21448 | 21392 | 5 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4623225.0 | 4376500.0 | 0.95x | 4494931.2 | 0.97x | +2.7% | 6276684 | 6276641 | 13 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2601153.1 | 2560906.2 | 0.98x | 2600918.8 | 1.00x | +1.6% | 2994123 | 2994123 | 10 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6478718.8 | 6458718.8 | 1.00x | 6578043.8 | 1.02x | +1.8% | 8778600 | 8778600 | 13 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6426981.2 | 6471337.5 | 1.01x | 6578881.2 | 1.02x | +1.7% | 8775404 | 8775448 | 8 % |  |
| fix/Orders128.yield-string | generated | hand | 81310.4 | 250676.1 | 3.08x | 262509.8 | 3.23x | +4.7% | 117936 | 117936 | 10 % |  |
| web/media-type.quoted | generated | control | 237.9 | 289.3 | 1.22x | 288.9 | 1.21x | -0.1% | 816 | 816 | 2 % |  |
| el/ladder | generated | hand | 954.7 | 1701.9 | 1.78x | 1831.7 | 1.92x | +7.6% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 954.7 | 1131.4 | 1.19x | 1129.9 | 1.18x | -0.1% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 112553.0 | 152490.0 | 1.35x | 149493.1 | 1.33x | -2.0% | 169104 | 169107 | 3 % |  |
| el/terms1000 | immediate | hand | 112553.0 | 125657.0 | 1.12x | 121766.2 | 1.08x | -3.1% | 177120 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6621.1 | 17711.9 | 2.68x | 17943.2 | 2.71x | +1.3% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2503.6 | 11761.1 | 4.70x | 11709.0 | 4.68x | -0.4% | 13552 | 13552 | 14 % |  |
