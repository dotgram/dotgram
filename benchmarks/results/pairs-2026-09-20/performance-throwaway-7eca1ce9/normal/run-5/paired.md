# Paired stand, 2026-09-21 03:07

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6722.0 | 16927.3 | 2.52x | 17118.0 | 2.55x | +1.1% | 21416 | 21416 | 433 % |  |
| sql/select20.window | generated | hand | 6662.4 | 17406.0 | 2.61x | 17394.4 | 2.61x | -0.1% | 21416 | 21416 | 1 % |  |
| sql/select20.scan | generated | control | 379.6 | 374.9 | 0.99x | 370.8 | 0.98x | -1.1% | 0 | 0 | 8 % |  |
| tsql/select20.scan | generated | control | 374.0 | 373.5 | 1.00x | 374.8 | 1.00x | +0.3% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 136.1 | 134.6 | 0.99x | 133.5 | 0.98x | -0.8% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 942.1 | 1634.1 | 1.73x | 1645.1 | 1.75x | +0.7% | 1776 | 1720 | 1 % |  |
| sql/refused-late.bool | generated | hand | 2511.7 | 11758.4 | 4.68x | 5682.0 | 2.26x | -51.7% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6691.5 | 17359.5 | 2.59x | 17369.0 | 2.60x | +0.1% | 21448 | 21392 | 11 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6526675.0 | 6695025.0 | 1.03x | 6230000.0 | 0.95x | -6.9% | 8778600 | 8778600 | 22 % |  |
| el/ladder | generated | hand | 947.0 | 1633.1 | 1.72x | 1649.5 | 1.74x | +1.0% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 947.0 | 1127.4 | 1.19x | 1132.8 | 1.20x | +0.5% | 1784 | 1784 | 3 % |  |
| sql/select20 | generated | hand | 6702.4 | 17414.6 | 2.60x | 17428.0 | 2.60x | +0.1% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2517.6 | 11712.5 | 4.65x | 11838.6 | 4.70x | +1.1% | 13552 | 13552 | 2 % |  |
