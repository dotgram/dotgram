# Paired stand, 2026-09-21 01:25

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.8 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 252625.0 | 232725.0 | 0.92x | 222825.0 | 0.88x | -4.3% | 403288 | 403288 | 72 % |  |
| tsql/columns1000 | generated | scriptdom | 1703075.0 | 177650.0 | 0.10x | 171850.0 | 0.10x | -3.3% | 200489 | 200489 | 6 % |  |
| web/json.array10000 | generated | hand | 194047.7 | 185821.1 | 0.96x | 185893.8 | 0.96x | 0.0% | 720048 | 720048 | 22 % |  |
| sql/select20.at | generated | hand | 6823.8 | 17549.2 | 2.57x | 17678.1 | 2.59x | +0.7% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 6862.2 | 17867.2 | 2.60x | 18147.3 | 2.64x | +1.6% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 388.9 | 386.8 | 0.99x | 382.7 | 0.98x | -1.1% | 0 | 0 | 13 % |  |
| tsql/select20.scan | generated | control | 381.2 | 382.3 | 1.00x | 384.9 | 1.01x | +0.7% | 0 | 0 | 17 % |  |
| el/ladder.scan | generated | control | 135.6 | 135.3 | 1.00x | 136.8 | 1.01x | +1.1% | 0 | 0 | 13 % |  |
| el/ladder.bool | generated | hand | 970.5 | 1699.7 | 1.75x | 1769.0 | 1.82x | +4.1% | 1776 | 1720 | 8 % |  |
| sql/refused-late.bool | generated | hand | 2590.5 | 12102.5 | 4.67x | 5835.4 | 2.25x | -51.8% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6950.0 | 17925.4 | 2.58x | 18459.2 | 2.66x | +3.0% | 21448 | 21392 | 11 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4505656.2 | 4490556.2 | 1.00x | 4515900.0 | 1.00x | +0.6% | 6276488 | 6276488 | 3 % |  |
| sql/refused-cliff-case-1738 | generated | control | 7933325.0 | 8506925.0 | 1.07x | 5835375.0 | 0.74x | -31.4% | 74362500 | 7842195 | 68 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2658262.5 | 2661618.8 | 1.00x | 2641975.0 | 0.99x | -0.7% | 2994166 | 2994056 | 17 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 5778084.4 | 6053621.9 | 1.05x | 3207150.0 | 0.56x | -47.0% | 12948505 | 3740043 | 62 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 9812106.2 | 8555318.8 | 0.87x | 5076675.0 | 0.52x | -40.7% | 21143714 | 5840044 | 36 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 31983637.5 | 19207762.5 | 0.60x | 6724937.5 | 0.21x | -65.0% | 94724852 | 7298283 | 83 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7508337.5 | 7640981.2 | 1.02x | 7881218.8 | 1.05x | +3.1% | 8778600 | 8778600 | 18 % |  |
| sql/refused-cliff-and-2715 | generated | control | 8041125.0 | 7654543.8 | 0.95x | 7211193.8 | 0.90x | -5.8% | 8775448 | 8775448 | 16 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 13560050.0 | 11209700.0 | 0.83x | 8081475.0 | 0.60x | -27.9% | 94857747 | 10969723 | 174 % |  |
| sql/refused-cliff-and-3393 | generated | control | 15110425.0 | 13753325.0 | 0.91x | 7939175.0 | 0.53x | -42.3% | 94854664 | 10966572 | 211 % |  |
| el/parse-200k-tokens | generated | control | 15469287.5 | 15848775.0 | 1.02x | 16561062.5 | 1.07x | +4.5% | 16801617 | 16801571 | 24 % |  |
| el/parse-500k-tokens | generated | control | 42826900.0 | 45679700.0 | 1.07x | 40124600.0 | 0.94x | -12.2% | 49503175 | 42002310 | 41 % |  |
| sql/worst-columns-100k | generated | control | 461094000.0 | 450312150.0 | 0.98x | 303055400.0 | 0.66x | -32.7% | 999698760 | 95920873 | 20 % |  |
| fix/Orders128.yield-string | generated | hand | 80225.0 | 267300.2 | 3.33x | 262212.5 | 3.27x | -1.9% | 117936 | 117936 | 7 % |  |
| web/media-type.quoted | generated | control | 247.4 | 328.0 | 1.33x | 297.2 | 1.20x | -9.4% | 816 | 816 | 3 % |  |
| el/ladder | generated | hand | 997.7 | 1743.9 | 1.75x | 1775.8 | 1.78x | +1.8% | 1776 | 1776 | 29 % |  |
| el/ladder | immediate | hand | 997.7 | 1169.8 | 1.17x | 1165.4 | 1.17x | -0.4% | 1784 | 1784 | 29 % |  |
| el/terms1000 | generated | hand | 107612.7 | 148925.3 | 1.38x | 150036.6 | 1.39x | +0.7% | 169104 | 169107 | 28 % |  |
| el/terms1000 | immediate | hand | 107612.7 | 119529.5 | 1.11x | 121544.1 | 1.13x | +1.7% | 177120 | 177120 | 28 % |  |
| sql/select20 | generated | hand | 6802.0 | 17800.7 | 2.62x | 18140.1 | 2.67x | +1.9% | 21448 | 21448 | 10 % |  |
| sql/refused-late | generated | hand | 2590.6 | 12026.4 | 4.64x | 11895.0 | 4.59x | -1.1% | 13552 | 13552 | 16 % |  |
