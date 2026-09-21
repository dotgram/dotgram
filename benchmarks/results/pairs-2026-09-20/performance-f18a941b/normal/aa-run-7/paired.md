# Paired stand, 2026-09-21 05:21

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164664.8 | 182768.8 | 1.11x | 182868.0 | 1.11x | +0.1% | 720048 | 720048 | 360 % |  |
| sql/select20.at | generated | hand | 6652.2 | 18327.9 | 2.76x | 16865.2 | 2.54x | -8.0% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6641.2 | 18679.6 | 2.81x | 17285.5 | 2.60x | -7.5% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 387.3 | 368.3 | 0.95x | 370.7 | 0.96x | +0.7% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 375.3 | 375.6 | 1.00x | 373.2 | 0.99x | -0.6% | 0 | 0 | 14 % |  |
| el/ladder.scan | generated | control | 135.8 | 134.7 | 0.99x | 134.0 | 0.99x | -0.5% | 0 | 0 | 4 % |  |
| el/ladder.bool | generated | hand | 945.0 | 1648.7 | 1.74x | 1610.0 | 1.70x | -2.3% | 1776 | 1720 | 9 % |  |
| sql/refused-late.bool | generated | hand | 2552.9 | 12431.1 | 4.87x | 5608.2 | 2.20x | -54.9% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6693.9 | 18676.6 | 2.79x | 17401.6 | 2.60x | -6.8% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4367950.0 | 4405100.0 | 1.01x | 4360793.8 | 1.00x | -1.0% | 6276684 | 6276598 | 8 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6364500.0 | 6373350.0 | 1.00x | 6375681.2 | 1.00x | 0.0% | 8778576 | 8778600 | 7 % |  |
| el/ladder | generated | hand | 948.3 | 1644.4 | 1.73x | 1696.1 | 1.79x | +3.1% | 1776 | 1776 | 7 % |  |
| el/ladder | immediate | hand | 948.3 | 1180.7 | 1.25x | 1127.9 | 1.19x | -4.5% | 1784 | 1784 | 7 % |  |
| el/terms1000 | generated | hand | 106415.3 | 148986.7 | 1.40x | 145531.6 | 1.37x | -2.3% | 169107 | 169104 | 17 % |  |
| el/terms1000 | immediate | hand | 106415.3 | 143212.3 | 1.35x | 117935.4 | 1.11x | -17.6% | 177120 | 177120 | 17 % |  |
| sql/select20 | generated | hand | 6693.0 | 18703.4 | 2.79x | 17351.4 | 2.59x | -7.2% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2532.8 | 12383.2 | 4.89x | 11709.6 | 4.62x | -5.4% | 13552 | 13552 | 1 % |  |
