# Paired stand, 2026-09-21 06:58

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 167578.1 | 182018.8 | 1.09x | 184802.3 | 1.10x | +1.5% | 720048 | 720048 | 160 % |  |
| sql/select20.at | generated | hand | 6680.2 | 17110.1 | 2.56x | 17341.9 | 2.60x | +1.4% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6609.1 | 17375.3 | 2.63x | 17860.2 | 2.70x | +2.8% | 21416 | 21416 | 13 % |  |
| sql/select20.scan | generated | control | 362.8 | 369.6 | 1.02x | 369.2 | 1.02x | -0.1% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 369.8 | 368.7 | 1.00x | 370.2 | 1.00x | +0.4% | 0 | 0 | 11 % |  |
| el/ladder.scan | generated | control | 133.9 | 134.6 | 1.00x | 134.1 | 1.00x | -0.3% | 0 | 0 | 6 % |  |
| el/ladder.bool | generated | hand | 931.6 | 1664.1 | 1.79x | 1652.9 | 1.77x | -0.7% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2486.0 | 11733.5 | 4.72x | 5797.3 | 2.33x | -50.6% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6651.8 | 17332.0 | 2.61x | 17876.7 | 2.69x | +3.1% | 21448 | 21392 | 12 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4345931.2 | 4623587.5 | 1.06x | 4428075.0 | 1.02x | -4.2% | 6276684 | 6276684 | 5 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6291012.5 | 6295356.2 | 1.00x | 6420906.2 | 1.02x | +2.0% | 8778600 | 8778576 | 4 % |  |
| el/ladder | generated | hand | 946.4 | 1680.1 | 1.78x | 1649.3 | 1.74x | -1.8% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 946.4 | 1140.2 | 1.20x | 1146.1 | 1.21x | +0.5% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 109173.8 | 147908.6 | 1.35x | 145202.8 | 1.33x | -1.8% | 169104 | 169131 | 2 % |  |
| el/terms1000 | immediate | hand | 109173.8 | 122114.9 | 1.12x | 121097.2 | 1.11x | -0.8% | 177120 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6601.7 | 17385.0 | 2.63x | 17938.1 | 2.72x | +3.2% | 21448 | 21448 | 1 % |  |
| sql/refused-late | generated | hand | 2499.8 | 11749.6 | 4.70x | 12042.5 | 4.82x | +2.5% | 13552 | 13552 | 13 % |  |
