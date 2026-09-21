# Paired stand, 2026-09-21 02:34

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 245276.6 | 219045.3 | 0.89x | 214770.3 | 0.88x | -2.0% | 403288 | 403288 | 13 % |  |
| tsql/columns1000 | generated | scriptdom | 1796562.5 | 175406.2 | 0.10x | 210575.0 | 0.12x | +20.0% | 200489 | 200489 | 60 % |  |
| web/json.array10000 | generated | hand | 167485.9 | 188228.9 | 1.12x | 183853.1 | 1.10x | -2.3% | 720048 | 720048 | 10 % |  |
| sql/select20.at | generated | hand | 6682.4 | 17887.9 | 2.68x | 17098.6 | 2.56x | -4.4% | 21416 | 21416 | 2 % |  |
| sql/select20.window | generated | hand | 6715.4 | 18358.7 | 2.73x | 17644.3 | 2.63x | -3.9% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 374.4 | 369.4 | 0.99x | 372.9 | 1.00x | +0.9% | 0 | 0 | 12 % |  |
| tsql/select20.scan | generated | control | 379.6 | 375.0 | 0.99x | 370.4 | 0.98x | -1.2% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 138.9 | 135.6 | 0.98x | 135.3 | 0.97x | -0.3% | 0 | 0 | 13 % |  |
| el/ladder.bool | generated | hand | 964.6 | 1828.9 | 1.90x | 1703.0 | 1.77x | -6.9% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2541.9 | 12124.8 | 4.77x | 5659.2 | 2.23x | -53.3% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6696.2 | 18259.7 | 2.73x | 17568.6 | 2.62x | -3.8% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4432775.0 | 4431587.5 | 1.00x | 4484856.2 | 1.01x | +1.2% | 6276684 | 6276488 | 19 % |  |
| sql/refused-cliff-case-1738 | generated | control | 10732375.0 | 5802525.0 | 0.54x | 9967100.0 | 0.93x | +71.8% | 7842195 | 74362572 | 391 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2454003.1 | 2542103.1 | 1.04x | 2490462.5 | 1.01x | -2.0% | 2994056 | 2994056 | 3 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 3926387.5 | 3379218.8 | 0.86x | 4249650.0 | 1.08x | +25.8% | 3740043 | 12948551 | 112 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 5588512.5 | 5291187.5 | 0.95x | 6546600.0 | 1.17x | +23.7% | 5840043 | 21143712 | 140 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 14688425.0 | 6179300.0 | 0.42x | 14240750.0 | 0.97x | +130.5% | 7298283 | 94724951 | 122 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6226800.0 | 6541750.0 | 1.05x | 6433350.0 | 1.03x | -1.7% | 8778360 | 8778600 | 138 % |  |
| sql/refused-cliff-and-2715 | generated | control | 7762556.2 | 7882775.0 | 1.02x | 7066518.8 | 0.91x | -10.4% | 8775208 | 8775467 | 18 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 14922725.0 | 8631425.0 | 0.58x | 13871100.0 | 0.93x | +60.7% | 10969785 | 94858088 | 253 % |  |
| sql/refused-cliff-and-3393 | generated | control | 13430075.0 | 8059500.0 | 0.60x | 15292975.0 | 1.14x | +89.8% | 10966787 | 94854749 | 299 % |  |
| el/parse-200k-tokens | generated | control | 15323250.0 | 17753600.0 | 1.16x | 14805575.0 | 0.97x | -16.6% | 16801571 | 16801507 | 59 % |  |
| el/parse-500k-tokens | generated | control | 40246250.0 | 45253200.0 | 1.12x | 39786400.0 | 0.99x | -12.1% | 42002327 | 49503195 | 38 % |  |
| sql/worst-columns-100k | generated | control | 423419300.0 | 276058900.0 | 0.65x | 430528700.0 | 1.02x | +56.0% | 95920882 | 999698405 | 24 % |  |
| fix/Orders128.yield-string | generated | hand | 79557.6 | 249629.1 | 3.14x | 262254.9 | 3.30x | +5.1% | 117936 | 117936 | 36 % |  |
| web/media-type.quoted | generated | control | 237.7 | 294.6 | 1.24x | 276.9 | 1.16x | -6.0% | 816 | 816 | 6 % |  |
| el/ladder | generated | hand | 967.2 | 1823.6 | 1.89x | 1721.1 | 1.78x | -5.6% | 1776 | 1776 | 8 % |  |
| el/ladder | immediate | hand | 967.2 | 1168.9 | 1.21x | 1160.8 | 1.20x | -0.7% | 1784 | 1784 | 8 % |  |
| el/terms1000 | generated | hand | 102688.2 | 169247.4 | 1.65x | 140464.4 | 1.37x | -17.0% | 169107 | 169104 | 13 % |  |
| el/terms1000 | immediate | hand | 102688.2 | 117263.2 | 1.14x | 116904.5 | 1.14x | -0.3% | 177120 | 177120 | 13 % |  |
| sql/select20 | generated | hand | 6671.3 | 18219.9 | 2.73x | 17575.0 | 2.63x | -3.5% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2528.4 | 12005.9 | 4.75x | 11555.6 | 4.57x | -3.8% | 13552 | 13552 | 5 % |  |
