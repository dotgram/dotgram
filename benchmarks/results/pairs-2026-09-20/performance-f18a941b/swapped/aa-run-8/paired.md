# Paired stand, 2026-09-21 05:50

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 163617.2 | 181267.2 | 1.11x | 182462.5 | 1.12x | +0.7% | 720048 | 720048 | 354 % |  |
| sql/select20.at | generated | hand | 6612.7 | 16954.8 | 2.56x | 16979.8 | 2.57x | +0.1% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6620.8 | 17519.5 | 2.65x | 17466.6 | 2.64x | -0.3% | 21416 | 21416 | 59 % |  |
| sql/select20.scan | generated | control | 374.8 | 366.0 | 0.98x | 375.8 | 1.00x | +2.7% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 370.1 | 369.5 | 1.00x | 367.6 | 0.99x | -0.5% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 133.9 | 134.2 | 1.00x | 134.3 | 1.00x | 0.0% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 941.7 | 1673.3 | 1.78x | 1658.3 | 1.76x | -0.9% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2477.2 | 11701.3 | 4.72x | 5587.2 | 2.26x | -52.3% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6569.0 | 17376.1 | 2.65x | 17580.7 | 2.68x | +1.2% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4345587.5 | 4301262.5 | 0.99x | 4377462.5 | 1.01x | +1.8% | 6276660 | 6276684 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6216437.5 | 6273968.8 | 1.01x | 6207262.5 | 1.00x | -1.1% | 8778600 | 8778600 | 14 % |  |
| el/ladder | generated | hand | 936.7 | 1681.4 | 1.79x | 1663.7 | 1.78x | -1.1% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 936.7 | 1165.2 | 1.24x | 1129.9 | 1.21x | -3.0% | 1784 | 1784 | 1 % |  |
| el/terms1000 | generated | hand | 107528.0 | 146229.9 | 1.36x | 147719.2 | 1.37x | +1.0% | 169128 | 169128 | 6 % |  |
| el/terms1000 | immediate | hand | 107528.0 | 116843.6 | 1.09x | 121225.7 | 1.13x | +3.8% | 177120 | 177120 | 6 % |  |
| sql/select20 | generated | hand | 6555.0 | 17366.3 | 2.65x | 17406.6 | 2.66x | +0.2% | 21448 | 21448 | 1 % |  |
| sql/refused-late | generated | hand | 2484.7 | 11666.4 | 4.70x | 11666.0 | 4.70x | 0.0% | 13552 | 13552 | 2 % |  |
