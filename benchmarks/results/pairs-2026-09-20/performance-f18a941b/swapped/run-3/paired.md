# Paired stand, 2026-09-21 05:36

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165518.8 | 182175.8 | 1.10x | 182423.4 | 1.10x | +0.1% | 720048 | 720048 | 347 % |  |
| sql/select20.at | generated | hand | 6579.5 | 17107.7 | 2.60x | 17101.4 | 2.60x | 0.0% | 21416 | 21416 | 12 % |  |
| sql/select20.window | generated | hand | 6680.3 | 17405.4 | 2.61x | 17368.4 | 2.60x | -0.2% | 21416 | 21416 | 19 % |  |
| sql/select20.scan | generated | control | 373.3 | 374.2 | 1.00x | 374.8 | 1.00x | +0.2% | 0 | 0 | 8 % |  |
| tsql/select20.scan | generated | control | 376.5 | 425.9 | 1.13x | 380.1 | 1.01x | -10.8% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 134.0 | 136.0 | 1.02x | 135.6 | 1.01x | -0.3% | 0 | 0 | 3 % |  |
| el/ladder.bool | generated | hand | 941.5 | 1662.7 | 1.77x | 1641.6 | 1.74x | -1.3% | 1776 | 1720 | 16 % |  |
| sql/refused-late.bool | generated | hand | 2499.3 | 11878.1 | 4.75x | 5716.0 | 2.29x | -51.9% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6551.0 | 17546.5 | 2.68x | 17621.2 | 2.69x | +0.4% | 21448 | 21392 | 6 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4343900.0 | 4567000.0 | 1.05x | 4309000.0 | 0.99x | -5.6% | 6276598 | 6276598 | 52 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6538850.0 | 6473131.2 | 0.99x | 6552125.0 | 1.00x | +1.2% | 8778619 | 8778600 | 11 % |  |
| el/ladder | generated | hand | 939.8 | 1647.2 | 1.75x | 1661.8 | 1.77x | +0.9% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 939.8 | 1134.0 | 1.21x | 1141.3 | 1.21x | +0.6% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 106813.6 | 145291.8 | 1.36x | 147274.3 | 1.38x | +1.4% | 169104 | 169150 | 9 % |  |
| el/terms1000 | immediate | hand | 106813.6 | 119000.6 | 1.11x | 123265.2 | 1.15x | +3.6% | 177144 | 177120 | 9 % |  |
| sql/select20 | generated | hand | 6596.3 | 17483.9 | 2.65x | 17346.0 | 2.63x | -0.8% | 21448 | 21448 | 7 % |  |
| sql/refused-late | generated | hand | 2490.0 | 11843.6 | 4.76x | 11712.0 | 4.70x | -1.1% | 13552 | 13552 | 3 % |  |
