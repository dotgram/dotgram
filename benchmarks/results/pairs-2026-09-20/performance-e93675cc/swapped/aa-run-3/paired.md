# Paired stand, 2026-09-21 02:21

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 245270.3 | 213348.4 | 0.87x | 216185.9 | 0.88x | +1.3% | 403288 | 403288 | 18 % |  |
| tsql/columns1000 | generated | scriptdom | 1941087.5 | 176581.2 | 0.09x | 183837.5 | 0.09x | +4.1% | 200489 | 200489 | 55 % |  |
| web/json.array10000 | generated | hand | 164835.9 | 186985.2 | 1.13x | 185293.0 | 1.12x | -0.9% | 720048 | 720048 | 6 % |  |
| sql/select20.at | generated | hand | 6677.0 | 17167.1 | 2.57x | 17721.1 | 2.65x | +3.2% | 21416 | 21416 | 37 % |  |
| sql/select20.window | generated | hand | 6693.5 | 17672.3 | 2.64x | 18206.0 | 2.72x | +3.0% | 21416 | 21416 | 16 % |  |
| sql/select20.scan | generated | control | 371.9 | 367.2 | 0.99x | 369.1 | 0.99x | +0.5% | 0 | 0 | 17 % |  |
| tsql/select20.scan | generated | control | 370.1 | 420.0 | 1.13x | 367.8 | 0.99x | -12.4% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 136.0 | 133.6 | 0.98x | 133.6 | 0.98x | 0.0% | 0 | 0 | 13 % |  |
| el/ladder.bool | generated | hand | 943.4 | 1713.6 | 1.82x | 1638.6 | 1.74x | -4.4% | 1776 | 1720 | 17 % |  |
| sql/refused-late.bool | generated | hand | 2526.8 | 11696.5 | 4.63x | 5720.9 | 2.26x | -51.1% | 13552 | 6616 | 14 % |  |
| sql/select20.bool | generated | hand | 6665.3 | 17712.0 | 2.66x | 17979.3 | 2.70x | +1.5% | 21448 | 21392 | 5 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4487100.0 | 4196850.0 | 0.94x | 4535162.5 | 1.01x | +8.1% | 6276660 | 6276684 | 11 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2501468.8 | 2526937.5 | 1.01x | 2535018.8 | 1.01x | +0.3% | 2994166 | 2994166 | 3 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6318843.8 | 6283275.0 | 0.99x | 6437287.5 | 1.02x | +2.5% | 8778600 | 8778600 | 6 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6251350.0 | 6266950.0 | 1.00x | 6279450.0 | 1.00x | +0.2% | 8775467 | 8775448 | 5 % |  |
| fix/Orders128.yield-string | generated | hand | 76852.6 | 256469.1 | 3.34x | 270854.9 | 3.52x | +5.6% | 117936 | 117936 | 16 % |  |
| web/media-type.quoted | generated | control | 244.3 | 294.9 | 1.21x | 282.1 | 1.15x | -4.3% | 816 | 816 | 17 % |  |
| el/ladder | generated | hand | 953.3 | 1722.9 | 1.81x | 1694.2 | 1.78x | -1.7% | 1776 | 1776 | 13 % |  |
| el/ladder | immediate | hand | 953.3 | 1146.1 | 1.20x | 1124.4 | 1.18x | -1.9% | 1784 | 1784 | 13 % |  |
| el/terms1000 | generated | hand | 106082.6 | 152139.2 | 1.43x | 147666.5 | 1.39x | -2.9% | 169104 | 169131 | 4 % |  |
| el/terms1000 | immediate | hand | 106082.6 | 128926.6 | 1.22x | 123355.4 | 1.16x | -4.3% | 177120 | 177120 | 4 % |  |
| sql/select20 | generated | hand | 6672.0 | 17587.3 | 2.64x | 17711.4 | 2.65x | +0.7% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2520.2 | 11731.4 | 4.65x | 11838.0 | 4.70x | +0.9% | 13552 | 13552 | 2 % |  |
