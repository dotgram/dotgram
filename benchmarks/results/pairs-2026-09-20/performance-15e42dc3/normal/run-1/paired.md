# Paired stand, 2026-09-21 06:41

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 174732.0 | 183583.6 | 1.05x | 185172.7 | 1.06x | +0.9% | 720048 | 720048 | 14 % |  |
| sql/select20.at | generated | hand | 6647.5 | 16985.4 | 2.56x | 16901.8 | 2.54x | -0.5% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6595.7 | 17516.0 | 2.66x | 17517.3 | 2.66x | 0.0% | 21416 | 21416 | 26 % |  |
| sql/select20.scan | generated | control | 372.0 | 369.9 | 0.99x | 373.7 | 1.00x | +1.0% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 385.1 | 373.3 | 0.97x | 372.7 | 0.97x | -0.2% | 0 | 0 | 10 % |  |
| el/ladder.scan | generated | control | 134.7 | 135.5 | 1.01x | 135.7 | 1.01x | +0.1% | 0 | 0 | 14 % |  |
| el/ladder.bool | generated | hand | 938.7 | 1708.1 | 1.82x | 1730.6 | 1.84x | +1.3% | 1776 | 1720 | 7 % |  |
| sql/refused-late.bool | generated | hand | 2492.1 | 11882.6 | 4.77x | 5666.6 | 2.27x | -52.3% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6674.6 | 17637.4 | 2.64x | 17539.0 | 2.63x | -0.6% | 21448 | 21392 | 10 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4347350.0 | 4543775.0 | 1.05x | 4301300.0 | 0.99x | -5.3% | 6276688 | 6276686 | 11 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6158643.8 | 6355912.5 | 1.03x | 6265950.0 | 1.02x | -1.4% | 8778600 | 8778619 | 16 % |  |
| el/ladder | generated | hand | 932.0 | 1684.6 | 1.81x | 1695.7 | 1.82x | +0.7% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 932.0 | 1130.7 | 1.21x | 1132.4 | 1.21x | +0.1% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 107299.8 | 144775.9 | 1.35x | 147095.4 | 1.37x | +1.6% | 169104 | 169107 | 8 % |  |
| el/terms1000 | immediate | hand | 107299.8 | 118181.2 | 1.10x | 119293.8 | 1.11x | +0.9% | 177120 | 177120 | 8 % |  |
| sql/select20 | generated | hand | 6544.1 | 17369.4 | 2.65x | 17283.1 | 2.64x | -0.5% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2491.8 | 11820.7 | 4.74x | 11783.0 | 4.73x | -0.3% | 13552 | 13552 | 49 % |  |
