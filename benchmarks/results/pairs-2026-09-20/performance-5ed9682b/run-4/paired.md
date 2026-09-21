# Paired stand, 2026-09-20 22:06

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.0 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Orders128.text | generated | hand | 82583.2 | 68637.9 | 0.83x | 68945.7 | 0.83x | +0.4% | 129112 | 129112 | 43 % |  |
| fix/Orders128.bytes | generated | hand | 80962.1 | 82557.8 | 1.02x | 82792.6 | 1.02x | +0.3% | 129168 | 129168 | 35 % |  |
| fix/Orders128.stream | generated | hand | 118172.7 | 91975.4 | 0.78x | 90560.7 | 0.77x | -1.5% | 118552 | 118552 | 4 % |  |
| fixmsg/Order.parse | generated | control | 1260.2 | 1823.6 | 1.45x | 1866.0 | 1.48x | +2.3% | 3400 | 3400 | 8 % |  |
| fixmsg/Order.build | generated | control | 1587.0 | 2379.3 | 1.50x | 2358.3 | 1.49x | -0.9% | 3592 | 3592 | 7 % |  |
| fix/orders400.text | generated | hand | 259468.2 | 217010.5 | 0.84x | 215194.1 | 0.83x | -0.8% | 403288 | 403288 | 3 % |  |
| tsql/script100.bool | generated | scriptdom | 674009.4 | 126078.1 | 0.19x | 127859.4 | 0.19x | +1.4% | 113600 | 105600 | 17 % |  |
| tsql/script100.boolboth | generated | scriptdom | 672004.7 | 125255.5 | 0.19x | 129348.4 | 0.19x | +3.3% | 105600 | 105600 | 10 % |  |
| tsql/script100 | generated | scriptdom | 673652.3 | 125239.1 | 0.19x | 131500.0 | 0.20x | +5.0% | 113600 | 113600 | 11 % |  |
| tsql/columns1000 | generated | scriptdom | 1758665.6 | 193420.3 | 0.11x | 192940.6 | 0.11x | -0.2% | 200464 | 200464 | 6 % |  |
| web/json.array10000 | generated | hand | 172227.1 | 196458.0 | 1.14x | 202098.2 | 1.17x | +2.9% | 720048 | 720048 | 9 % |  |
| web/json.object10000 | generated | hand | 646785.2 | 1259434.4 | 1.95x | 1184743.0 | 1.83x | -5.9% | 1599240 | 1599240 | 91 % |  |
| web/url.path1000 | generated | hand | 9420.7 | 11009.2 | 1.17x | 10921.6 | 1.16x | -0.8% | 8384 | 8384 | 9 % |  |
| web/media-type.params1000 | generated | control | 49318.0 | 44773.6 | 0.91x | 45409.3 | 0.92x | +1.4% | 151472 | 151472 | 21 % |  |
| web/sf.list10000 | generated | control | 1230103.1 | 1250042.2 | 1.02x | 1274754.7 | 1.04x | +2.0% | 2720056 | 2720056 | 12 % |  |
| sql/select20.at | generated | hand | 7045.1 | 18039.9 | 2.56x | 17918.7 | 2.54x | -0.7% | 21416 | 21416 | 59 % |  |
| sql/select20.window | generated | hand | 6794.1 | 19027.2 | 2.80x | 18122.3 | 2.67x | -4.8% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 384.4 | 387.0 | 1.01x | 388.4 | 1.01x | +0.4% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 375.2 | 390.1 | 1.04x | 385.1 | 1.03x | -1.3% | 0 | 0 | 7 % |  |
| el/ladder.scan | generated | control | 138.9 | 142.1 | 1.02x | 143.5 | 1.03x | +1.0% | 0 | 0 | 5 % |  |
| fixmsg/Order.parse-stream | generated | control | 1977.0 | 2211.9 | 1.12x | 2194.2 | 1.11x | -0.8% | 8224 | 8224 | 8 % |  |
| fixmsg/Order.parse-reader | generated | control | 1954.8 | 2043.8 | 1.05x | 2076.4 | 1.06x | +1.6% | 12112 | 12112 | 5 % |  |
| fixmsg/Order.parse-span | generated | control | 1986.7 | 1858.1 | 0.94x | 1835.5 | 0.92x | -1.2% | 3672 | 3672 | 12 % |  |
| fixmsg/Order.read-stream100 | generated | control | 208098.8 | 202368.6 | 0.97x | 206095.3 | 0.99x | +1.8% | 389248 | 389248 | 10 % |  |
| fixmsg/Order.read-reader100 | generated | control | 206826.4 | 189857.6 | 0.92x | 193481.8 | 0.94x | +1.9% | 375712 | 375712 | 13 % |  |
| fix/Orders128.log-text | generated | hand | 92595.1 | 71160.6 | 0.77x | 69492.9 | 0.75x | -2.3% | 129112 | 129112 | 9 % |  |
| fix/Orders128.log-bytes | generated | hand | 93993.1 | 94824.8 | 1.01x | 95707.5 | 1.02x | +0.9% | 129168 | 129168 | 5 % |  |
| fix/Orders128.log-stream | generated | hand | 93030.1 | 103713.7 | 1.11x | 103395.4 | 1.11x | -0.3% | 118552 | 118552 | 13 % |  |
| el/ladder.bool | generated | hand | 1019.9 | 1736.0 | 1.70x | 1779.8 | 1.75x | +2.5% | 1776 | 1720 | 29 % |  |
| sql/refused-late.bool | generated | hand | 2573.2 | 12122.3 | 4.71x | 5809.7 | 2.26x | -52.1% | 13552 | 6616 | 9 % |  |
| sql/select20.bool | generated | hand | 6876.8 | 18240.3 | 2.65x | 17845.6 | 2.60x | -2.2% | 21448 | 21392 | 11 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4581887.5 | 4605025.0 | 1.01x | 4534893.8 | 0.99x | -1.5% | 6276488 | 6276488 | 18 % |  |
| sql/refused-cliff-case-1738 | generated | control | 23164975.0 | 31398250.0 | 1.36x | 30275700.0 | 1.31x | -3.6% | 74363492 | 74363294 | 93 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2624953.1 | 2703578.1 | 1.03x | 2611337.5 | 0.99x | -3.4% | 2994166 | 2994142 | 15 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 5988543.8 | 5560268.8 | 0.93x | 5551800.0 | 0.93x | -0.2% | 12948799 | 12948801 | 34 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 9764900.0 | 11194312.5 | 1.15x | 10725000.0 | 1.10x | -4.2% | 21144081 | 21144164 | 32 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 19690650.0 | 29047350.0 | 1.48x | 23557900.0 | 1.20x | -18.9% | 94726522 | 94726714 | 173 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 9144275.0 | 10379731.2 | 1.14x | 9666412.5 | 1.06x | -6.9% | 8778600 | 8778600 | 43 % |  |
| sql/refused-cliff-and-2715 | generated | control | 9917643.8 | 10247500.0 | 1.03x | 9710550.0 | 0.98x | -5.2% | 8775448 | 8775404 | 68 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 36617950.0 | 34439200.0 | 0.94x | 40702625.0 | 1.11x | +18.2% | 94860357 | 94861051 | 139 % |  |
| sql/refused-cliff-and-3393 | generated | control | 34059050.0 | 30981300.0 | 0.91x | 33515200.0 | 0.98x | +8.2% | 94857367 | 94857730 | 46 % |  |
| fix/Orders128.yield-reader | generated | hand | 143459.2 | 95435.0 | 0.67x | 93865.6 | 0.65x | -1.6% | 118520 | 118520 | 16 % |  |
| fix/Orders128.yield-string | generated | hand | 84797.1 | 274792.5 | 3.24x | 283967.7 | 3.35x | +3.3% | 117936 | 117936 | 5 % |  |
| fix/Orders128.yield-memory | generated | hand | 85572.5 | 89041.3 | 1.04x | 87293.8 | 1.02x | -2.0% | 118016 | 118016 | 26 % |  |
| fix/Orders128.whole-stream | generated | hand | 119538.3 | 85002.1 | 0.71x | 85845.0 | 0.72x | +1.0% | 129368 | 129368 | 25 % |  |
| fix/Orders128.whole-reader | generated | hand | 119621.9 | 85729.0 | 0.72x | 86271.9 | 0.72x | +0.6% | 129336 | 129336 | 9 % |  |
| feeds/stock-count.good.text | generated | hand | 36953.5 | 29986.0 | 0.81x | 29616.7 | 0.80x | -1.2% | 111312 | 111403 | 17 % |  |
| feeds/stock-count.good.reader | generated | hand | 45180.1 | 38078.9 | 0.84x | 37925.1 | 0.84x | -0.4% | 111488 | 111488 | 54 % |  |
| feeds/stock-count.good.reader64 | generated | hand | 47932.6 | 43899.2 | 0.92x | 42989.5 | 0.90x | -2.1% | 111536 | 111627 | 15 % |  |
| web/url.plain | generated | hand | 97.4 | 229.7 | 2.36x | 232.3 | 2.39x | +1.2% | 360 | 360 | 10 % |  |
| web/url.full | generated | hand | 174.6 | 299.4 | 1.71x | 305.6 | 1.75x | +2.1% | 536 | 536 | 24 % |  |
| web/url.ipv4 | generated | hand | 112.6 | 243.4 | 2.16x | 243.6 | 2.16x | +0.1% | 384 | 384 | 9 % |  |
| web/url.long-path | generated | hand | 185.2 | 444.8 | 2.40x | 461.7 | 2.49x | +3.8% | 512 | 512 | 43 % |  |
| web/url.refused | generated | hand | 73.7 | 209.0 | 2.84x | 215.1 | 2.92x | +2.9% | 64 | 64 | 18 % |  |
| web/url.relative | generated | hand | 85.8 | 235.7 | 2.75x | 208.7 | 2.43x | -11.5% | 312 | 312 | 50 % |  |
| web/url.relative-dot-colon | generated | hand | 66.4 | 208.9 | 3.14x | 194.7 | 2.93x | -6.8% | 280 | 280 | 22 % |  |
| web/url.relative-letters | generated | hand | 134.9 | 308.0 | 2.28x | 302.1 | 2.24x | -1.9% | 312 | 312 | 37 % |  |
| web/json.object | generated | hand | 734.7 | 1271.1 | 1.73x | 1191.7 | 1.62x | -6.2% | 2584 | 2584 | 27 % |  |
| web/json.array | generated | hand | 715.3 | 971.7 | 1.36x | 938.9 | 1.31x | -3.4% | 2400 | 2400 | 30 % |  |
| web/media-type.plain | generated | control | 187.8 | 267.1 | 1.42x | 244.2 | 1.30x | -8.6% | 448 | 448 | 47 % |  |
| web/media-type.quoted | generated | control | 318.4 | 372.8 | 1.17x | 379.9 | 1.19x | +1.9% | 816 | 816 | 26 % |  |
| web/media-type.refused | generated | control | 55.2 | 99.5 | 1.80x | 97.9 | 1.77x | -1.6% | 64 | 64 | 23 % |  |
| web/addr-spec.plain | generated | control | 100.7 | 149.5 | 1.48x | 145.6 | 1.45x | -2.6% | 176 | 176 | 34 % |  |
| web/addr-spec.refused | generated | control | 28.9 | 72.1 | 2.50x | 72.0 | 2.49x | -0.2% | 64 | 64 | 22 % |  |
| web/json-patch.full | generated | control | 1722.3 | 1689.4 | 0.98x | 1700.9 | 0.99x | +0.7% | 4552 | 4552 | 22 % |  |
| web/link.full | generated | control | 1512.4 | 1663.1 | 1.10x | 1658.6 | 1.10x | -0.3% | 2968 | 2968 | 35 % |  |
| web/link.refused | generated | control | 23.5 | 61.8 | 2.63x | 63.2 | 2.69x | +2.2% | 64 | 64 | 18 % |  |
| web/sf.item | generated | control | 310.4 | 354.5 | 1.14x | 369.2 | 1.19x | +4.2% | 800 | 800 | 35 % |  |
| web/sf.list | generated | control | 1336.3 | 1428.2 | 1.07x | 1450.6 | 1.09x | +1.6% | 3064 | 3064 | 9 % |  |
| web/sf.dictionary | generated | control | 1332.0 | 1417.6 | 1.06x | 1331.8 | 1.00x | -6.1% | 3280 | 3280 | 11 % |  |
| el/ladder | generated | hand | 1039.5 | 1794.2 | 1.73x | 1801.8 | 1.73x | +0.4% | 1776 | 1776 | 5 % |  |
| el/ladder | immediate | hand | 1039.5 | 1223.9 | 1.18x | 1260.2 | 1.21x | +3.0% | 1784 | 1784 | 5 % |  |
| el/terms1000 | generated | hand | 116370.5 | 160227.6 | 1.38x | 161051.1 | 1.38x | +0.5% | 169104 | 169104 | 18 % |  |
| el/terms1000 | immediate | hand | 116370.5 | 132280.2 | 1.14x | 132107.0 | 1.14x | -0.1% | 177120 | 177123 | 18 % |  |
| sql/select20 | generated | hand | 7269.5 | 19192.5 | 2.64x | 18830.4 | 2.59x | -1.9% | 21448 | 21448 | 10 % |  |
| sql/refused-late | generated | hand | 2784.7 | 12762.9 | 4.58x | 12664.7 | 4.55x | -0.8% | 13552 | 13552 | 24 % |  |
