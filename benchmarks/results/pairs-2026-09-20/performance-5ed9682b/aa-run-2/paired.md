# Paired stand, 2026-09-20 21:44

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Orders128.text | generated | hand | 81955.1 | 72132.4 | 0.88x | 69886.3 | 0.85x | -3.1% | 129112 | 129112 | 35 % |  |
| fix/Orders128.bytes | generated | hand | 81320.7 | 82651.2 | 1.02x | 81437.1 | 1.00x | -1.5% | 129168 | 129168 | 4 % |  |
| fix/Orders128.stream | generated | hand | 117211.3 | 91118.7 | 0.78x | 91185.1 | 0.78x | +0.1% | 118552 | 118552 | 6 % |  |
| fixmsg/Order.parse | generated | control | 1197.8 | 1862.0 | 1.55x | 1869.4 | 1.56x | +0.4% | 3400 | 3400 | 6 % |  |
| fixmsg/Order.build | generated | control | 1514.6 | 2353.2 | 1.55x | 2369.0 | 1.56x | +0.7% | 3592 | 3592 | 4 % |  |
| fix/orders400.text | generated | hand | 252057.6 | 226219.3 | 0.90x | 218657.4 | 0.87x | -3.3% | 403288 | 403288 | 2 % |  |
| tsql/script100.bool | generated | scriptdom | 669875.0 | 123925.0 | 0.18x | 126656.2 | 0.19x | +2.2% | 113600 | 105600 | 21 % |  |
| tsql/script100.boolboth | generated | scriptdom | 667461.7 | 123537.5 | 0.19x | 123000.0 | 0.18x | -0.4% | 105600 | 105600 | 16 % |  |
| tsql/script100 | generated | scriptdom | 668105.5 | 123870.3 | 0.19x | 125218.0 | 0.19x | +1.1% | 113600 | 113600 | 3 % |  |
| tsql/columns1000 | generated | scriptdom | 1749868.8 | 190695.3 | 0.11x | 194095.3 | 0.11x | +1.8% | 200464 | 200464 | 25 % |  |
| web/json.array10000 | generated | hand | 171909.0 | 198767.0 | 1.16x | 189454.1 | 1.10x | -4.7% | 720048 | 720048 | 6 % |  |
| web/json.object10000 | generated | hand | 784990.6 | 1306850.8 | 1.66x | 1373993.8 | 1.75x | +5.1% | 1599240 | 1599240 | 150 % |  |
| web/url.path1000 | generated | hand | 9670.8 | 11444.5 | 1.18x | 11198.5 | 1.16x | -2.1% | 8384 | 8384 | 10 % |  |
| web/media-type.params1000 | generated | control | 47434.3 | 44742.7 | 0.94x | 44952.2 | 0.95x | +0.5% | 151472 | 151472 | 7 % |  |
| web/sf.list10000 | generated | control | 1185989.1 | 1262200.0 | 1.06x | 1276510.9 | 1.08x | +1.1% | 2720056 | 2720056 | 22 % |  |
| sql/select20.at | generated | hand | 6988.5 | 18029.6 | 2.58x | 17750.8 | 2.54x | -1.5% | 21416 | 21440 | 10 % |  |
| sql/select20.window | generated | hand | 6868.4 | 18594.8 | 2.71x | 18225.0 | 2.65x | -2.0% | 21416 | 21416 | 7 % |  |
| sql/select20.scan | generated | control | 391.5 | 397.0 | 1.01x | 385.7 | 0.99x | -2.8% | 0 | 0 | 8 % |  |
| tsql/select20.scan | generated | control | 402.2 | 383.6 | 0.95x | 386.0 | 0.96x | +0.7% | 0 | 0 | 15 % |  |
| el/ladder.scan | generated | control | 140.4 | 139.3 | 0.99x | 137.9 | 0.98x | -1.0% | 0 | 0 | 77 % |  |
| fixmsg/Order.parse-stream | generated | control | 1966.8 | 2188.4 | 1.11x | 2204.8 | 1.12x | +0.8% | 8224 | 8224 | 4 % |  |
| fixmsg/Order.parse-reader | generated | control | 1940.3 | 2147.4 | 1.11x | 2129.2 | 1.10x | -0.8% | 12112 | 12112 | 3 % |  |
| fixmsg/Order.parse-span | generated | control | 1954.9 | 1876.8 | 0.96x | 1888.8 | 0.97x | +0.6% | 3672 | 3672 | 4 % |  |
| fixmsg/Order.read-stream100 | generated | control | 203552.7 | 201940.8 | 0.99x | 203081.1 | 1.00x | +0.6% | 389248 | 389248 | 6 % |  |
| fixmsg/Order.read-reader100 | generated | control | 204573.4 | 198670.9 | 0.97x | 198894.1 | 0.97x | +0.1% | 375712 | 375712 | 16 % |  |
| fix/Orders128.log-text | generated | hand | 92062.8 | 75132.1 | 0.82x | 72585.4 | 0.79x | -3.4% | 129112 | 129112 | 7 % |  |
| fix/Orders128.log-bytes | generated | hand | 92828.5 | 97228.7 | 1.05x | 97204.1 | 1.05x | 0.0% | 129168 | 129168 | 10 % |  |
| fix/Orders128.log-stream | generated | hand | 92581.4 | 104219.1 | 1.13x | 104072.6 | 1.12x | -0.1% | 118552 | 118552 | 4 % |  |
| el/ladder.bool | generated | hand | 1020.3 | 1746.5 | 1.71x | 1850.2 | 1.81x | +5.9% | 1776 | 1720 | 16 % |  |
| sql/refused-late.bool | generated | hand | 2639.9 | 12608.2 | 4.78x | 5901.4 | 2.24x | -53.2% | 13552 | 6616 | 13 % |  |
| sql/select20.bool | generated | hand | 7064.6 | 18943.9 | 2.68x | 18543.3 | 2.62x | -2.1% | 21448 | 21392 | 60 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4407268.8 | 4629646.9 | 1.05x | 4584250.0 | 1.04x | -1.0% | 6276488 | 6276684 | 5 % |  |
| sql/refused-cliff-case-1738 | generated | control | 21140900.0 | 24840200.0 | 1.17x | 22842150.0 | 1.08x | -8.0% | 74363543 | 74363469 | 130 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2598096.9 | 2689000.0 | 1.03x | 2525471.9 | 0.97x | -6.1% | 2994056 | 2994056 | 22 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 6435487.5 | 6483337.5 | 1.01x | 6534900.0 | 1.02x | +0.8% | 12948785 | 12948794 | 32 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 10071837.5 | 10493743.8 | 1.04x | 9763193.8 | 0.97x | -7.0% | 21144054 | 21144119 | 40 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 29709950.0 | 30643100.0 | 1.03x | 30569425.0 | 1.03x | -0.2% | 94726298 | 94726279 | 55 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6812887.5 | 6971350.0 | 1.02x | 6711662.5 | 0.99x | -3.7% | 8778576 | 8778600 | 13 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6674506.2 | 6647575.0 | 1.00x | 6639943.8 | 0.99x | -0.1% | 8775448 | 8775448 | 11 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 23757975.0 | 29065600.0 | 1.22x | 29509675.0 | 1.24x | +1.5% | 94860398 | 94860424 | 107 % |  |
| sql/refused-cliff-and-3393 | generated | control | 27993200.0 | 28209375.0 | 1.01x | 24603375.0 | 0.88x | -12.8% | 94857362 | 94857776 | 102 % |  |
| fix/Orders128.yield-reader | generated | hand | 149895.3 | 99689.1 | 0.67x | 100819.1 | 0.67x | +1.1% | 118520 | 118520 | 20 % |  |
| fix/Orders128.yield-string | generated | hand | 81301.4 | 260512.1 | 3.20x | 270000.7 | 3.32x | +3.6% | 117936 | 117936 | 13 % |  |
| fix/Orders128.yield-memory | generated | hand | 80188.4 | 85122.9 | 1.06x | 85704.8 | 1.07x | +0.7% | 118016 | 118016 | 6 % |  |
| fix/Orders128.whole-stream | generated | hand | 117889.6 | 82709.8 | 0.70x | 83255.3 | 0.71x | +0.7% | 129368 | 129368 | 8 % |  |
| fix/Orders128.whole-reader | generated | hand | 118091.3 | 87976.7 | 0.74x | 87925.4 | 0.74x | -0.1% | 129336 | 129336 | 13 % |  |
| feeds/stock-count.good.text | generated | hand | 37786.5 | 29540.5 | 0.78x | 29441.8 | 0.78x | -0.3% | 111403 | 111403 | 14 % |  |
| feeds/stock-count.good.reader | generated | hand | 44969.7 | 37191.7 | 0.83x | 37142.0 | 0.83x | -0.1% | 111488 | 111579 | 4 % |  |
| feeds/stock-count.good.reader64 | generated | hand | 45793.1 | 41489.2 | 0.91x | 42371.3 | 0.93x | +2.1% | 111536 | 111627 | 29 % |  |
| web/url.plain | generated | hand | 97.0 | 224.6 | 2.32x | 225.5 | 2.32x | +0.4% | 360 | 360 | 14 % |  |
| web/url.full | generated | hand | 173.4 | 304.7 | 1.76x | 295.5 | 1.70x | -3.0% | 536 | 536 | 13 % |  |
| web/url.ipv4 | generated | hand | 112.1 | 233.6 | 2.08x | 233.8 | 2.09x | +0.1% | 384 | 384 | 4 % |  |
| web/url.long-path | generated | hand | 176.4 | 421.2 | 2.39x | 417.3 | 2.37x | -0.9% | 512 | 512 | 7 % |  |
| web/url.refused | generated | hand | 67.8 | 200.0 | 2.95x | 197.5 | 2.91x | -1.3% | 64 | 64 | 7 % |  |
| web/url.relative | generated | hand | 76.6 | 193.1 | 2.52x | 195.4 | 2.55x | +1.2% | 312 | 312 | 18 % |  |
| web/url.relative-dot-colon | generated | hand | 64.4 | 170.6 | 2.65x | 171.9 | 2.67x | +0.8% | 280 | 280 | 8 % |  |
| web/url.relative-letters | generated | hand | 106.0 | 232.8 | 2.20x | 238.3 | 2.25x | +2.3% | 312 | 312 | 5 % |  |
| web/json.object | generated | hand | 595.4 | 929.9 | 1.56x | 936.4 | 1.57x | +0.7% | 2584 | 2584 | 4 % |  |
| web/json.array | generated | hand | 599.6 | 734.6 | 1.23x | 758.3 | 1.26x | +3.2% | 2400 | 2400 | 10 % |  |
| web/media-type.plain | generated | control | 162.7 | 200.2 | 1.23x | 228.8 | 1.41x | +14.3% | 448 | 448 | 18 % |  |
| web/media-type.quoted | generated | control | 253.2 | 310.4 | 1.23x | 327.1 | 1.29x | +5.4% | 816 | 816 | 15 % |  |
| web/media-type.refused | generated | control | 45.4 | 80.7 | 1.78x | 81.2 | 1.79x | +0.6% | 64 | 64 | 22 % |  |
| web/addr-spec.plain | generated | control | 87.9 | 120.0 | 1.37x | 121.7 | 1.38x | +1.4% | 176 | 176 | 14 % |  |
| web/addr-spec.refused | generated | control | 25.2 | 62.8 | 2.49x | 64.8 | 2.57x | +3.1% | 64 | 64 | 20 % |  |
| web/json-patch.full | generated | control | 1409.8 | 1457.4 | 1.03x | 1525.6 | 1.08x | +4.7% | 4552 | 4552 | 27 % |  |
| web/link.full | generated | control | 1549.3 | 1548.5 | 1.00x | 1577.2 | 1.02x | +1.9% | 2968 | 2968 | 24 % |  |
| web/link.refused | generated | control | 21.0 | 54.6 | 2.60x | 55.4 | 2.64x | +1.5% | 64 | 64 | 21 % |  |
| web/sf.item | generated | control | 260.9 | 317.1 | 1.22x | 299.3 | 1.15x | -5.6% | 800 | 800 | 18 % |  |
| web/sf.list | generated | control | 1216.0 | 1301.7 | 1.07x | 1326.1 | 1.09x | +1.9% | 3064 | 3064 | 15 % |  |
| web/sf.dictionary | generated | control | 1220.5 | 1254.7 | 1.03x | 1257.9 | 1.03x | +0.2% | 3280 | 3280 | 3 % |  |
| el/ladder | generated | hand | 1002.0 | 1763.6 | 1.76x | 1878.1 | 1.87x | +6.5% | 1776 | 1776 | 8 % |  |
| el/ladder | immediate | hand | 1002.0 | 1179.7 | 1.18x | 1168.9 | 1.17x | -0.9% | 1784 | 1784 | 8 % |  |
| el/terms1000 | generated | hand | 110251.8 | 153388.1 | 1.39x | 152055.2 | 1.38x | -0.9% | 169107 | 169104 | 10 % |  |
| el/terms1000 | immediate | hand | 110251.8 | 127979.2 | 1.16x | 126711.9 | 1.15x | -1.0% | 177120 | 177120 | 10 % |  |
| sql/select20 | generated | hand | 6926.3 | 18532.6 | 2.68x | 18321.0 | 2.65x | -1.1% | 21448 | 21448 | 13 % |  |
| sql/refused-late | generated | hand | 2580.4 | 12290.0 | 4.76x | 12002.6 | 4.65x | -2.3% | 13552 | 13552 | 23 % |  |
