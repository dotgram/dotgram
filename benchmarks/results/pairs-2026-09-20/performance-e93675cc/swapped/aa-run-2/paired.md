# Paired stand, 2026-09-21 02:13

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 247412.5 | 215400.0 | 0.87x | 220504.7 | 0.89x | +2.4% | 403288 | 403288 | 19 % |  |
| tsql/columns1000 | generated | scriptdom | 1924137.5 | 174206.2 | 0.09x | 171037.5 | 0.09x | -1.8% | 200489 | 200489 | 58 % |  |
| web/json.array10000 | generated | hand | 164585.2 | 186406.2 | 1.13x | 183518.0 | 1.12x | -1.5% | 720048 | 720048 | 60 % |  |
| sql/select20.at | generated | hand | 6615.9 | 17325.9 | 2.62x | 17131.5 | 2.59x | -1.1% | 21416 | 21416 | 2 % |  |
| sql/select20.window | generated | hand | 6627.1 | 17611.7 | 2.66x | 17487.5 | 2.64x | -0.7% | 21416 | 21441 | 3 % |  |
| sql/select20.scan | generated | control | 370.0 | 375.0 | 1.01x | 374.4 | 1.01x | -0.2% | 0 | 0 | 1 % |  |
| tsql/select20.scan | generated | control | 369.6 | 379.8 | 1.03x | 381.1 | 1.03x | +0.4% | 0 | 0 | 10 % |  |
| el/ladder.scan | generated | control | 134.3 | 138.5 | 1.03x | 135.6 | 1.01x | -2.1% | 0 | 0 | 17 % |  |
| el/ladder.bool | generated | hand | 945.7 | 1703.2 | 1.80x | 1644.6 | 1.74x | -3.4% | 1776 | 1720 | 25 % |  |
| sql/refused-late.bool | generated | hand | 2505.7 | 11673.1 | 4.66x | 5685.1 | 2.27x | -51.3% | 13552 | 6616 | 24 % |  |
| sql/select20.bool | generated | hand | 6631.5 | 17689.6 | 2.67x | 17604.0 | 2.65x | -0.5% | 21448 | 21392 | 6 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4318900.0 | 4485800.0 | 1.04x | 4379825.0 | 1.01x | -2.4% | 6276660 | 6276660 | 22 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2525803.1 | 2535415.6 | 1.00x | 2553718.8 | 1.01x | +0.7% | 2994166 | 2994142 | 16 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6372181.2 | 6240193.8 | 0.98x | 6375425.0 | 1.00x | +2.2% | 8778600 | 8778600 | 20 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6303768.8 | 6277093.8 | 1.00x | 6426718.8 | 1.02x | +2.4% | 8775467 | 8775448 | 6 % |  |
| fix/Orders128.yield-string | generated | hand | 77381.4 | 252001.6 | 3.26x | 244648.0 | 3.16x | -2.9% | 117936 | 117936 | 9 % |  |
| web/media-type.quoted | generated | control | 242.1 | 281.7 | 1.16x | 273.8 | 1.13x | -2.8% | 816 | 816 | 4 % |  |
| el/ladder | generated | hand | 947.4 | 1704.1 | 1.80x | 1672.7 | 1.77x | -1.8% | 1776 | 1776 | 19 % |  |
| el/ladder | immediate | hand | 947.4 | 1210.4 | 1.28x | 1131.9 | 1.19x | -6.5% | 1784 | 1784 | 19 % |  |
| el/terms1000 | generated | hand | 109934.1 | 149429.9 | 1.36x | 148577.3 | 1.35x | -0.6% | 169104 | 169128 | 5 % |  |
| el/terms1000 | immediate | hand | 109934.1 | 122573.7 | 1.11x | 122563.1 | 1.11x | 0.0% | 177123 | 177120 | 5 % |  |
| sql/select20 | generated | hand | 6643.9 | 17776.2 | 2.68x | 17490.5 | 2.63x | -1.6% | 21448 | 21472 | 9 % |  |
| sql/refused-late | generated | hand | 2509.7 | 11694.8 | 4.66x | 11692.0 | 4.66x | 0.0% | 13552 | 13552 | 27 % |  |
