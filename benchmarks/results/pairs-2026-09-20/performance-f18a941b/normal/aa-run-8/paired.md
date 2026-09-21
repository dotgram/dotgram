# Paired stand, 2026-09-21 05:24

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164421.9 | 182410.2 | 1.11x | 183428.1 | 1.12x | +0.6% | 720048 | 720048 | 357 % |  |
| sql/select20.at | generated | hand | 6599.3 | 17051.1 | 2.58x | 16881.8 | 2.56x | -1.0% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6615.6 | 17539.1 | 2.65x | 17278.7 | 2.61x | -1.5% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 367.8 | 368.6 | 1.00x | 368.4 | 1.00x | -0.1% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 363.7 | 374.8 | 1.03x | 368.5 | 1.01x | -1.7% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 135.8 | 134.3 | 0.99x | 133.5 | 0.98x | -0.6% | 0 | 0 | 5 % |  |
| el/ladder.bool | generated | hand | 937.8 | 1648.7 | 1.76x | 1648.9 | 1.76x | 0.0% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2499.1 | 11649.9 | 4.66x | 5565.2 | 2.23x | -52.2% | 13552 | 6616 | 8 % |  |
| sql/select20.bool | generated | hand | 6595.1 | 17499.9 | 2.65x | 17377.0 | 2.63x | -0.7% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4523693.8 | 4393212.5 | 0.97x | 4393843.8 | 0.97x | 0.0% | 6276598 | 6276598 | 8 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6326506.2 | 6251168.8 | 0.99x | 6217150.0 | 0.98x | -0.5% | 8778600 | 8778600 | 5 % |  |
| el/ladder | generated | hand | 936.4 | 1655.5 | 1.77x | 1671.5 | 1.79x | +1.0% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 936.4 | 1122.3 | 1.20x | 1110.7 | 1.19x | -1.0% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 108363.6 | 146348.5 | 1.35x | 149739.4 | 1.38x | +2.3% | 169104 | 169107 | 15 % |  |
| el/terms1000 | immediate | hand | 108363.6 | 120043.2 | 1.11x | 121789.1 | 1.12x | +1.5% | 177120 | 177120 | 15 % |  |
| sql/select20 | generated | hand | 6638.9 | 17540.2 | 2.64x | 17350.0 | 2.61x | -1.1% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2487.5 | 11677.2 | 4.69x | 11678.8 | 4.70x | 0.0% | 13552 | 13552 | 7 % |  |
