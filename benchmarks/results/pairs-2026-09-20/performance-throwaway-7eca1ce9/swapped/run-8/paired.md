# Paired stand, 2026-09-21 03:53

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6750.0 | 17052.7 | 2.53x | 17050.1 | 2.53x | 0.0% | 21416 | 21441 | 432 % |  |
| sql/select20.window | generated | hand | 6681.7 | 17440.6 | 2.61x | 17442.1 | 2.61x | 0.0% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 370.9 | 370.1 | 1.00x | 367.8 | 0.99x | -0.6% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 368.5 | 367.7 | 1.00x | 375.4 | 1.02x | +2.1% | 0 | 0 | 11 % |  |
| el/ladder.scan | generated | control | 136.4 | 137.9 | 1.01x | 134.3 | 0.98x | -2.6% | 0 | 0 | 15 % |  |
| el/ladder.bool | generated | hand | 943.5 | 1729.0 | 1.83x | 1713.5 | 1.82x | -0.9% | 1776 | 1720 | 7 % |  |
| sql/refused-late.bool | generated | hand | 2523.8 | 11906.1 | 4.72x | 5683.9 | 2.25x | -52.3% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6686.1 | 17326.7 | 2.59x | 17526.4 | 2.62x | +1.2% | 21448 | 21392 | 10 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6225350.0 | 6182550.0 | 0.99x | 6275300.0 | 1.01x | +1.5% | 8778595 | 8778556 | 43 % |  |
| el/ladder | generated | hand | 929.3 | 1695.0 | 1.82x | 1659.1 | 1.79x | -2.1% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 929.3 | 1129.4 | 1.22x | 1121.0 | 1.21x | -0.7% | 1784 | 1784 | 2 % |  |
| sql/select20 | generated | hand | 6658.2 | 17348.9 | 2.61x | 17401.1 | 2.61x | +0.3% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2525.5 | 11949.3 | 4.73x | 11816.8 | 4.68x | -1.1% | 13552 | 13552 | 8 % |  |
