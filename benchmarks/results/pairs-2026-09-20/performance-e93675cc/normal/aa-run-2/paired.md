# Paired stand, 2026-09-21 01:35

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 256942.2 | 227021.9 | 0.88x | 220575.0 | 0.86x | -2.8% | 403288 | 403288 | 24 % |  |
| tsql/columns1000 | generated | scriptdom | 1779156.2 | 176387.5 | 0.10x | 171918.8 | 0.10x | -2.5% | 200489 | 200489 | 57 % |  |
| web/json.array10000 | generated | hand | 167306.2 | 187331.2 | 1.12x | 188164.1 | 1.12x | +0.4% | 720048 | 720048 | 16 % |  |
| sql/select20.at | generated | hand | 6744.5 | 17384.1 | 2.58x | 17325.7 | 2.57x | -0.3% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6846.3 | 17637.2 | 2.58x | 17780.4 | 2.60x | +0.8% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 373.9 | 376.5 | 1.01x | 374.3 | 1.00x | -0.6% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 370.1 | 373.2 | 1.01x | 373.3 | 1.01x | 0.0% | 0 | 0 | 17 % |  |
| el/ladder.scan | generated | control | 136.4 | 135.4 | 0.99x | 134.8 | 0.99x | -0.4% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 1015.9 | 1777.1 | 1.75x | 1754.3 | 1.73x | -1.3% | 1776 | 1720 | 22 % |  |
| sql/refused-late.bool | generated | hand | 2541.9 | 11852.0 | 4.66x | 5833.9 | 2.30x | -50.8% | 13552 | 6616 | 15 % |  |
| sql/select20.bool | generated | hand | 6707.1 | 17576.8 | 2.62x | 17950.3 | 2.68x | +2.1% | 21448 | 21392 | 6 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4467456.2 | 4540987.5 | 1.02x | 4561506.2 | 1.02x | +0.5% | 6276598 | 6276555 | 9 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2521112.5 | 2549906.2 | 1.01x | 2552921.9 | 1.01x | +0.1% | 2994166 | 2994166 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6272818.8 | 6349887.5 | 1.01x | 6466687.5 | 1.03x | +1.8% | 8778556 | 8778600 | 7 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6322737.5 | 6326875.0 | 1.00x | 6417025.0 | 1.01x | +1.4% | 8775448 | 8775448 | 5 % |  |
| fix/Orders128.yield-string | generated | hand | 80054.1 | 262570.9 | 3.28x | 259849.6 | 3.25x | -1.0% | 117936 | 117936 | 13 % |  |
| web/media-type.quoted | generated | control | 245.3 | 291.2 | 1.19x | 279.3 | 1.14x | -4.1% | 816 | 816 | 6 % |  |
| el/ladder | generated | hand | 949.9 | 1694.0 | 1.78x | 1664.3 | 1.75x | -1.8% | 1776 | 1776 | 8 % |  |
| el/ladder | immediate | hand | 949.9 | 1147.5 | 1.21x | 1119.5 | 1.18x | -2.4% | 1784 | 1784 | 8 % |  |
| el/terms1000 | generated | hand | 110725.1 | 148047.3 | 1.34x | 149000.5 | 1.35x | +0.6% | 169104 | 169131 | 7 % |  |
| el/terms1000 | immediate | hand | 110725.1 | 128055.4 | 1.16x | 123573.8 | 1.12x | -3.5% | 177120 | 177120 | 7 % |  |
| sql/select20 | generated | hand | 6737.0 | 17736.1 | 2.63x | 17809.8 | 2.64x | +0.4% | 21448 | 21448 | 12 % |  |
| sql/refused-late | generated | hand | 2561.2 | 11829.9 | 4.62x | 12121.6 | 4.73x | +2.5% | 13552 | 13552 | 3 % |  |
