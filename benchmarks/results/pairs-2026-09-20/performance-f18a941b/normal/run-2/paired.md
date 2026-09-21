# Paired stand, 2026-09-21 05:07

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 167987.5 | 187901.6 | 1.12x | 183469.5 | 1.09x | -2.4% | 720048 | 720048 | 41 % |  |
| sql/select20.at | generated | hand | 6710.2 | 17166.5 | 2.56x | 17033.2 | 2.54x | -0.8% | 21416 | 21416 | 20 % |  |
| sql/select20.window | generated | hand | 6630.9 | 17421.8 | 2.63x | 17334.7 | 2.61x | -0.5% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 370.5 | 368.3 | 0.99x | 375.3 | 1.01x | +1.9% | 0 | 0 | 14 % |  |
| tsql/select20.scan | generated | control | 365.8 | 373.9 | 1.02x | 367.0 | 1.00x | -1.8% | 0 | 0 | 9 % |  |
| el/ladder.scan | generated | control | 134.0 | 134.2 | 1.00x | 134.4 | 1.00x | +0.1% | 0 | 0 | 2 % |  |
| el/ladder.bool | generated | hand | 960.7 | 1634.7 | 1.70x | 1693.2 | 1.76x | +3.6% | 1776 | 1720 | 8 % |  |
| sql/refused-late.bool | generated | hand | 2475.7 | 11772.4 | 4.76x | 5616.3 | 2.27x | -52.3% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6569.3 | 17562.8 | 2.67x | 17294.0 | 2.63x | -1.5% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4193275.0 | 4274050.0 | 1.02x | 4346000.0 | 1.04x | +1.7% | 6276664 | 6276688 | 36 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6297506.2 | 6240356.2 | 0.99x | 6257943.8 | 0.99x | +0.3% | 8778576 | 8778600 | 14 % |  |
| el/ladder | generated | hand | 960.9 | 1638.2 | 1.70x | 1679.0 | 1.75x | +2.5% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 960.9 | 1126.6 | 1.17x | 1147.6 | 1.19x | +1.9% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 109314.0 | 149015.4 | 1.36x | 146548.2 | 1.34x | -1.7% | 169104 | 169104 | 2 % |  |
| el/terms1000 | immediate | hand | 109314.0 | 119963.3 | 1.10x | 119578.7 | 1.09x | -0.3% | 177120 | 177123 | 2 % |  |
| sql/select20 | generated | hand | 6590.1 | 17600.3 | 2.67x | 17311.0 | 2.63x | -1.6% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2488.0 | 11844.5 | 4.76x | 11741.2 | 4.72x | -0.9% | 13552 | 13552 | 14 % |  |
