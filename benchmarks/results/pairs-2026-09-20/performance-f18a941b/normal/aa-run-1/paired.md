# Paired stand, 2026-09-21 05:06

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166563.3 | 185428.1 | 1.11x | 185516.4 | 1.11x | 0.0% | 720048 | 720048 | 11 % |  |
| sql/select20.at | generated | hand | 6710.2 | 17213.5 | 2.57x | 17283.8 | 2.58x | +0.4% | 21416 | 21416 | 11 % |  |
| sql/select20.window | generated | hand | 6697.3 | 17801.7 | 2.66x | 17600.8 | 2.63x | -1.1% | 21416 | 21416 | 14 % |  |
| sql/select20.scan | generated | control | 368.7 | 368.4 | 1.00x | 370.0 | 1.00x | +0.4% | 0 | 0 | 10 % |  |
| tsql/select20.scan | generated | control | 381.0 | 369.4 | 0.97x | 369.0 | 0.97x | -0.1% | 0 | 0 | 15 % |  |
| el/ladder.scan | generated | control | 134.1 | 134.8 | 1.01x | 134.5 | 1.00x | -0.3% | 0 | 0 | 18 % |  |
| el/ladder.bool | generated | hand | 959.4 | 1658.0 | 1.73x | 1619.0 | 1.69x | -2.4% | 1776 | 1720 | 19 % |  |
| sql/refused-late.bool | generated | hand | 2479.9 | 11796.9 | 4.76x | 5662.1 | 2.28x | -52.0% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6650.6 | 17670.9 | 2.66x | 17704.7 | 2.66x | +0.2% | 21448 | 21392 | 11 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4416512.5 | 4408318.8 | 1.00x | 4350962.5 | 0.99x | -1.3% | 6276598 | 6276641 | 5 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6426318.8 | 6361856.2 | 0.99x | 6275981.2 | 0.98x | -1.3% | 8778600 | 8778600 | 16 % |  |
| el/ladder | generated | hand | 942.9 | 1668.6 | 1.77x | 1648.2 | 1.75x | -1.2% | 1776 | 1776 | 9 % |  |
| el/ladder | immediate | hand | 942.9 | 1166.1 | 1.24x | 1114.3 | 1.18x | -4.4% | 1784 | 1784 | 9 % |  |
| el/terms1000 | generated | hand | 109343.8 | 145559.3 | 1.33x | 147492.8 | 1.35x | +1.3% | 169104 | 169128 | 8 % |  |
| el/terms1000 | immediate | hand | 109343.8 | 118873.8 | 1.09x | 120509.0 | 1.10x | +1.4% | 177123 | 177120 | 8 % |  |
| sql/select20 | generated | hand | 6700.0 | 17644.6 | 2.63x | 17457.0 | 2.61x | -1.1% | 21448 | 21448 | 6 % |  |
| sql/refused-late | generated | hand | 2496.4 | 11697.3 | 4.69x | 11833.7 | 4.74x | +1.2% | 13552 | 13552 | 6 % |  |
