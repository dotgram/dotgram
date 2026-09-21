# Paired stand, 2026-09-21 07:27

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 170399.2 | 187598.4 | 1.10x | 185097.7 | 1.09x | -1.3% | 720048 | 720048 | 337 % |  |
| sql/select20.at | generated | hand | 6689.6 | 17131.0 | 2.56x | 17174.9 | 2.57x | +0.3% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6600.8 | 19368.2 | 2.93x | 17539.7 | 2.66x | -9.4% | 21416 | 21416 | 7 % |  |
| sql/select20.scan | generated | control | 367.4 | 369.4 | 1.01x | 368.9 | 1.00x | -0.1% | 0 | 0 | 27 % |  |
| tsql/select20.scan | generated | control | 367.6 | 366.5 | 1.00x | 367.0 | 1.00x | +0.1% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 133.9 | 133.2 | 1.00x | 134.1 | 1.00x | +0.6% | 0 | 0 | 3 % |  |
| el/ladder.bool | generated | hand | 986.0 | 1648.4 | 1.67x | 1640.1 | 1.66x | -0.5% | 1776 | 1720 | 22 % |  |
| sql/refused-late.bool | generated | hand | 2490.9 | 11859.8 | 4.76x | 5656.9 | 2.27x | -52.3% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6630.1 | 17573.1 | 2.65x | 17612.8 | 2.66x | +0.2% | 21448 | 21392 | 14 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4559493.8 | 4451731.2 | 0.98x | 4687487.5 | 1.03x | +5.3% | 6276684 | 6276684 | 20 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6433937.5 | 6317456.2 | 0.98x | 6329181.2 | 0.98x | +0.2% | 8778600 | 8778576 | 13 % |  |
| el/ladder | generated | hand | 976.4 | 1655.8 | 1.70x | 1672.2 | 1.71x | +1.0% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 976.4 | 1124.5 | 1.15x | 1131.2 | 1.16x | +0.6% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 109584.3 | 148797.9 | 1.36x | 147086.9 | 1.34x | -1.1% | 169107 | 169104 | 2 % |  |
| el/terms1000 | immediate | hand | 109584.3 | 121115.9 | 1.11x | 120462.1 | 1.10x | -0.5% | 177120 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6609.0 | 17480.8 | 2.65x | 18216.8 | 2.76x | +4.2% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2492.5 | 11857.3 | 4.76x | 11998.6 | 4.81x | +1.2% | 13552 | 13552 | 4 % |  |
