# Paired stand, 2026-09-21 07:15

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.4 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164442.2 | 183786.7 | 1.12x | 189364.8 | 1.15x | +3.0% | 720048 | 720048 | 360 % |  |
| sql/select20.at | generated | hand | 6773.8 | 17142.4 | 2.53x | 17067.6 | 2.52x | -0.4% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6747.1 | 17509.8 | 2.60x | 17381.4 | 2.58x | -0.7% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 375.7 | 369.5 | 0.98x | 369.2 | 0.98x | -0.1% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 366.8 | 373.4 | 1.02x | 366.9 | 1.00x | -1.7% | 0 | 0 | 8 % |  |
| el/ladder.scan | generated | control | 133.6 | 134.1 | 1.00x | 134.8 | 1.01x | +0.5% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 943.7 | 1688.7 | 1.79x | 1648.8 | 1.75x | -2.4% | 1776 | 1720 | 16 % |  |
| sql/refused-late.bool | generated | hand | 2507.6 | 11773.1 | 4.69x | 5622.3 | 2.24x | -52.2% | 13552 | 6616 | 14 % |  |
| sql/select20.bool | generated | hand | 6659.5 | 17417.6 | 2.62x | 17470.6 | 2.62x | +0.3% | 21448 | 21392 | 38 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4334500.0 | 4509756.2 | 1.04x | 4419625.0 | 1.02x | -2.0% | 6276598 | 6276598 | 14 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6422700.0 | 6649187.5 | 1.04x | 6232662.5 | 0.97x | -6.3% | 8778600 | 8778600 | 16 % |  |
| el/ladder | generated | hand | 937.9 | 1686.9 | 1.80x | 1669.9 | 1.78x | -1.0% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 937.9 | 1157.3 | 1.23x | 1127.8 | 1.20x | -2.6% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 109426.4 | 150469.7 | 1.38x | 145653.4 | 1.33x | -3.2% | 169104 | 169107 | 11 % |  |
| el/terms1000 | immediate | hand | 109426.4 | 126465.2 | 1.16x | 121493.4 | 1.11x | -3.9% | 177120 | 177120 | 11 % |  |
| sql/select20 | generated | hand | 6693.4 | 17452.2 | 2.61x | 17504.0 | 2.62x | +0.3% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2502.7 | 11758.4 | 4.70x | 11676.2 | 4.67x | -0.7% | 13552 | 13552 | 2 % |  |
