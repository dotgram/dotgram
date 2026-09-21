# Paired stand, 2026-09-21 03:43

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6867.9 | 17109.8 | 2.49x | 17240.1 | 2.51x | +0.8% | 21416 | 21416 | 373 % |  |
| sql/select20.window | generated | hand | 6821.4 | 17454.8 | 2.56x | 17463.2 | 2.56x | 0.0% | 21416 | 21416 | 10 % |  |
| sql/select20.scan | generated | control | 372.5 | 376.0 | 1.01x | 366.0 | 0.98x | -2.7% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 388.8 | 369.7 | 0.95x | 369.9 | 0.95x | +0.1% | 0 | 0 | 6 % |  |
| el/ladder.scan | generated | control | 133.8 | 133.8 | 1.00x | 134.5 | 1.01x | +0.5% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 939.5 | 1727.3 | 1.84x | 1644.3 | 1.75x | -4.8% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2562.8 | 11758.9 | 4.59x | 5680.9 | 2.22x | -51.7% | 13552 | 6616 | 8 % |  |
| sql/select20.bool | generated | hand | 6867.4 | 17402.9 | 2.53x | 17705.6 | 2.58x | +1.7% | 21448 | 21392 | 8 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6390200.0 | 6351550.0 | 0.99x | 6302400.0 | 0.99x | -0.8% | 8778595 | 8778600 | 45 % |  |
| el/ladder | generated | hand | 939.9 | 1728.1 | 1.84x | 1675.1 | 1.78x | -3.1% | 1776 | 1776 | 14 % |  |
| el/ladder | immediate | hand | 939.9 | 1142.6 | 1.22x | 1145.2 | 1.22x | +0.2% | 1784 | 1784 | 14 % |  |
| sql/select20 | generated | hand | 6871.9 | 17290.5 | 2.52x | 17849.9 | 2.60x | +3.2% | 21448 | 21472 | 16 % |  |
| sql/refused-late | generated | hand | 2577.2 | 11760.8 | 4.56x | 11945.6 | 4.64x | +1.6% | 13552 | 13552 | 8 % |  |
