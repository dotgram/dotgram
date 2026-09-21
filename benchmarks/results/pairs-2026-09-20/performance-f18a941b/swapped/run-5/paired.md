# Paired stand, 2026-09-21 05:41

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165407.8 | 181929.7 | 1.10x | 181587.5 | 1.10x | -0.2% | 720048 | 720048 | 13 % |  |
| sql/select20.at | generated | hand | 6603.9 | 17124.7 | 2.59x | 17086.2 | 2.59x | -0.2% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6617.6 | 17432.6 | 2.63x | 17460.3 | 2.64x | +0.2% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 369.1 | 369.2 | 1.00x | 367.4 | 1.00x | -0.5% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 369.7 | 373.0 | 1.01x | 374.4 | 1.01x | +0.4% | 0 | 0 | 10 % |  |
| el/ladder.scan | generated | control | 136.8 | 133.4 | 0.97x | 133.3 | 0.97x | -0.1% | 0 | 0 | 23 % |  |
| el/ladder.bool | generated | hand | 953.4 | 1656.3 | 1.74x | 1625.2 | 1.70x | -1.9% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2495.1 | 11825.2 | 4.74x | 5616.7 | 2.25x | -52.5% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6629.9 | 17627.2 | 2.66x | 17534.6 | 2.64x | -0.5% | 21448 | 21392 | 5 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4388637.5 | 4357881.2 | 0.99x | 4394875.0 | 1.00x | +0.8% | 6276531 | 6276660 | 5 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6302650.0 | 6238037.5 | 0.99x | 6276662.5 | 1.00x | +0.6% | 8778556 | 8778600 | 14 % |  |
| el/ladder | generated | hand | 956.0 | 1652.9 | 1.73x | 1648.2 | 1.72x | -0.3% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 956.0 | 1121.4 | 1.17x | 1135.1 | 1.19x | +1.2% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 108443.7 | 147183.9 | 1.36x | 146199.4 | 1.35x | -0.7% | 169104 | 169104 | 14 % |  |
| el/terms1000 | immediate | hand | 108443.7 | 120714.3 | 1.11x | 120733.8 | 1.11x | 0.0% | 177120 | 177123 | 14 % |  |
| sql/select20 | generated | hand | 6653.7 | 17586.4 | 2.64x | 17633.8 | 2.65x | +0.3% | 21448 | 21448 | 5 % |  |
| sql/refused-late | generated | hand | 2483.2 | 11864.9 | 4.78x | 11742.9 | 4.73x | -1.0% | 13552 | 13552 | 13 % |  |
