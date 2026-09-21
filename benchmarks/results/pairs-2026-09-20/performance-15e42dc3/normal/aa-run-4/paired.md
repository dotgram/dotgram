# Paired stand, 2026-09-21 06:51

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166956.2 | 182370.3 | 1.09x | 184103.1 | 1.10x | +1.0% | 720048 | 720048 | 8 % |  |
| sql/select20.at | generated | hand | 6555.0 | 17098.8 | 2.61x | 17673.6 | 2.70x | +3.4% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6567.6 | 17467.6 | 2.66x | 17667.8 | 2.69x | +1.1% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 362.5 | 366.2 | 1.01x | 367.0 | 1.01x | +0.2% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 369.3 | 382.9 | 1.04x | 369.5 | 1.00x | -3.5% | 0 | 0 | 13 % |  |
| el/ladder.scan | generated | control | 133.2 | 133.9 | 1.01x | 133.6 | 1.00x | -0.3% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 930.6 | 1640.4 | 1.76x | 1637.6 | 1.76x | -0.2% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2484.8 | 11684.5 | 4.70x | 5755.6 | 2.32x | -50.7% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6553.8 | 17519.7 | 2.67x | 18235.1 | 2.78x | +4.1% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4312275.0 | 4329887.5 | 1.00x | 4675975.0 | 1.08x | +8.0% | 6276617 | 6276555 | 14 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6361231.2 | 6250812.5 | 0.98x | 6251256.2 | 0.98x | 0.0% | 8778556 | 8778600 | 4 % |  |
| el/ladder | generated | hand | 937.2 | 1660.5 | 1.77x | 1728.1 | 1.84x | +4.1% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 937.2 | 1127.3 | 1.20x | 1118.6 | 1.19x | -0.8% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 108600.3 | 146862.2 | 1.35x | 144426.4 | 1.33x | -1.7% | 169104 | 169128 | 9 % |  |
| el/terms1000 | immediate | hand | 108600.3 | 121168.7 | 1.12x | 119961.7 | 1.10x | -1.0% | 177120 | 177123 | 9 % |  |
| sql/select20 | generated | hand | 6571.0 | 17482.2 | 2.66x | 17702.3 | 2.69x | +1.3% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2483.6 | 11707.9 | 4.71x | 11915.3 | 4.80x | +1.8% | 13552 | 13552 | 14 % |  |
