# Paired stand, 2026-09-21 05:05

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 163111.7 | 179867.2 | 1.10x | 186677.3 | 1.14x | +3.8% | 720048 | 720048 | 365 % |  |
| sql/select20.at | generated | hand | 6682.0 | 16965.2 | 2.54x | 16946.1 | 2.54x | -0.1% | 21416 | 21416 | 30 % |  |
| sql/select20.window | generated | hand | 6592.4 | 17595.6 | 2.67x | 17295.9 | 2.62x | -1.7% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 362.9 | 367.8 | 1.01x | 369.2 | 1.02x | +0.4% | 0 | 0 | 28 % |  |
| tsql/select20.scan | generated | control | 372.3 | 393.2 | 1.06x | 374.8 | 1.01x | -4.7% | 0 | 0 | 8 % |  |
| el/ladder.scan | generated | control | 134.0 | 134.2 | 1.00x | 134.6 | 1.00x | +0.4% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 942.6 | 1642.0 | 1.74x | 1625.3 | 1.72x | -1.0% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2492.4 | 11773.2 | 4.72x | 5675.4 | 2.28x | -51.8% | 13552 | 6616 | 16 % |  |
| sql/select20.bool | generated | hand | 6650.8 | 17398.1 | 2.62x | 17487.1 | 2.63x | +0.5% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4591431.2 | 4347481.2 | 0.95x | 4279343.8 | 0.93x | -1.6% | 6276684 | 6276684 | 9 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6330993.8 | 6407168.8 | 1.01x | 6290143.8 | 0.99x | -1.8% | 8778600 | 8778600 | 18 % |  |
| el/ladder | generated | hand | 949.2 | 1663.6 | 1.75x | 1651.4 | 1.74x | -0.7% | 1776 | 1776 | 13 % |  |
| el/ladder | immediate | hand | 949.2 | 1125.3 | 1.19x | 1128.5 | 1.19x | +0.3% | 1784 | 1784 | 13 % |  |
| el/terms1000 | generated | hand | 108068.6 | 147989.0 | 1.37x | 150721.4 | 1.39x | +1.8% | 169104 | 169128 | 17 % |  |
| el/terms1000 | immediate | hand | 108068.6 | 125419.9 | 1.16x | 123523.8 | 1.14x | -1.5% | 177120 | 177120 | 17 % |  |
| sql/select20 | generated | hand | 6645.1 | 17420.8 | 2.62x | 17355.8 | 2.61x | -0.4% | 21448 | 21448 | 6 % |  |
| sql/refused-late | generated | hand | 2510.6 | 11690.0 | 4.66x | 11619.0 | 4.63x | -0.6% | 13552 | 13552 | 12 % |  |
