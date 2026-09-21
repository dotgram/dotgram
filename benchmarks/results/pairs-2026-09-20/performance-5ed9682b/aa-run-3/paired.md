# Paired stand, 2026-09-20 21:59

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Orders128.text | generated | hand | 80211.7 | 69571.9 | 0.87x | 71352.0 | 0.89x | +2.6% | 129112 | 129112 | 37 % |  |
| fix/Orders128.bytes | generated | hand | 79143.0 | 79448.4 | 1.00x | 82238.3 | 1.04x | +3.5% | 129168 | 129168 | 9 % |  |
| fix/Orders128.stream | generated | hand | 116674.5 | 89154.9 | 0.76x | 89979.9 | 0.77x | +0.9% | 118552 | 118552 | 4 % |  |
| fixmsg/Order.parse | generated | control | 1229.5 | 1857.2 | 1.51x | 1840.4 | 1.50x | -0.9% | 3400 | 3400 | 7 % |  |
| fixmsg/Order.build | generated | control | 1546.6 | 2329.6 | 1.51x | 2329.0 | 1.51x | 0.0% | 3592 | 3592 | 7 % |  |
| fix/orders400.text | generated | hand | 259061.9 | 219064.8 | 0.85x | 226762.3 | 0.88x | +3.5% | 403288 | 403288 | 5 % |  |
| tsql/script100.bool | generated | scriptdom | 656887.5 | 124465.6 | 0.19x | 121337.5 | 0.18x | -2.5% | 113600 | 105600 | 5 % |  |
| tsql/script100.boolboth | generated | scriptdom | 661410.9 | 126313.3 | 0.19x | 122512.5 | 0.19x | -3.0% | 105600 | 105600 | 13 % |  |
| tsql/script100 | generated | scriptdom | 658552.3 | 124834.4 | 0.19x | 121775.8 | 0.18x | -2.5% | 113600 | 113600 | 14 % |  |
| tsql/columns1000 | generated | scriptdom | 1743870.3 | 189509.4 | 0.11x | 192096.9 | 0.11x | +1.4% | 200464 | 200464 | 7 % |  |
| web/json.array10000 | generated | hand | 170360.9 | 200643.4 | 1.18x | 207562.1 | 1.22x | +3.4% | 720048 | 720048 | 11 % |  |
| web/json.object10000 | generated | hand | 800079.7 | 1231225.4 | 1.54x | 1113285.9 | 1.39x | -9.6% | 1599240 | 1599240 | 37 % |  |
| web/url.path1000 | generated | hand | 9358.2 | 10957.4 | 1.17x | 10936.5 | 1.17x | -0.2% | 8384 | 8384 | 3 % |  |
| web/media-type.params1000 | generated | control | 47798.6 | 48177.8 | 1.01x | 44336.1 | 0.93x | -8.0% | 151472 | 151472 | 6 % |  |
| web/sf.list10000 | generated | control | 1161681.2 | 1151443.8 | 0.99x | 1347912.5 | 1.16x | +17.1% | 2720056 | 2720056 | 28 % |  |
| sql/select20.at | generated | hand | 6861.5 | 17674.7 | 2.58x | 17688.7 | 2.58x | +0.1% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 6793.1 | 18212.8 | 2.68x | 17903.2 | 2.64x | -1.7% | 21416 | 21416 | 10 % |  |
| sql/select20.scan | generated | control | 387.9 | 383.9 | 0.99x | 380.4 | 0.98x | -0.9% | 0 | 0 | 19 % |  |
| tsql/select20.scan | generated | control | 384.4 | 391.9 | 1.02x | 390.6 | 1.02x | -0.3% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 138.9 | 139.3 | 1.00x | 139.4 | 1.00x | +0.1% | 0 | 0 | 11 % |  |
| fixmsg/Order.parse-stream | generated | control | 1954.3 | 2169.9 | 1.11x | 2133.7 | 1.09x | -1.7% | 8224 | 8224 | 5 % |  |
| fixmsg/Order.parse-reader | generated | control | 1957.7 | 2114.7 | 1.08x | 2100.7 | 1.07x | -0.7% | 12112 | 12112 | 3 % |  |
| fixmsg/Order.parse-span | generated | control | 1955.8 | 1835.1 | 0.94x | 1804.4 | 0.92x | -1.7% | 3672 | 3672 | 3 % |  |
| fixmsg/Order.read-stream100 | generated | control | 206387.1 | 202462.5 | 0.98x | 199828.9 | 0.97x | -1.3% | 389248 | 389248 | 5 % |  |
| fixmsg/Order.read-reader100 | generated | control | 206296.9 | 193475.4 | 0.94x | 192393.8 | 0.93x | -0.6% | 375712 | 375712 | 5 % |  |
| fix/Orders128.log-text | generated | hand | 92638.6 | 72469.8 | 0.78x | 73040.9 | 0.79x | +0.8% | 129112 | 129112 | 11 % |  |
| fix/Orders128.log-bytes | generated | hand | 93135.4 | 93427.1 | 1.00x | 96628.1 | 1.04x | +3.4% | 129168 | 129168 | 16 % |  |
| fix/Orders128.log-stream | generated | hand | 92860.4 | 102463.3 | 1.10x | 102963.6 | 1.11x | +0.5% | 118552 | 118552 | 23 % |  |
| el/ladder.bool | generated | hand | 982.9 | 1727.2 | 1.76x | 1694.3 | 1.72x | -1.9% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2605.6 | 11976.7 | 4.60x | 5869.1 | 2.25x | -51.0% | 13552 | 6616 | 28 % |  |
| sql/select20.bool | generated | hand | 6822.5 | 18435.7 | 2.70x | 18101.8 | 2.65x | -1.8% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4673053.1 | 4605050.0 | 0.99x | 4697062.5 | 1.01x | +2.0% | 6276684 | 6276684 | 61 % |  |
| sql/refused-cliff-case-1738 | generated | control | 26155625.0 | 27888600.0 | 1.07x | 22539225.0 | 0.86x | -19.2% | 74363360 | 74363380 | 83 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2621012.5 | 2617106.2 | 1.00x | 2602540.6 | 0.99x | -0.6% | 2994166 | 2994166 | 9 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 6723237.5 | 6649812.5 | 0.99x | 6771450.0 | 1.01x | +1.8% | 12948846 | 12948836 | 57 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 10279418.8 | 10371050.0 | 1.01x | 10181356.2 | 0.99x | -1.8% | 21144106 | 21144188 | 44 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 30406950.0 | 30639925.0 | 1.01x | 30937675.0 | 1.02x | +1.0% | 94726782 | 94726242 | 50 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7086243.8 | 7082450.0 | 1.00x | 7049112.5 | 0.99x | -0.5% | 8778600 | 8778619 | 8 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6992181.2 | 6932068.8 | 0.99x | 6990375.0 | 1.00x | +0.8% | 8775448 | 8775448 | 7 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 31151625.0 | 31594450.0 | 1.01x | 32225100.0 | 1.03x | +2.0% | 94863474 | 94864002 | 68 % |  |
| sql/refused-cliff-and-3393 | generated | control | 34256625.0 | 29176125.0 | 0.85x | 31031900.0 | 0.91x | +6.4% | 94858822 | 94857908 | 125 % |  |
| fix/Orders128.yield-reader | generated | hand | 141646.8 | 94068.6 | 0.66x | 94808.6 | 0.67x | +0.8% | 118520 | 118520 | 14 % |  |
| fix/Orders128.yield-string | generated | hand | 84911.9 | 276233.6 | 3.25x | 263344.8 | 3.10x | -4.7% | 117936 | 117936 | 12 % |  |
| fix/Orders128.yield-memory | generated | hand | 82612.2 | 85794.2 | 1.04x | 86234.5 | 1.04x | +0.5% | 118016 | 118016 | 17 % |  |
| fix/Orders128.whole-stream | generated | hand | 121744.9 | 85657.6 | 0.70x | 88369.8 | 0.73x | +3.2% | 129368 | 129368 | 45 % |  |
| fix/Orders128.whole-reader | generated | hand | 117214.6 | 84931.2 | 0.72x | 87116.8 | 0.74x | +2.6% | 129336 | 129336 | 4 % |  |
| feeds/stock-count.good.text | generated | hand | 38174.2 | 28751.9 | 0.75x | 29172.2 | 0.76x | +1.5% | 111403 | 111403 | 3 % |  |
| feeds/stock-count.good.reader | generated | hand | 47949.9 | 37288.7 | 0.78x | 36954.4 | 0.77x | -0.9% | 111488 | 111488 | 5 % |  |
| feeds/stock-count.good.reader64 | generated | hand | 50192.9 | 41762.5 | 0.83x | 42431.4 | 0.85x | +1.6% | 111536 | 111627 | 4 % |  |
| web/url.plain | generated | hand | 97.6 | 224.9 | 2.30x | 231.3 | 2.37x | +2.9% | 360 | 360 | 9 % |  |
| web/url.full | generated | hand | 166.0 | 295.4 | 1.78x | 306.2 | 1.85x | +3.7% | 536 | 536 | 9 % |  |
| web/url.ipv4 | generated | hand | 114.8 | 237.3 | 2.07x | 262.6 | 2.29x | +10.7% | 384 | 384 | 17 % |  |
| web/url.long-path | generated | hand | 182.2 | 432.9 | 2.38x | 437.3 | 2.40x | +1.0% | 512 | 512 | 63 % |  |
| web/url.refused | generated | hand | 68.1 | 194.9 | 2.86x | 199.2 | 2.92x | +2.2% | 64 | 64 | 6 % |  |
| web/url.relative | generated | hand | 76.6 | 197.5 | 2.58x | 201.2 | 2.63x | +1.9% | 312 | 312 | 6 % |  |
| web/url.relative-dot-colon | generated | hand | 63.5 | 173.1 | 2.73x | 177.1 | 2.79x | +2.3% | 280 | 280 | 10 % |  |
| web/url.relative-letters | generated | hand | 106.6 | 238.2 | 2.23x | 243.0 | 2.28x | +2.0% | 312 | 312 | 13 % |  |
| web/json.object | generated | hand | 594.2 | 949.1 | 1.60x | 954.1 | 1.61x | +0.5% | 2584 | 2584 | 3 % |  |
| web/json.array | generated | hand | 593.3 | 756.3 | 1.27x | 757.8 | 1.28x | +0.2% | 2400 | 2400 | 11 % |  |
| web/media-type.plain | generated | control | 168.1 | 233.9 | 1.39x | 200.7 | 1.19x | -14.2% | 448 | 448 | 13 % |  |
| web/media-type.quoted | generated | control | 267.0 | 329.9 | 1.24x | 296.4 | 1.11x | -10.2% | 816 | 816 | 9 % |  |
| web/media-type.refused | generated | control | 46.7 | 82.0 | 1.76x | 82.4 | 1.76x | +0.4% | 64 | 64 | 19 % |  |
| web/addr-spec.plain | generated | control | 94.1 | 126.1 | 1.34x | 121.1 | 1.29x | -3.9% | 176 | 176 | 3 % |  |
| web/addr-spec.refused | generated | control | 24.5 | 59.7 | 2.44x | 59.9 | 2.45x | +0.3% | 64 | 64 | 3 % |  |
| web/json-patch.full | generated | control | 1381.3 | 1453.2 | 1.05x | 1441.8 | 1.04x | -0.8% | 4552 | 4552 | 7 % |  |
| web/link.full | generated | control | 1318.9 | 1424.2 | 1.08x | 1454.7 | 1.10x | +2.1% | 2968 | 2968 | 18 % |  |
| web/link.refused | generated | control | 20.3 | 54.3 | 2.68x | 54.4 | 2.69x | +0.3% | 64 | 64 | 46 % |  |
| web/sf.item | generated | control | 275.7 | 324.5 | 1.18x | 311.3 | 1.13x | -4.1% | 800 | 800 | 8 % |  |
| web/sf.list | generated | control | 1258.8 | 1287.2 | 1.02x | 1296.9 | 1.03x | +0.8% | 3064 | 3064 | 12 % |  |
| web/sf.dictionary | generated | control | 1263.9 | 1294.4 | 1.02x | 1289.7 | 1.02x | -0.4% | 3280 | 3280 | 7 % |  |
| el/ladder | generated | hand | 1021.6 | 1749.1 | 1.71x | 1751.4 | 1.71x | +0.1% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 1021.6 | 1220.5 | 1.19x | 1203.4 | 1.18x | -1.4% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 112065.6 | 153638.8 | 1.37x | 154868.4 | 1.38x | +0.8% | 169104 | 169107 | 5 % |  |
| el/terms1000 | immediate | hand | 112065.6 | 129483.2 | 1.16x | 127008.1 | 1.13x | -1.9% | 177120 | 177120 | 5 % |  |
| sql/select20 | generated | hand | 7106.4 | 18841.4 | 2.65x | 18466.4 | 2.60x | -2.0% | 21448 | 21448 | 10 % |  |
| sql/refused-late | generated | hand | 2690.1 | 12371.9 | 4.60x | 12173.9 | 4.53x | -1.6% | 13552 | 13552 | 12 % |  |
