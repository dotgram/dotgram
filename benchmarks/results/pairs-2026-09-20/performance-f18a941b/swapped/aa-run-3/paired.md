# Paired stand, 2026-09-21 05:37

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166494.5 | 182872.7 | 1.10x | 183039.8 | 1.10x | +0.1% | 720048 | 720048 | 60 % |  |
| sql/select20.at | generated | hand | 6692.5 | 16949.3 | 2.53x | 17750.7 | 2.65x | +4.7% | 21416 | 21416 | 46 % |  |
| sql/select20.window | generated | hand | 6696.1 | 17447.4 | 2.61x | 17603.8 | 2.63x | +0.9% | 21416 | 21416 | 6 % |  |
| sql/select20.scan | generated | control | 363.7 | 370.6 | 1.02x | 368.7 | 1.01x | -0.5% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 372.0 | 380.9 | 1.02x | 369.1 | 0.99x | -3.1% | 0 | 0 | 24 % |  |
| el/ladder.scan | generated | control | 133.7 | 134.2 | 1.00x | 133.9 | 1.00x | -0.2% | 0 | 0 | 4 % |  |
| el/ladder.bool | generated | hand | 946.2 | 1647.4 | 1.74x | 1651.4 | 1.75x | +0.2% | 1776 | 1720 | 12 % |  |
| sql/refused-late.bool | generated | hand | 2534.1 | 11672.7 | 4.61x | 5620.8 | 2.22x | -51.8% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6644.1 | 17375.2 | 2.62x | 17926.0 | 2.70x | +3.2% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4384356.2 | 4414506.2 | 1.01x | 4382912.5 | 1.00x | -0.7% | 6276684 | 6276684 | 7 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6251337.5 | 6328850.0 | 1.01x | 6268081.2 | 1.00x | -1.0% | 8778556 | 8778595 | 7 % |  |
| el/ladder | generated | hand | 966.9 | 1665.5 | 1.72x | 1721.2 | 1.78x | +3.3% | 1776 | 1776 | 7 % |  |
| el/ladder | immediate | hand | 966.9 | 1137.2 | 1.18x | 1131.9 | 1.17x | -0.5% | 1784 | 1784 | 7 % |  |
| el/terms1000 | generated | hand | 110557.9 | 144108.1 | 1.30x | 148526.0 | 1.34x | +3.1% | 169104 | 169104 | 11 % |  |
| el/terms1000 | immediate | hand | 110557.9 | 121146.5 | 1.10x | 118979.8 | 1.08x | -1.8% | 177120 | 177123 | 11 % |  |
| sql/select20 | generated | hand | 6645.5 | 17338.7 | 2.61x | 17983.1 | 2.71x | +3.7% | 21448 | 21472 | 2 % |  |
| sql/refused-late | generated | hand | 2494.8 | 11680.2 | 4.68x | 11745.5 | 4.71x | +0.6% | 13552 | 13552 | 19 % |  |
