# Paired stand, 2026-09-20 21:29

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Orders128.text | generated | hand | 81513.7 | 71838.7 | 0.88x | 71991.0 | 0.88x | +0.2% | 129112 | 129112 | 33 % |  |
| fix/Orders128.bytes | generated | hand | 79910.2 | 80920.3 | 1.01x | 81572.7 | 1.02x | +0.8% | 129168 | 129168 | 12 % |  |
| fix/Orders128.stream | generated | hand | 116746.6 | 90624.2 | 0.78x | 90450.5 | 0.77x | -0.2% | 118552 | 118552 | 3 % |  |
| fixmsg/Order.parse | generated | control | 1236.1 | 1825.4 | 1.48x | 1828.6 | 1.48x | +0.2% | 3400 | 3400 | 5 % |  |
| fixmsg/Order.build | generated | control | 1554.7 | 2319.8 | 1.49x | 2324.4 | 1.50x | +0.2% | 3592 | 3592 | 3 % |  |
| fix/orders400.text | generated | hand | 267628.3 | 226702.9 | 0.85x | 232689.3 | 0.87x | +2.6% | 403288 | 403288 | 21 % |  |
| tsql/script100.bool | generated | scriptdom | 662034.4 | 125450.0 | 0.19x | 126331.2 | 0.19x | +0.7% | 113600 | 105600 | 5 % |  |
| tsql/script100.boolboth | generated | scriptdom | 673135.2 | 127776.6 | 0.19x | 126329.7 | 0.19x | -1.1% | 105600 | 105603 | 18 % |  |
| tsql/script100 | generated | scriptdom | 681375.0 | 127705.5 | 0.19x | 126311.7 | 0.19x | -1.1% | 113600 | 113600 | 8 % |  |
| tsql/columns1000 | generated | scriptdom | 1779381.2 | 192945.3 | 0.11x | 193420.3 | 0.11x | +0.2% | 200464 | 200464 | 4 % |  |
| web/json.array10000 | generated | hand | 173520.3 | 200466.4 | 1.16x | 202533.6 | 1.17x | +1.0% | 720048 | 720048 | 7 % |  |
| web/json.object10000 | generated | hand | 649531.2 | 1142240.6 | 1.76x | 1429358.6 | 2.20x | +25.1% | 1599240 | 1599240 | 104 % |  |
| web/url.path1000 | generated | hand | 9393.1 | 10995.0 | 1.17x | 10956.2 | 1.17x | -0.4% | 8384 | 8384 | 2 % |  |
| web/media-type.params1000 | generated | control | 48405.3 | 45571.5 | 0.94x | 46188.9 | 0.95x | +1.4% | 151472 | 151472 | 8 % |  |
| web/sf.list10000 | generated | control | 1196890.6 | 1231060.9 | 1.03x | 1208606.2 | 1.01x | -1.8% | 2720056 | 2720056 | 12 % |  |
| sql/select20.at | generated | hand | 7047.1 | 18165.7 | 2.58x | 17467.6 | 2.48x | -3.8% | 21416 | 21440 | 28 % |  |
| sql/select20.window | generated | hand | 6985.7 | 18291.4 | 2.62x | 18044.1 | 2.58x | -1.4% | 21416 | 21440 | 12 % |  |
| sql/select20.scan | generated | control | 379.5 | 387.3 | 1.02x | 392.9 | 1.04x | +1.5% | 0 | 0 | 13 % |  |
| tsql/select20.scan | generated | control | 389.1 | 397.8 | 1.02x | 395.9 | 1.02x | -0.5% | 0 | 0 | 15 % |  |
| el/ladder.scan | generated | control | 142.4 | 141.4 | 0.99x | 142.5 | 1.00x | +0.8% | 0 | 0 | 10 % |  |
| fixmsg/Order.parse-stream | generated | control | 1986.6 | 2230.5 | 1.12x | 2237.1 | 1.13x | +0.3% | 8224 | 8224 | 10 % |  |
| fixmsg/Order.parse-reader | generated | control | 2011.8 | 2182.7 | 1.08x | 2211.4 | 1.10x | +1.3% | 12112 | 12112 | 31 % |  |
| fixmsg/Order.parse-span | generated | control | 2001.9 | 1884.2 | 0.94x | 1909.8 | 0.95x | +1.4% | 3672 | 3672 | 11 % |  |
| fixmsg/Order.read-stream100 | generated | control | 213068.9 | 208686.9 | 0.98x | 210528.7 | 0.99x | +0.9% | 389248 | 389248 | 15 % |  |
| fixmsg/Order.read-reader100 | generated | control | 210973.2 | 197273.8 | 0.94x | 197634.0 | 0.94x | +0.2% | 375712 | 375712 | 9 % |  |
| fix/Orders128.log-text | generated | hand | 95945.9 | 75029.1 | 0.78x | 78782.1 | 0.82x | +5.0% | 129112 | 129112 | 16 % |  |
| fix/Orders128.log-bytes | generated | hand | 96479.6 | 97834.5 | 1.01x | 99760.2 | 1.03x | +2.0% | 129168 | 129168 | 21 % |  |
| fix/Orders128.log-stream | generated | hand | 97551.9 | 105991.2 | 1.09x | 108994.0 | 1.12x | +2.8% | 118552 | 118552 | 19 % |  |
| el/ladder.bool | generated | hand | 1031.9 | 1765.3 | 1.71x | 1839.2 | 1.78x | +4.2% | 1776 | 1720 | 39 % |  |
| sql/refused-late.bool | generated | hand | 2779.5 | 12631.0 | 4.54x | 6054.0 | 2.18x | -52.1% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 7142.5 | 19014.7 | 2.66x | 19073.7 | 2.67x | +0.3% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4829125.0 | 5195962.5 | 1.08x | 4843900.0 | 1.00x | -6.8% | 6276684 | 6276684 | 9 % |  |
| sql/refused-cliff-case-1738 | generated | control | 22530075.0 | 22899300.0 | 1.02x | 24802250.0 | 1.10x | +8.3% | 74363532 | 74363650 | 66 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2649331.2 | 2754937.5 | 1.04x | 2692250.0 | 1.02x | -2.3% | 2994123 | 2994123 | 12 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 6097806.2 | 6069368.8 | 1.00x | 6352228.1 | 1.04x | +4.7% | 12948942 | 12948856 | 13 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 9326625.0 | 9172262.5 | 0.98x | 10353462.5 | 1.11x | +12.9% | 21144106 | 21144075 | 77 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 31548125.0 | 30847775.0 | 0.98x | 28750850.0 | 0.91x | -6.8% | 94726495 | 94726954 | 86 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7046825.0 | 7203256.2 | 1.02x | 7258131.2 | 1.03x | +0.8% | 8778600 | 8778600 | 25 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6934887.5 | 6942718.8 | 1.00x | 7155800.0 | 1.03x | +3.1% | 8775467 | 8775404 | 58 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 35099300.0 | 30282950.0 | 0.86x | 28801825.0 | 0.82x | -4.9% | 94861028 | 94861103 | 58 % |  |
| sql/refused-cliff-and-3393 | generated | control | 27889475.0 | 26311875.0 | 0.94x | 33859850.0 | 1.21x | +28.7% | 94858371 | 94857953 | 85 % |  |
| fix/Orders128.yield-reader | generated | hand | 141020.5 | 94892.8 | 0.67x | 95872.7 | 0.68x | +1.0% | 118520 | 118520 | 13 % |  |
| fix/Orders128.yield-string | generated | hand | 84615.7 | 264444.3 | 3.13x | 259279.8 | 3.06x | -2.0% | 117936 | 117936 | 9 % |  |
| fix/Orders128.yield-memory | generated | hand | 81869.8 | 88151.1 | 1.08x | 86337.8 | 1.05x | -2.1% | 118016 | 118016 | 5 % |  |
| fix/Orders128.whole-stream | generated | hand | 117317.4 | 81771.5 | 0.70x | 84182.2 | 0.72x | +2.9% | 129368 | 129368 | 15 % |  |
| fix/Orders128.whole-reader | generated | hand | 117583.1 | 85661.9 | 0.73x | 88303.1 | 0.75x | +3.1% | 129336 | 129336 | 3 % |  |
| feeds/stock-count.good.text | generated | hand | 37764.5 | 30460.0 | 0.81x | 29620.1 | 0.78x | -2.8% | 111403 | 111403 | 44 % |  |
| feeds/stock-count.good.reader | generated | hand | 45804.3 | 38417.5 | 0.84x | 37729.0 | 0.82x | -1.8% | 111488 | 111579 | 23 % |  |
| feeds/stock-count.good.reader64 | generated | hand | 47948.3 | 42868.3 | 0.89x | 42766.2 | 0.89x | -0.2% | 111536 | 111627 | 9 % |  |
| web/url.plain | generated | hand | 96.3 | 226.1 | 2.35x | 224.7 | 2.33x | -0.6% | 360 | 360 | 11 % |  |
| web/url.full | generated | hand | 168.1 | 301.0 | 1.79x | 298.1 | 1.77x | -1.0% | 536 | 536 | 3 % |  |
| web/url.ipv4 | generated | hand | 115.5 | 243.9 | 2.11x | 240.2 | 2.08x | -1.5% | 384 | 384 | 11 % |  |
| web/url.long-path | generated | hand | 181.2 | 431.2 | 2.38x | 437.9 | 2.42x | +1.5% | 512 | 512 | 36 % |  |
| web/url.refused | generated | hand | 69.3 | 201.5 | 2.91x | 197.5 | 2.85x | -2.0% | 64 | 64 | 8 % |  |
| web/url.relative | generated | hand | 76.9 | 206.7 | 2.69x | 204.7 | 2.66x | -1.0% | 312 | 312 | 6 % |  |
| web/url.relative-dot-colon | generated | hand | 65.9 | 185.8 | 2.82x | 185.3 | 2.81x | -0.2% | 280 | 280 | 17 % |  |
| web/url.relative-letters | generated | hand | 106.2 | 248.5 | 2.34x | 243.7 | 2.30x | -1.9% | 312 | 312 | 11 % |  |
| web/json.object | generated | hand | 597.2 | 938.9 | 1.57x | 956.3 | 1.60x | +1.9% | 2584 | 2584 | 3 % |  |
| web/json.array | generated | hand | 605.4 | 754.6 | 1.25x | 750.9 | 1.24x | -0.5% | 2400 | 2400 | 11 % |  |
| web/media-type.plain | generated | control | 158.7 | 205.5 | 1.29x | 208.2 | 1.31x | +1.3% | 448 | 448 | 27 % |  |
| web/media-type.quoted | generated | control | 258.2 | 303.6 | 1.18x | 301.2 | 1.17x | -0.8% | 816 | 816 | 3 % |  |
| web/media-type.refused | generated | control | 46.3 | 82.0 | 1.77x | 82.2 | 1.77x | +0.1% | 64 | 64 | 5 % |  |
| web/addr-spec.plain | generated | control | 92.3 | 125.5 | 1.36x | 122.0 | 1.32x | -2.7% | 176 | 176 | 4 % |  |
| web/addr-spec.refused | generated | control | 25.6 | 60.4 | 2.36x | 60.3 | 2.35x | -0.2% | 64 | 64 | 3 % |  |
| web/json-patch.full | generated | control | 1379.1 | 1447.9 | 1.05x | 1436.7 | 1.04x | -0.8% | 4552 | 4552 | 5 % |  |
| web/link.full | generated | control | 1321.6 | 1390.7 | 1.05x | 1374.9 | 1.04x | -1.1% | 2968 | 2968 | 7 % |  |
| web/link.refused | generated | control | 20.7 | 54.2 | 2.62x | 53.7 | 2.59x | -0.9% | 64 | 64 | 11 % |  |
| web/sf.item | generated | control | 258.3 | 315.4 | 1.22x | 306.8 | 1.19x | -2.7% | 800 | 800 | 2 % |  |
| web/sf.list | generated | control | 1175.2 | 1262.0 | 1.07x | 1258.2 | 1.07x | -0.3% | 3064 | 3064 | 3 % |  |
| web/sf.dictionary | generated | control | 1260.7 | 1332.4 | 1.06x | 1327.8 | 1.05x | -0.3% | 3280 | 3280 | 6 % |  |
| el/ladder | generated | hand | 996.7 | 1719.4 | 1.72x | 1756.3 | 1.76x | +2.1% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 996.7 | 1206.4 | 1.21x | 1168.8 | 1.17x | -3.1% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 111187.4 | 153422.9 | 1.38x | 153093.9 | 1.38x | -0.2% | 169104 | 169128 | 23 % |  |
| el/terms1000 | immediate | hand | 111187.4 | 124980.9 | 1.12x | 123406.6 | 1.11x | -1.3% | 177123 | 177120 | 23 % |  |
| sql/select20 | generated | hand | 6852.1 | 18483.3 | 2.70x | 17997.8 | 2.63x | -2.6% | 21448 | 21448 | 16 % |  |
| sql/refused-late | generated | hand | 2606.9 | 12131.1 | 4.65x | 11869.8 | 4.55x | -2.2% | 13552 | 13552 | 15 % |  |
