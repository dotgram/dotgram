# Paired stand, 2026-09-20 22:13

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Orders128.text | generated | hand | 86558.6 | 74757.0 | 0.86x | 74026.2 | 0.86x | -1.0% | 129112 | 129112 | 54 % |  |
| fix/Orders128.bytes | generated | hand | 83799.6 | 87853.1 | 1.05x | 88626.2 | 1.06x | +0.9% | 129168 | 129168 | 40 % |  |
| fix/Orders128.stream | generated | hand | 125556.3 | 98669.9 | 0.79x | 96520.7 | 0.77x | -2.2% | 118552 | 118552 | 15 % |  |
| fixmsg/Order.parse | generated | control | 1295.7 | 1907.1 | 1.47x | 1870.5 | 1.44x | -1.9% | 3400 | 3400 | 15 % |  |
| fixmsg/Order.build | generated | control | 1656.7 | 2429.0 | 1.47x | 2449.3 | 1.48x | +0.8% | 3592 | 3592 | 8 % |  |
| fix/orders400.text | generated | hand | 264703.5 | 238953.1 | 0.90x | 240623.8 | 0.91x | +0.7% | 403288 | 403288 | 12 % |  |
| tsql/script100.bool | generated | scriptdom | 680875.0 | 134296.9 | 0.20x | 125050.0 | 0.18x | -6.9% | 113600 | 105603 | 15 % |  |
| tsql/script100.boolboth | generated | scriptdom | 705914.1 | 135614.1 | 0.19x | 124360.2 | 0.18x | -8.3% | 105600 | 105603 | 10 % |  |
| tsql/script100 | generated | scriptdom | 693310.9 | 134882.8 | 0.19x | 126214.8 | 0.18x | -6.4% | 113600 | 113600 | 14 % |  |
| tsql/columns1000 | generated | scriptdom | 1840875.0 | 195943.8 | 0.11x | 208634.4 | 0.11x | +6.5% | 200464 | 200464 | 5 % |  |
| web/json.array10000 | generated | hand | 178551.2 | 210004.5 | 1.18x | 207876.4 | 1.16x | -1.0% | 720048 | 720048 | 14 % |  |
| web/json.object10000 | generated | hand | 966546.9 | 1196464.1 | 1.24x | 1433299.2 | 1.48x | +19.8% | 1599240 | 1599240 | 114 % |  |
| web/url.path1000 | generated | hand | 9583.7 | 11331.7 | 1.18x | 11607.5 | 1.21x | +2.4% | 8384 | 8384 | 10 % |  |
| web/media-type.params1000 | generated | control | 49744.3 | 45624.5 | 0.92x | 45108.3 | 0.91x | -1.1% | 151472 | 151472 | 24 % |  |
| web/sf.list10000 | generated | control | 1221209.4 | 1206770.3 | 0.99x | 1208843.8 | 0.99x | +0.2% | 2720056 | 2720056 | 25 % |  |
| sql/select20.at | generated | hand | 7102.8 | 18061.9 | 2.54x | 19689.4 | 2.77x | +9.0% | 21416 | 21416 | 25 % |  |
| sql/select20.window | generated | hand | 7016.3 | 18332.0 | 2.61x | 20177.1 | 2.88x | +10.1% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 384.9 | 396.8 | 1.03x | 388.2 | 1.01x | -2.1% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 382.5 | 400.1 | 1.05x | 396.2 | 1.04x | -1.0% | 0 | 0 | 8 % |  |
| el/ladder.scan | generated | control | 142.4 | 141.5 | 0.99x | 141.2 | 0.99x | -0.2% | 0 | 0 | 12 % |  |
| fixmsg/Order.parse-stream | generated | control | 1959.0 | 2239.4 | 1.14x | 2257.9 | 1.15x | +0.8% | 8224 | 8224 | 71 % |  |
| fixmsg/Order.parse-reader | generated | control | 1925.8 | 2059.9 | 1.07x | 2128.9 | 1.11x | +3.3% | 12112 | 12112 | 5 % |  |
| fixmsg/Order.parse-span | generated | control | 1944.7 | 1794.0 | 0.92x | 1829.6 | 0.94x | +2.0% | 3672 | 3672 | 4 % |  |
| fixmsg/Order.read-stream100 | generated | control | 201513.1 | 201950.8 | 1.00x | 203603.7 | 1.01x | +0.8% | 389248 | 389248 | 5 % |  |
| fixmsg/Order.read-reader100 | generated | control | 203463.1 | 193762.1 | 0.95x | 195821.7 | 0.96x | +1.1% | 375712 | 375712 | 8 % |  |
| fix/Orders128.log-text | generated | hand | 93149.4 | 73975.3 | 0.79x | 71628.1 | 0.77x | -3.2% | 129112 | 129112 | 6 % |  |
| fix/Orders128.log-bytes | generated | hand | 95039.6 | 98366.9 | 1.04x | 96421.5 | 1.01x | -2.0% | 129168 | 129168 | 20 % |  |
| fix/Orders128.log-stream | generated | hand | 94323.7 | 106113.3 | 1.12x | 107047.4 | 1.13x | +0.9% | 118552 | 118552 | 13 % |  |
| el/ladder.bool | generated | hand | 1054.8 | 1842.4 | 1.75x | 1765.5 | 1.67x | -4.2% | 1776 | 1720 | 31 % |  |
| sql/refused-late.bool | generated | hand | 2655.3 | 12063.6 | 4.54x | 6211.7 | 2.34x | -48.5% | 13552 | 6616 | 13 % |  |
| sql/select20.bool | generated | hand | 6937.6 | 18282.0 | 2.64x | 19936.3 | 2.87x | +9.0% | 21448 | 21392 | 15 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4622440.6 | 4617218.8 | 1.00x | 4635443.8 | 1.00x | +0.4% | 6276488 | 6276488 | 41 % |  |
| sql/refused-cliff-case-1738 | generated | control | 24912925.0 | 26789100.0 | 1.08x | 23641300.0 | 0.95x | -11.8% | 74363441 | 74363513 | 116 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2678312.5 | 2649925.0 | 0.99x | 2698815.6 | 1.01x | +1.8% | 2994142 | 2994123 | 9 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 6359337.5 | 5602525.0 | 0.88x | 5362450.0 | 0.84x | -4.3% | 12948766 | 12948764 | 61 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 10355275.0 | 10927487.5 | 1.06x | 10583525.0 | 1.02x | -3.1% | 21144124 | 21144135 | 43 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 29420450.0 | 30277900.0 | 1.03x | 30962950.0 | 1.05x | +2.3% | 94726498 | 94726942 | 110 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6847100.0 | 6800843.8 | 0.99x | 6855337.5 | 1.00x | +0.8% | 8778600 | 8778360 | 19 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6866156.2 | 6845281.2 | 1.00x | 6662662.5 | 0.97x | -2.7% | 8775448 | 8775208 | 9 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 27246775.0 | 28941000.0 | 1.06x | 29041950.0 | 1.07x | +0.3% | 94860361 | 94861390 | 63 % |  |
| sql/refused-cliff-and-3393 | generated | control | 26810850.0 | 28105975.0 | 1.05x | 28529125.0 | 1.06x | +1.5% | 94857668 | 94857406 | 76 % |  |
| fix/Orders128.yield-reader | generated | hand | 141363.9 | 94297.5 | 0.67x | 94817.0 | 0.67x | +0.6% | 118520 | 118520 | 7 % |  |
| fix/Orders128.yield-string | generated | hand | 84868.3 | 275452.6 | 3.25x | 270895.7 | 3.19x | -1.7% | 117936 | 117936 | 26 % |  |
| fix/Orders128.yield-memory | generated | hand | 85199.1 | 90577.0 | 1.06x | 90534.5 | 1.06x | 0.0% | 118016 | 118016 | 81 % |  |
| fix/Orders128.whole-stream | generated | hand | 120876.1 | 89120.9 | 0.74x | 87416.9 | 0.72x | -1.9% | 129368 | 129368 | 7 % |  |
| fix/Orders128.whole-reader | generated | hand | 120962.5 | 88956.0 | 0.74x | 87712.1 | 0.73x | -1.4% | 129336 | 129336 | 5 % |  |
| feeds/stock-count.good.text | generated | hand | 41352.0 | 31475.0 | 0.76x | 32912.1 | 0.80x | +4.6% | 111312 | 111312 | 53 % |  |
| feeds/stock-count.good.reader | generated | hand | 48191.3 | 39033.7 | 0.81x | 41835.2 | 0.87x | +7.2% | 111579 | 111579 | 8 % |  |
| feeds/stock-count.good.reader64 | generated | hand | 48394.1 | 43784.9 | 0.90x | 46476.3 | 0.96x | +6.1% | 111536 | 111627 | 18 % |  |
| web/url.plain | generated | hand | 98.1 | 230.1 | 2.35x | 232.5 | 2.37x | +1.0% | 360 | 360 | 12 % |  |
| web/url.full | generated | hand | 171.4 | 297.9 | 1.74x | 310.2 | 1.81x | +4.1% | 536 | 536 | 12 % |  |
| web/url.ipv4 | generated | hand | 117.3 | 239.5 | 2.04x | 241.2 | 2.06x | +0.7% | 384 | 384 | 12 % |  |
| web/url.long-path | generated | hand | 182.6 | 460.0 | 2.52x | 454.8 | 2.49x | -1.1% | 512 | 512 | 25 % |  |
| web/url.refused | generated | hand | 71.5 | 207.0 | 2.89x | 217.0 | 3.03x | +4.8% | 64 | 64 | 85 % |  |
| web/url.relative | generated | hand | 78.5 | 201.9 | 2.57x | 199.6 | 2.54x | -1.1% | 312 | 312 | 6 % |  |
| web/url.relative-dot-colon | generated | hand | 65.3 | 175.4 | 2.69x | 173.3 | 2.66x | -1.2% | 280 | 280 | 4 % |  |
| web/url.relative-letters | generated | hand | 109.8 | 244.7 | 2.23x | 244.6 | 2.23x | 0.0% | 312 | 312 | 9 % |  |
| web/json.object | generated | hand | 616.6 | 1004.3 | 1.63x | 950.4 | 1.54x | -5.4% | 2584 | 2584 | 7 % |  |
| web/json.array | generated | hand | 609.6 | 786.0 | 1.29x | 758.0 | 1.24x | -3.6% | 2400 | 2400 | 10 % |  |
| web/media-type.plain | generated | control | 163.9 | 216.1 | 1.32x | 216.2 | 1.32x | 0.0% | 448 | 448 | 15 % |  |
| web/media-type.quoted | generated | control | 257.8 | 310.8 | 1.21x | 311.6 | 1.21x | +0.2% | 816 | 816 | 8 % |  |
| web/media-type.refused | generated | control | 47.6 | 82.9 | 1.74x | 83.9 | 1.76x | +1.2% | 64 | 64 | 10 % |  |
| web/addr-spec.plain | generated | control | 89.8 | 130.7 | 1.45x | 126.6 | 1.41x | -3.1% | 176 | 176 | 7 % |  |
| web/addr-spec.refused | generated | control | 25.6 | 62.1 | 2.42x | 62.5 | 2.44x | +0.8% | 64 | 64 | 8 % |  |
| web/json-patch.full | generated | control | 1392.3 | 1473.5 | 1.06x | 1487.2 | 1.07x | +0.9% | 4552 | 4552 | 12 % |  |
| web/link.full | generated | control | 1375.2 | 1430.5 | 1.04x | 1449.9 | 1.05x | +1.4% | 2968 | 2968 | 18 % |  |
| web/link.refused | generated | control | 20.7 | 55.4 | 2.68x | 55.5 | 2.69x | +0.2% | 64 | 64 | 15 % |  |
| web/sf.item | generated | control | 278.8 | 328.1 | 1.18x | 320.5 | 1.15x | -2.3% | 800 | 800 | 14 % |  |
| web/sf.list | generated | control | 1212.3 | 1307.4 | 1.08x | 1319.5 | 1.09x | +0.9% | 3064 | 3064 | 5 % |  |
| web/sf.dictionary | generated | control | 1272.4 | 1320.4 | 1.04x | 1323.5 | 1.04x | +0.2% | 3280 | 3280 | 21 % |  |
| el/ladder | generated | hand | 1022.5 | 1784.9 | 1.75x | 1794.7 | 1.76x | +0.6% | 1776 | 1776 | 6 % |  |
| el/ladder | immediate | hand | 1022.5 | 1223.2 | 1.20x | 1197.2 | 1.17x | -2.1% | 1784 | 1784 | 6 % |  |
| el/terms1000 | generated | hand | 112679.0 | 155351.2 | 1.38x | 153132.1 | 1.36x | -1.4% | 169104 | 169131 | 22 % |  |
| el/terms1000 | immediate | hand | 112679.0 | 128085.4 | 1.14x | 130021.9 | 1.15x | +1.5% | 177120 | 177120 | 22 % |  |
| sql/select20 | generated | hand | 6954.5 | 18520.2 | 2.66x | 20324.3 | 2.92x | +9.7% | 21448 | 21448 | 8 % |  |
| sql/refused-late | generated | hand | 2643.8 | 12396.5 | 4.69x | 12788.7 | 4.84x | +3.2% | 13552 | 13552 | 29 % |  |
