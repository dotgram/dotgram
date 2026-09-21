# Paired stand, 2026-09-20 21:51

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Orders128.text | generated | hand | 81169.1 | 72109.4 | 0.89x | 69832.4 | 0.86x | -3.2% | 129112 | 129112 | 44 % |  |
| fix/Orders128.bytes | generated | hand | 80139.5 | 82059.4 | 1.02x | 81913.3 | 1.02x | -0.2% | 129168 | 129168 | 7 % |  |
| fix/Orders128.stream | generated | hand | 117974.4 | 91060.3 | 0.77x | 91211.0 | 0.77x | +0.2% | 118552 | 118552 | 3 % |  |
| fixmsg/Order.parse | generated | control | 1252.4 | 1917.7 | 1.53x | 1890.0 | 1.51x | -1.4% | 3400 | 3400 | 4 % |  |
| fixmsg/Order.build | generated | control | 1574.7 | 2373.7 | 1.51x | 2366.0 | 1.50x | -0.3% | 3592 | 3592 | 26 % |  |
| fix/orders400.text | generated | hand | 256938.3 | 224520.3 | 0.87x | 216715.4 | 0.84x | -3.5% | 403288 | 403288 | 10 % |  |
| tsql/script100.bool | generated | scriptdom | 663221.9 | 119518.8 | 0.18x | 124928.1 | 0.19x | +4.5% | 113600 | 105600 | 13 % |  |
| tsql/script100.boolboth | generated | scriptdom | 669114.1 | 121305.5 | 0.18x | 124007.0 | 0.19x | +2.2% | 105600 | 105600 | 5 % |  |
| tsql/script100 | generated | scriptdom | 665734.4 | 120514.8 | 0.18x | 124645.3 | 0.19x | +3.4% | 113600 | 113600 | 8 % |  |
| tsql/columns1000 | generated | scriptdom | 1762923.4 | 191689.1 | 0.11x | 188560.9 | 0.11x | -1.6% | 200464 | 200464 | 5 % |  |
| web/json.array10000 | generated | hand | 170228.7 | 193280.3 | 1.14x | 198002.9 | 1.16x | +2.4% | 720048 | 720048 | 6 % |  |
| web/json.object10000 | generated | hand | 808028.9 | 1167931.2 | 1.45x | 1232761.7 | 1.53x | +5.6% | 1599240 | 1599240 | 76 % |  |
| web/url.path1000 | generated | hand | 9454.5 | 10929.2 | 1.16x | 10923.6 | 1.16x | -0.1% | 8384 | 8384 | 3 % |  |
| web/media-type.params1000 | generated | control | 46557.5 | 47465.1 | 1.02x | 44082.7 | 0.95x | -7.1% | 151472 | 151472 | 10 % |  |
| web/sf.list10000 | generated | control | 1218709.4 | 1182953.1 | 0.97x | 1165806.2 | 0.96x | -1.4% | 2720056 | 2720056 | 30 % |  |
| sql/select20.at | generated | hand | 6843.2 | 17319.0 | 2.53x | 17449.7 | 2.55x | +0.8% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6826.0 | 17993.7 | 2.64x | 18154.9 | 2.66x | +0.9% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 375.6 | 391.0 | 1.04x | 408.0 | 1.09x | +4.4% | 0 | 0 | 10 % |  |
| tsql/select20.scan | generated | control | 377.2 | 392.3 | 1.04x | 383.9 | 1.02x | -2.1% | 0 | 0 | 9 % |  |
| el/ladder.scan | generated | control | 137.4 | 147.3 | 1.07x | 141.5 | 1.03x | -3.9% | 0 | 0 | 8 % |  |
| fixmsg/Order.parse-stream | generated | control | 1967.5 | 2160.2 | 1.10x | 2156.8 | 1.10x | -0.2% | 8224 | 8224 | 14 % |  |
| fixmsg/Order.parse-reader | generated | control | 1966.9 | 2148.2 | 1.09x | 2103.2 | 1.07x | -2.1% | 12112 | 12112 | 3 % |  |
| fixmsg/Order.parse-span | generated | control | 2000.1 | 1862.8 | 0.93x | 1866.7 | 0.93x | +0.2% | 3672 | 3672 | 16 % |  |
| fixmsg/Order.read-stream100 | generated | control | 207409.0 | 204690.6 | 0.99x | 201479.3 | 0.97x | -1.6% | 389248 | 389248 | 3 % |  |
| fixmsg/Order.read-reader100 | generated | control | 205481.1 | 195839.5 | 0.95x | 193151.6 | 0.94x | -1.4% | 375712 | 375712 | 4 % |  |
| fix/Orders128.log-text | generated | hand | 92762.5 | 72853.4 | 0.79x | 70680.1 | 0.76x | -3.0% | 129112 | 129112 | 8 % |  |
| fix/Orders128.log-bytes | generated | hand | 91348.9 | 94376.4 | 1.03x | 100589.3 | 1.10x | +6.6% | 129168 | 129168 | 4 % |  |
| fix/Orders128.log-stream | generated | hand | 91317.0 | 101978.3 | 1.12x | 102313.4 | 1.12x | +0.3% | 118552 | 118552 | 15 % |  |
| el/ladder.bool | generated | hand | 976.8 | 1673.7 | 1.71x | 1705.0 | 1.75x | +1.9% | 1776 | 1720 | 12 % |  |
| sql/refused-late.bool | generated | hand | 2570.0 | 11727.3 | 4.56x | 5727.2 | 2.23x | -51.2% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6964.6 | 17865.6 | 2.57x | 18167.6 | 2.61x | +1.7% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4519493.8 | 4574775.0 | 1.01x | 4614350.0 | 1.02x | +0.9% | 6276488 | 6276660 | 14 % |  |
| sql/refused-cliff-case-1738 | generated | control | 16031975.0 | 23786675.0 | 1.48x | 28201175.0 | 1.76x | +18.6% | 74363264 | 74363478 | 143 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2616550.0 | 2539518.8 | 0.97x | 2617950.0 | 1.00x | +3.1% | 2994056 | 2994056 | 14 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 6445556.2 | 6455125.0 | 1.00x | 6589928.1 | 1.02x | +2.1% | 12948767 | 12948763 | 19 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 9822900.0 | 9757137.5 | 0.99x | 8427987.5 | 0.86x | -13.6% | 21144103 | 21144138 | 71 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 30540150.0 | 31036275.0 | 1.02x | 30770700.0 | 1.01x | -0.9% | 94726470 | 94726542 | 56 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6610625.0 | 6684087.5 | 1.01x | 6846925.0 | 1.04x | +2.4% | 8778600 | 8778600 | 24 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6737306.2 | 6801337.5 | 1.01x | 6623662.5 | 0.98x | -2.6% | 8775448 | 8775467 | 19 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 31191950.0 | 30237425.0 | 0.97x | 29033925.0 | 0.93x | -4.0% | 94863815 | 94863193 | 83 % |  |
| sql/refused-cliff-and-3393 | generated | control | 28839275.0 | 26118875.0 | 0.91x | 29146325.0 | 1.01x | +11.6% | 94857595 | 94857607 | 55 % |  |
| fix/Orders128.yield-reader | generated | hand | 138700.4 | 94530.3 | 0.68x | 92509.6 | 0.67x | -2.1% | 118520 | 118520 | 7 % |  |
| fix/Orders128.yield-string | generated | hand | 82928.1 | 268473.9 | 3.24x | 292185.6 | 3.52x | +8.8% | 117936 | 117936 | 3 % |  |
| fix/Orders128.yield-memory | generated | hand | 82207.0 | 86083.1 | 1.05x | 86827.4 | 1.06x | +0.9% | 118016 | 118016 | 10 % |  |
| fix/Orders128.whole-stream | generated | hand | 118827.6 | 83514.5 | 0.70x | 82208.1 | 0.69x | -1.6% | 129368 | 129368 | 9 % |  |
| fix/Orders128.whole-reader | generated | hand | 118342.0 | 87935.1 | 0.74x | 86210.0 | 0.73x | -2.0% | 129336 | 129336 | 18 % |  |
| feeds/stock-count.good.text | generated | hand | 36742.9 | 32517.8 | 0.89x | 29696.4 | 0.81x | -8.7% | 111464 | 111403 | 2 % |  |
| feeds/stock-count.good.reader | generated | hand | 44712.2 | 41115.9 | 0.92x | 37978.4 | 0.85x | -7.6% | 111488 | 111488 | 4 % |  |
| feeds/stock-count.good.reader64 | generated | hand | 46629.0 | 45247.0 | 0.97x | 42044.1 | 0.90x | -7.1% | 111536 | 111627 | 3 % |  |
| web/url.plain | generated | hand | 96.4 | 223.1 | 2.31x | 231.4 | 2.40x | +3.7% | 360 | 360 | 9 % |  |
| web/url.full | generated | hand | 166.6 | 300.8 | 1.81x | 299.1 | 1.80x | -0.6% | 536 | 536 | 3 % |  |
| web/url.ipv4 | generated | hand | 112.6 | 231.4 | 2.06x | 236.1 | 2.10x | +2.0% | 384 | 384 | 3 % |  |
| web/url.long-path | generated | hand | 178.1 | 420.8 | 2.36x | 427.4 | 2.40x | +1.6% | 512 | 512 | 20 % |  |
| web/url.refused | generated | hand | 68.1 | 194.8 | 2.86x | 199.5 | 2.93x | +2.4% | 64 | 64 | 10 % |  |
| web/url.relative | generated | hand | 76.0 | 202.7 | 2.67x | 195.0 | 2.56x | -3.8% | 312 | 312 | 6 % |  |
| web/url.relative-dot-colon | generated | hand | 64.2 | 178.5 | 2.78x | 171.3 | 2.67x | -4.0% | 280 | 280 | 18 % |  |
| web/url.relative-letters | generated | hand | 105.2 | 242.5 | 2.30x | 238.8 | 2.27x | -1.5% | 312 | 312 | 2 % |  |
| web/json.object | generated | hand | 595.5 | 946.5 | 1.59x | 963.1 | 1.62x | +1.8% | 2584 | 2584 | 4 % |  |
| web/json.array | generated | hand | 600.9 | 741.3 | 1.23x | 739.1 | 1.23x | -0.3% | 2400 | 2400 | 3 % |  |
| web/media-type.plain | generated | control | 156.8 | 205.4 | 1.31x | 206.5 | 1.32x | +0.5% | 448 | 448 | 22 % |  |
| web/media-type.quoted | generated | control | 254.1 | 299.8 | 1.18x | 303.9 | 1.20x | +1.4% | 816 | 816 | 6 % |  |
| web/media-type.refused | generated | control | 45.5 | 81.5 | 1.79x | 81.9 | 1.80x | +0.5% | 64 | 64 | 4 % |  |
| web/addr-spec.plain | generated | control | 87.9 | 123.2 | 1.40x | 122.0 | 1.39x | -1.0% | 176 | 176 | 6 % |  |
| web/addr-spec.refused | generated | control | 24.9 | 61.8 | 2.48x | 60.6 | 2.43x | -1.9% | 64 | 64 | 33 % |  |
| web/json-patch.full | generated | control | 1354.1 | 1412.3 | 1.04x | 1423.1 | 1.05x | +0.8% | 4552 | 4552 | 13 % |  |
| web/link.full | generated | control | 1316.9 | 1418.1 | 1.08x | 1435.4 | 1.09x | +1.2% | 2968 | 2968 | 5 % |  |
| web/link.refused | generated | control | 20.8 | 53.9 | 2.59x | 53.7 | 2.58x | -0.3% | 64 | 64 | 4 % |  |
| web/sf.item | generated | control | 273.6 | 305.3 | 1.12x | 304.5 | 1.11x | -0.3% | 800 | 800 | 11 % |  |
| web/sf.list | generated | control | 1223.8 | 1246.0 | 1.02x | 1271.9 | 1.04x | +2.1% | 3064 | 3064 | 7 % |  |
| web/sf.dictionary | generated | control | 1214.9 | 1243.5 | 1.02x | 1258.8 | 1.04x | +1.2% | 3280 | 3280 | 8 % |  |
| el/ladder | generated | hand | 980.4 | 1684.4 | 1.72x | 1771.2 | 1.81x | +5.1% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 980.4 | 1152.7 | 1.18x | 1154.7 | 1.18x | +0.2% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 111956.4 | 152461.2 | 1.36x | 150916.7 | 1.35x | -1.0% | 169128 | 169128 | 4 % |  |
| el/terms1000 | immediate | hand | 111956.4 | 123662.3 | 1.10x | 124030.2 | 1.11x | +0.3% | 177120 | 177123 | 4 % |  |
| sql/select20 | generated | hand | 6806.4 | 17844.7 | 2.62x | 18209.2 | 2.68x | +2.0% | 21448 | 21448 | 12 % |  |
| sql/refused-late | generated | hand | 2571.3 | 11858.2 | 4.61x | 11965.6 | 4.65x | +0.9% | 13552 | 13552 | 5 % |  |
