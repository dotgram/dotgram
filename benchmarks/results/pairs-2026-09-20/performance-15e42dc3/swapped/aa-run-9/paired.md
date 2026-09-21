# Paired stand, 2026-09-21 07:29

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 174135.2 | 186700.0 | 1.07x | 186843.8 | 1.07x | +0.1% | 720048 | 720048 | 154 % |  |
| sql/select20.at | generated | hand | 6680.4 | 17017.0 | 2.55x | 17320.1 | 2.59x | +1.8% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6632.2 | 17447.5 | 2.63x | 17695.4 | 2.67x | +1.4% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 363.5 | 371.7 | 1.02x | 371.7 | 1.02x | 0.0% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 365.6 | 379.0 | 1.04x | 370.8 | 1.01x | -2.1% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 133.6 | 136.7 | 1.02x | 135.6 | 1.01x | -0.8% | 0 | 0 | 15 % |  |
| el/ladder.bool | generated | hand | 967.4 | 1731.5 | 1.79x | 1670.0 | 1.73x | -3.6% | 1776 | 1720 | 7 % |  |
| sql/refused-late.bool | generated | hand | 2507.3 | 11982.4 | 4.78x | 5647.0 | 2.25x | -52.9% | 13552 | 6616 | 1 % |  |
| sql/select20.bool | generated | hand | 6642.4 | 17910.7 | 2.70x | 17696.9 | 2.66x | -1.2% | 21448 | 21392 | 11 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4556443.8 | 4431237.5 | 0.97x | 4368650.0 | 0.96x | -1.4% | 6276642 | 6276641 | 5 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6413925.0 | 6391937.5 | 1.00x | 6343637.5 | 0.99x | -0.8% | 8778556 | 8778595 | 11 % |  |
| el/ladder | generated | hand | 965.5 | 1726.0 | 1.79x | 1747.0 | 1.81x | +1.2% | 1776 | 1776 | 8 % |  |
| el/ladder | immediate | hand | 965.5 | 1140.5 | 1.18x | 1160.9 | 1.20x | +1.8% | 1784 | 1784 | 8 % |  |
| el/terms1000 | generated | hand | 109829.3 | 147649.3 | 1.34x | 158255.3 | 1.44x | +7.2% | 169104 | 169131 | 5 % |  |
| el/terms1000 | immediate | hand | 109829.3 | 124049.5 | 1.13x | 126047.3 | 1.15x | +1.6% | 177120 | 177120 | 5 % |  |
| sql/select20 | generated | hand | 6676.8 | 17849.9 | 2.67x | 17380.8 | 2.60x | -2.6% | 21448 | 21448 | 16 % |  |
| sql/refused-late | generated | hand | 2508.8 | 11968.5 | 4.77x | 11795.8 | 4.70x | -1.4% | 13552 | 13552 | 4 % |  |
