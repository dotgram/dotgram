# Paired stand, 2026-09-20 21:36

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/Orders128.text | generated | hand | 83977.7 | 72881.6 | 0.87x | 71799.6 | 0.85x | -1.5% | 129112 | 129112 | 31 % |  |
| fix/Orders128.bytes | generated | hand | 80840.6 | 82220.7 | 1.02x | 82314.1 | 1.02x | +0.1% | 129168 | 129168 | 10 % |  |
| fix/Orders128.stream | generated | hand | 116558.6 | 90728.5 | 0.78x | 96077.1 | 0.82x | +5.9% | 118552 | 118552 | 2 % |  |
| fixmsg/Order.parse | generated | control | 1204.5 | 1857.2 | 1.54x | 1853.2 | 1.54x | -0.2% | 3400 | 3400 | 5 % |  |
| fixmsg/Order.build | generated | control | 1478.6 | 2324.1 | 1.57x | 2357.7 | 1.59x | +1.4% | 3592 | 3592 | 2 % |  |
| fix/orders400.text | generated | hand | 247696.5 | 222741.4 | 0.90x | 218571.1 | 0.88x | -1.9% | 403288 | 403288 | 3 % |  |
| tsql/script100.bool | generated | scriptdom | 660543.8 | 122990.6 | 0.19x | 123475.0 | 0.19x | +0.4% | 113600 | 105600 | 19 % |  |
| tsql/script100.boolboth | generated | scriptdom | 666768.0 | 125903.9 | 0.19x | 124564.8 | 0.19x | -1.1% | 105600 | 105600 | 15 % |  |
| tsql/script100 | generated | scriptdom | 659147.7 | 123909.4 | 0.19x | 122101.6 | 0.19x | -1.5% | 113600 | 113600 | 4 % |  |
| tsql/columns1000 | generated | scriptdom | 1732140.6 | 188287.5 | 0.11x | 189345.3 | 0.11x | +0.6% | 200464 | 200464 | 14 % |  |
| web/json.array10000 | generated | hand | 170176.8 | 193383.4 | 1.14x | 194738.3 | 1.14x | +0.7% | 720048 | 720048 | 23 % |  |
| web/json.object10000 | generated | hand | 654956.2 | 1172372.7 | 1.79x | 1410133.6 | 2.15x | +20.3% | 1599240 | 1599240 | 80 % |  |
| web/url.path1000 | generated | hand | 9356.4 | 10941.7 | 1.17x | 10968.2 | 1.17x | +0.2% | 8384 | 8384 | 4 % |  |
| web/media-type.params1000 | generated | control | 48333.3 | 44500.5 | 0.92x | 44650.7 | 0.92x | +0.3% | 151472 | 151472 | 4 % |  |
| web/sf.list10000 | generated | control | 1121978.1 | 1162692.2 | 1.04x | 1258142.2 | 1.12x | +8.2% | 2720056 | 2720056 | 24 % |  |
| sql/select20.at | generated | hand | 6779.3 | 17233.0 | 2.54x | 17299.2 | 2.55x | +0.4% | 21416 | 21416 | 19 % |  |
| sql/select20.window | generated | hand | 7289.3 | 18437.3 | 2.53x | 18717.5 | 2.57x | +1.5% | 21416 | 21416 | 47 % |  |
| sql/select20.scan | generated | control | 420.3 | 380.7 | 0.91x | 381.3 | 0.91x | +0.2% | 0 | 0 | 85 % |  |
| tsql/select20.scan | generated | control | 383.7 | 388.3 | 1.01x | 385.9 | 1.01x | -0.6% | 0 | 0 | 73 % |  |
| el/ladder.scan | generated | control | 138.6 | 137.5 | 0.99x | 137.8 | 0.99x | +0.2% | 0 | 0 | 2 % |  |
| fixmsg/Order.parse-stream | generated | control | 1889.3 | 2149.4 | 1.14x | 2183.2 | 1.16x | +1.6% | 8224 | 8224 | 5 % |  |
| fixmsg/Order.parse-reader | generated | control | 1900.4 | 2100.3 | 1.11x | 2102.0 | 1.11x | +0.1% | 12112 | 12112 | 15 % |  |
| fixmsg/Order.parse-span | generated | control | 1894.9 | 1838.4 | 0.97x | 1844.7 | 0.97x | +0.3% | 3672 | 3672 | 8 % |  |
| fixmsg/Order.read-stream100 | generated | control | 199931.4 | 200192.8 | 1.00x | 202464.8 | 1.01x | +1.1% | 389248 | 389248 | 10 % |  |
| fixmsg/Order.read-reader100 | generated | control | 201649.4 | 194568.9 | 0.96x | 195986.1 | 0.97x | +0.7% | 375712 | 375712 | 16 % |  |
| fix/Orders128.log-text | generated | hand | 89745.5 | 71478.7 | 0.80x | 70990.9 | 0.79x | -0.7% | 129112 | 129112 | 8 % |  |
| fix/Orders128.log-bytes | generated | hand | 89886.5 | 92999.6 | 1.03x | 93989.2 | 1.05x | +1.1% | 129168 | 129168 | 9 % |  |
| fix/Orders128.log-stream | generated | hand | 90145.9 | 101895.1 | 1.13x | 108025.9 | 1.20x | +6.0% | 118552 | 118552 | 2 % |  |
| el/ladder.bool | generated | hand | 991.9 | 1715.2 | 1.73x | 1707.9 | 1.72x | -0.4% | 1776 | 1720 | 17 % |  |
| sql/refused-late.bool | generated | hand | 2563.3 | 12001.2 | 4.68x | 5788.7 | 2.26x | -51.8% | 13552 | 6616 | 15 % |  |
| sql/select20.bool | generated | hand | 6814.0 | 18030.0 | 2.65x | 18102.8 | 2.66x | +0.4% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4536268.8 | 4572993.8 | 1.01x | 4572025.0 | 1.01x | 0.0% | 6276488 | 6276684 | 7 % |  |
| sql/refused-cliff-case-1738 | generated | control | 22233525.0 | 26995900.0 | 1.21x | 29862400.0 | 1.34x | +10.6% | 74363488 | 74363657 | 124 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2556596.9 | 2538434.4 | 0.99x | 2633878.1 | 1.03x | +3.8% | 2994123 | 2994166 | 13 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 5821087.5 | 5690493.8 | 0.98x | 5751800.0 | 0.99x | +1.1% | 12948798 | 12948780 | 63 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 10917706.2 | 10338025.0 | 0.95x | 10471093.8 | 0.96x | +1.3% | 21144137 | 21144220 | 28 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 31197325.0 | 31230575.0 | 1.00x | 31121100.0 | 1.00x | -0.4% | 94726733 | 94726332 | 91 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6686912.5 | 6812875.0 | 1.02x | 6725275.0 | 1.01x | -1.3% | 8778600 | 8778600 | 10 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6469900.0 | 6761456.2 | 1.05x | 6614356.2 | 1.02x | -2.2% | 8775448 | 8775448 | 15 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 28495975.0 | 26989350.0 | 0.95x | 26007875.0 | 0.91x | -3.6% | 94860575 | 94859949 | 94 % |  |
| sql/refused-cliff-and-3393 | generated | control | 26501625.0 | 24696225.0 | 0.93x | 29738100.0 | 1.12x | +20.4% | 94857775 | 94857944 | 78 % |  |
| fix/Orders128.yield-reader | generated | hand | 140983.6 | 95063.1 | 0.67x | 94531.3 | 0.67x | -0.6% | 118520 | 118520 | 7 % |  |
| fix/Orders128.yield-string | generated | hand | 81470.3 | 260652.7 | 3.20x | 265932.9 | 3.26x | +2.0% | 117936 | 117936 | 5 % |  |
| fix/Orders128.yield-memory | generated | hand | 114531.8 | 121459.7 | 1.06x | 128035.9 | 1.12x | +5.4% | 118016 | 118016 | 46 % |  |
| fix/Orders128.whole-stream | generated | hand | 173438.2 | 112816.1 | 0.65x | 142481.4 | 0.82x | +26.3% | 129368 | 129368 | 44 % |  |
| fix/Orders128.whole-reader | generated | hand | 116202.1 | 85195.3 | 0.73x | 85084.0 | 0.73x | -0.1% | 129336 | 129336 | 21 % |  |
| feeds/stock-count.good.text | generated | hand | 40217.2 | 28532.8 | 0.71x | 28910.5 | 0.72x | +1.3% | 111312 | 111403 | 5 % |  |
| feeds/stock-count.good.reader | generated | hand | 44767.9 | 36996.6 | 0.83x | 36779.7 | 0.82x | -0.6% | 111488 | 111579 | 11 % |  |
| feeds/stock-count.good.reader64 | generated | hand | 45733.2 | 41374.4 | 0.90x | 41138.6 | 0.90x | -0.6% | 111536 | 111627 | 12 % |  |
| web/url.plain | generated | hand | 95.7 | 219.7 | 2.29x | 222.0 | 2.32x | +1.1% | 360 | 360 | 24 % |  |
| web/url.full | generated | hand | 166.2 | 281.4 | 1.69x | 302.5 | 1.82x | +7.5% | 536 | 536 | 15 % |  |
| web/url.ipv4 | generated | hand | 111.3 | 229.4 | 2.06x | 231.1 | 2.08x | +0.7% | 384 | 384 | 3 % |  |
| web/url.long-path | generated | hand | 176.0 | 413.7 | 2.35x | 416.4 | 2.37x | +0.7% | 512 | 512 | 8 % |  |
| web/url.refused | generated | hand | 68.1 | 193.4 | 2.84x | 202.8 | 2.98x | +4.9% | 64 | 64 | 3 % |  |
| web/url.relative | generated | hand | 76.6 | 190.4 | 2.49x | 192.6 | 2.51x | +1.1% | 312 | 312 | 21 % |  |
| web/url.relative-dot-colon | generated | hand | 63.3 | 168.7 | 2.67x | 168.1 | 2.66x | -0.4% | 280 | 280 | 19 % |  |
| web/url.relative-letters | generated | hand | 105.3 | 232.2 | 2.21x | 228.7 | 2.17x | -1.5% | 312 | 312 | 2 % |  |
| web/json.object | generated | hand | 587.4 | 942.3 | 1.60x | 933.6 | 1.59x | -0.9% | 2584 | 2584 | 6 % |  |
| web/json.array | generated | hand | 595.2 | 755.8 | 1.27x | 739.6 | 1.24x | -2.2% | 2400 | 2400 | 5 % |  |
| web/media-type.plain | generated | control | 158.2 | 197.6 | 1.25x | 201.2 | 1.27x | +1.8% | 448 | 448 | 35 % |  |
| web/media-type.quoted | generated | control | 267.7 | 284.6 | 1.06x | 306.8 | 1.15x | +7.8% | 816 | 816 | 10 % |  |
| web/media-type.refused | generated | control | 45.9 | 81.6 | 1.78x | 82.0 | 1.78x | +0.4% | 64 | 64 | 8 % |  |
| web/addr-spec.plain | generated | control | 89.2 | 121.6 | 1.36x | 121.0 | 1.36x | -0.4% | 176 | 176 | 5 % |  |
| web/addr-spec.refused | generated | control | 25.3 | 59.8 | 2.36x | 60.0 | 2.37x | +0.3% | 64 | 64 | 15 % |  |
| web/json-patch.full | generated | control | 1378.4 | 1432.2 | 1.04x | 1421.5 | 1.03x | -0.7% | 4552 | 4552 | 3 % |  |
| web/link.full | generated | control | 1297.6 | 1419.0 | 1.09x | 1396.7 | 1.08x | -1.6% | 2968 | 2968 | 5 % |  |
| web/link.refused | generated | control | 20.6 | 53.4 | 2.60x | 53.7 | 2.61x | +0.7% | 64 | 64 | 12 % |  |
| web/sf.item | generated | control | 272.9 | 311.5 | 1.14x | 310.0 | 1.14x | -0.5% | 800 | 800 | 5 % |  |
| web/sf.list | generated | control | 1185.7 | 1253.9 | 1.06x | 1265.3 | 1.07x | +0.9% | 3064 | 3064 | 4 % |  |
| web/sf.dictionary | generated | control | 1235.9 | 1248.6 | 1.01x | 1271.3 | 1.03x | +1.8% | 3280 | 3280 | 2 % |  |
| el/ladder | generated | hand | 1002.0 | 1723.0 | 1.72x | 1736.1 | 1.73x | +0.8% | 1776 | 1776 | 7 % |  |
| el/ladder | immediate | hand | 1002.0 | 1192.3 | 1.19x | 1179.8 | 1.18x | -1.1% | 1784 | 1784 | 7 % |  |
| el/terms1000 | generated | hand | 112284.6 | 153692.0 | 1.37x | 156244.8 | 1.39x | +1.7% | 169107 | 169104 | 15 % |  |
| el/terms1000 | immediate | hand | 112284.6 | 126460.9 | 1.13x | 128918.9 | 1.15x | +1.9% | 177120 | 177120 | 15 % |  |
| sql/select20 | generated | hand | 6827.8 | 18046.3 | 2.64x | 18023.6 | 2.64x | -0.1% | 21448 | 21448 | 18 % |  |
| sql/refused-late | generated | hand | 2590.6 | 12066.8 | 4.66x | 12065.8 | 4.66x | 0.0% | 13552 | 13552 | 3 % |  |
