# Paired stand, 2026-09-21 03:37

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6640.7 | 17021.2 | 2.56x | 17131.2 | 2.58x | +0.6% | 21416 | 21416 | 437 % |  |
| sql/select20.window | generated | hand | 6636.4 | 17573.8 | 2.65x | 17533.8 | 2.64x | -0.2% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 374.1 | 368.1 | 0.98x | 384.1 | 1.03x | +4.4% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 372.4 | 371.7 | 1.00x | 367.4 | 0.99x | -1.2% | 0 | 0 | 7 % |  |
| el/ladder.scan | generated | control | 135.8 | 134.4 | 0.99x | 134.4 | 0.99x | 0.0% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 935.6 | 1636.7 | 1.75x | 1609.4 | 1.72x | -1.7% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2479.2 | 11669.1 | 4.71x | 5849.0 | 2.36x | -49.9% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6657.1 | 17314.2 | 2.60x | 17537.9 | 2.63x | +1.3% | 21448 | 21392 | 10 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6202987.5 | 6292587.5 | 1.01x | 6796100.0 | 1.10x | +8.0% | 8778600 | 8778619 | 13 % |  |
| el/ladder | generated | hand | 934.6 | 1632.7 | 1.75x | 1637.7 | 1.75x | +0.3% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 934.6 | 1127.3 | 1.21x | 1123.6 | 1.20x | -0.3% | 1784 | 1784 | 4 % |  |
| sql/select20 | generated | hand | 6634.1 | 17407.7 | 2.62x | 17367.1 | 2.62x | -0.2% | 21448 | 21448 | 12 % |  |
| sql/refused-late | generated | hand | 2523.0 | 11663.9 | 4.62x | 11953.0 | 4.74x | +2.5% | 13552 | 13552 | 11 % |  |
