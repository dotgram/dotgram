# Paired stand, 2026-09-21 03:39

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6659.0 | 17144.7 | 2.57x | 17163.5 | 2.58x | +0.1% | 21416 | 21416 | 445 % |  |
| sql/select20.window | generated | hand | 6615.6 | 17387.4 | 2.63x | 17691.7 | 2.67x | +1.7% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 369.4 | 371.8 | 1.01x | 368.1 | 1.00x | -1.0% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 369.1 | 369.3 | 1.00x | 372.0 | 1.01x | +0.7% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 139.2 | 134.0 | 0.96x | 134.9 | 0.97x | +0.7% | 0 | 0 | 14 % |  |
| el/ladder.bool | generated | hand | 953.8 | 1664.4 | 1.74x | 1616.1 | 1.69x | -2.9% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2500.4 | 11832.1 | 4.73x | 5758.3 | 2.30x | -51.3% | 13552 | 6616 | 27 % |  |
| sql/select20.bool | generated | hand | 6679.7 | 17566.0 | 2.63x | 17793.2 | 2.66x | +1.3% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6355325.0 | 6319725.0 | 0.99x | 6670187.5 | 1.05x | +5.5% | 8778600 | 8778600 | 12 % |  |
| el/ladder | generated | hand | 943.9 | 1641.5 | 1.74x | 1628.1 | 1.72x | -0.8% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 943.9 | 1120.6 | 1.19x | 1122.1 | 1.19x | +0.1% | 1784 | 1784 | 1 % |  |
| sql/select20 | generated | hand | 6719.5 | 17631.3 | 2.62x | 17849.7 | 2.66x | +1.2% | 21448 | 21472 | 5 % |  |
| sql/refused-late | generated | hand | 2519.7 | 11867.3 | 4.71x | 11893.7 | 4.72x | +0.2% | 13552 | 13552 | 3 % |  |
