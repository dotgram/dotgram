# Paired stand, 2026-09-21 05:51

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 163296.1 | 182694.5 | 1.12x | 183175.0 | 1.12x | +0.3% | 720048 | 720048 | 354 % |  |
| sql/select20.at | generated | hand | 6689.6 | 17396.1 | 2.60x | 16956.9 | 2.53x | -2.5% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6724.5 | 17550.9 | 2.61x | 17399.7 | 2.59x | -0.9% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 361.9 | 367.3 | 1.02x | 368.7 | 1.02x | +0.4% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 385.7 | 366.4 | 0.95x | 367.5 | 0.95x | +0.3% | 0 | 0 | 9 % |  |
| el/ladder.scan | generated | control | 135.9 | 133.7 | 0.98x | 133.1 | 0.98x | -0.4% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 946.9 | 1669.5 | 1.76x | 1617.1 | 1.71x | -3.1% | 1776 | 1720 | 7 % |  |
| sql/refused-late.bool | generated | hand | 2517.3 | 11765.3 | 4.67x | 5720.3 | 2.27x | -51.4% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6719.4 | 18075.7 | 2.69x | 18219.4 | 2.71x | +0.8% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4312487.5 | 4373268.8 | 1.01x | 4414700.0 | 1.02x | +0.9% | 6276574 | 6276598 | 3 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6271037.5 | 6266675.0 | 1.00x | 6302550.0 | 1.01x | +0.6% | 8778556 | 8778600 | 4 % |  |
| el/ladder | generated | hand | 939.4 | 1669.4 | 1.78x | 1639.7 | 1.75x | -1.8% | 1776 | 1776 | 14 % |  |
| el/ladder | immediate | hand | 939.4 | 1126.8 | 1.20x | 1147.3 | 1.22x | +1.8% | 1784 | 1784 | 14 % |  |
| el/terms1000 | generated | hand | 109258.0 | 146170.3 | 1.34x | 147257.3 | 1.35x | +0.7% | 169104 | 169128 | 24 % |  |
| el/terms1000 | immediate | hand | 109258.0 | 120019.2 | 1.10x | 124299.6 | 1.14x | +3.6% | 177120 | 177123 | 24 % |  |
| sql/select20 | generated | hand | 6688.4 | 18060.1 | 2.70x | 17598.5 | 2.63x | -2.6% | 21448 | 21472 | 3 % |  |
| sql/refused-late | generated | hand | 2514.3 | 11791.2 | 4.69x | 11867.6 | 4.72x | +0.6% | 13552 | 13552 | 4 % |  |
