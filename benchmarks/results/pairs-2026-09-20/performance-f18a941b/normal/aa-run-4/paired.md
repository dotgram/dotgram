# Paired stand, 2026-09-21 05:14

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166591.4 | 182328.9 | 1.09x | 183453.1 | 1.10x | +0.6% | 720048 | 720048 | 50 % |  |
| sql/select20.at | generated | hand | 6626.6 | 17257.3 | 2.60x | 16867.0 | 2.55x | -2.3% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6654.6 | 17776.4 | 2.67x | 17421.8 | 2.62x | -2.0% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 387.4 | 366.5 | 0.95x | 366.2 | 0.95x | -0.1% | 0 | 0 | 29 % |  |
| tsql/select20.scan | generated | control | 366.6 | 372.9 | 1.02x | 374.7 | 1.02x | +0.5% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 135.3 | 134.5 | 0.99x | 133.0 | 0.98x | -1.1% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 938.5 | 1666.2 | 1.78x | 1637.9 | 1.75x | -1.7% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2467.3 | 11943.4 | 4.84x | 5519.6 | 2.24x | -53.8% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6562.0 | 17678.9 | 2.69x | 17460.8 | 2.66x | -1.2% | 21448 | 21392 | 12 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4189250.0 | 4240675.0 | 1.01x | 4190750.0 | 1.00x | -1.2% | 6276600 | 6276557 | 27 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6473231.2 | 6396668.8 | 0.99x | 6128581.2 | 0.95x | -4.2% | 8778600 | 8778600 | 8 % |  |
| el/ladder | generated | hand | 946.9 | 1664.1 | 1.76x | 1665.7 | 1.76x | +0.1% | 1776 | 1776 | 6 % |  |
| el/ladder | immediate | hand | 946.9 | 1123.8 | 1.19x | 1113.8 | 1.18x | -0.9% | 1784 | 1784 | 6 % |  |
| el/terms1000 | generated | hand | 107437.8 | 144278.3 | 1.34x | 145236.3 | 1.35x | +0.7% | 169104 | 169107 | 14 % |  |
| el/terms1000 | immediate | hand | 107437.8 | 117688.5 | 1.10x | 119496.7 | 1.11x | +1.5% | 177120 | 177120 | 14 % |  |
| sql/select20 | generated | hand | 6582.0 | 17666.5 | 2.68x | 17338.2 | 2.63x | -1.9% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2474.1 | 11907.1 | 4.81x | 11744.2 | 4.75x | -1.4% | 13552 | 13552 | 3 % |  |
