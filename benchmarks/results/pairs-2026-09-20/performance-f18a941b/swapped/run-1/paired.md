# Paired stand, 2026-09-21 05:30

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 163677.3 | 180943.8 | 1.11x | 182256.2 | 1.11x | +0.7% | 720048 | 720048 | 42 % |  |
| sql/select20.at | generated | hand | 6696.0 | 17759.4 | 2.65x | 17029.5 | 2.54x | -4.1% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6718.9 | 18631.8 | 2.77x | 17450.8 | 2.60x | -6.3% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 375.2 | 376.8 | 1.00x | 372.4 | 0.99x | -1.2% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 368.3 | 371.3 | 1.01x | 373.2 | 1.01x | +0.5% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 136.3 | 136.2 | 1.00x | 135.7 | 1.00x | -0.3% | 0 | 0 | 5 % |  |
| el/ladder.bool | generated | hand | 935.3 | 1671.2 | 1.79x | 1668.6 | 1.78x | -0.2% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2501.2 | 11985.7 | 4.79x | 5614.8 | 2.24x | -53.2% | 13552 | 6616 | 1 % |  |
| sql/select20.bool | generated | hand | 6677.0 | 18026.4 | 2.70x | 17460.1 | 2.61x | -3.1% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4152450.0 | 4341150.0 | 1.05x | 4342400.0 | 1.05x | 0.0% | 6276688 | 6276684 | 17 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6325656.2 | 6410950.0 | 1.01x | 6551656.2 | 1.04x | +2.2% | 8778556 | 8778595 | 10 % |  |
| el/ladder | generated | hand | 940.0 | 1691.2 | 1.80x | 1677.3 | 1.78x | -0.8% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 940.0 | 1149.0 | 1.22x | 1131.1 | 1.20x | -1.6% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 106119.4 | 147597.3 | 1.39x | 145158.3 | 1.37x | -1.7% | 169104 | 169128 | 14 % |  |
| el/terms1000 | immediate | hand | 106119.4 | 120349.3 | 1.13x | 119769.7 | 1.13x | -0.5% | 177120 | 177123 | 14 % |  |
| sql/select20 | generated | hand | 6657.5 | 17993.1 | 2.70x | 17337.1 | 2.60x | -3.6% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2504.2 | 11914.9 | 4.76x | 11849.4 | 4.73x | -0.5% | 13552 | 13552 | 3 % |  |
