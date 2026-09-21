# Paired stand, 2026-09-20 23:52

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

This binary, and the libraries it holds as the control, was built from b746f489. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5de9a75d, framework net10.0, no properties, emitted 194b78b7193dfafe). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 254656.2 | 218050.0 | 0.86x | 230293.8 | 0.90x | +5.6% | 403288 | 403288 | 17 % |  |
| tsql/columns1000 | generated | scriptdom | 1774156.2 | 172412.5 | 0.10x | 178443.8 | 0.10x | +3.5% | 200489 | 200489 | 19 % |  |
| web/json.array10000 | generated | hand | 170161.7 | 186199.2 | 1.09x | 201443.8 | 1.18x | +8.2% | 720048 | 720048 | 12 % |  |
| sql/select20.at | generated | hand | 6829.9 | 17658.8 | 2.59x | 18018.9 | 2.64x | +2.0% | 21416 | 21416 | 3 % |  |
| sql/select20.window | generated | hand | 6875.9 | 17914.5 | 2.61x | 18488.3 | 2.69x | +3.2% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 382.2 | 378.4 | 0.99x | 383.5 | 1.00x | +1.4% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 379.6 | 382.8 | 1.01x | 381.7 | 1.01x | -0.3% | 0 | 0 | 11 % |  |
| el/ladder.scan | generated | control | 139.4 | 139.2 | 1.00x | 143.5 | 1.03x | +3.1% | 0 | 0 | 4 % |  |
| el/ladder.bool | generated | hand | 978.0 | 1724.7 | 1.76x | 1744.2 | 1.78x | +1.1% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2575.2 | 12119.6 | 4.71x | 5970.5 | 2.32x | -50.7% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6860.3 | 17920.7 | 2.61x | 18426.3 | 2.69x | +2.8% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4561318.8 | 4753543.8 | 1.04x | 4544975.0 | 1.00x | -4.4% | 6276684 | 6276488 | 17 % |  |
| sql/refused-cliff-case-1738 | generated | control | 17582975.0 | 13027300.0 | 0.74x | 5853900.0 | 0.33x | -55.1% | 74362450 | 7842195 | 121 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2685962.5 | 2736006.2 | 1.02x | 2688928.1 | 1.00x | -1.7% | 2994166 | 2994056 | 7 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 4415268.8 | 5501618.8 | 1.25x | 3392075.0 | 0.77x | -38.3% | 12948506 | 3740045 | 105 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 9513187.5 | 8877662.5 | 0.93x | 5261643.8 | 0.55x | -40.7% | 21143713 | 5840086 | 43 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 22339387.5 | 15967012.5 | 0.71x | 6398612.5 | 0.29x | -59.9% | 94724886 | 7298412 | 132 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6747506.2 | 6934612.5 | 1.03x | 7026062.5 | 1.04x | +1.3% | 8778600 | 8778360 | 13 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6763337.5 | 6791550.0 | 1.00x | 6706068.8 | 0.99x | -1.3% | 8775448 | 8775208 | 11 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 16347100.0 | 13287800.0 | 0.81x | 8418825.0 | 0.52x | -36.6% | 94857892 | 10969680 | 202 % |  |
| sql/refused-cliff-and-3393 | generated | control | 30414362.5 | 39270637.5 | 1.29x | 8794950.0 | 0.29x | -77.6% | 94854554 | 10966808 | 128 % |  |
| el/parse-200k-tokens | generated | control | 16473412.5 | 16216650.0 | 0.98x | 16865062.5 | 1.02x | +4.0% | 16801614 | 16801571 | 17 % |  |
| el/parse-500k-tokens | generated | control | 41534750.0 | 40144800.0 | 0.97x | 40688100.0 | 0.98x | +1.4% | 49503192 | 42002327 | 42 % |  |
| sql/worst-columns-100k | generated | control | 445199600.0 | 458788900.0 | 1.03x | 287563600.0 | 0.65x | -37.3% | 999698410 | 95920972 | 17 % |  |
| fix/Orders128.yield-string | generated | hand | 89289.5 | 278458.4 | 3.12x | 271889.6 | 3.05x | -2.4% | 117936 | 117936 | 29 % |  |
| web/media-type.quoted | generated | control | 245.3 | 294.7 | 1.20x | 295.7 | 1.21x | +0.3% | 816 | 816 | 12 % |  |
| el/ladder | generated | hand | 976.7 | 1752.2 | 1.79x | 1778.8 | 1.82x | +1.5% | 1776 | 1776 | 10 % |  |
| el/ladder | immediate | hand | 976.7 | 1209.5 | 1.24x | 1193.4 | 1.22x | -1.3% | 1784 | 1784 | 10 % |  |
| el/terms1000 | generated | hand | 105655.7 | 148145.4 | 1.40x | 149391.4 | 1.41x | +0.8% | 169104 | 169104 | 9 % |  |
| el/terms1000 | immediate | hand | 105655.7 | 124594.4 | 1.18x | 122567.6 | 1.16x | -1.6% | 177123 | 177120 | 9 % |  |
| sql/select20 | generated | hand | 6872.5 | 18593.9 | 2.71x | 18523.6 | 2.70x | -0.4% | 21448 | 21448 | 11 % |  |
| sql/refused-late | generated | hand | 2574.2 | 12146.6 | 4.72x | 12097.5 | 4.70x | -0.4% | 13552 | 13552 | 21 % |  |
