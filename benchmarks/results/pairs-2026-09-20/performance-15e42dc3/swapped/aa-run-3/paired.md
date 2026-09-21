# Paired stand, 2026-09-21 07:14

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 167438.3 | 185075.0 | 1.11x | 182145.3 | 1.09x | -1.6% | 720048 | 720048 | 4 % |  |
| sql/select20.at | generated | hand | 6626.1 | 17287.2 | 2.61x | 16986.1 | 2.56x | -1.7% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6663.2 | 17578.3 | 2.64x | 17435.3 | 2.62x | -0.8% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 370.0 | 373.3 | 1.01x | 376.2 | 1.02x | +0.8% | 0 | 0 | 18 % |  |
| tsql/select20.scan | generated | control | 369.2 | 386.0 | 1.05x | 371.2 | 1.01x | -3.8% | 0 | 0 | 8 % |  |
| el/ladder.scan | generated | control | 134.9 | 136.0 | 1.01x | 138.2 | 1.02x | +1.6% | 0 | 0 | 15 % |  |
| el/ladder.bool | generated | hand | 968.8 | 1691.0 | 1.75x | 1657.7 | 1.71x | -2.0% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2510.5 | 11938.4 | 4.76x | 5595.5 | 2.23x | -53.1% | 13552 | 6616 | 14 % |  |
| sql/select20.bool | generated | hand | 6616.5 | 17613.0 | 2.66x | 17361.6 | 2.62x | -1.4% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4450843.8 | 4371362.5 | 0.98x | 4363100.0 | 0.98x | -0.2% | 6276684 | 6276684 | 5 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6280768.8 | 6401356.2 | 1.02x | 6269768.8 | 1.00x | -2.1% | 8778556 | 8778600 | 6 % |  |
| el/ladder | generated | hand | 968.8 | 1699.9 | 1.75x | 1662.7 | 1.72x | -2.2% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 968.8 | 1148.3 | 1.19x | 1179.5 | 1.22x | +2.7% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 107708.9 | 147013.7 | 1.36x | 148913.9 | 1.38x | +1.3% | 169104 | 169107 | 3 % |  |
| el/terms1000 | immediate | hand | 107708.9 | 122540.3 | 1.14x | 120685.4 | 1.12x | -1.5% | 177120 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6628.5 | 17644.3 | 2.66x | 17392.7 | 2.62x | -1.4% | 21448 | 21448 | 1 % |  |
| sql/refused-late | generated | hand | 2507.2 | 11913.6 | 4.75x | 11762.0 | 4.69x | -1.3% | 13552 | 13552 | 2 % |  |
