# Paired stand, 2026-09-21 07:01

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 168257.0 | 184215.6 | 1.09x | 184296.1 | 1.10x | 0.0% | 720048 | 720048 | 39 % |  |
| sql/select20.at | generated | hand | 6776.8 | 17033.5 | 2.51x | 17554.4 | 2.59x | +3.1% | 21416 | 21416 | 14 % |  |
| sql/select20.window | generated | hand | 6689.5 | 17385.0 | 2.60x | 17443.9 | 2.61x | +0.3% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 370.9 | 368.3 | 0.99x | 370.6 | 1.00x | +0.6% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 369.1 | 369.9 | 1.00x | 369.8 | 1.00x | 0.0% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 133.3 | 133.4 | 1.00x | 133.8 | 1.00x | +0.3% | 0 | 0 | 1 % |  |
| el/ladder.bool | generated | hand | 959.4 | 1662.9 | 1.73x | 1652.0 | 1.72x | -0.7% | 1776 | 1720 | 46 % |  |
| sql/refused-late.bool | generated | hand | 2506.7 | 12214.9 | 4.87x | 5664.5 | 2.26x | -53.6% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6705.9 | 19107.1 | 2.85x | 17612.1 | 2.63x | -7.8% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4284900.0 | 4398700.0 | 1.03x | 4399850.0 | 1.03x | 0.0% | 6276664 | 6276664 | 12 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6477256.2 | 6304662.5 | 0.97x | 6358875.0 | 0.98x | +0.9% | 8778619 | 8778600 | 14 % |  |
| el/ladder | generated | hand | 964.5 | 1656.2 | 1.72x | 1679.9 | 1.74x | +1.4% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 964.5 | 1136.2 | 1.18x | 1150.0 | 1.19x | +1.2% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 106894.5 | 147681.8 | 1.38x | 147388.7 | 1.38x | -0.2% | 169104 | 169104 | 6 % |  |
| el/terms1000 | immediate | hand | 106894.5 | 121938.0 | 1.14x | 126413.4 | 1.18x | +3.7% | 177123 | 177120 | 6 % |  |
| sql/select20 | generated | hand | 6665.6 | 19039.6 | 2.86x | 17517.3 | 2.63x | -8.0% | 21448 | 21448 | 11 % |  |
| sql/refused-late | generated | hand | 2501.1 | 12266.6 | 4.90x | 11891.8 | 4.75x | -3.1% | 13552 | 13552 | 4 % |  |
