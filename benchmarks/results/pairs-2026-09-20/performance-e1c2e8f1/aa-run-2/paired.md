# Paired stand, 2026-09-21 00:53

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 276809.4 | 249103.1 | 0.90x | 236481.2 | 0.85x | -5.1% | 403288 | 403288 | 91 % |  |
| tsql/columns1000 | generated | scriptdom | 2127112.5 | 281981.2 | 0.13x | 277700.0 | 0.13x | -1.5% | 200489 | 200489 | 52 % |  |
| web/json.array10000 | generated | hand | 177073.4 | 198092.2 | 1.12x | 194044.5 | 1.10x | -2.0% | 720048 | 720048 | 137 % |  |
| sql/select20.at | generated | hand | 7004.0 | 17792.6 | 2.54x | 18491.1 | 2.64x | +3.9% | 21416 | 21416 | 68 % |  |
| sql/select20.window | generated | hand | 6926.2 | 18722.3 | 2.70x | 19012.3 | 2.74x | +1.5% | 21416 | 21416 | 62 % |  |
| sql/select20.scan | generated | control | 371.2 | 377.6 | 1.02x | 374.4 | 1.01x | -0.8% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 374.7 | 373.5 | 1.00x | 371.4 | 0.99x | -0.6% | 0 | 0 | 59 % |  |
| el/ladder.scan | generated | control | 140.9 | 135.8 | 0.96x | 135.8 | 0.96x | 0.0% | 0 | 0 | 15 % |  |
| el/ladder.bool | generated | hand | 958.8 | 1738.2 | 1.81x | 1706.5 | 1.78x | -1.8% | 1776 | 1720 | 19 % |  |
| sql/refused-late.bool | generated | hand | 2573.9 | 12094.3 | 4.70x | 5842.6 | 2.27x | -51.7% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6726.1 | 17932.4 | 2.67x | 18520.9 | 2.75x | +3.3% | 21448 | 21392 | 39 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4638887.5 | 4589087.5 | 0.99x | 4688700.0 | 1.01x | +2.2% | 6276684 | 6276684 | 51 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2580762.5 | 2564150.0 | 0.99x | 2656837.5 | 1.03x | +3.6% | 2994123 | 2994123 | 50 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6428781.2 | 6413400.0 | 1.00x | 6448993.8 | 1.00x | +0.6% | 8778600 | 8778619 | 17 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6548637.5 | 6390400.0 | 0.98x | 6469300.0 | 0.99x | +1.2% | 8775448 | 8775448 | 25 % |  |
| fix/Orders128.yield-string | generated | hand | 84277.5 | 253129.6 | 3.00x | 256490.5 | 3.04x | +1.3% | 117936 | 117936 | 72 % |  |
| web/media-type.quoted | generated | control | 254.4 | 298.5 | 1.17x | 309.6 | 1.22x | +3.7% | 816 | 816 | 11 % |  |
| el/ladder | generated | hand | 963.7 | 1753.5 | 1.82x | 1765.7 | 1.83x | +0.7% | 1776 | 1776 | 10 % |  |
| el/ladder | immediate | hand | 963.7 | 1195.7 | 1.24x | 1203.6 | 1.25x | +0.7% | 1784 | 1784 | 10 % |  |
| el/terms1000 | generated | hand | 109753.4 | 150163.9 | 1.37x | 152224.1 | 1.39x | +1.4% | 169104 | 169107 | 15 % |  |
| el/terms1000 | immediate | hand | 109753.4 | 126069.4 | 1.15x | 122503.1 | 1.12x | -2.8% | 177120 | 177120 | 15 % |  |
| sql/select20 | generated | hand | 6801.4 | 17921.2 | 2.63x | 18736.9 | 2.75x | +4.6% | 21448 | 21448 | 31 % |  |
| sql/refused-late | generated | hand | 2659.8 | 12827.2 | 4.82x | 12984.3 | 4.88x | +1.2% | 13552 | 13552 | 36 % |  |
