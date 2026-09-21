Median of 5 of 5 runs, each in a process of its own; control 30.8 ns (the runs' controls: 30.8, 30.8, 31.0, 30.7, 30.4).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 01:58

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.8 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| fix/orders400.text | generated | hand | 248340.6 | 223079.7 | 0.90x | 222825.0 | 0.90x | -0.1% | 403288 | 403288 | 4 % | [-4.3%..+23.5%], 1 of 5 positive | +1.4% [-3.2%..+5.3%], 2 of 5 positive |
| tsql/columns1000 | generated | scriptdom | 1703075.0 | 179068.8 | 0.11x | 171850.0 | 0.10x | -4.0% | 200489 | 200489 | 30 % | [-18.1%..+2.4%], 1 of 5 positive | -2.5% [-8.3%..+3.6%], 1 of 5 positive |
| web/json.array10000 | generated | hand | 187732.0 | 185821.1 | 0.99x | 184669.5 | 0.98x | -0.6% | 720048 | 720048 | 16 % | [-1.8%..+0.5%], 3 of 5 positive | -0.8% [-3.8%..+0.4%], 1 of 5 positive |
| sql/select20.at | generated | hand | 6797.8 | 17480.2 | 2.57x | 17636.4 | 2.59x | +0.9% | 21416 | 21416 | 3 % | [-1.5%..+11.0%], 3 of 5 positive | -0.7% [-2.6%..+43.4%], 1 of 5 positive |
| sql/select20.window | generated | hand | 6795.6 | 17820.7 | 2.62x | 18120.3 | 2.67x | +1.7% | 21416 | 21416 | 4 % | [-0.1%..+10.3%], 4 of 5 positive | -0.8% [-1.5%..+0.8%], 1 of 5 positive |
| sql/select20.scan | generated | control | 388.1 | 373.8 | 0.96x | 374.6 | 0.97x | +0.2% | 0 | 0 | 7 % | [-1.1%..+1.3%], 3 of 5 positive | -0.6% [-0.7%..+0.1%], 1 of 5 positive |
| tsql/select20.scan | generated | control | 372.4 | 382.3 | 1.03x | 373.9 | 1.00x | -2.2% | 0 | 0 | 3 % | [-4.7%..+0.7%], 2 of 5 positive | +1.0% [+0.0%..+1.0%], 5 of 5 positive |
| el/ladder.scan | generated | control | 138.7 | 135.5 | 0.98x | 136.8 | 0.99x | +0.9% | 0 | 0 | 5 % | [-0.5%..+1.1%], 3 of 5 positive | -0.5% [-1.2%..-0.4%], 0 of 5 positive |
| el/ladder.bool | generated | hand | 974.1 | 1692.9 | 1.74x | 1725.8 | 1.77x | +1.9% | 1776 | 1720 | 4 % | [+0.3%..+4.1%], 5 of 5 positive | +0.8% [-1.5%..+2.4%], 2 of 5 positive |
| sql/refused-late.bool | generated | hand | 2557.0 | 12019.1 | 4.70x | 5835.4 | 2.28x | -51.4% | 13552 | 6616 | 4 % | [-52.4%..-48.8%], 0 of 5 positive | -51.5% [-53.5%..-50.8%], 0 of 5 positive |
| sql/select20.bool | generated | hand | 6734.2 | 17865.4 | 2.65x | 18453.8 | 2.74x | +3.3% | 21448 | 21392 | 4 % | [-2.5%..+11.8%], 4 of 5 positive | -0.4% [-3.1%..+2.1%], 3 of 5 positive |
| sql/refused-cliff-case-1391 | generated | control | 4505656.2 | 4541450.0 | 1.01x | 4515900.0 | 1.00x | -0.6% | 6276488 | 6276488 | 3 % | [-2.6%..+0.6%], 2 of 5 positive | +2.3% [-0.4%..+2.7%], 4 of 5 positive |
| sql/refused-cliff-case-1738 | generated | control | 10476750.0 | 8863750.0 | 0.85x | 6047825.0 | 0.58x | -31.8% | 74362500 | 7842262 | 90 % | [-51.5%..-28.0%], 0 of 5 positive |  |
| sql/refused-cliff-joins-891 | generated | control | 2514450.0 | 2661618.8 | 1.06x | 2550296.9 | 1.01x | -4.2% | 2994166 | 2994123 | 6 % | [-7.0%..+1.7%], 1 of 5 positive | -0.2% [-2.7%..+1.6%], 3 of 5 positive |
| sql/refused-cliff-joins-1113 | generated | control | 5034168.8 | 5676531.2 | 1.13x | 3218618.8 | 0.64x | -43.3% | 12948506 | 3740064 | 32 % | [-47.0%..-28.9%], 0 of 5 positive |  |
| sql/refused-cliff-joins-1738 | generated | control | 8882756.2 | 9212893.8 | 1.04x | 5092625.0 | 0.57x | -44.7% | 21143693 | 5840149 | 15 % | [-52.1%..-36.2%], 0 of 5 positive |  |
| sql/refused-cliff-joins-2172 | generated | control | 14760637.5 | 19207762.5 | 1.30x | 6724937.5 | 0.46x | -65.0% | 94724913 | 7298283 | 148 % | [-77.2%..-25.9%], 0 of 5 positive |  |
| sql/refused-cliff-paren-2715 | generated | control | 7517400.0 | 7640981.2 | 1.02x | 7264287.5 | 0.97x | -4.9% | 8778600 | 8778600 | 20 % | [-6.6%..+3.1%], 3 of 5 positive | -0.2% [-2.3%..+1.8%], 3 of 5 positive |
| sql/refused-cliff-and-2715 | generated | control | 7127525.0 | 7481362.5 | 1.05x | 7421575.0 | 1.04x | -0.8% | 8775448 | 8775424 | 25 % | [-6.6%..+6.0%], 3 of 5 positive | +1.4% [-0.3%..+2.5%], 4 of 5 positive |
| sql/refused-cliff-paren-3393 | generated | control | 13560050.0 | 11311700.0 | 0.83x | 8166800.0 | 0.60x | -27.8% | 94857772 | 10969723 | 169 % | [-76.4%..-27.7%], 0 of 5 positive |  |
| sql/refused-cliff-and-3393 | generated | control | 14270325.0 | 12905475.0 | 0.90x | 8039500.0 | 0.56x | -37.7% | 94854700 | 10966571 | 110 % | [-70.1%..-26.2%], 0 of 5 positive |  |
| el/parse-200k-tokens | generated | control | 15258050.0 | 15848775.0 | 1.04x | 15902575.0 | 1.04x | +0.3% | 16801614 | 16801528 | 3 % | [-7.8%..+4.5%], 3 of 5 positive |  |
| el/parse-500k-tokens | generated | control | 41456550.0 | 41086900.0 | 0.99x | 40124600.0 | 0.97x | -2.3% | 49503175 | 42002327 | 6 % | [-12.2%..+4.2%], 2 of 5 positive |  |
| sql/worst-columns-100k | generated | control | 441747100.0 | 425730900.0 | 0.96x | 285668900.0 | 0.65x | -32.9% | 999698544 | 95920877 | 8 % | [-36.4%..-32.5%], 0 of 5 positive |  |
| fix/Orders128.yield-string | generated | hand | 79918.4 | 250093.6 | 3.13x | 256130.5 | 3.20x | +2.4% | 117936 | 117936 | 4 % | [-1.9%..+5.2%], 3 of 5 positive | 0.0% [-2.7%..+8.1%], 2 of 5 positive |
| web/media-type.quoted | generated | control | 246.1 | 299.1 | 1.22x | 282.9 | 1.15x | -5.4% | 816 | 816 | 3 % | [-9.4%..-2.3%], 0 of 5 positive | -0.1% [-4.1%..+1.6%], 3 of 5 positive |
| el/ladder | generated | hand | 979.8 | 1712.2 | 1.75x | 1750.6 | 1.79x | +2.2% | 1776 | 1776 | 5 % | [-0.3%..+3.0%], 4 of 5 positive | +1.9% [-1.8%..+7.6%], 3 of 5 positive |
| el/ladder | immediate | hand | 979.8 | 1144.3 | 1.17x | 1149.4 | 1.17x | +0.4% | 1784 | 1784 | 5 % | [-0.5%..+2.0%], 3 of 5 positive | -1.0% [-2.4%..+0.8%], 1 of 5 positive |
| el/terms1000 | generated | hand | 104069.6 | 143452.0 | 1.38x | 142728.5 | 1.37x | -0.5% | 169104 | 169104 | 5 % | [-1.7%..+0.7%], 1 of 5 positive | +0.2% [-2.0%..+1.1%], 3 of 5 positive |
| el/terms1000 | immediate | hand | 104069.6 | 116446.3 | 1.12x | 116306.1 | 1.12x | -0.1% | 177120 | 177120 | 5 % | [-2.7%..+1.7%], 4 of 5 positive | -1.7% [-3.5%..-1.1%], 0 of 5 positive |
| sql/select20 | generated | hand | 6709.5 | 17800.7 | 2.65x | 17922.6 | 2.67x | +0.7% | 21448 | 21448 | 2 % | [-3.5%..+4.6%], 4 of 5 positive | -0.5% [-1.6%..+1.3%], 2 of 5 positive |
| sql/refused-late | generated | hand | 2579.7 | 12026.4 | 4.66x | 11901.7 | 4.61x | -1.0% | 13552 | 13552 | 3 % | [-1.9%..+2.9%], 2 of 5 positive | -1.0% [-3.2%..+2.5%], 1 of 5 positive |
