# Paired stand, 2026-09-21 03:30

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.4 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6650.5 | 17225.2 | 2.59x | 16816.1 | 2.53x | -2.4% | 21416 | 21441 | 445 % |  |
| sql/select20.window | generated | hand | 6590.8 | 17776.2 | 2.70x | 17254.9 | 2.62x | -2.9% | 21416 | 21416 | 14 % |  |
| sql/select20.scan | generated | control | 366.1 | 373.3 | 1.02x | 374.7 | 1.02x | +0.4% | 0 | 0 | 15 % |  |
| tsql/select20.scan | generated | control | 373.4 | 385.8 | 1.03x | 374.0 | 1.00x | -3.1% | 0 | 0 | 17 % |  |
| el/ladder.scan | generated | control | 136.7 | 135.7 | 0.99x | 139.9 | 1.02x | +3.1% | 0 | 0 | 17 % |  |
| el/ladder.bool | generated | hand | 945.1 | 1670.2 | 1.77x | 1627.3 | 1.72x | -2.6% | 1776 | 1720 | 2 % |  |
| sql/refused-late.bool | generated | hand | 2493.6 | 11750.9 | 4.71x | 5605.0 | 2.25x | -52.3% | 13552 | 6616 | 15 % |  |
| sql/select20.bool | generated | hand | 6579.1 | 17614.5 | 2.68x | 17205.5 | 2.62x | -2.3% | 21448 | 21392 | 5 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6286962.5 | 6496450.0 | 1.03x | 6273675.0 | 1.00x | -3.4% | 8778619 | 8778600 | 14 % |  |
| el/ladder | generated | hand | 945.5 | 1667.7 | 1.76x | 1641.3 | 1.74x | -1.6% | 1776 | 1776 | 15 % |  |
| el/ladder | immediate | hand | 945.5 | 1190.0 | 1.26x | 1163.6 | 1.23x | -2.2% | 1784 | 1784 | 15 % |  |
| sql/select20 | generated | hand | 6668.1 | 17573.4 | 2.64x | 17342.2 | 2.60x | -1.3% | 21448 | 21448 | 7 % |  |
| sql/refused-late | generated | hand | 2490.7 | 11774.7 | 4.73x | 11692.9 | 4.69x | -0.7% | 13552 | 13552 | 2 % |  |
