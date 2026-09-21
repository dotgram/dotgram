# Paired stand, 2026-09-20 22:28

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.9 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Orders128.text | generated | hand | 84689.5 | 68566.0 | 0.81x | 78284.4 | 0.92x | +14.2% | 129112 | 129112 | 47 % |  |
| fix/Orders128.bytes | generated | hand | 85393.0 | 83019.5 | 0.97x | 86593.0 | 1.01x | +4.3% | 129168 | 129168 | 9 % |  |
| fix/Orders128.stream | generated | hand | 119458.0 | 93553.4 | 0.78x | 94921.2 | 0.79x | +1.5% | 118552 | 118552 | 17 % |  |
| fixmsg/Order.parse | generated | control | 1254.0 | 1848.2 | 1.47x | 1909.1 | 1.52x | +3.3% | 3400 | 3400 | 8 % |  |
| fixmsg/Order.build | generated | control | 1632.1 | 2392.9 | 1.47x | 2420.9 | 1.48x | +1.2% | 3592 | 3592 | 22 % |  |
| fix/orders400.text | generated | hand | 280046.1 | 229494.5 | 0.82x | 256660.2 | 0.92x | +11.8% | 403288 | 403288 | 43 % |  |
| tsql/script100.bool | generated | scriptdom | 682215.6 | 127587.5 | 0.19x | 129175.0 | 0.19x | +1.2% | 113600 | 105600 | 29 % |  |
| tsql/script100.boolboth | generated | scriptdom | 679335.2 | 131251.6 | 0.19x | 124027.3 | 0.18x | -5.5% | 105600 | 105600 | 15 % |  |
| tsql/script100 | generated | scriptdom | 690553.9 | 132602.3 | 0.19x | 129430.5 | 0.19x | -2.4% | 113600 | 113600 | 14 % |  |
| tsql/columns1000 | generated | scriptdom | 1782823.4 | 196948.4 | 0.11x | 194337.5 | 0.11x | -1.3% | 200464 | 200464 | 5 % |  |
| web/json.array10000 | generated | hand | 174352.1 | 196680.3 | 1.13x | 197878.1 | 1.13x | +0.6% | 720048 | 720048 | 3 % |  |
| web/json.object10000 | generated | hand | 670290.6 | 1355201.6 | 2.02x | 1344423.4 | 2.01x | -0.8% | 1599240 | 1599241 | 99 % |  |
| web/url.path1000 | generated | hand | 9455.3 | 11177.9 | 1.18x | 11498.8 | 1.22x | +2.9% | 8384 | 8384 | 8 % |  |
| web/media-type.params1000 | generated | control | 48300.1 | 45674.7 | 0.95x | 46588.4 | 0.96x | +2.0% | 151472 | 151472 | 29 % |  |
| web/sf.list10000 | generated | control | 1225864.1 | 1273543.8 | 1.04x | 1254700.0 | 1.02x | -1.5% | 2720056 | 2720056 | 25 % |  |
| sql/select20.at | generated | hand | 7251.0 | 18260.0 | 2.52x | 19631.5 | 2.71x | +7.5% | 21416 | 21440 | 27 % |  |
| sql/select20.window | generated | hand | 6948.7 | 18373.0 | 2.64x | 18890.3 | 2.72x | +2.8% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 383.9 | 386.3 | 1.01x | 387.3 | 1.01x | +0.3% | 0 | 0 | 14 % |  |
| tsql/select20.scan | generated | control | 386.3 | 396.6 | 1.03x | 387.4 | 1.00x | -2.3% | 0 | 0 | 18 % |  |
| el/ladder.scan | generated | control | 144.4 | 144.2 | 1.00x | 148.2 | 1.03x | +2.8% | 0 | 0 | 9 % |  |
| fixmsg/Order.parse-stream | generated | control | 2040.6 | 2358.8 | 1.16x | 2345.7 | 1.15x | -0.6% | 8224 | 8224 | 12 % |  |
| fixmsg/Order.parse-reader | generated | control | 1992.0 | 2098.8 | 1.05x | 2124.3 | 1.07x | +1.2% | 12112 | 12112 | 10 % |  |
| fixmsg/Order.parse-span | generated | control | 2018.7 | 1859.2 | 0.92x | 1883.1 | 0.93x | +1.3% | 3672 | 3672 | 6 % |  |
| fixmsg/Order.read-stream100 | generated | control | 207845.9 | 207823.4 | 1.00x | 203224.4 | 0.98x | -2.2% | 389248 | 389248 | 12 % |  |
| fixmsg/Order.read-reader100 | generated | control | 205537.3 | 190760.7 | 0.93x | 194921.5 | 0.95x | +2.2% | 375712 | 375712 | 10 % |  |
| fix/Orders128.log-text | generated | hand | 94138.1 | 70561.8 | 0.75x | 76601.3 | 0.81x | +8.6% | 129112 | 129112 | 11 % |  |
| fix/Orders128.log-bytes | generated | hand | 94920.5 | 94543.8 | 1.00x | 95954.4 | 1.01x | +1.5% | 129168 | 129168 | 5 % |  |
| fix/Orders128.log-stream | generated | hand | 96544.5 | 103068.0 | 1.07x | 102871.6 | 1.07x | -0.2% | 118552 | 118552 | 10 % |  |
| el/ladder.bool | generated | hand | 1002.2 | 1742.5 | 1.74x | 1710.7 | 1.71x | -1.8% | 1776 | 1720 | 7 % |  |
| sql/refused-late.bool | generated | hand | 2649.4 | 12221.1 | 4.61x | 6061.5 | 2.29x | -50.4% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 7289.0 | 18783.5 | 2.58x | 19503.1 | 2.68x | +3.8% | 21448 | 21392 | 15 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4628975.0 | 4596568.8 | 0.99x | 4668000.0 | 1.01x | +1.6% | 6276488 | 6276660 | 11 % |  |
| sql/refused-cliff-case-1738 | generated | control | 26390750.0 | 31121150.0 | 1.18x | 28234325.0 | 1.07x | -9.3% | 74363545 | 74363472 | 86 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2708112.5 | 2644906.2 | 0.98x | 2706818.8 | 1.00x | +2.3% | 2994056 | 2994123 | 11 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 6558956.2 | 6835825.0 | 1.04x | 6893525.0 | 1.05x | +0.8% | 12948799 | 12948798 | 20 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 10394406.2 | 10331887.5 | 0.99x | 10614700.0 | 1.02x | +2.7% | 21144103 | 21144092 | 25 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 30794300.0 | 30737500.0 | 1.00x | 31144400.0 | 1.01x | +1.3% | 94726581 | 94726889 | 60 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6656975.0 | 6492887.5 | 0.98x | 6577862.5 | 0.99x | +1.3% | 8778600 | 8778600 | 9 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6509362.5 | 6550356.2 | 1.01x | 6445243.8 | 0.99x | -1.6% | 8775404 | 8775448 | 5 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 27181375.0 | 27455350.0 | 1.01x | 27781100.0 | 1.02x | +1.2% | 94863441 | 94860219 | 90 % |  |
| sql/refused-cliff-and-3393 | generated | control | 23771750.0 | 25472900.0 | 1.07x | 24997800.0 | 1.05x | -1.9% | 94857636 | 94857683 | 90 % |  |
| fix/Orders128.yield-reader | generated | hand | 140754.5 | 91567.8 | 0.65x | 96510.4 | 0.69x | +5.4% | 118520 | 118520 | 4 % |  |
| fix/Orders128.yield-string | generated | hand | 84239.5 | 273045.5 | 3.24x | 285186.5 | 3.39x | +4.4% | 117936 | 117936 | 4 % |  |
| fix/Orders128.yield-memory | generated | hand | 83054.8 | 86240.3 | 1.04x | 85844.6 | 1.03x | -0.5% | 118016 | 118016 | 2 % |  |
| fix/Orders128.whole-stream | generated | hand | 117058.4 | 83124.5 | 0.71x | 84597.2 | 0.72x | +1.8% | 129368 | 129368 | 7 % |  |
| fix/Orders128.whole-reader | generated | hand | 117730.2 | 83676.6 | 0.71x | 90894.1 | 0.77x | +8.6% | 129336 | 129336 | 41 % |  |
| feeds/stock-count.good.text | generated | hand | 41625.5 | 30060.3 | 0.72x | 29744.7 | 0.71x | -1.0% | 111312 | 111403 | 7 % |  |
| feeds/stock-count.good.reader | generated | hand | 47783.0 | 37305.1 | 0.78x | 37230.8 | 0.78x | -0.2% | 111488 | 111579 | 8 % |  |
| feeds/stock-count.good.reader64 | generated | hand | 50240.7 | 41397.0 | 0.82x | 41334.5 | 0.82x | -0.2% | 111536 | 111627 | 56 % |  |
| web/url.plain | generated | hand | 98.0 | 223.1 | 2.28x | 225.9 | 2.30x | +1.3% | 360 | 360 | 15 % |  |
| web/url.full | generated | hand | 168.3 | 307.6 | 1.83x | 290.8 | 1.73x | -5.5% | 536 | 536 | 4 % |  |
| web/url.ipv4 | generated | hand | 112.6 | 231.6 | 2.06x | 234.0 | 2.08x | +1.1% | 384 | 384 | 8 % |  |
| web/url.long-path | generated | hand | 180.0 | 421.6 | 2.34x | 425.4 | 2.36x | +0.9% | 512 | 512 | 6 % |  |
| web/url.refused | generated | hand | 69.7 | 199.8 | 2.87x | 198.4 | 2.85x | -0.7% | 64 | 64 | 12 % |  |
| web/url.relative | generated | hand | 77.4 | 200.8 | 2.60x | 205.4 | 2.65x | +2.3% | 312 | 312 | 19 % |  |
| web/url.relative-dot-colon | generated | hand | 64.2 | 172.5 | 2.69x | 178.9 | 2.79x | +3.7% | 280 | 280 | 9 % |  |
| web/url.relative-letters | generated | hand | 107.4 | 234.2 | 2.18x | 245.5 | 2.29x | +4.8% | 312 | 312 | 18 % |  |
| web/json.object | generated | hand | 588.6 | 950.0 | 1.61x | 932.8 | 1.58x | -1.8% | 2584 | 2584 | 13 % |  |
| web/json.array | generated | hand | 633.2 | 752.8 | 1.19x | 773.8 | 1.22x | +2.8% | 2400 | 2400 | 13 % |  |
| web/media-type.plain | generated | control | 166.1 | 211.5 | 1.27x | 199.0 | 1.20x | -5.9% | 448 | 448 | 8 % |  |
| web/media-type.quoted | generated | control | 255.0 | 301.6 | 1.18x | 302.3 | 1.19x | +0.2% | 816 | 816 | 6 % |  |
| web/media-type.refused | generated | control | 46.0 | 82.2 | 1.79x | 81.8 | 1.78x | -0.5% | 64 | 64 | 4 % |  |
| web/addr-spec.plain | generated | control | 88.6 | 126.2 | 1.42x | 125.2 | 1.41x | -0.8% | 176 | 176 | 14 % |  |
| web/addr-spec.refused | generated | control | 25.9 | 62.7 | 2.42x | 71.3 | 2.75x | +13.7% | 64 | 64 | 20 % |  |
| web/json-patch.full | generated | control | 1452.6 | 1523.1 | 1.05x | 1509.9 | 1.04x | -0.9% | 4552 | 4552 | 33 % |  |
| web/link.full | generated | control | 1316.6 | 1396.8 | 1.06x | 1415.6 | 1.08x | +1.3% | 2968 | 2968 | 3 % |  |
| web/link.refused | generated | control | 20.8 | 54.5 | 2.62x | 54.9 | 2.64x | +0.8% | 64 | 64 | 46 % |  |
| web/sf.item | generated | control | 294.1 | 366.4 | 1.25x | 349.2 | 1.19x | -4.7% | 800 | 800 | 37 % |  |
| web/sf.list | generated | control | 1264.5 | 1308.1 | 1.03x | 1335.0 | 1.06x | +2.1% | 3064 | 3064 | 24 % |  |
| web/sf.dictionary | generated | control | 1326.3 | 1474.5 | 1.11x | 1394.2 | 1.05x | -5.4% | 3280 | 3280 | 19 % |  |
| el/ladder | generated | hand | 1021.4 | 1777.9 | 1.74x | 1777.4 | 1.74x | 0.0% | 1776 | 1776 | 9 % |  |
| el/ladder | immediate | hand | 1021.4 | 1226.9 | 1.20x | 1246.4 | 1.22x | +1.6% | 1784 | 1784 | 9 % |  |
| el/terms1000 | generated | hand | 114148.0 | 157231.4 | 1.38x | 154959.5 | 1.36x | -1.4% | 169104 | 169104 | 12 % |  |
| el/terms1000 | immediate | hand | 114148.0 | 132229.3 | 1.16x | 130034.7 | 1.14x | -1.7% | 177166 | 177144 | 12 % |  |
| sql/select20 | generated | hand | 6926.1 | 18437.5 | 2.66x | 19059.3 | 2.75x | +3.4% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2640.0 | 12272.5 | 4.65x | 12532.8 | 4.75x | +2.1% | 13552 | 13552 | 20 % |  |
