# Paired stand, 2026-09-21 03:16

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6752.3 | 16998.9 | 2.52x | 17074.6 | 2.53x | +0.4% | 21416 | 21441 | 434 % |  |
| sql/select20.window | generated | hand | 6664.7 | 17457.3 | 2.62x | 17665.5 | 2.65x | +1.2% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 370.6 | 372.1 | 1.00x | 366.8 | 0.99x | -1.4% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 373.4 | 374.0 | 1.00x | 370.8 | 0.99x | -0.8% | 0 | 0 | 7 % |  |
| el/ladder.scan | generated | control | 135.6 | 135.7 | 1.00x | 134.2 | 0.99x | -1.1% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 957.9 | 1635.3 | 1.71x | 1617.7 | 1.69x | -1.1% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2503.7 | 11671.9 | 4.66x | 5651.4 | 2.26x | -51.6% | 13552 | 6616 | 25 % |  |
| sql/select20.bool | generated | hand | 6675.0 | 17385.8 | 2.60x | 17536.5 | 2.63x | +0.9% | 21448 | 21392 | 1 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6126050.0 | 6273900.0 | 1.02x | 6268250.0 | 1.02x | -0.1% | 8778619 | 8778556 | 40 % |  |
| el/ladder | generated | hand | 956.5 | 1641.3 | 1.72x | 1661.4 | 1.74x | +1.2% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 956.5 | 1121.9 | 1.17x | 1116.6 | 1.17x | -0.5% | 1784 | 1784 | 2 % |  |
| sql/select20 | generated | hand | 6708.4 | 17286.7 | 2.58x | 17498.1 | 2.61x | +1.2% | 21448 | 21448 | 5 % |  |
| sql/refused-late | generated | hand | 2526.9 | 11703.2 | 4.63x | 11839.2 | 4.69x | +1.2% | 13552 | 13552 | 2 % |  |
