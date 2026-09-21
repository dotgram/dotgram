# Paired stand, 2026-09-21 05:18

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165872.7 | 182364.1 | 1.10x | 187814.1 | 1.13x | +3.0% | 720048 | 720048 | 348 % |  |
| sql/select20.at | generated | hand | 6659.5 | 17542.7 | 2.63x | 18025.4 | 2.71x | +2.8% | 21416 | 21416 | 13 % |  |
| sql/select20.window | generated | hand | 6588.8 | 17916.4 | 2.72x | 18398.6 | 2.79x | +2.7% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 367.9 | 369.4 | 1.00x | 370.1 | 1.01x | +0.2% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 368.6 | 370.4 | 1.00x | 370.9 | 1.01x | +0.1% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 138.2 | 134.2 | 0.97x | 134.2 | 0.97x | 0.0% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 939.8 | 1648.2 | 1.75x | 1655.7 | 1.76x | +0.5% | 1776 | 1720 | 22 % |  |
| sql/refused-late.bool | generated | hand | 2493.2 | 11906.0 | 4.78x | 5753.3 | 2.31x | -51.7% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6661.1 | 17942.4 | 2.69x | 18368.6 | 2.76x | +2.4% | 21448 | 21392 | 23 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4255025.0 | 4435475.0 | 1.04x | 4247025.0 | 1.00x | -4.2% | 6276688 | 6276664 | 31 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6191431.2 | 6423525.0 | 1.04x | 6372725.0 | 1.03x | -0.8% | 8778556 | 8778595 | 11 % |  |
| el/ladder | generated | hand | 938.2 | 1650.0 | 1.76x | 1685.5 | 1.80x | +2.2% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 938.2 | 1122.6 | 1.20x | 1112.6 | 1.19x | -0.9% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 107464.4 | 148941.2 | 1.39x | 147555.0 | 1.37x | -0.9% | 169104 | 169104 | 22 % |  |
| el/terms1000 | immediate | hand | 107464.4 | 123117.1 | 1.15x | 120651.2 | 1.12x | -2.0% | 177120 | 177120 | 22 % |  |
| sql/select20 | generated | hand | 6654.0 | 17976.4 | 2.70x | 18465.7 | 2.78x | +2.7% | 21448 | 21448 | 11 % |  |
| sql/refused-late | generated | hand | 2504.9 | 11914.1 | 4.76x | 11979.2 | 4.78x | +0.5% | 13552 | 13552 | 9 % |  |
