# Paired stand, 2026-09-21 02:58

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6747.4 | 17168.8 | 2.54x | 17522.0 | 2.60x | +2.1% | 21416 | 21416 | 456 % |  |
| sql/select20.window | generated | hand | 6729.9 | 17337.8 | 2.58x | 18064.7 | 2.68x | +4.2% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 371.1 | 375.6 | 1.01x | 371.9 | 1.00x | -1.0% | 0 | 0 | 8 % |  |
| tsql/select20.scan | generated | control | 368.8 | 367.2 | 1.00x | 383.4 | 1.04x | +4.4% | 0 | 0 | 1 % |  |
| el/ladder.scan | generated | control | 134.7 | 135.5 | 1.01x | 133.3 | 0.99x | -1.6% | 0 | 0 | 13 % |  |
| el/ladder.bool | generated | hand | 932.8 | 1625.9 | 1.74x | 1612.2 | 1.73x | -0.8% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2507.6 | 11909.2 | 4.75x | 5777.3 | 2.30x | -51.5% | 13552 | 6616 | 12 % |  |
| sql/select20.bool | generated | hand | 6686.8 | 17321.2 | 2.59x | 17771.5 | 2.66x | +2.6% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6113950.0 | 6292100.0 | 1.03x | 6472900.0 | 1.06x | +2.9% | 8778600 | 8778600 | 73 % |  |
| el/ladder | generated | hand | 935.3 | 1624.7 | 1.74x | 1641.5 | 1.76x | +1.0% | 1776 | 1776 | 13 % |  |
| el/ladder | immediate | hand | 935.3 | 1149.0 | 1.23x | 1119.4 | 1.20x | -2.6% | 1784 | 1784 | 13 % |  |
| sql/select20 | generated | hand | 6687.7 | 17375.4 | 2.60x | 17914.1 | 2.68x | +3.1% | 21448 | 21448 | 12 % |  |
| sql/refused-late | generated | hand | 2519.8 | 11826.1 | 4.69x | 11936.0 | 4.74x | +0.9% | 13552 | 13552 | 2 % |  |
