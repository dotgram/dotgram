# Paired stand, 2026-09-21 06:45

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166583.6 | 185835.2 | 1.12x | 187325.8 | 1.12x | +0.8% | 720048 | 720048 | 161 % |  |
| sql/select20.at | generated | hand | 6635.3 | 17248.1 | 2.60x | 17301.5 | 2.61x | +0.3% | 21416 | 21416 | 41 % |  |
| sql/select20.window | generated | hand | 6587.6 | 17578.9 | 2.67x | 17536.5 | 2.66x | -0.2% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 374.0 | 369.5 | 0.99x | 370.2 | 0.99x | +0.2% | 0 | 0 | 9 % |  |
| tsql/select20.scan | generated | control | 367.2 | 368.7 | 1.00x | 383.1 | 1.04x | +3.9% | 0 | 0 | 1 % |  |
| el/ladder.scan | generated | control | 139.5 | 133.5 | 0.96x | 133.1 | 0.95x | -0.3% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 931.9 | 1664.0 | 1.79x | 1662.3 | 1.78x | -0.1% | 1776 | 1720 | 14 % |  |
| sql/refused-late.bool | generated | hand | 2478.6 | 11898.6 | 4.80x | 5607.0 | 2.26x | -52.9% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6678.9 | 17814.3 | 2.67x | 17840.2 | 2.67x | +0.1% | 21448 | 21392 | 8 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4282300.0 | 4374500.0 | 1.02x | 4440337.5 | 1.04x | +1.5% | 6276688 | 6276688 | 11 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6284312.5 | 6439331.2 | 1.02x | 6275062.5 | 1.00x | -2.6% | 8778600 | 8778600 | 12 % |  |
| el/ladder | generated | hand | 946.5 | 1658.8 | 1.75x | 1669.4 | 1.76x | +0.6% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 946.5 | 1128.3 | 1.19x | 1131.4 | 1.20x | +0.3% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 107750.5 | 146787.6 | 1.36x | 146097.2 | 1.36x | -0.5% | 169107 | 169104 | 2 % |  |
| el/terms1000 | immediate | hand | 107750.5 | 121169.9 | 1.12x | 119941.8 | 1.11x | -1.0% | 177120 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6602.5 | 17611.1 | 2.67x | 17671.3 | 2.68x | +0.3% | 21448 | 21472 | 27 % |  |
| sql/refused-late | generated | hand | 2473.6 | 11990.9 | 4.85x | 11759.9 | 4.75x | -1.9% | 13552 | 13552 | 2 % |  |
