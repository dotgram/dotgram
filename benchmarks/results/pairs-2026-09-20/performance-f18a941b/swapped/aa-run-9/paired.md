# Paired stand, 2026-09-21 05:52

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164559.4 | 177785.9 | 1.08x | 177540.6 | 1.08x | -0.1% | 720048 | 720048 | 198 % |  |
| sql/select20.at | generated | hand | 6603.7 | 16918.6 | 2.56x | 16970.4 | 2.57x | +0.3% | 21416 | 21416 | 21 % |  |
| sql/select20.window | generated | hand | 6535.0 | 17280.1 | 2.64x | 17261.2 | 2.64x | -0.1% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 374.1 | 372.7 | 1.00x | 369.8 | 0.99x | -0.8% | 0 | 0 | 7 % |  |
| tsql/select20.scan | generated | control | 378.4 | 370.7 | 0.98x | 368.8 | 0.97x | -0.5% | 0 | 0 | 14 % |  |
| el/ladder.scan | generated | control | 133.9 | 133.8 | 1.00x | 134.2 | 1.00x | +0.3% | 0 | 0 | 3 % |  |
| el/ladder.bool | generated | hand | 943.5 | 1674.4 | 1.77x | 1680.9 | 1.78x | +0.4% | 1776 | 1720 | 63 % |  |
| sql/refused-late.bool | generated | hand | 2473.1 | 11716.0 | 4.74x | 5629.0 | 2.28x | -52.0% | 13552 | 6616 | 1 % |  |
| sql/select20.bool | generated | hand | 6538.8 | 17580.8 | 2.69x | 17385.5 | 2.66x | -1.1% | 21448 | 21392 | 14 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4424375.0 | 4343693.8 | 0.98x | 4358968.8 | 0.99x | +0.4% | 6276531 | 6276660 | 10 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6274481.2 | 6242643.8 | 0.99x | 6222700.0 | 0.99x | -0.3% | 8778556 | 8778600 | 9 % |  |
| el/ladder | generated | hand | 945.7 | 1683.9 | 1.78x | 1704.0 | 1.80x | +1.2% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 945.7 | 1117.6 | 1.18x | 1133.6 | 1.20x | +1.4% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 107484.3 | 148554.0 | 1.38x | 151062.6 | 1.41x | +1.7% | 169104 | 169104 | 10 % |  |
| el/terms1000 | immediate | hand | 107484.3 | 120316.6 | 1.12x | 121871.6 | 1.13x | +1.3% | 177123 | 177120 | 10 % |  |
| sql/select20 | generated | hand | 6546.9 | 17488.8 | 2.67x | 17300.2 | 2.64x | -1.1% | 21448 | 21448 | 19 % |  |
| sql/refused-late | generated | hand | 2473.3 | 11728.3 | 4.74x | 11780.9 | 4.76x | +0.4% | 13552 | 13552 | 2 % |  |
