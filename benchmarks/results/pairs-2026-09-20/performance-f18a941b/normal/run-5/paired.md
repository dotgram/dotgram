# Paired stand, 2026-09-21 05:15

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 162886.7 | 180835.2 | 1.11x | 182611.7 | 1.12x | +1.0% | 720048 | 720048 | 359 % |  |
| sql/select20.at | generated | hand | 6607.2 | 18094.9 | 2.74x | 17953.7 | 2.72x | -0.8% | 21416 | 21441 | 14 % |  |
| sql/select20.window | generated | hand | 6671.4 | 17251.2 | 2.59x | 17174.9 | 2.57x | -0.4% | 21416 | 21416 | 10 % |  |
| sql/select20.scan | generated | control | 365.4 | 368.0 | 1.01x | 368.8 | 1.01x | +0.2% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 397.5 | 396.4 | 1.00x | 370.0 | 0.93x | -6.6% | 0 | 0 | 10 % |  |
| el/ladder.scan | generated | control | 133.9 | 133.9 | 1.00x | 134.0 | 1.00x | +0.1% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 924.9 | 1706.5 | 1.85x | 1627.9 | 1.76x | -4.6% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2487.0 | 11609.7 | 4.67x | 5603.6 | 2.25x | -51.7% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6661.2 | 17519.9 | 2.63x | 17485.9 | 2.63x | -0.2% | 21448 | 21392 | 19 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4469200.0 | 4393687.5 | 0.98x | 4367937.5 | 0.98x | -0.6% | 6276684 | 6276684 | 10 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6456425.0 | 6263293.8 | 0.97x | 6348612.5 | 0.98x | +1.4% | 8778600 | 8778600 | 2 % |  |
| el/ladder | generated | hand | 919.4 | 1702.1 | 1.85x | 1664.8 | 1.81x | -2.2% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 919.4 | 1151.4 | 1.25x | 1122.5 | 1.22x | -2.5% | 1784 | 1784 | 1 % |  |
| el/terms1000 | generated | hand | 107185.1 | 147743.7 | 1.38x | 147328.5 | 1.37x | -0.3% | 169107 | 169104 | 3 % |  |
| el/terms1000 | immediate | hand | 107185.1 | 122519.2 | 1.14x | 120905.9 | 1.13x | -1.3% | 177120 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6603.4 | 17257.5 | 2.61x | 17195.7 | 2.60x | -0.4% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2484.5 | 11626.3 | 4.68x | 11671.2 | 4.70x | +0.4% | 13552 | 13552 | 6 % |  |
