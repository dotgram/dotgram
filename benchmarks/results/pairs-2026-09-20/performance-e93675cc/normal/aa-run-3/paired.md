# Paired stand, 2026-09-21 01:43

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.9 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 249196.9 | 215706.2 | 0.87x | 221912.5 | 0.89x | +2.9% | 403288 | 403288 | 70 % |  |
| tsql/columns1000 | generated | scriptdom | 1943737.5 | 204656.2 | 0.11x | 189762.5 | 0.10x | -7.3% | 200489 | 200489 | 60 % |  |
| web/json.array10000 | generated | hand | 163689.8 | 189684.4 | 1.16x | 185364.8 | 1.13x | -2.3% | 720048 | 720048 | 8 % |  |
| sql/select20.at | generated | hand | 6849.6 | 17651.3 | 2.58x | 17494.1 | 2.55x | -0.9% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6741.3 | 17896.8 | 2.65x | 17748.5 | 2.63x | -0.8% | 21416 | 21441 | 4 % |  |
| sql/select20.scan | generated | control | 379.9 | 373.5 | 0.98x | 372.4 | 0.98x | -0.3% | 0 | 0 | 9 % |  |
| tsql/select20.scan | generated | control | 377.7 | 385.2 | 1.02x | 386.5 | 1.02x | +0.3% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 139.0 | 135.6 | 0.98x | 134.6 | 0.97x | -0.7% | 0 | 0 | 16 % |  |
| el/ladder.bool | generated | hand | 962.0 | 1685.2 | 1.75x | 1671.1 | 1.74x | -0.8% | 1776 | 1720 | 18 % |  |
| sql/refused-late.bool | generated | hand | 2623.5 | 12028.2 | 4.58x | 5798.7 | 2.21x | -51.8% | 13552 | 6616 | 14 % |  |
| sql/select20.bool | generated | hand | 6744.1 | 18056.9 | 2.68x | 18173.2 | 2.69x | +0.6% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4445818.8 | 4360031.2 | 0.98x | 4460675.0 | 1.00x | +2.3% | 6276660 | 6276684 | 23 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2556328.1 | 2555253.1 | 1.00x | 2571250.0 | 1.01x | +0.6% | 2994166 | 2994166 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6284543.8 | 6335406.2 | 1.01x | 6339868.8 | 1.01x | +0.1% | 8778556 | 8778600 | 7 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6262625.0 | 6268575.0 | 1.00x | 6426256.2 | 1.03x | +2.5% | 8775448 | 8775404 | 11 % |  |
| fix/Orders128.yield-string | generated | hand | 78979.5 | 276485.0 | 3.50x | 276335.4 | 3.50x | -0.1% | 117936 | 117936 | 3 % |  |
| web/media-type.quoted | generated | control | 244.5 | 286.4 | 1.17x | 290.8 | 1.19x | +1.6% | 816 | 816 | 10 % |  |
| el/ladder | generated | hand | 965.1 | 1690.4 | 1.75x | 1681.1 | 1.74x | -0.5% | 1776 | 1776 | 11 % |  |
| el/ladder | immediate | hand | 965.1 | 1143.7 | 1.18x | 1138.7 | 1.18x | -0.4% | 1784 | 1784 | 11 % |  |
| el/terms1000 | generated | hand | 111822.4 | 148794.6 | 1.33x | 146875.7 | 1.31x | -1.3% | 169104 | 169104 | 17 % |  |
| el/terms1000 | immediate | hand | 111822.4 | 125129.8 | 1.12x | 123791.8 | 1.11x | -1.1% | 177123 | 177120 | 17 % |  |
| sql/select20 | generated | hand | 6699.5 | 17887.8 | 2.67x | 17710.3 | 2.64x | -1.0% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2524.1 | 11767.4 | 4.66x | 11642.8 | 4.61x | -1.1% | 13552 | 13552 | 6 % |  |
