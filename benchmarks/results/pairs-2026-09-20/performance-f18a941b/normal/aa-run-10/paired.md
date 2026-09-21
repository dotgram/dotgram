# Paired stand, 2026-09-21 05:29

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 168548.4 | 179898.4 | 1.07x | 179037.5 | 1.06x | -0.5% | 720048 | 720048 | 8 % |  |
| sql/select20.at | generated | hand | 6758.9 | 17052.1 | 2.52x | 17135.0 | 2.54x | +0.5% | 21416 | 21416 | 13 % |  |
| sql/select20.window | generated | hand | 6681.0 | 17413.5 | 2.61x | 17472.5 | 2.62x | +0.3% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 364.9 | 368.8 | 1.01x | 366.9 | 1.01x | -0.5% | 0 | 0 | 10 % |  |
| tsql/select20.scan | generated | control | 365.9 | 364.8 | 1.00x | 366.3 | 1.00x | +0.4% | 0 | 0 | 13 % |  |
| el/ladder.scan | generated | control | 133.7 | 134.3 | 1.00x | 134.0 | 1.00x | -0.2% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 967.8 | 1692.1 | 1.75x | 1679.1 | 1.73x | -0.8% | 1776 | 1720 | 10 % |  |
| sql/refused-late.bool | generated | hand | 2528.2 | 11817.0 | 4.67x | 5663.1 | 2.24x | -52.1% | 13552 | 6616 | 9 % |  |
| sql/select20.bool | generated | hand | 6694.8 | 17429.2 | 2.60x | 17499.0 | 2.61x | +0.4% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4171350.0 | 4527000.0 | 1.09x | 4283700.0 | 1.03x | -5.4% | 6276688 | 6276688 | 20 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6226937.5 | 6272968.8 | 1.01x | 6346462.5 | 1.02x | +1.2% | 8778556 | 8778595 | 7 % |  |
| el/ladder | generated | hand | 961.0 | 1697.2 | 1.77x | 1712.8 | 1.78x | +0.9% | 1776 | 1776 | 6 % |  |
| el/ladder | immediate | hand | 961.0 | 1126.2 | 1.17x | 1121.3 | 1.17x | -0.4% | 1784 | 1784 | 6 % |  |
| el/terms1000 | generated | hand | 108515.1 | 150177.9 | 1.38x | 145751.1 | 1.34x | -2.9% | 169150 | 169128 | 2 % |  |
| el/terms1000 | immediate | hand | 108515.1 | 121809.0 | 1.12x | 117691.3 | 1.08x | -3.4% | 177120 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6728.6 | 17469.6 | 2.60x | 17512.7 | 2.60x | +0.2% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2519.8 | 11922.0 | 4.73x | 11894.6 | 4.72x | -0.2% | 13552 | 13552 | 9 % |  |
