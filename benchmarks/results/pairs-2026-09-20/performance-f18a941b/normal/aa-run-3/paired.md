# Paired stand, 2026-09-21 05:11

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166790.6 | 181194.5 | 1.09x | 179728.9 | 1.08x | -0.8% | 720048 | 720048 | 16 % |  |
| sql/select20.at | generated | hand | 6667.1 | 17307.7 | 2.60x | 17013.8 | 2.55x | -1.7% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6514.3 | 17489.9 | 2.68x | 17656.3 | 2.71x | +1.0% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 374.5 | 367.4 | 0.98x | 367.5 | 0.98x | 0.0% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 402.4 | 371.0 | 0.92x | 382.1 | 0.95x | +3.0% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 135.8 | 133.0 | 0.98x | 133.3 | 0.98x | +0.2% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 945.1 | 1662.4 | 1.76x | 1647.3 | 1.74x | -0.9% | 1776 | 1720 | 8 % |  |
| sql/refused-late.bool | generated | hand | 2449.8 | 11859.5 | 4.84x | 5637.0 | 2.30x | -52.5% | 13552 | 6616 | 9 % |  |
| sql/select20.bool | generated | hand | 6520.9 | 17606.6 | 2.70x | 17538.5 | 2.69x | -0.4% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4331200.0 | 4337550.0 | 1.00x | 4438700.0 | 1.02x | +2.3% | 6276620 | 6276599 | 41 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6361237.5 | 6311862.5 | 0.99x | 6207718.8 | 0.98x | -1.6% | 8778600 | 8778600 | 19 % |  |
| el/ladder | generated | hand | 935.5 | 1656.8 | 1.77x | 1668.4 | 1.78x | +0.7% | 1776 | 1776 | 8 % |  |
| el/ladder | immediate | hand | 935.5 | 1124.9 | 1.20x | 1111.3 | 1.19x | -1.2% | 1784 | 1784 | 8 % |  |
| el/terms1000 | generated | hand | 108526.3 | 147390.8 | 1.36x | 149145.4 | 1.37x | +1.2% | 169104 | 169107 | 2 % |  |
| el/terms1000 | immediate | hand | 108526.3 | 120148.9 | 1.11x | 121607.0 | 1.12x | +1.2% | 177120 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6515.0 | 17489.4 | 2.68x | 17483.7 | 2.68x | 0.0% | 21448 | 21448 | 17 % |  |
| sql/refused-late | generated | hand | 2458.1 | 11853.6 | 4.82x | 11779.1 | 4.79x | -0.6% | 13552 | 13552 | 2 % |  |
