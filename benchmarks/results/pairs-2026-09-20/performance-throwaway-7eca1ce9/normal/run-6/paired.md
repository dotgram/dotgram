# Paired stand, 2026-09-21 03:11

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6691.6 | 16968.9 | 2.54x | 17289.9 | 2.58x | +1.9% | 21416 | 21460 | 432 % |  |
| sql/select20.window | generated | hand | 6681.3 | 17573.7 | 2.63x | 17636.8 | 2.64x | +0.4% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 373.7 | 372.3 | 1.00x | 367.0 | 0.98x | -1.4% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 370.5 | 367.9 | 0.99x | 384.5 | 1.04x | +4.5% | 0 | 0 | 13 % |  |
| el/ladder.scan | generated | control | 137.8 | 134.1 | 0.97x | 135.2 | 0.98x | +0.8% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 942.6 | 1632.6 | 1.73x | 1615.4 | 1.71x | -1.1% | 1776 | 1720 | 16 % |  |
| sql/refused-late.bool | generated | hand | 2493.9 | 11879.5 | 4.76x | 5655.9 | 2.27x | -52.4% | 13552 | 6616 | 10 % |  |
| sql/select20.bool | generated | hand | 6683.8 | 17379.0 | 2.60x | 17683.1 | 2.65x | +1.7% | 21448 | 21392 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6495750.0 | 6332800.0 | 0.97x | 6285400.0 | 0.97x | -0.7% | 8778619 | 8778360 | 48 % |  |
| el/ladder | generated | hand | 941.8 | 1634.7 | 1.74x | 1633.5 | 1.73x | -0.1% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 941.8 | 1127.9 | 1.20x | 1131.3 | 1.20x | +0.3% | 1784 | 1784 | 1 % |  |
| sql/select20 | generated | hand | 6724.1 | 17457.0 | 2.60x | 17523.2 | 2.61x | +0.4% | 21448 | 21448 | 7 % |  |
| sql/refused-late | generated | hand | 2510.6 | 11936.4 | 4.75x | 11824.9 | 4.71x | -0.9% | 13552 | 13552 | 4 % |  |
