# Paired stand, 2026-09-21 06:48

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 167181.2 | 190304.7 | 1.14x | 197160.9 | 1.18x | +3.6% | 720048 | 720048 | 161 % |  |
| sql/select20.at | generated | hand | 6577.8 | 16810.9 | 2.56x | 17364.6 | 2.64x | +3.3% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6558.6 | 17265.3 | 2.63x | 17984.4 | 2.74x | +4.2% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 362.1 | 373.7 | 1.03x | 368.6 | 1.02x | -1.4% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 363.8 | 367.3 | 1.01x | 383.1 | 1.05x | +4.3% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 133.5 | 133.9 | 1.00x | 133.9 | 1.00x | 0.0% | 0 | 0 | 1 % |  |
| el/ladder.bool | generated | hand | 946.1 | 1669.9 | 1.77x | 1611.2 | 1.70x | -3.5% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2502.7 | 11471.2 | 4.58x | 5535.2 | 2.21x | -51.7% | 13552 | 6616 | 18 % |  |
| sql/select20.bool | generated | hand | 6647.2 | 17265.9 | 2.60x | 17541.2 | 2.64x | +1.6% | 21448 | 21392 | 12 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4570431.2 | 4379831.2 | 0.96x | 4439375.0 | 0.97x | +1.4% | 6276684 | 6276684 | 8 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6490093.8 | 6396462.5 | 0.99x | 6438393.8 | 0.99x | +0.7% | 8778600 | 8778600 | 14 % |  |
| el/ladder | generated | hand | 938.0 | 1663.4 | 1.77x | 1642.7 | 1.75x | -1.2% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 938.0 | 1135.2 | 1.21x | 1129.4 | 1.20x | -0.5% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 106718.3 | 146953.5 | 1.38x | 148111.0 | 1.39x | +0.8% | 169104 | 169131 | 9 % |  |
| el/terms1000 | immediate | hand | 106718.3 | 119489.8 | 1.12x | 122279.3 | 1.15x | +2.3% | 177120 | 177120 | 9 % |  |
| sql/select20 | generated | hand | 6635.1 | 17147.3 | 2.58x | 17620.4 | 2.66x | +2.8% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2521.4 | 11448.4 | 4.54x | 11685.7 | 4.63x | +2.1% | 13552 | 13552 | 13 % |  |
