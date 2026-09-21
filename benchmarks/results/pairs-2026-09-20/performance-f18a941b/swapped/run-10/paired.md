# Paired stand, 2026-09-21 05:54

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 163978.9 | 180494.5 | 1.10x | 182090.6 | 1.11x | +0.9% | 720048 | 720048 | 45 % |  |
| sql/select20.at | generated | hand | 6627.3 | 17049.1 | 2.57x | 17132.6 | 2.59x | +0.5% | 21416 | 21416 | 8 % |  |
| sql/select20.window | generated | hand | 6572.0 | 17615.1 | 2.68x | 18215.4 | 2.77x | +3.4% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 363.6 | 370.8 | 1.02x | 371.3 | 1.02x | +0.1% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 382.6 | 371.9 | 0.97x | 381.4 | 1.00x | +2.6% | 0 | 0 | 1 % |  |
| el/ladder.scan | generated | control | 137.6 | 133.1 | 0.97x | 136.7 | 0.99x | +2.7% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 931.5 | 1674.4 | 1.80x | 1637.5 | 1.76x | -2.2% | 1776 | 1720 | 9 % |  |
| sql/refused-late.bool | generated | hand | 2496.7 | 11772.6 | 4.72x | 5640.9 | 2.26x | -52.1% | 13552 | 6616 | 14 % |  |
| sql/select20.bool | generated | hand | 6598.7 | 17539.7 | 2.66x | 17643.3 | 2.67x | +0.6% | 21448 | 21392 | 10 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4290468.8 | 4293387.5 | 1.00x | 4369468.8 | 1.02x | +1.8% | 6276660 | 6276660 | 5 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6182531.2 | 6294012.5 | 1.02x | 6264625.0 | 1.01x | -0.5% | 8778556 | 8778600 | 4 % |  |
| el/ladder | generated | hand | 932.6 | 1678.9 | 1.80x | 1629.7 | 1.75x | -2.9% | 1776 | 1776 | 18 % |  |
| el/ladder | immediate | hand | 932.6 | 1120.9 | 1.20x | 1114.1 | 1.19x | -0.6% | 1784 | 1784 | 18 % |  |
| el/terms1000 | generated | hand | 107760.9 | 148756.6 | 1.38x | 146966.5 | 1.36x | -1.2% | 169104 | 169128 | 2 % |  |
| el/terms1000 | immediate | hand | 107760.9 | 121844.0 | 1.13x | 121223.6 | 1.12x | -0.5% | 177123 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6573.3 | 17518.0 | 2.67x | 17718.4 | 2.70x | +1.1% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2502.6 | 11773.5 | 4.70x | 11736.4 | 4.69x | -0.3% | 13552 | 13552 | 4 % |  |
