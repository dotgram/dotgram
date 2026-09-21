# Paired stand, 2026-09-21 05:16

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 164813.3 | 183040.6 | 1.11x | 184031.2 | 1.12x | +0.5% | 720048 | 720048 | 354 % |  |
| sql/select20.at | generated | hand | 6666.0 | 17057.4 | 2.56x | 16848.9 | 2.53x | -1.2% | 21416 | 21416 | 29 % |  |
| sql/select20.window | generated | hand | 6670.9 | 17454.8 | 2.62x | 17390.2 | 2.61x | -0.4% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 364.8 | 368.7 | 1.01x | 369.4 | 1.01x | +0.2% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 373.5 | 369.6 | 0.99x | 367.5 | 0.98x | -0.6% | 0 | 0 | 13 % |  |
| el/ladder.scan | generated | control | 135.1 | 133.6 | 0.99x | 133.6 | 0.99x | 0.0% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 915.2 | 1666.9 | 1.82x | 1622.7 | 1.77x | -2.7% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2476.0 | 11704.2 | 4.73x | 5595.2 | 2.26x | -52.2% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6604.7 | 17545.8 | 2.66x | 17451.2 | 2.64x | -0.5% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4377487.5 | 4297237.5 | 0.98x | 4281762.5 | 0.98x | -0.4% | 6276688 | 6276598 | 43 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6458606.2 | 6424775.0 | 0.99x | 6354481.2 | 0.98x | -1.1% | 8778600 | 8778600 | 3 % |  |
| el/ladder | generated | hand | 922.9 | 1665.6 | 1.80x | 1663.7 | 1.80x | -0.1% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 922.9 | 1119.5 | 1.21x | 1118.8 | 1.21x | -0.1% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 107303.1 | 147181.8 | 1.37x | 146758.1 | 1.37x | -0.3% | 169104 | 169104 | 13 % |  |
| el/terms1000 | immediate | hand | 107303.1 | 122508.7 | 1.14x | 119716.2 | 1.12x | -2.3% | 177120 | 177123 | 13 % |  |
| sql/select20 | generated | hand | 6602.1 | 17419.3 | 2.64x | 17250.9 | 2.61x | -1.0% | 21448 | 21448 | 8 % |  |
| sql/refused-late | generated | hand | 2500.6 | 11678.7 | 4.67x | 11741.2 | 4.70x | +0.5% | 13552 | 13552 | 23 % |  |
