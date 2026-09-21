# Paired stand, 2026-09-21 07:09

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165972.7 | 184095.3 | 1.11x | 183939.1 | 1.11x | -0.1% | 720048 | 720048 | 15 % |  |
| sql/select20.at | generated | hand | 6692.7 | 17295.4 | 2.58x | 17270.6 | 2.58x | -0.1% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6718.1 | 17534.5 | 2.61x | 17526.4 | 2.61x | 0.0% | 21416 | 21416 | 10 % |  |
| sql/select20.scan | generated | control | 367.7 | 370.9 | 1.01x | 369.6 | 1.00x | -0.4% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 367.8 | 380.9 | 1.04x | 371.7 | 1.01x | -2.4% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 136.0 | 135.6 | 1.00x | 135.2 | 0.99x | -0.3% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 926.4 | 1670.0 | 1.80x | 1650.9 | 1.78x | -1.1% | 1776 | 1720 | 41 % |  |
| sql/refused-late.bool | generated | hand | 2525.4 | 11862.8 | 4.70x | 5736.9 | 2.27x | -51.6% | 13552 | 6616 | 11 % |  |
| sql/select20.bool | generated | hand | 6634.4 | 17492.4 | 2.64x | 17580.8 | 2.65x | +0.5% | 21448 | 21392 | 9 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4368450.0 | 4389200.0 | 1.00x | 4600700.0 | 1.05x | +4.8% | 6276618 | 6276598 | 17 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6283468.8 | 6469056.2 | 1.03x | 6494781.2 | 1.03x | +0.4% | 8778600 | 8778576 | 9 % |  |
| el/ladder | generated | hand | 929.5 | 1679.3 | 1.81x | 1689.2 | 1.82x | +0.6% | 1776 | 1776 | 21 % |  |
| el/ladder | immediate | hand | 929.5 | 1168.2 | 1.26x | 1129.4 | 1.22x | -3.3% | 1784 | 1784 | 21 % |  |
| el/terms1000 | generated | hand | 106702.8 | 147146.9 | 1.38x | 148693.5 | 1.39x | +1.1% | 169104 | 169107 | 20 % |  |
| el/terms1000 | immediate | hand | 106702.8 | 122498.4 | 1.15x | 122208.0 | 1.15x | -0.2% | 177120 | 177120 | 20 % |  |
| sql/select20 | generated | hand | 6668.6 | 17533.5 | 2.63x | 17544.4 | 2.63x | +0.1% | 21448 | 21448 | 21 % |  |
| sql/refused-late | generated | hand | 2525.0 | 11850.4 | 4.69x | 11945.0 | 4.73x | +0.8% | 13552 | 13552 | 2 % |  |
