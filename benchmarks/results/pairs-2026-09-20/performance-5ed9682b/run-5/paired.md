# Paired stand, 2026-09-20 22:21

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.9 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Orders128.text | generated | hand | 81221.9 | 72111.3 | 0.89x | 72659.0 | 0.89x | +0.8% | 129112 | 129112 | 32 % |  |
| fix/Orders128.bytes | generated | hand | 80866.4 | 82555.9 | 1.02x | 83794.1 | 1.04x | +1.5% | 129168 | 129168 | 30 % |  |
| fix/Orders128.stream | generated | hand | 121605.3 | 94111.7 | 0.77x | 91358.8 | 0.75x | -2.9% | 118552 | 118552 | 14 % |  |
| fixmsg/Order.parse | generated | control | 1208.2 | 1836.8 | 1.52x | 1905.6 | 1.58x | +3.7% | 3400 | 3400 | 8 % |  |
| fixmsg/Order.build | generated | control | 1572.7 | 2393.9 | 1.52x | 2451.6 | 1.56x | +2.4% | 3592 | 3592 | 27 % |  |
| fix/orders400.text | generated | hand | 257887.3 | 231089.6 | 0.90x | 230649.6 | 0.89x | -0.2% | 403288 | 403288 | 22 % |  |
| tsql/script100.bool | generated | scriptdom | 686562.5 | 127881.2 | 0.19x | 128565.6 | 0.19x | +0.5% | 113600 | 105600 | 7 % |  |
| tsql/script100.boolboth | generated | scriptdom | 697236.7 | 132700.8 | 0.19x | 127984.4 | 0.18x | -3.6% | 105600 | 105600 | 11 % |  |
| tsql/script100 | generated | scriptdom | 696897.7 | 129239.1 | 0.19x | 129275.0 | 0.19x | 0.0% | 113600 | 113600 | 12 % |  |
| tsql/columns1000 | generated | scriptdom | 1815410.9 | 213784.4 | 0.12x | 194904.7 | 0.11x | -8.8% | 200464 | 200464 | 4 % |  |
| web/json.array10000 | generated | hand | 176043.9 | 194887.9 | 1.11x | 205031.8 | 1.16x | +5.2% | 720048 | 720048 | 7 % |  |
| web/json.object10000 | generated | hand | 764749.2 | 1027362.5 | 1.34x | 1105932.0 | 1.45x | +7.6% | 1599240 | 1599240 | 106 % |  |
| web/url.path1000 | generated | hand | 9643.1 | 11129.8 | 1.15x | 11175.7 | 1.16x | +0.4% | 8384 | 8384 | 20 % |  |
| web/media-type.params1000 | generated | control | 48064.7 | 45250.2 | 0.94x | 45401.3 | 0.94x | +0.3% | 151472 | 151472 | 7 % |  |
| web/sf.list10000 | generated | control | 1197653.1 | 1239928.1 | 1.04x | 1238103.1 | 1.03x | -0.1% | 2720056 | 2720056 | 13 % |  |
| sql/select20.at | generated | hand | 6900.2 | 18177.9 | 2.63x | 17970.8 | 2.60x | -1.1% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 6920.1 | 18733.6 | 2.71x | 18081.9 | 2.61x | -3.5% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 386.9 | 387.0 | 1.00x | 388.0 | 1.00x | +0.2% | 0 | 0 | 16 % |  |
| tsql/select20.scan | generated | control | 386.3 | 389.7 | 1.01x | 416.7 | 1.08x | +6.9% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 140.0 | 138.0 | 0.99x | 137.7 | 0.98x | -0.2% | 0 | 0 | 16 % |  |
| fixmsg/Order.parse-stream | generated | control | 1932.0 | 2184.5 | 1.13x | 2162.6 | 1.12x | -1.0% | 8224 | 8224 | 4 % |  |
| fixmsg/Order.parse-reader | generated | control | 1949.5 | 2113.1 | 1.08x | 2191.5 | 1.12x | +3.7% | 12112 | 12112 | 7 % |  |
| fixmsg/Order.parse-span | generated | control | 1979.4 | 1843.9 | 0.93x | 1881.2 | 0.95x | +2.0% | 3672 | 3672 | 18 % |  |
| fixmsg/Order.read-stream100 | generated | control | 208008.0 | 204526.6 | 0.98x | 204905.9 | 0.99x | +0.2% | 389248 | 389248 | 5 % |  |
| fixmsg/Order.read-reader100 | generated | control | 203410.9 | 193062.3 | 0.95x | 198067.0 | 0.97x | +2.6% | 375712 | 375712 | 5 % |  |
| fix/Orders128.log-text | generated | hand | 92343.4 | 72847.3 | 0.79x | 72701.3 | 0.79x | -0.2% | 129112 | 129112 | 12 % |  |
| fix/Orders128.log-bytes | generated | hand | 91268.6 | 95215.6 | 1.04x | 96484.1 | 1.06x | +1.3% | 129168 | 129168 | 7 % |  |
| fix/Orders128.log-stream | generated | hand | 91652.0 | 102566.2 | 1.12x | 103761.1 | 1.13x | +1.2% | 118552 | 118552 | 12 % |  |
| el/ladder.bool | generated | hand | 1032.3 | 1754.2 | 1.70x | 1764.6 | 1.71x | +0.6% | 1776 | 1720 | 17 % |  |
| sql/refused-late.bool | generated | hand | 2651.2 | 12439.3 | 4.69x | 5952.1 | 2.25x | -52.2% | 13552 | 6616 | 8 % |  |
| sql/select20.bool | generated | hand | 7151.5 | 18941.4 | 2.65x | 18633.6 | 2.61x | -1.6% | 21448 | 21392 | 9 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4593668.8 | 4707406.2 | 1.02x | 4648931.2 | 1.01x | -1.2% | 6276641 | 6276598 | 13 % |  |
| sql/refused-cliff-case-1738 | generated | control | 23196275.0 | 28807325.0 | 1.24x | 30224725.0 | 1.30x | +4.9% | 74363446 | 74363549 | 95 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2639859.4 | 2652550.0 | 1.00x | 2575493.8 | 0.98x | -2.9% | 2994056 | 2994056 | 16 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 5156668.8 | 5223768.8 | 1.01x | 5252075.0 | 1.02x | +0.5% | 12948797 | 12948780 | 74 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 10353581.2 | 11366937.5 | 1.10x | 9885618.8 | 0.95x | -13.0% | 21144192 | 21144096 | 61 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 31253675.0 | 31361800.0 | 1.00x | 30388750.0 | 0.97x | -3.1% | 94726174 | 94726922 | 63 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6562768.8 | 6786300.0 | 1.03x | 6594412.5 | 1.00x | -2.8% | 8778619 | 8778600 | 5 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6639118.8 | 6707581.2 | 1.01x | 6690831.2 | 1.01x | -0.2% | 8775448 | 8775448 | 7 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 30290100.0 | 29545850.0 | 0.98x | 27724550.0 | 0.92x | -6.2% | 94859967 | 94862077 | 141 % |  |
| sql/refused-cliff-and-3393 | generated | control | 27555050.0 | 36430000.0 | 1.32x | 28787425.0 | 1.04x | -21.0% | 94856951 | 94857253 | 84 % |  |
| fix/Orders128.yield-reader | generated | hand | 145996.7 | 97608.6 | 0.67x | 98379.3 | 0.67x | +0.8% | 118520 | 118520 | 10 % |  |
| fix/Orders128.yield-string | generated | hand | 85568.3 | 266975.5 | 3.12x | 297668.7 | 3.48x | +11.5% | 117936 | 117936 | 100 % |  |
| fix/Orders128.yield-memory | generated | hand | 80929.9 | 86459.6 | 1.07x | 86726.0 | 1.07x | +0.3% | 118016 | 118016 | 9 % |  |
| fix/Orders128.whole-stream | generated | hand | 118795.2 | 82836.0 | 0.70x | 83852.6 | 0.71x | +1.2% | 129368 | 129368 | 4 % |  |
| fix/Orders128.whole-reader | generated | hand | 118713.4 | 86621.3 | 0.73x | 86317.7 | 0.73x | -0.4% | 129336 | 129336 | 4 % |  |
| feeds/stock-count.good.text | generated | hand | 37069.6 | 29938.0 | 0.81x | 29868.5 | 0.81x | -0.2% | 111464 | 111403 | 75 % |  |
| feeds/stock-count.good.reader | generated | hand | 45572.5 | 38402.1 | 0.84x | 38365.8 | 0.84x | -0.1% | 111640 | 111579 | 81 % |  |
| feeds/stock-count.good.reader64 | generated | hand | 46793.8 | 42717.5 | 0.91x | 42813.1 | 0.91x | +0.2% | 111536 | 111627 | 11 % |  |
| web/url.plain | generated | hand | 98.4 | 227.0 | 2.31x | 229.1 | 2.33x | +0.9% | 360 | 360 | 7 % |  |
| web/url.full | generated | hand | 172.2 | 307.6 | 1.79x | 296.5 | 1.72x | -3.6% | 536 | 536 | 5 % |  |
| web/url.ipv4 | generated | hand | 115.9 | 238.0 | 2.05x | 239.6 | 2.07x | +0.7% | 384 | 384 | 6 % |  |
| web/url.long-path | generated | hand | 301.5 | 729.6 | 2.42x | 796.2 | 2.64x | +9.1% | 512 | 512 | 55 % |  |
| web/url.refused | generated | hand | 68.3 | 199.5 | 2.92x | 204.5 | 2.99x | +2.5% | 64 | 64 | 6 % |  |
| web/url.relative | generated | hand | 82.4 | 204.0 | 2.48x | 203.9 | 2.47x | 0.0% | 312 | 312 | 14 % |  |
| web/url.relative-dot-colon | generated | hand | 68.4 | 179.4 | 2.62x | 177.2 | 2.59x | -1.2% | 280 | 280 | 11 % |  |
| web/url.relative-letters | generated | hand | 118.7 | 254.5 | 2.14x | 251.7 | 2.12x | -1.1% | 312 | 312 | 10 % |  |
| web/json.object | generated | hand | 626.4 | 1003.8 | 1.60x | 1037.4 | 1.66x | +3.4% | 2584 | 2584 | 31 % |  |
| web/json.array | generated | hand | 619.2 | 779.3 | 1.26x | 775.3 | 1.25x | -0.5% | 2400 | 2400 | 11 % |  |
| web/media-type.plain | generated | control | 165.3 | 206.0 | 1.25x | 216.7 | 1.31x | +5.2% | 448 | 448 | 11 % |  |
| web/media-type.quoted | generated | control | 284.2 | 333.9 | 1.18x | 335.5 | 1.18x | +0.5% | 816 | 816 | 57 % |  |
| web/media-type.refused | generated | control | 48.4 | 86.2 | 1.78x | 87.9 | 1.82x | +1.9% | 64 | 64 | 41 % |  |
| web/addr-spec.plain | generated | control | 89.1 | 121.7 | 1.37x | 124.8 | 1.40x | +2.5% | 176 | 176 | 17 % |  |
| web/addr-spec.refused | generated | control | 24.9 | 60.8 | 2.44x | 60.7 | 2.43x | -0.2% | 64 | 64 | 5 % |  |
| web/json-patch.full | generated | control | 1393.8 | 1447.8 | 1.04x | 1434.7 | 1.03x | -0.9% | 4552 | 4552 | 6 % |  |
| web/link.full | generated | control | 1359.1 | 1460.3 | 1.07x | 1419.3 | 1.04x | -2.8% | 2968 | 2968 | 12 % |  |
| web/link.refused | generated | control | 25.8 | 67.2 | 2.60x | 66.9 | 2.59x | -0.5% | 64 | 64 | 53 % |  |
| web/sf.item | generated | control | 290.9 | 365.1 | 1.26x | 351.3 | 1.21x | -3.8% | 800 | 800 | 25 % |  |
| web/sf.list | generated | control | 1227.8 | 1336.9 | 1.09x | 1320.5 | 1.08x | -1.2% | 3064 | 3064 | 17 % |  |
| web/sf.dictionary | generated | control | 1254.8 | 1288.6 | 1.03x | 1331.9 | 1.06x | +3.4% | 3280 | 3280 | 7 % |  |
| el/ladder | generated | hand | 1035.9 | 1769.4 | 1.71x | 1824.5 | 1.76x | +3.1% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 1035.9 | 1230.1 | 1.19x | 1207.1 | 1.17x | -1.9% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 121115.8 | 159622.9 | 1.32x | 169630.7 | 1.40x | +6.3% | 169104 | 169104 | 10 % |  |
| el/terms1000 | immediate | hand | 121115.8 | 132476.3 | 1.09x | 135126.6 | 1.12x | +2.0% | 177120 | 177123 | 10 % |  |
| sql/select20 | generated | hand | 7232.0 | 19533.3 | 2.70x | 18601.3 | 2.57x | -4.8% | 21448 | 21448 | 14 % |  |
| sql/refused-late | generated | hand | 2750.8 | 12820.4 | 4.66x | 12565.9 | 4.57x | -2.0% | 13552 | 13552 | 10 % |  |
