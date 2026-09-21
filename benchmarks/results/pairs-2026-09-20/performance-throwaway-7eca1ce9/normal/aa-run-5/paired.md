# Paired stand, 2026-09-21 03:09

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6746.8 | 17385.5 | 2.58x | 17227.0 | 2.55x | -0.9% | 21416 | 21416 | 33 % |  |
| sql/select20.window | generated | hand | 6635.1 | 17356.9 | 2.62x | 18210.9 | 2.74x | +4.9% | 21416 | 21416 | 22 % |  |
| sql/select20.scan | generated | control | 373.7 | 378.8 | 1.01x | 379.2 | 1.01x | +0.1% | 0 | 0 | 12 % |  |
| tsql/select20.scan | generated | control | 374.4 | 380.7 | 1.02x | 387.3 | 1.03x | +1.7% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 138.9 | 135.7 | 0.98x | 138.5 | 1.00x | +2.1% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 930.9 | 1621.1 | 1.74x | 1619.0 | 1.74x | -0.1% | 1776 | 1720 | 2 % |  |
| sql/refused-late.bool | generated | hand | 2482.7 | 11846.5 | 4.77x | 5631.0 | 2.27x | -52.5% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6595.8 | 17447.9 | 2.65x | 17563.0 | 2.66x | +0.7% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6331750.0 | 6510050.0 | 1.03x | 6278975.0 | 0.99x | -3.5% | 8778620 | 8778600 | 25 % |  |
| el/ladder | generated | hand | 943.2 | 1643.0 | 1.74x | 1642.1 | 1.74x | -0.1% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 943.2 | 1123.4 | 1.19x | 1135.8 | 1.20x | +1.1% | 1784 | 1784 | 4 % |  |
| sql/select20 | generated | hand | 6644.6 | 17492.5 | 2.63x | 17587.6 | 2.65x | +0.5% | 21448 | 21448 | 7 % |  |
| sql/refused-late | generated | hand | 2508.9 | 11802.0 | 4.70x | 11690.9 | 4.66x | -0.9% | 13552 | 13552 | 2 % |  |
