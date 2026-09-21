# Paired stand, 2026-09-21 05:28

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165293.0 | 181585.9 | 1.10x | 189102.3 | 1.14x | +4.1% | 720048 | 720048 | 350 % |  |
| sql/select20.at | generated | hand | 6616.7 | 16953.7 | 2.56x | 16887.9 | 2.55x | -0.4% | 21416 | 21416 | 11 % |  |
| sql/select20.window | generated | hand | 6619.7 | 17448.2 | 2.64x | 17394.4 | 2.63x | -0.3% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 371.3 | 371.7 | 1.00x | 369.4 | 1.00x | -0.6% | 0 | 0 | 4 % |  |
| tsql/select20.scan | generated | control | 397.8 | 380.6 | 0.96x | 372.0 | 0.94x | -2.3% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 134.2 | 135.9 | 1.01x | 135.5 | 1.01x | -0.3% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 967.2 | 1619.5 | 1.67x | 1632.8 | 1.69x | +0.8% | 1776 | 1720 | 2 % |  |
| sql/refused-late.bool | generated | hand | 2502.0 | 11754.5 | 4.70x | 5628.8 | 2.25x | -52.1% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6610.7 | 18158.0 | 2.75x | 17431.6 | 2.64x | -4.0% | 21448 | 21392 | 2 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4530275.0 | 4368975.0 | 0.96x | 4283237.5 | 0.95x | -2.0% | 6276688 | 6276688 | 20 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6272181.2 | 6275093.8 | 1.00x | 6269343.8 | 1.00x | -0.1% | 8778600 | 8778600 | 3 % |  |
| el/ladder | generated | hand | 966.2 | 1630.2 | 1.69x | 1678.0 | 1.74x | +2.9% | 1776 | 1776 | 12 % |  |
| el/ladder | immediate | hand | 966.2 | 1126.9 | 1.17x | 1126.1 | 1.17x | -0.1% | 1784 | 1784 | 12 % |  |
| el/terms1000 | generated | hand | 107603.9 | 144835.9 | 1.35x | 146895.6 | 1.37x | +1.4% | 169104 | 169128 | 2 % |  |
| el/terms1000 | immediate | hand | 107603.9 | 119480.7 | 1.11x | 121516.3 | 1.13x | +1.7% | 177120 | 177123 | 2 % |  |
| sql/select20 | generated | hand | 6606.1 | 18080.8 | 2.74x | 17411.2 | 2.64x | -3.7% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2497.5 | 11776.8 | 4.72x | 11759.3 | 4.71x | -0.1% | 13552 | 13552 | 18 % |  |
