# Paired stand, 2026-09-21 03:00

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6747.5 | 17858.8 | 2.65x | 17083.2 | 2.53x | -4.3% | 21416 | 21441 | 433 % |  |
| sql/select20.window | generated | hand | 6637.7 | 18380.6 | 2.77x | 17355.4 | 2.61x | -5.6% | 21416 | 21416 | 12 % |  |
| sql/select20.scan | generated | control | 365.4 | 374.7 | 1.03x | 373.2 | 1.02x | -0.4% | 0 | 0 | 16 % |  |
| tsql/select20.scan | generated | control | 368.2 | 376.9 | 1.02x | 372.5 | 1.01x | -1.2% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 135.6 | 139.5 | 1.03x | 136.5 | 1.01x | -2.1% | 0 | 0 | 17 % |  |
| el/ladder.bool | generated | hand | 950.5 | 1642.0 | 1.73x | 1592.7 | 1.68x | -3.0% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2491.2 | 12013.5 | 4.82x | 5583.4 | 2.24x | -53.5% | 13552 | 6616 | 10 % |  |
| sql/select20.bool | generated | hand | 6609.5 | 18349.7 | 2.78x | 17366.5 | 2.63x | -5.4% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6407750.0 | 6187450.0 | 0.97x | 6185400.0 | 0.97x | 0.0% | 8778619 | 8778556 | 40 % |  |
| el/ladder | generated | hand | 951.9 | 1643.6 | 1.73x | 1637.2 | 1.72x | -0.4% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 951.9 | 1140.1 | 1.20x | 1151.9 | 1.21x | +1.0% | 1784 | 1784 | 2 % |  |
| sql/select20 | generated | hand | 6626.2 | 18410.3 | 2.78x | 17365.9 | 2.62x | -5.7% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2507.3 | 12022.9 | 4.80x | 11718.0 | 4.67x | -2.5% | 13552 | 13552 | 7 % |  |
