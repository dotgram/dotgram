# Paired stand, 2026-09-21 01:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 248340.6 | 223079.7 | 0.90x | 222835.9 | 0.90x | -0.1% | 403288 | 403288 | 59 % |  |
| tsql/columns1000 | generated | scriptdom | 1695268.8 | 179068.8 | 0.11x | 183412.5 | 0.11x | +2.4% | 200489 | 200489 | 31 % |  |
| web/json.array10000 | generated | hand | 187732.0 | 183923.4 | 0.98x | 182377.3 | 0.97x | -0.8% | 720048 | 720048 | 7 % |  |
| sql/select20.at | generated | hand | 6761.3 | 17316.9 | 2.56x | 17576.0 | 2.60x | +1.5% | 21416 | 21416 | 2 % |  |
| sql/select20.window | generated | hand | 6795.6 | 17805.0 | 2.62x | 18120.3 | 2.67x | +1.8% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 374.0 | 380.9 | 1.02x | 383.9 | 1.03x | +0.8% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 372.4 | 389.1 | 1.04x | 381.5 | 1.02x | -1.9% | 0 | 0 | 7 % |  |
| el/ladder.scan | generated | control | 138.7 | 141.1 | 1.02x | 140.4 | 1.01x | -0.5% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 983.6 | 1687.2 | 1.72x | 1725.8 | 1.75x | +2.3% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2557.0 | 11832.6 | 4.63x | 5861.7 | 2.29x | -50.5% | 13552 | 6616 | 7 % |  |
| sql/select20.bool | generated | hand | 6749.7 | 17691.9 | 2.62x | 18453.8 | 2.73x | +4.3% | 21448 | 21392 | 16 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4563056.2 | 4560218.8 | 1.00x | 4564343.8 | 1.00x | +0.1% | 6276488 | 6276488 | 17 % |  |
| sql/refused-cliff-case-1738 | generated | control | 17316612.5 | 12750312.5 | 0.74x | 7399250.0 | 0.43x | -42.0% | 74362624 | 7842305 | 218 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2499490.6 | 2507703.1 | 1.00x | 2550296.9 | 1.02x | +1.7% | 2994056 | 2994123 | 4 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 4143106.2 | 6114087.5 | 1.48x | 3320762.5 | 0.80x | -45.7% | 12948549 | 3740086 | 102 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 8446012.5 | 8016112.5 | 0.95x | 5112337.5 | 0.61x | -36.2% | 21143693 | 5840149 | 42 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 22303125.0 | 28113200.0 | 1.26x | 6406775.0 | 0.29x | -77.2% | 94725071 | 7298283 | 131 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7517400.0 | 7580281.2 | 1.01x | 7624218.8 | 1.01x | +0.6% | 8778619 | 8778600 | 16 % |  |
| sql/refused-cliff-and-2715 | generated | control | 8096425.0 | 7944943.8 | 0.98x | 7421575.0 | 0.92x | -6.6% | 8775448 | 8775448 | 13 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 33756312.5 | 35234587.5 | 1.04x | 8324150.0 | 0.25x | -76.4% | 94857854 | 10969680 | 110 % |  |
| sql/refused-cliff-and-3393 | generated | control | 26914212.5 | 28761912.5 | 1.07x | 8597925.0 | 0.32x | -70.1% | 94854743 | 10966590 | 127 % |  |
| el/parse-200k-tokens | generated | control | 15258050.0 | 15199262.5 | 1.00x | 15859187.5 | 1.04x | +4.3% | 16801614 | 16801507 | 22 % |  |
| el/parse-500k-tokens | generated | control | 42148250.0 | 39852950.0 | 0.95x | 40634850.0 | 0.96x | +2.0% | 49503178 | 42002327 | 39 % |  |
| sql/worst-columns-100k | generated | control | 429549850.0 | 425730900.0 | 0.99x | 275774150.0 | 0.64x | -35.2% | 999698568 | 95921045 | 19 % |  |
| fix/Orders128.yield-string | generated | hand | 78354.5 | 259157.8 | 3.31x | 254774.4 | 3.25x | -1.7% | 117936 | 117936 | 12 % |  |
| web/media-type.quoted | generated | control | 246.1 | 299.1 | 1.22x | 279.5 | 1.14x | -6.6% | 816 | 816 | 12 % |  |
| el/ladder | generated | hand | 970.9 | 1675.6 | 1.73x | 1726.5 | 1.78x | +3.0% | 1776 | 1776 | 11 % |  |
| el/ladder | immediate | hand | 970.9 | 1138.9 | 1.17x | 1133.1 | 1.17x | -0.5% | 1784 | 1784 | 11 % |  |
| el/terms1000 | generated | hand | 104069.6 | 142241.5 | 1.37x | 141720.1 | 1.36x | -0.4% | 169104 | 169107 | 50 % |  |
| el/terms1000 | immediate | hand | 104069.6 | 119548.2 | 1.15x | 116260.7 | 1.12x | -2.7% | 177120 | 177120 | 50 % |  |
| sql/select20 | generated | hand | 6785.3 | 17724.8 | 2.61x | 17922.6 | 2.64x | +1.1% | 21448 | 21448 | 11 % |  |
| sql/refused-late | generated | hand | 2579.7 | 11723.4 | 4.54x | 12062.8 | 4.68x | +2.9% | 13552 | 13552 | 9 % |  |
