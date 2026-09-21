# Paired stand, 2026-09-21 07:16

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166932.0 | 183497.7 | 1.10x | 184146.1 | 1.10x | +0.4% | 720048 | 720048 | 28 % |  |
| sql/select20.at | generated | hand | 6587.0 | 17160.9 | 2.61x | 17424.4 | 2.65x | +1.5% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6632.3 | 17502.9 | 2.64x | 17783.2 | 2.68x | +1.6% | 21416 | 21416 | 12 % |  |
| sql/select20.scan | generated | control | 368.5 | 370.5 | 1.01x | 368.5 | 1.00x | -0.5% | 0 | 0 | 7 % |  |
| tsql/select20.scan | generated | control | 394.0 | 366.9 | 0.93x | 364.3 | 0.92x | -0.7% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 135.3 | 133.4 | 0.99x | 133.0 | 0.98x | -0.3% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 934.3 | 1654.6 | 1.77x | 1640.9 | 1.76x | -0.8% | 1776 | 1720 | 51 % |  |
| sql/refused-late.bool | generated | hand | 2520.3 | 11764.8 | 4.67x | 5791.0 | 2.30x | -50.8% | 13552 | 6616 | 19 % |  |
| sql/select20.bool | generated | hand | 6664.0 | 18217.7 | 2.73x | 17595.8 | 2.64x | -3.4% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4378418.8 | 4668743.8 | 1.07x | 4380475.0 | 1.00x | -6.2% | 6276617 | 6276684 | 12 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6472443.8 | 6382556.2 | 0.99x | 6396756.2 | 0.99x | +0.2% | 8778600 | 8778600 | 3 % |  |
| el/ladder | generated | hand | 933.6 | 1652.2 | 1.77x | 1683.1 | 1.80x | +1.9% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 933.6 | 1142.5 | 1.22x | 1291.4 | 1.38x | +13.0% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 105900.3 | 145715.7 | 1.38x | 147654.0 | 1.39x | +1.3% | 169104 | 169128 | 4 % |  |
| el/terms1000 | immediate | hand | 105900.3 | 119408.1 | 1.13x | 160802.1 | 1.52x | +34.7% | 177123 | 177120 | 4 % |  |
| sql/select20 | generated | hand | 6753.3 | 18149.6 | 2.69x | 17641.2 | 2.61x | -2.8% | 21448 | 21472 | 3 % |  |
| sql/refused-late | generated | hand | 2525.4 | 11765.0 | 4.66x | 12057.8 | 4.77x | +2.5% | 13552 | 13552 | 15 % |  |
