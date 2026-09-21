# Paired stand, 2026-09-20 23:45

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from b746f489. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5de9a75d, framework net10.0, no properties, emitted 194b78b7193dfafe). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 277718.8 | 226050.0 | 0.81x | 230368.8 | 0.83x | +1.9% | 403288 | 403288 | 43 % |  |
| tsql/columns1000 | generated | scriptdom | 1696525.0 | 172762.5 | 0.10x | 176506.2 | 0.10x | +2.2% | 200489 | 200489 | 5 % |  |
| web/json.array10000 | generated | hand | 172761.7 | 193463.3 | 1.12x | 202510.2 | 1.17x | +4.7% | 720048 | 720048 | 9 % |  |
| sql/select20.at | generated | hand | 7009.0 | 18193.4 | 2.60x | 18196.9 | 2.60x | 0.0% | 21416 | 21416 | 10 % |  |
| sql/select20.window | generated | hand | 7061.2 | 18307.1 | 2.59x | 18220.2 | 2.58x | -0.5% | 21416 | 21416 | 16 % |  |
| sql/select20.scan | generated | control | 398.3 | 390.9 | 0.98x | 397.6 | 1.00x | +1.7% | 0 | 0 | 68 % |  |
| tsql/select20.scan | generated | control | 366.7 | 369.0 | 1.01x | 376.4 | 1.03x | +2.0% | 0 | 0 | 13 % |  |
| el/ladder.scan | generated | control | 132.8 | 134.0 | 1.01x | 135.0 | 1.02x | +0.7% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 1024.1 | 1742.6 | 1.70x | 1717.6 | 1.68x | -1.4% | 1776 | 1720 | 10 % |  |
| sql/refused-late.bool | generated | hand | 2559.7 | 11785.3 | 4.60x | 5823.6 | 2.28x | -50.6% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6771.0 | 17764.8 | 2.62x | 17956.3 | 2.65x | +1.1% | 21448 | 21392 | 13 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4493118.8 | 4409843.8 | 0.98x | 4530443.8 | 1.01x | +2.7% | 6276684 | 6276488 | 6 % |  |
| sql/refused-cliff-case-1738 | generated | control | 17701300.0 | 11355337.5 | 0.64x | 5594762.5 | 0.32x | -50.7% | 74362362 | 7842348 | 132 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2551356.2 | 2546000.0 | 1.00x | 2574725.0 | 1.01x | +1.1% | 2994056 | 2994056 | 11 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 5266103.1 | 5409112.5 | 1.03x | 3268121.9 | 0.62x | -39.6% | 12948506 | 3740043 | 45 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 7415687.5 | 9784431.2 | 1.32x | 5174318.8 | 0.70x | -47.1% | 21143691 | 5840086 | 47 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 9384250.0 | 13133950.0 | 1.40x | 7232000.0 | 0.77x | -44.9% | 94724790 | 7298240 | 439 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 8087893.8 | 7987331.2 | 0.99x | 7574400.0 | 0.94x | -5.2% | 8778600 | 8778619 | 20 % |  |
| sql/refused-cliff-and-2715 | generated | control | 7368212.5 | 7530275.0 | 1.02x | 8370437.5 | 1.14x | +11.2% | 8775448 | 8775448 | 15 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 29365550.0 | 37560700.0 | 1.28x | 8427125.0 | 0.29x | -77.6% | 94857834 | 10969958 | 173 % |  |
| sql/refused-cliff-and-3393 | generated | control | 35285012.5 | 29100037.5 | 0.82x | 8373825.0 | 0.24x | -71.2% | 94854576 | 10966790 | 100 % |  |
| el/parse-200k-tokens | generated | control | 16578437.5 | 15964225.0 | 0.96x | 15508087.5 | 0.94x | -2.9% | 16801528 | 16801614 | 41 % |  |
| el/parse-500k-tokens | generated | control | 51533950.0 | 46477450.0 | 0.90x | 39155200.0 | 0.76x | -15.8% | 49503195 | 42002310 | 32 % |  |
| sql/worst-columns-100k | generated | control | 456148000.0 | 435569000.0 | 0.95x | 269313350.0 | 0.59x | -38.2% | 999698387 | 95920871 | 19 % |  |
| fix/Orders128.yield-string | generated | hand | 78845.3 | 276957.4 | 3.51x | 286270.3 | 3.63x | +3.4% | 117936 | 117936 | 10 % |  |
| web/media-type.quoted | generated | control | 261.9 | 305.7 | 1.17x | 290.2 | 1.11x | -5.1% | 816 | 816 | 7 % |  |
| el/ladder | generated | hand | 991.8 | 1711.8 | 1.73x | 1707.6 | 1.72x | -0.2% | 1776 | 1776 | 10 % |  |
| el/ladder | immediate | hand | 991.8 | 1178.7 | 1.19x | 1164.2 | 1.17x | -1.2% | 1784 | 1784 | 10 % |  |
| el/terms1000 | generated | hand | 107395.6 | 148743.1 | 1.39x | 145121.7 | 1.35x | -2.4% | 169104 | 169104 | 67 % |  |
| el/terms1000 | immediate | hand | 107395.6 | 120432.0 | 1.12x | 118155.8 | 1.10x | -1.9% | 177120 | 177120 | 67 % |  |
| sql/select20 | generated | hand | 7753.3 | 19518.9 | 2.52x | 19743.7 | 2.55x | +1.2% | 21448 | 21448 | 52 % |  |
| sql/refused-late | generated | hand | 3245.5 | 14231.9 | 4.39x | 14396.4 | 4.44x | +1.2% | 13552 | 13552 | 47 % |  |
