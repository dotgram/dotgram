# Paired stand, 2026-09-21 01:58

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 242492.2 | 214173.4 | 0.88x | 225420.3 | 0.93x | +5.3% | 403288 | 403288 | 26 % |  |
| tsql/columns1000 | generated | scriptdom | 1653175.0 | 170612.5 | 0.10x | 166675.0 | 0.10x | -2.3% | 200489 | 200489 | 7 % |  |
| web/json.array10000 | generated | hand | 165697.7 | 185635.2 | 1.12x | 184062.5 | 1.11x | -0.8% | 720048 | 720048 | 41 % |  |
| sql/select20.at | generated | hand | 6937.4 | 17421.7 | 2.51x | 17020.1 | 2.45x | -2.3% | 21416 | 21416 | 25 % |  |
| sql/select20.window | generated | hand | 6971.5 | 17647.0 | 2.53x | 17576.9 | 2.52x | -0.4% | 21416 | 21416 | 17 % |  |
| sql/select20.scan | generated | control | 370.0 | 375.5 | 1.02x | 373.3 | 1.01x | -0.6% | 0 | 0 | 21 % |  |
| tsql/select20.scan | generated | control | 373.5 | 376.3 | 1.01x | 380.2 | 1.02x | +1.0% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 137.2 | 137.6 | 1.00x | 135.9 | 0.99x | -1.2% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 959.2 | 1682.3 | 1.75x | 1723.1 | 1.80x | +2.4% | 1776 | 1720 | 12 % |  |
| sql/refused-late.bool | generated | hand | 2600.9 | 11936.6 | 4.59x | 5550.0 | 2.13x | -53.5% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6999.1 | 17919.6 | 2.56x | 17557.8 | 2.51x | -2.0% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4237425.0 | 4292025.0 | 1.01x | 4379325.0 | 1.03x | +2.0% | 6276641 | 6276641 | 45 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2523884.4 | 2556993.8 | 1.01x | 2518000.0 | 1.00x | -1.5% | 2994166 | 2994166 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6249650.0 | 6455662.5 | 1.03x | 6304100.0 | 1.01x | -2.3% | 8778600 | 8778600 | 8 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6166218.8 | 6341825.0 | 1.03x | 6323618.8 | 1.03x | -0.3% | 8775424 | 8775448 | 22 % |  |
| fix/Orders128.yield-string | generated | hand | 77318.2 | 243829.7 | 3.15x | 263529.2 | 3.41x | +8.1% | 117936 | 117936 | 8 % |  |
| web/media-type.quoted | generated | control | 234.9 | 289.1 | 1.23x | 292.3 | 1.24x | +1.1% | 816 | 816 | 11 % |  |
| el/ladder | generated | hand | 956.0 | 1668.1 | 1.74x | 1727.0 | 1.81x | +3.5% | 1776 | 1776 | 7 % |  |
| el/ladder | immediate | hand | 956.0 | 1118.0 | 1.17x | 1126.4 | 1.18x | +0.8% | 1784 | 1784 | 7 % |  |
| el/terms1000 | generated | hand | 110345.3 | 146419.3 | 1.33x | 147997.1 | 1.34x | +1.1% | 169104 | 169131 | 9 % |  |
| el/terms1000 | immediate | hand | 110345.3 | 124957.6 | 1.13x | 123586.6 | 1.12x | -1.1% | 177120 | 177120 | 9 % |  |
| sql/select20 | generated | hand | 6975.3 | 17893.2 | 2.57x | 17600.3 | 2.52x | -1.6% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2620.7 | 11871.1 | 4.53x | 11747.3 | 4.48x | -1.0% | 13552 | 13552 | 16 % |  |
