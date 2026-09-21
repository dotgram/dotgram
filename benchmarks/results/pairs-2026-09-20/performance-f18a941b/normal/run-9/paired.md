# Paired stand, 2026-09-21 05:25

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 163876.6 | 180573.4 | 1.10x | 189354.7 | 1.16x | +4.9% | 720048 | 720048 | 69 % |  |
| sql/select20.at | generated | hand | 6569.0 | 17055.8 | 2.60x | 17157.3 | 2.61x | +0.6% | 21416 | 21416 | 13 % |  |
| sql/select20.window | generated | hand | 6531.9 | 17288.5 | 2.65x | 17565.0 | 2.69x | +1.6% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 368.4 | 371.3 | 1.01x | 383.8 | 1.04x | +3.4% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 369.4 | 373.9 | 1.01x | 373.9 | 1.01x | 0.0% | 0 | 0 | 26 % |  |
| el/ladder.scan | generated | control | 135.6 | 138.0 | 1.02x | 134.5 | 0.99x | -2.6% | 0 | 0 | 21 % |  |
| el/ladder.bool | generated | hand | 928.5 | 1677.9 | 1.81x | 1618.1 | 1.74x | -3.6% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2481.0 | 11730.0 | 4.73x | 5656.2 | 2.28x | -51.8% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6593.1 | 17276.4 | 2.62x | 17812.5 | 2.70x | +3.1% | 21448 | 21392 | 15 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4393750.0 | 4225650.0 | 0.96x | 4272300.0 | 0.97x | +1.1% | 6276664 | 6276688 | 50 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6289587.5 | 6208325.0 | 0.99x | 6275193.8 | 1.00x | +1.1% | 8778556 | 8778600 | 4 % |  |
| el/ladder | generated | hand | 941.1 | 1697.9 | 1.80x | 1653.1 | 1.76x | -2.6% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 941.1 | 1129.6 | 1.20x | 1134.5 | 1.21x | +0.4% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 108869.3 | 149198.0 | 1.37x | 147305.3 | 1.35x | -1.3% | 169104 | 169104 | 5 % |  |
| el/terms1000 | immediate | hand | 108869.3 | 121360.9 | 1.11x | 123577.1 | 1.14x | +1.8% | 177120 | 177166 | 5 % |  |
| sql/select20 | generated | hand | 6582.1 | 17280.9 | 2.63x | 17532.8 | 2.66x | +1.5% | 21448 | 21448 | 11 % |  |
| sql/refused-late | generated | hand | 2493.2 | 11616.7 | 4.66x | 11811.7 | 4.74x | +1.7% | 13552 | 13552 | 8 % |  |
