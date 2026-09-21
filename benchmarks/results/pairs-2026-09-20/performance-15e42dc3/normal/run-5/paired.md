# Paired stand, 2026-09-21 06:52

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 169421.9 | 183202.3 | 1.08x | 182260.2 | 1.08x | -0.5% | 720048 | 720048 | 11 % |  |
| sql/select20.at | generated | hand | 6635.7 | 17084.5 | 2.57x | 16928.9 | 2.55x | -0.9% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 6604.9 | 17681.7 | 2.68x | 17385.1 | 2.63x | -1.7% | 21416 | 21416 | 1 % |  |
| sql/select20.scan | generated | control | 378.8 | 373.5 | 0.99x | 369.9 | 0.98x | -1.0% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 368.3 | 372.0 | 1.01x | 376.0 | 1.02x | +1.1% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 133.2 | 134.5 | 1.01x | 134.2 | 1.01x | -0.2% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 953.1 | 1688.2 | 1.77x | 1705.4 | 1.79x | +1.0% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2514.5 | 11710.3 | 4.66x | 5564.8 | 2.21x | -52.5% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6623.4 | 17652.7 | 2.67x | 17375.1 | 2.62x | -1.6% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4414331.2 | 4388693.8 | 0.99x | 4386956.2 | 0.99x | 0.0% | 6276685 | 6276641 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6378831.2 | 6420818.8 | 1.01x | 6265475.0 | 0.98x | -2.4% | 8778600 | 8778600 | 12 % |  |
| el/ladder | generated | hand | 948.7 | 1675.3 | 1.77x | 1714.6 | 1.81x | +2.3% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 948.7 | 1131.9 | 1.19x | 1145.7 | 1.21x | +1.2% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 107452.6 | 147593.0 | 1.37x | 144916.5 | 1.35x | -1.8% | 169104 | 169128 | 43 % |  |
| el/terms1000 | immediate | hand | 107452.6 | 120390.9 | 1.12x | 118029.2 | 1.10x | -2.0% | 177120 | 177123 | 43 % |  |
| sql/select20 | generated | hand | 6709.8 | 17820.2 | 2.66x | 17467.5 | 2.60x | -2.0% | 21448 | 21448 | 11 % |  |
| sql/refused-late | generated | hand | 2509.5 | 11713.8 | 4.67x | 11654.0 | 4.64x | -0.5% | 13552 | 13552 | 19 % |  |
