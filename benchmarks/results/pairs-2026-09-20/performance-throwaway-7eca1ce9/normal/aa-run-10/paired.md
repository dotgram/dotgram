# Paired stand, 2026-09-21 03:26

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6712.9 | 16943.7 | 2.52x | 17223.4 | 2.57x | +1.7% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6674.2 | 17189.9 | 2.58x | 17755.1 | 2.66x | +3.3% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 375.0 | 369.1 | 0.98x | 370.8 | 0.99x | +0.4% | 0 | 0 | 8 % |  |
| tsql/select20.scan | generated | control | 366.0 | 371.6 | 1.02x | 372.4 | 1.02x | +0.2% | 0 | 0 | 13 % |  |
| el/ladder.scan | generated | control | 134.4 | 134.0 | 1.00x | 134.6 | 1.00x | +0.5% | 0 | 0 | 6 % |  |
| el/ladder.bool | generated | hand | 982.6 | 1630.8 | 1.66x | 1611.2 | 1.64x | -1.2% | 1776 | 1720 | 36 % |  |
| sql/refused-late.bool | generated | hand | 2495.2 | 11540.9 | 4.63x | 5690.8 | 2.28x | -50.7% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6595.0 | 17171.0 | 2.60x | 17671.0 | 2.68x | +2.9% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6221250.0 | 6230350.0 | 1.00x | 6299750.0 | 1.01x | +1.1% | 8778622 | 8778600 | 45 % |  |
| el/ladder | generated | hand | 980.3 | 1625.8 | 1.66x | 1644.2 | 1.68x | +1.1% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 980.3 | 1127.1 | 1.15x | 1123.5 | 1.15x | -0.3% | 1784 | 1784 | 4 % |  |
| sql/select20 | generated | hand | 6643.2 | 17178.2 | 2.59x | 17533.8 | 2.64x | +2.1% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2505.2 | 11527.8 | 4.60x | 11812.3 | 4.72x | +2.5% | 13552 | 13552 | 5 % |  |
