# Paired stand, 2026-09-21 02:36

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 251300.0 | 215714.1 | 0.86x | 214579.7 | 0.85x | -0.5% | 403288 | 403288 | 19 % |  |
| tsql/columns1000 | generated | scriptdom | 1978862.5 | 206887.5 | 0.10x | 178125.0 | 0.09x | -13.9% | 200489 | 200489 | 60 % |  |
| web/json.array10000 | generated | hand | 165549.2 | 185288.3 | 1.12x | 184001.6 | 1.11x | -0.7% | 720048 | 720048 | 5 % |  |
| sql/select20.at | generated | hand | 6672.1 | 17270.8 | 2.59x | 16848.9 | 2.53x | -2.4% | 21416 | 21416 | 4 % |  |
| sql/select20.window | generated | hand | 6690.8 | 17542.2 | 2.62x | 17357.2 | 2.59x | -1.1% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 367.2 | 372.9 | 1.02x | 373.9 | 1.02x | +0.3% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 369.1 | 373.6 | 1.01x | 375.4 | 1.02x | +0.5% | 0 | 0 | 10 % |  |
| el/ladder.scan | generated | control | 135.3 | 138.4 | 1.02x | 137.7 | 1.02x | -0.5% | 0 | 0 | 2 % |  |
| el/ladder.bool | generated | hand | 946.2 | 1649.6 | 1.74x | 1667.1 | 1.76x | +1.1% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2506.5 | 11949.2 | 4.77x | 5560.6 | 2.22x | -53.5% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6587.8 | 17732.2 | 2.69x | 17304.1 | 2.63x | -2.4% | 21448 | 21392 | 1 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4301475.0 | 4336475.0 | 1.01x | 4625587.5 | 1.08x | +6.7% | 6276684 | 6276684 | 14 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2559450.0 | 2529040.6 | 0.99x | 2508306.2 | 0.98x | -0.8% | 2994166 | 2994166 | 12 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6322312.5 | 6266956.2 | 0.99x | 6236843.8 | 0.99x | -0.5% | 8778600 | 8778600 | 5 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6276825.0 | 6207462.5 | 0.99x | 6250525.0 | 1.00x | +0.7% | 8775448 | 8775448 | 8 % |  |
| fix/Orders128.yield-string | generated | hand | 78975.7 | 256378.2 | 3.25x | 248658.0 | 3.15x | -3.0% | 117936 | 117936 | 5 % |  |
| web/media-type.quoted | generated | control | 234.8 | 278.6 | 1.19x | 268.9 | 1.14x | -3.5% | 816 | 816 | 2 % |  |
| el/ladder | generated | hand | 945.4 | 1650.1 | 1.75x | 1676.1 | 1.77x | +1.6% | 1776 | 1776 | 10 % |  |
| el/ladder | immediate | hand | 945.4 | 1140.0 | 1.21x | 1140.7 | 1.21x | +0.1% | 1784 | 1784 | 10 % |  |
| el/terms1000 | generated | hand | 109345.2 | 150373.3 | 1.38x | 148384.5 | 1.36x | -1.3% | 169104 | 169107 | 5 % |  |
| el/terms1000 | immediate | hand | 109345.2 | 124641.8 | 1.14x | 121774.6 | 1.11x | -2.3% | 177120 | 177120 | 5 % |  |
| sql/select20 | generated | hand | 6617.8 | 17851.4 | 2.70x | 17455.7 | 2.64x | -2.2% | 21448 | 21448 | 5 % |  |
| sql/refused-late | generated | hand | 2507.7 | 11984.0 | 4.78x | 11587.8 | 4.62x | -3.3% | 13552 | 13552 | 3 % |  |
