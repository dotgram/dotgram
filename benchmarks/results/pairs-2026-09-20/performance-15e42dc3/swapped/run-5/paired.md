# Paired stand, 2026-09-21 07:18

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 168459.4 | 183565.6 | 1.09x | 183429.7 | 1.09x | -0.1% | 720048 | 720048 | 49 % |  |
| sql/select20.at | generated | hand | 6734.3 | 17072.4 | 2.54x | 17088.7 | 2.54x | +0.1% | 21416 | 21416 | 7 % |  |
| sql/select20.window | generated | hand | 6688.7 | 17480.9 | 2.61x | 17551.2 | 2.62x | +0.4% | 21416 | 21416 | 7 % |  |
| sql/select20.scan | generated | control | 363.1 | 370.0 | 1.02x | 370.0 | 1.02x | 0.0% | 0 | 0 | 11 % |  |
| tsql/select20.scan | generated | control | 398.3 | 369.8 | 0.93x | 374.6 | 0.94x | +1.3% | 0 | 0 | 25 % |  |
| el/ladder.scan | generated | control | 133.0 | 134.0 | 1.01x | 134.3 | 1.01x | +0.2% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 931.6 | 1680.2 | 1.80x | 1634.8 | 1.75x | -2.7% | 1776 | 1720 | 8 % |  |
| sql/refused-late.bool | generated | hand | 2518.0 | 11659.8 | 4.63x | 5611.6 | 2.23x | -51.9% | 13552 | 6616 | 21 % |  |
| sql/select20.bool | generated | hand | 6750.6 | 17468.7 | 2.59x | 17699.4 | 2.62x | +1.3% | 21448 | 21392 | 49 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4723993.8 | 4381387.5 | 0.93x | 4423743.8 | 0.94x | +1.0% | 6276660 | 6276660 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6414300.0 | 6315668.8 | 0.98x | 6231975.0 | 0.97x | -1.3% | 8778600 | 8778619 | 17 % |  |
| el/ladder | generated | hand | 948.4 | 1686.2 | 1.78x | 1665.0 | 1.76x | -1.3% | 1776 | 1776 | 14 % |  |
| el/ladder | immediate | hand | 948.4 | 1184.0 | 1.25x | 1161.9 | 1.23x | -1.9% | 1784 | 1784 | 14 % |  |
| el/terms1000 | generated | hand | 108169.4 | 148367.7 | 1.37x | 144809.2 | 1.34x | -2.4% | 169104 | 169128 | 2 % |  |
| el/terms1000 | immediate | hand | 108169.4 | 120531.2 | 1.11x | 123680.6 | 1.14x | +2.6% | 177123 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6712.6 | 17327.1 | 2.58x | 17455.8 | 2.60x | +0.7% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2511.0 | 11688.8 | 4.66x | 11784.7 | 4.69x | +0.8% | 13552 | 13552 | 3 % |  |
