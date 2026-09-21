# Paired stand, 2026-09-21 05:09

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165230.5 | 183553.9 | 1.11x | 185095.3 | 1.12x | +0.8% | 720048 | 720048 | 349 % |  |
| sql/select20.at | generated | hand | 6581.1 | 17153.9 | 2.61x | 16931.4 | 2.57x | -1.3% | 21416 | 21441 | 7 % |  |
| sql/select20.window | generated | hand | 6578.6 | 17537.0 | 2.67x | 17264.0 | 2.62x | -1.6% | 21416 | 21416 | 1 % |  |
| sql/select20.scan | generated | control | 376.9 | 365.3 | 0.97x | 371.5 | 0.99x | +1.7% | 0 | 0 | 15 % |  |
| tsql/select20.scan | generated | control | 375.5 | 371.5 | 0.99x | 371.1 | 0.99x | -0.1% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 137.9 | 133.3 | 0.97x | 133.5 | 0.97x | +0.2% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 949.6 | 1661.5 | 1.75x | 1616.4 | 1.70x | -2.7% | 1776 | 1720 | 19 % |  |
| sql/refused-late.bool | generated | hand | 2489.3 | 11508.0 | 4.62x | 5620.8 | 2.26x | -51.2% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6694.5 | 17841.5 | 2.67x | 17532.9 | 2.62x | -1.7% | 21448 | 21392 | 5 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4332343.8 | 4215000.0 | 0.97x | 4400437.5 | 1.02x | +4.4% | 6276598 | 6276555 | 9 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6167993.8 | 6108206.2 | 0.99x | 6417031.2 | 1.04x | +5.1% | 8778556 | 8778600 | 5 % |  |
| el/ladder | generated | hand | 938.7 | 1665.3 | 1.77x | 1655.1 | 1.76x | -0.6% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 938.7 | 1151.9 | 1.23x | 1115.1 | 1.19x | -3.2% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 116910.0 | 146663.9 | 1.25x | 149892.4 | 1.28x | +2.2% | 169107 | 169104 | 7 % |  |
| el/terms1000 | immediate | hand | 116910.0 | 123238.8 | 1.05x | 122313.1 | 1.05x | -0.8% | 177120 | 177120 | 7 % |  |
| sql/select20 | generated | hand | 6714.4 | 17687.2 | 2.63x | 17470.6 | 2.60x | -1.2% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2505.9 | 11503.9 | 4.59x | 11882.2 | 4.74x | +3.3% | 13552 | 13552 | 11 % |  |
