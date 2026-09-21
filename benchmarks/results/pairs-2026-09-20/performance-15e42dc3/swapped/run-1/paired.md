# Paired stand, 2026-09-21 07:07

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166532.0 | 184197.7 | 1.11x | 183727.3 | 1.10x | -0.3% | 720048 | 720048 | 162 % |  |
| sql/select20.at | generated | hand | 6884.2 | 17136.6 | 2.49x | 17012.9 | 2.47x | -0.7% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6901.4 | 17338.8 | 2.51x | 17419.8 | 2.52x | +0.5% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 365.3 | 365.9 | 1.00x | 368.9 | 1.01x | +0.8% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 368.1 | 379.7 | 1.03x | 366.2 | 0.99x | -3.6% | 0 | 0 | 7 % |  |
| el/ladder.scan | generated | control | 133.3 | 133.7 | 1.00x | 133.9 | 1.00x | +0.1% | 0 | 0 | 17 % |  |
| el/ladder.bool | generated | hand | 960.2 | 1670.0 | 1.74x | 1652.3 | 1.72x | -1.1% | 1776 | 1720 | 45 % |  |
| sql/refused-late.bool | generated | hand | 2585.4 | 11661.4 | 4.51x | 5617.6 | 2.17x | -51.8% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6866.6 | 17458.3 | 2.54x | 17506.0 | 2.55x | +0.3% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4643712.5 | 4403593.8 | 0.95x | 4455468.8 | 0.96x | +1.2% | 6276684 | 6276684 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6527531.2 | 6557168.8 | 1.00x | 6314750.0 | 0.97x | -3.7% | 8778556 | 8778595 | 11 % |  |
| el/ladder | generated | hand | 966.6 | 1683.7 | 1.74x | 1687.9 | 1.75x | +0.2% | 1776 | 1776 | 6 % |  |
| el/ladder | immediate | hand | 966.6 | 1139.8 | 1.18x | 1134.2 | 1.17x | -0.5% | 1784 | 1784 | 6 % |  |
| el/terms1000 | generated | hand | 109762.9 | 147117.8 | 1.34x | 148590.3 | 1.35x | +1.0% | 169104 | 169131 | 3 % |  |
| el/terms1000 | immediate | hand | 109762.9 | 121060.4 | 1.10x | 125295.1 | 1.14x | +3.5% | 177120 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6877.6 | 17401.9 | 2.53x | 17386.6 | 2.53x | -0.1% | 21448 | 21472 | 14 % |  |
| sql/refused-late | generated | hand | 2586.4 | 11675.1 | 4.51x | 11672.5 | 4.51x | 0.0% | 13552 | 13576 | 13 % |  |
