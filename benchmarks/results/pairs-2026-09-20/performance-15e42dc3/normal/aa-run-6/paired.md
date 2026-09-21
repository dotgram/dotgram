# Paired stand, 2026-09-21 06:56

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 163975.8 | 182259.4 | 1.11x | 182825.8 | 1.11x | +0.3% | 720048 | 720048 | 166 % |  |
| sql/select20.at | generated | hand | 6718.0 | 17327.9 | 2.58x | 16976.1 | 2.53x | -2.0% | 21416 | 21416 | 12 % |  |
| sql/select20.window | generated | hand | 6608.8 | 17718.2 | 2.68x | 17461.3 | 2.64x | -1.5% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 363.9 | 367.9 | 1.01x | 369.0 | 1.01x | +0.3% | 0 | 0 | 10 % |  |
| tsql/select20.scan | generated | control | 368.6 | 376.1 | 1.02x | 364.6 | 0.99x | -3.1% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 133.1 | 133.4 | 1.00x | 134.0 | 1.01x | +0.5% | 0 | 0 | 12 % |  |
| el/ladder.bool | generated | hand | 941.6 | 1666.6 | 1.77x | 1629.8 | 1.73x | -2.2% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2507.0 | 11904.4 | 4.75x | 5627.0 | 2.24x | -52.7% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6596.4 | 17715.9 | 2.69x | 17466.9 | 2.65x | -1.4% | 21448 | 21392 | 12 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4865975.0 | 4300850.0 | 0.88x | 4488475.0 | 0.92x | +4.4% | 6276620 | 6276598 | 32 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6382131.2 | 6345337.5 | 0.99x | 6264537.5 | 0.98x | -1.3% | 8778556 | 8778600 | 10 % |  |
| el/ladder | generated | hand | 939.6 | 1670.3 | 1.78x | 1660.6 | 1.77x | -0.6% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 939.6 | 1126.4 | 1.20x | 1124.0 | 1.20x | -0.2% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 109319.8 | 147779.0 | 1.35x | 147280.4 | 1.35x | -0.3% | 169104 | 169104 | 8 % |  |
| el/terms1000 | immediate | hand | 109319.8 | 120927.8 | 1.11x | 121408.0 | 1.11x | +0.4% | 177123 | 177120 | 8 % |  |
| sql/select20 | generated | hand | 6622.5 | 17792.7 | 2.69x | 17385.7 | 2.63x | -2.3% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2499.0 | 11940.1 | 4.78x | 11685.7 | 4.68x | -2.1% | 13552 | 13552 | 3 % |  |
