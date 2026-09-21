# Paired stand, 2026-09-21 05:33

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165589.8 | 180693.8 | 1.09x | 182678.1 | 1.10x | +1.1% | 720048 | 720048 | 347 % |  |
| sql/select20.at | generated | hand | 6584.1 | 17150.3 | 2.60x | 17171.2 | 2.61x | +0.1% | 21416 | 21416 | 7 % |  |
| sql/select20.window | generated | hand | 6598.3 | 17459.2 | 2.65x | 17384.2 | 2.63x | -0.4% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 365.0 | 373.9 | 1.02x | 369.3 | 1.01x | -1.2% | 0 | 0 | 17 % |  |
| tsql/select20.scan | generated | control | 368.5 | 373.5 | 1.01x | 369.7 | 1.00x | -1.0% | 0 | 0 | 14 % |  |
| el/ladder.scan | generated | control | 134.5 | 134.2 | 1.00x | 134.3 | 1.00x | 0.0% | 0 | 0 | 6 % |  |
| el/ladder.bool | generated | hand | 964.0 | 1639.4 | 1.70x | 1657.6 | 1.72x | +1.1% | 1776 | 1720 | 46 % |  |
| sql/refused-late.bool | generated | hand | 2483.4 | 11885.9 | 4.79x | 5659.2 | 2.28x | -52.4% | 13552 | 6616 | 1 % |  |
| sql/select20.bool | generated | hand | 6630.1 | 17503.9 | 2.64x | 17653.7 | 2.66x | +0.9% | 21448 | 21392 | 5 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4311175.0 | 4376262.5 | 1.02x | 4325825.0 | 1.00x | -1.2% | 6276643 | 6276641 | 16 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6197218.8 | 6211381.2 | 1.00x | 6292118.8 | 1.02x | +1.3% | 8778556 | 8778600 | 13 % |  |
| el/ladder | generated | hand | 943.5 | 1636.6 | 1.73x | 1666.9 | 1.77x | +1.9% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 943.5 | 1120.6 | 1.19x | 1108.2 | 1.17x | -1.1% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 105961.3 | 146939.5 | 1.39x | 147416.7 | 1.39x | +0.3% | 169104 | 169104 | 3 % |  |
| el/terms1000 | immediate | hand | 105961.3 | 121306.4 | 1.14x | 121568.3 | 1.15x | +0.2% | 177120 | 177123 | 3 % |  |
| sql/select20 | generated | hand | 6594.7 | 17434.1 | 2.64x | 17481.9 | 2.65x | +0.3% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2498.3 | 11866.5 | 4.75x | 11777.1 | 4.71x | -0.8% | 13552 | 13552 | 3 % |  |
