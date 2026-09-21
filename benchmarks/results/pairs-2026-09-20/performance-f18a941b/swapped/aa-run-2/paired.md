# Paired stand, 2026-09-21 05:34

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164945.3 | 182994.5 | 1.11x | 182689.1 | 1.11x | -0.2% | 720048 | 720048 | 352 % |  |
| sql/select20.at | generated | hand | 6654.2 | 18386.6 | 2.76x | 16960.6 | 2.55x | -7.8% | 21416 | 21416 | 32 % |  |
| sql/select20.window | generated | hand | 6633.9 | 18395.9 | 2.77x | 17550.8 | 2.65x | -4.6% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 369.8 | 366.8 | 0.99x | 366.8 | 0.99x | 0.0% | 0 | 0 | 10 % |  |
| tsql/select20.scan | generated | control | 368.7 | 366.8 | 0.99x | 369.4 | 1.00x | +0.7% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 134.3 | 134.3 | 1.00x | 134.2 | 1.00x | -0.1% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 922.8 | 1641.1 | 1.78x | 1658.8 | 1.80x | +1.1% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2506.1 | 12827.0 | 5.12x | 5644.3 | 2.25x | -56.0% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6633.0 | 19966.5 | 3.01x | 17589.1 | 2.65x | -11.9% | 21448 | 21392 | 9 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4386581.2 | 4479875.0 | 1.02x | 4406987.5 | 1.00x | -1.6% | 6276684 | 6276684 | 8 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6250106.2 | 6665843.8 | 1.07x | 6350100.0 | 1.02x | -4.7% | 8778556 | 8778600 | 4 % |  |
| el/ladder | generated | hand | 932.4 | 1654.6 | 1.77x | 1700.0 | 1.82x | +2.7% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 932.4 | 1133.1 | 1.22x | 1127.7 | 1.21x | -0.5% | 1784 | 1784 | 1 % |  |
| el/terms1000 | generated | hand | 106593.6 | 147789.8 | 1.39x | 145838.8 | 1.37x | -1.3% | 169104 | 169104 | 3 % |  |
| el/terms1000 | immediate | hand | 106593.6 | 121300.6 | 1.14x | 121674.7 | 1.14x | +0.3% | 177120 | 177123 | 3 % |  |
| sql/select20 | generated | hand | 6651.6 | 19799.1 | 2.98x | 17352.5 | 2.61x | -12.4% | 21448 | 21448 | 6 % |  |
| sql/refused-late | generated | hand | 2508.7 | 12773.7 | 5.09x | 11772.2 | 4.69x | -7.8% | 13552 | 13552 | 3 % |  |
