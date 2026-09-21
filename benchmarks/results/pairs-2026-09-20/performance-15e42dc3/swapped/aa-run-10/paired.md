# Paired stand, 2026-09-21 07:32

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 169036.7 | 183650.0 | 1.09x | 184676.6 | 1.09x | +0.6% | 720048 | 720048 | 160 % |  |
| sql/select20.at | generated | hand | 6762.6 | 17648.1 | 2.61x | 17308.0 | 2.56x | -1.9% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6605.3 | 18111.8 | 2.74x | 17608.8 | 2.67x | -2.8% | 21416 | 21416 | 6 % |  |
| sql/select20.scan | generated | control | 371.5 | 368.9 | 0.99x | 369.0 | 0.99x | 0.0% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 369.2 | 369.4 | 1.00x | 366.5 | 0.99x | -0.8% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 135.1 | 133.2 | 0.99x | 132.7 | 0.98x | -0.3% | 0 | 0 | 5 % |  |
| el/ladder.bool | generated | hand | 963.7 | 1685.9 | 1.75x | 1658.3 | 1.72x | -1.6% | 1776 | 1720 | 61 % |  |
| sql/refused-late.bool | generated | hand | 2487.5 | 11920.0 | 4.79x | 5635.4 | 2.27x | -52.7% | 13552 | 6616 | 8 % |  |
| sql/select20.bool | generated | hand | 6626.1 | 18166.9 | 2.74x | 17689.4 | 2.67x | -2.6% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4399493.8 | 4568793.8 | 1.04x | 4470531.2 | 1.02x | -2.2% | 6276660 | 6276660 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6338425.0 | 6507137.5 | 1.03x | 6277681.2 | 0.99x | -3.5% | 8778600 | 8778600 | 21 % |  |
| el/ladder | generated | hand | 952.4 | 1691.4 | 1.78x | 1702.6 | 1.79x | +0.7% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 952.4 | 1148.2 | 1.21x | 1136.7 | 1.19x | -1.0% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 107201.3 | 150025.7 | 1.40x | 147463.4 | 1.38x | -1.7% | 169104 | 169107 | 2 % |  |
| el/terms1000 | immediate | hand | 107201.3 | 123224.1 | 1.15x | 118210.7 | 1.10x | -4.1% | 177120 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6611.0 | 18116.0 | 2.74x | 17796.0 | 2.69x | -1.8% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2492.2 | 11918.4 | 4.78x | 11820.2 | 4.74x | -0.8% | 13552 | 13552 | 4 % |  |
