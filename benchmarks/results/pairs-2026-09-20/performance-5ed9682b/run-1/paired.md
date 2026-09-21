# Paired stand, 2026-09-20 21:22

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Orders128.text | generated | hand | 85387.5 | 74855.5 | 0.88x | 79412.9 | 0.93x | +6.1% | 129112 | 129112 | 26 % |  |
| fix/Orders128.bytes | generated | hand | 85171.9 | 85966.4 | 1.01x | 88557.0 | 1.04x | +3.0% | 129168 | 129168 | 87 % |  |
| fix/Orders128.stream | generated | hand | 127263.5 | 95809.2 | 0.75x | 98680.4 | 0.78x | +3.0% | 118552 | 118552 | 45 % |  |
| fixmsg/Order.parse | generated | control | 1368.3 | 2177.6 | 1.59x | 2144.4 | 1.57x | -1.5% | 3400 | 3400 | 69 % |  |
| fixmsg/Order.build | generated | control | 1627.0 | 2561.8 | 1.57x | 2695.4 | 1.66x | +5.2% | 3592 | 3592 | 43 % |  |
| fix/orders400.text | generated | hand | 266281.4 | 223431.6 | 0.84x | 228610.0 | 0.86x | +2.3% | 403288 | 403288 | 16 % |  |
| tsql/script100.bool | generated | scriptdom | 670793.8 | 126953.1 | 0.19x | 126825.0 | 0.19x | -0.1% | 113600 | 105603 | 5 % |  |
| tsql/script100.boolboth | generated | scriptdom | 676302.3 | 125635.2 | 0.19x | 126225.0 | 0.19x | +0.5% | 105600 | 105600 | 16 % |  |
| tsql/script100 | generated | scriptdom | 679646.1 | 126843.0 | 0.19x | 125321.1 | 0.18x | -1.2% | 113600 | 113600 | 4 % |  |
| tsql/columns1000 | generated | scriptdom | 1801373.4 | 208103.1 | 0.12x | 199882.8 | 0.11x | -4.0% | 200464 | 200464 | 3 % |  |
| web/json.array10000 | generated | hand | 180883.6 | 203570.7 | 1.13x | 203622.9 | 1.13x | 0.0% | 720048 | 720048 | 57 % |  |
| web/json.object10000 | generated | hand | 810856.2 | 1437097.7 | 1.77x | 1286956.2 | 1.59x | -10.4% | 1599240 | 1599240 | 83 % |  |
| web/url.path1000 | generated | hand | 9476.1 | 11197.7 | 1.18x | 11314.3 | 1.19x | +1.0% | 8384 | 8384 | 3 % |  |
| web/media-type.params1000 | generated | control | 48721.5 | 45977.3 | 0.94x | 46882.5 | 0.96x | +2.0% | 151472 | 151472 | 5 % |  |
| web/sf.list10000 | generated | control | 1166792.2 | 1229214.1 | 1.05x | 1222826.6 | 1.05x | -0.5% | 2720056 | 2720056 | 13 % |  |
| sql/select20.at | generated | hand | 7178.3 | 17780.2 | 2.48x | 17915.7 | 2.50x | +0.8% | 21416 | 21416 | 21 % |  |
| sql/select20.window | generated | hand | 6969.2 | 18296.4 | 2.63x | 18443.7 | 2.65x | +0.8% | 21416 | 21440 | 62 % |  |
| sql/select20.scan | generated | control | 381.7 | 385.9 | 1.01x | 385.4 | 1.01x | -0.1% | 0 | 0 | 7 % |  |
| tsql/select20.scan | generated | control | 388.8 | 385.5 | 0.99x | 388.7 | 1.00x | +0.8% | 0 | 0 | 8 % |  |
| el/ladder.scan | generated | control | 144.0 | 140.8 | 0.98x | 140.2 | 0.97x | -0.4% | 0 | 0 | 13 % |  |
| fixmsg/Order.parse-stream | generated | control | 1967.5 | 2185.0 | 1.11x | 2205.2 | 1.12x | +0.9% | 8224 | 8224 | 13 % |  |
| fixmsg/Order.parse-reader | generated | control | 1955.6 | 2137.4 | 1.09x | 2145.9 | 1.10x | +0.4% | 12112 | 12112 | 2 % |  |
| fixmsg/Order.parse-span | generated | control | 1984.0 | 1835.2 | 0.93x | 1866.2 | 0.94x | +1.7% | 3672 | 3672 | 9 % |  |
| fixmsg/Order.read-stream100 | generated | control | 204646.7 | 202291.4 | 0.99x | 207571.5 | 1.01x | +2.6% | 389248 | 389248 | 11 % |  |
| fixmsg/Order.read-reader100 | generated | control | 204870.7 | 190733.6 | 0.93x | 195144.5 | 0.95x | +2.3% | 375712 | 375712 | 16 % |  |
| fix/Orders128.log-text | generated | hand | 94269.1 | 72188.1 | 0.77x | 72296.3 | 0.77x | +0.1% | 129112 | 129112 | 9 % |  |
| fix/Orders128.log-bytes | generated | hand | 93944.8 | 96652.4 | 1.03x | 95034.8 | 1.01x | -1.7% | 129168 | 129168 | 5 % |  |
| fix/Orders128.log-stream | generated | hand | 93943.9 | 102889.8 | 1.10x | 103496.4 | 1.10x | +0.6% | 118552 | 118552 | 10 % |  |
| el/ladder.bool | generated | hand | 1005.4 | 1732.5 | 1.72x | 1764.4 | 1.75x | +1.8% | 1776 | 1720 | 60 % |  |
| sql/refused-late.bool | generated | hand | 2635.4 | 11994.5 | 4.55x | 5818.8 | 2.21x | -51.5% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6968.5 | 18000.3 | 2.58x | 18169.9 | 2.61x | +0.9% | 21448 | 21392 | 14 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4562881.2 | 4833525.0 | 1.06x | 4724587.5 | 1.04x | -2.3% | 6276684 | 6276684 | 4 % |  |
| sql/refused-cliff-case-1738 | generated | control | 17846825.0 | 25106500.0 | 1.41x | 29922650.0 | 1.68x | +19.2% | 74363524 | 74363883 | 125 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2603687.5 | 2579921.9 | 0.99x | 2593775.0 | 1.00x | +0.5% | 2994166 | 2994166 | 5 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 5985625.0 | 5971537.5 | 1.00x | 5801456.2 | 0.97x | -2.8% | 12948795 | 12948779 | 31 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 10990062.5 | 10718112.5 | 0.98x | 11162693.8 | 1.02x | +4.1% | 21144108 | 21144067 | 29 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 31717425.0 | 31967150.0 | 1.01x | 30538550.0 | 0.96x | -4.5% | 94726414 | 94726513 | 56 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7450262.5 | 6924887.5 | 0.93x | 7140325.0 | 0.96x | +3.1% | 8778600 | 8778600 | 33 % |  |
| sql/refused-cliff-and-2715 | generated | control | 7188206.2 | 6895568.8 | 0.96x | 6691175.0 | 0.93x | -3.0% | 8775443 | 8775404 | 6 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 33598575.0 | 31541725.0 | 0.94x | 31005725.0 | 0.92x | -1.7% | 94864206 | 94859716 | 33 % |  |
| sql/refused-cliff-and-3393 | generated | control | 16442200.0 | 16673100.0 | 1.01x | 15318500.0 | 0.93x | -8.1% | 94857394 | 94857746 | 229 % |  |
| fix/Orders128.yield-reader | generated | hand | 140559.7 | 92630.7 | 0.66x | 95340.6 | 0.68x | +2.9% | 118520 | 118520 | 8 % |  |
| fix/Orders128.yield-string | generated | hand | 85056.9 | 286005.7 | 3.36x | 269509.0 | 3.17x | -5.8% | 117936 | 117936 | 12 % |  |
| fix/Orders128.yield-memory | generated | hand | 84403.8 | 87205.4 | 1.03x | 87714.2 | 1.04x | +0.6% | 118016 | 118016 | 4 % |  |
| fix/Orders128.whole-stream | generated | hand | 118529.8 | 83713.5 | 0.71x | 82092.6 | 0.69x | -1.9% | 129368 | 129368 | 6 % |  |
| fix/Orders128.whole-reader | generated | hand | 118897.9 | 85865.1 | 0.72x | 86148.7 | 0.72x | +0.3% | 129336 | 129336 | 9 % |  |
| feeds/stock-count.good.text | generated | hand | 38885.1 | 31042.2 | 0.80x | 30892.2 | 0.79x | -0.5% | 111312 | 111403 | 13 % |  |
| feeds/stock-count.good.reader | generated | hand | 47644.5 | 38393.9 | 0.81x | 38568.8 | 0.81x | +0.5% | 111488 | 111579 | 15 % |  |
| feeds/stock-count.good.reader64 | generated | hand | 46840.8 | 42250.4 | 0.90x | 42361.1 | 0.90x | +0.3% | 111536 | 111627 | 5 % |  |
| web/url.plain | generated | hand | 98.4 | 235.6 | 2.39x | 231.5 | 2.35x | -1.7% | 360 | 360 | 7 % |  |
| web/url.full | generated | hand | 169.3 | 314.1 | 1.86x | 303.0 | 1.79x | -3.5% | 536 | 536 | 11 % |  |
| web/url.ipv4 | generated | hand | 111.9 | 247.5 | 2.21x | 236.4 | 2.11x | -4.5% | 384 | 384 | 7 % |  |
| web/url.long-path | generated | hand | 179.3 | 435.3 | 2.43x | 428.9 | 2.39x | -1.5% | 512 | 512 | 13 % |  |
| web/url.refused | generated | hand | 68.1 | 199.6 | 2.93x | 198.0 | 2.91x | -0.8% | 64 | 64 | 4 % |  |
| web/url.relative | generated | hand | 76.9 | 200.4 | 2.60x | 202.8 | 2.64x | +1.2% | 312 | 312 | 10 % |  |
| web/url.relative-dot-colon | generated | hand | 64.3 | 172.6 | 2.68x | 179.3 | 2.79x | +3.9% | 280 | 280 | 13 % |  |
| web/url.relative-letters | generated | hand | 105.5 | 235.3 | 2.23x | 243.0 | 2.30x | +3.3% | 312 | 312 | 11 % |  |
| web/json.object | generated | hand | 605.6 | 943.4 | 1.56x | 967.3 | 1.60x | +2.5% | 2584 | 2584 | 12 % |  |
| web/json.array | generated | hand | 598.1 | 761.2 | 1.27x | 740.7 | 1.24x | -2.7% | 2400 | 2400 | 2 % |  |
| web/media-type.plain | generated | control | 169.7 | 207.8 | 1.22x | 218.9 | 1.29x | +5.3% | 448 | 448 | 6 % |  |
| web/media-type.quoted | generated | control | 258.4 | 304.3 | 1.18x | 331.3 | 1.28x | +8.9% | 816 | 816 | 33 % |  |
| web/media-type.refused | generated | control | 46.1 | 83.0 | 1.80x | 82.1 | 1.78x | -1.1% | 64 | 64 | 12 % |  |
| web/addr-spec.plain | generated | control | 89.3 | 121.8 | 1.36x | 124.6 | 1.40x | +2.3% | 176 | 176 | 5 % |  |
| web/addr-spec.refused | generated | control | 25.1 | 60.8 | 2.43x | 60.3 | 2.41x | -0.8% | 64 | 64 | 23 % |  |
| web/json-patch.full | generated | control | 1372.5 | 1445.0 | 1.05x | 1426.1 | 1.04x | -1.3% | 4552 | 4552 | 3 % |  |
| web/link.full | generated | control | 1308.1 | 1419.1 | 1.08x | 1500.2 | 1.15x | +5.7% | 2968 | 2968 | 24 % |  |
| web/link.refused | generated | control | 20.4 | 55.0 | 2.70x | 55.6 | 2.72x | +0.9% | 64 | 64 | 12 % |  |
| web/sf.item | generated | control | 283.9 | 355.7 | 1.25x | 333.0 | 1.17x | -6.4% | 800 | 800 | 42 % |  |
| web/sf.list | generated | control | 1201.8 | 1359.5 | 1.13x | 1323.7 | 1.10x | -2.6% | 3064 | 3064 | 24 % |  |
| web/sf.dictionary | generated | control | 1270.1 | 1343.8 | 1.06x | 1316.2 | 1.04x | -2.1% | 3280 | 3280 | 23 % |  |
| el/ladder | generated | hand | 1031.0 | 1778.1 | 1.72x | 1781.4 | 1.73x | +0.2% | 1776 | 1776 | 27 % |  |
| el/ladder | immediate | hand | 1031.0 | 1231.0 | 1.19x | 1210.7 | 1.17x | -1.7% | 1784 | 1784 | 27 % |  |
| el/terms1000 | generated | hand | 115529.1 | 162898.1 | 1.41x | 155729.5 | 1.35x | -4.4% | 169104 | 169104 | 18 % |  |
| el/terms1000 | immediate | hand | 115529.1 | 129285.1 | 1.12x | 130977.4 | 1.13x | +1.3% | 177120 | 177123 | 18 % |  |
| sql/select20 | generated | hand | 7969.2 | 20250.4 | 2.54x | 20692.2 | 2.60x | +2.2% | 21448 | 21448 | 28 % |  |
| sql/refused-late | generated | hand | 2744.7 | 12240.5 | 4.46x | 12341.5 | 4.50x | +0.8% | 13552 | 13552 | 31 % |  |
