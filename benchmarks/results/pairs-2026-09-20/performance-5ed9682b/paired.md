Median of 5 of 5 runs, each in a process of its own; control 31.7 ns (the runs' controls: 31.7, 31.3, 31.5, 32.0, 31.9).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 22:28

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

This binary, and the libraries it holds as the control, was built from 0c0b75ff. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit d28533cb, framework net10.0, no properties, emitted b60541f92d7b864c); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| fix/Orders128.text | generated | hand | 82583.2 | 72111.3 | 0.87x | 71799.6 | 0.87x | -0.4% | 129112 | 129112 | 5 % | [-3.2%..+6.1%], 3 of 5 positive | +0.2% [-3.1%..+14.2%], 3 of 5 positive |
| fix/Orders128.bytes | generated | hand | 80866.4 | 82555.9 | 1.02x | 82792.6 | 1.02x | +0.3% | 129168 | 129168 | 6 % | [-0.2%..+3.0%], 4 of 5 positive | -0.5% [-1.5%..+4.3%], 4 of 5 positive |
| fix/Orders128.stream | generated | hand | 118172.7 | 91975.4 | 0.78x | 91358.8 | 0.77x | -0.7% | 118552 | 118552 | 9 % | [-2.9%..+5.9%], 3 of 5 positive | +0.1% [-2.2%..+1.5%], 3 of 5 positive |
| fixmsg/Order.parse | generated | control | 1252.4 | 1857.2 | 1.48x | 1890.0 | 1.51x | +1.8% | 3400 | 3400 | 13 % | [-1.5%..+3.7%], 2 of 5 positive | +0.7% [-1.9%..+3.3%], 3 of 5 positive |
| fixmsg/Order.build | generated | control | 1574.7 | 2379.3 | 1.51x | 2366.0 | 1.50x | -0.6% | 3592 | 3592 | 9 % | [-0.9%..+5.2%], 3 of 5 positive | +0.7% [+0.0%..+1.2%], 4 of 5 positive |
| fix/orders400.text | generated | hand | 257887.3 | 223431.6 | 0.87x | 218571.1 | 0.85x | -2.2% | 403288 | 403288 | 7 % | [-3.5%..+2.3%], 1 of 5 positive | +2.6% [-3.3%..+11.8%], 4 of 5 positive |
| tsql/script100.bool | generated | scriptdom | 670793.8 | 126078.1 | 0.19x | 126825.0 | 0.19x | +0.6% | 113600 | 105600 | 4 % | [-0.1%..+4.5%], 4 of 5 positive | +0.7% [-6.9%..+2.2%], 3 of 5 positive |
| tsql/script100.boolboth | generated | scriptdom | 672004.7 | 125635.2 | 0.19x | 126225.0 | 0.19x | +0.5% | 105600 | 105600 | 5 % | [-3.6%..+3.3%], 3 of 5 positive | -2.9% [-8.3%..-0.4%], 0 of 5 positive |
| tsql/script100 | generated | scriptdom | 673652.3 | 125239.1 | 0.19x | 125321.1 | 0.19x | +0.1% | 113600 | 113600 | 6 % | [-1.5%..+5.0%], 3 of 5 positive | -1.2% [-6.4%..+1.1%], 1 of 5 positive |
| tsql/columns1000 | generated | scriptdom | 1762923.4 | 193420.3 | 0.11x | 192940.6 | 0.11x | -0.2% | 200464 | 200464 | 5 % | [-8.8%..+0.6%], 1 of 5 positive | +0.6% [-1.3%..+6.5%], 4 of 5 positive |
| web/json.array10000 | generated | hand | 172227.1 | 194887.9 | 1.13x | 202098.2 | 1.17x | +3.7% | 720048 | 720048 | 6 % | [+0.0%..+5.2%], 5 of 5 positive | +1.0% [-4.7%..+3.4%], 3 of 5 positive |
| web/json.object10000 | generated | hand | 764749.2 | 1172372.7 | 1.53x | 1232761.7 | 1.61x | +5.2% | 1599240 | 1599240 | 21 % | [-10.4%..+20.3%], 3 of 5 positive | +11.6% [-9.6%..+25.1%], 3 of 5 positive |
| web/url.path1000 | generated | hand | 9454.5 | 11009.2 | 1.16x | 10968.2 | 1.16x | -0.4% | 8384 | 8384 | 3 % | [-0.8%..+1.0%], 3 of 5 positive | +0.2% [-2.1%..+2.9%], 2 of 5 positive |
| web/media-type.params1000 | generated | control | 48333.3 | 45250.2 | 0.94x | 45401.3 | 0.94x | +0.3% | 151472 | 151472 | 6 % | [-7.1%..+2.0%], 4 of 5 positive | -1.1% [-8.0%..+2.0%], 3 of 5 positive |
| web/sf.list10000 | generated | control | 1197653.1 | 1229214.1 | 1.03x | 1238103.1 | 1.03x | +0.7% | 2720056 | 2720056 | 9 % | [-1.4%..+8.2%], 2 of 5 positive | +1.9% [-1.8%..+17.1%], 3 of 5 positive |
| sql/select20.at | generated | hand | 6900.2 | 17780.2 | 2.58x | 17915.7 | 2.60x | +0.8% | 21416 | 21416 | 6 % | [-1.1%..+0.8%], 3 of 5 positive | -1.7% [-3.8%..+9.0%], 3 of 5 positive |
| sql/select20.window | generated | hand | 6920.1 | 18437.3 | 2.66x | 18154.9 | 2.62x | -1.5% | 21416 | 21416 | 7 % | [-4.8%..+1.5%], 3 of 5 positive | -0.6% [-2.0%..+10.1%], 2 of 5 positive |
| sql/select20.scan | generated | control | 384.4 | 387.0 | 1.01x | 388.0 | 1.01x | +0.2% | 0 | 0 | 12 % | [-0.1%..+4.4%], 4 of 5 positive | 0.0% [-2.8%..+1.5%], 2 of 5 positive |
| tsql/select20.scan | generated | control | 383.7 | 389.7 | 1.02x | 385.9 | 1.01x | -1.0% | 0 | 0 | 4 % | [-2.1%..+6.9%], 2 of 5 positive | -1.5% [-2.3%..+0.7%], 1 of 5 positive |
| el/ladder.scan | generated | control | 138.9 | 140.8 | 1.01x | 140.2 | 1.01x | -0.4% | 0 | 0 | 5 % | [-3.9%..+1.0%], 2 of 5 positive | -0.2% [-1.0%..+2.8%], 3 of 5 positive |
| fixmsg/Order.parse-stream | generated | control | 1967.5 | 2184.5 | 1.11x | 2183.2 | 1.11x | -0.1% | 8224 | 8224 | 4 % | [-1.0%..+1.6%], 2 of 5 positive | +0.3% [-1.7%..+0.8%], 3 of 5 positive |
| fixmsg/Order.parse-reader | generated | control | 1954.8 | 2113.1 | 1.08x | 2103.2 | 1.08x | -0.5% | 12112 | 12112 | 3 % | [-2.1%..+3.7%], 4 of 5 positive | +0.7% [-0.8%..+3.3%], 3 of 5 positive |
| fixmsg/Order.parse-span | generated | control | 1984.0 | 1843.9 | 0.93x | 1866.2 | 0.94x | +1.2% | 3672 | 3672 | 5 % | [-1.2%..+2.0%], 4 of 5 positive | +1.3% [-1.7%..+2.0%], 4 of 5 positive |
| fixmsg/Order.read-stream100 | generated | control | 207409.0 | 202368.6 | 0.98x | 204905.9 | 0.99x | +1.3% | 389248 | 389248 | 4 % | [-1.6%..+2.6%], 4 of 5 positive | +0.4% [-2.2%..+0.9%], 3 of 5 positive |
| fixmsg/Order.read-reader100 | generated | control | 204870.7 | 193062.3 | 0.94x | 195144.5 | 0.95x | +1.1% | 375712 | 375712 | 3 % | [-1.4%..+2.6%], 4 of 5 positive | +1.1% [-0.6%..+2.2%], 4 of 5 positive |
| fix/Orders128.log-text | generated | hand | 92595.1 | 72188.1 | 0.78x | 70990.9 | 0.77x | -1.7% | 129112 | 129112 | 5 % | [-3.0%..+0.1%], 1 of 5 positive | -1.3% [-3.4%..+8.6%], 3 of 5 positive |
| fix/Orders128.log-bytes | generated | hand | 91348.9 | 94824.8 | 1.04x | 95707.5 | 1.05x | +0.9% | 129168 | 129168 | 4 % | [-1.7%..+6.6%], 4 of 5 positive | -0.6% [-2.0%..+3.4%], 3 of 5 positive |
| fix/Orders128.log-stream | generated | hand | 91652.0 | 102566.2 | 1.12x | 103496.4 | 1.13x | +0.9% | 118552 | 118552 | 4 % | [-0.3%..+6.0%], 4 of 5 positive | -0.1% [-0.2%..+2.8%], 3 of 5 positive |
| el/ladder.bool | generated | hand | 1005.4 | 1732.5 | 1.72x | 1764.4 | 1.75x | +1.8% | 1776 | 1720 | 6 % | [-0.4%..+2.5%], 4 of 5 positive | +1.1% [-4.2%..+5.9%], 2 of 5 positive |
| sql/refused-late.bool | generated | hand | 2573.2 | 12001.2 | 4.66x | 5809.7 | 2.26x | -51.6% | 13552 | 6616 | 3 % | [-52.2%..-51.2%], 0 of 5 positive | -50.5% [-53.2%..-48.5%], 0 of 5 positive |
| sql/select20.bool | generated | hand | 6964.6 | 18030.0 | 2.59x | 18167.6 | 2.61x | +0.8% | 21448 | 21392 | 5 % | [-2.2%..+1.7%], 3 of 5 positive | +1.5% [-2.1%..+9.0%], 3 of 5 positive |
| sql/refused-cliff-case-1391 | generated | control | 4562881.2 | 4605025.0 | 1.01x | 4614350.0 | 1.01x | +0.2% | 6276488 | 6276660 | 2 % | [-2.3%..+0.9%], 1 of 5 positive | +1.1% [-6.8%..+2.0%], 3 of 5 positive |
| sql/refused-cliff-case-1738 | generated | control | 22233525.0 | 26995900.0 | 1.21x | 29922650.0 | 1.35x | +10.8% | 74363488 | 74363549 | 32 % | [-3.6%..+19.2%], 4 of 5 positive | -11.8% [-19.2%..+8.3%], 1 of 5 positive |
| sql/refused-cliff-joins-891 | generated | control | 2616550.0 | 2579921.9 | 0.99x | 2611337.5 | 1.00x | +1.2% | 2994123 | 2994142 | 3 % | [-3.4%..+3.8%], 3 of 5 positive | +1.6% [-6.1%..+2.3%], 2 of 5 positive |
| sql/refused-cliff-joins-1113 | generated | control | 5985625.0 | 5690493.8 | 0.95x | 5751800.0 | 0.96x | +1.1% | 12948797 | 12948780 | 22 % | [-2.8%..+2.1%], 3 of 5 positive | +0.8% [-4.3%..+4.7%], 4 of 5 positive |
| sql/refused-cliff-joins-1738 | generated | control | 10353581.2 | 10718112.5 | 1.04x | 10471093.8 | 1.01x | -2.3% | 21144108 | 21144138 | 12 % | [-13.6%..+4.1%], 2 of 5 positive | -0.2% [-7.0%..+12.9%], 2 of 5 positive |
| sql/refused-cliff-joins-2172 | generated | control | 31197325.0 | 31230575.0 | 1.00x | 30538550.0 | 0.98x | -2.2% | 94726470 | 94726542 | 39 % | [-18.9%..-0.4%], 0 of 5 positive | +1.0% [-6.8%..+2.3%], 3 of 5 positive |
| sql/refused-cliff-paren-2715 | generated | control | 6686912.5 | 6812875.0 | 1.02x | 6846925.0 | 1.02x | +0.5% | 8778600 | 8778600 | 39 % | [-6.9%..+3.1%], 2 of 5 positive | -1.7% [-3.7%..+1.3%], 3 of 5 positive |
| sql/refused-cliff-and-2715 | generated | control | 6737306.2 | 6801337.5 | 1.01x | 6690831.2 | 0.99x | -1.6% | 8775448 | 8775448 | 51 % | [-5.2%..-0.2%], 0 of 5 positive | -2.7% [-2.7%..+3.1%], 2 of 5 positive |
| sql/refused-cliff-paren-3393 | generated | control | 31191950.0 | 30237425.0 | 0.97x | 29033925.0 | 0.93x | -4.0% | 94860575 | 94861051 | 26 % | [-6.2%..+18.2%], 1 of 5 positive | -0.1% [-4.9%..+2.0%], 4 of 5 positive |
| sql/refused-cliff-and-3393 | generated | control | 27555050.0 | 26118875.0 | 0.95x | 29146325.0 | 1.06x | +11.6% | 94857394 | 94857730 | 64 % | [-21.0%..+20.4%], 3 of 5 positive | +1.5% [-12.8%..+28.7%], 3 of 5 positive |
| fix/Orders128.yield-reader | generated | hand | 140983.6 | 95063.1 | 0.67x | 94531.3 | 0.67x | -0.6% | 118520 | 118520 | 5 % | [-2.1%..+2.9%], 2 of 5 positive | +1.7% [+0.6%..+5.4%], 5 of 5 positive |
| fix/Orders128.yield-string | generated | hand | 84797.1 | 268473.9 | 3.17x | 283967.7 | 3.35x | +5.8% | 117936 | 117936 | 5 % | [-5.8%..+11.5%], 4 of 5 positive | -1.1% [-4.7%..+4.4%], 2 of 5 positive |
| fix/Orders128.yield-memory | generated | hand | 84403.8 | 87205.4 | 1.03x | 87293.8 | 1.03x | +0.1% | 118016 | 118016 | 40 % | [-2.0%..+5.4%], 4 of 5 positive | 0.0% [-2.1%..+0.7%], 2 of 5 positive |
| fix/Orders128.whole-stream | generated | hand | 118827.6 | 83713.5 | 0.70x | 83852.6 | 0.71x | +0.2% | 129368 | 129368 | 46 % | [-1.9%..+26.3%], 3 of 5 positive | +1.8% [-1.9%..+3.2%], 4 of 5 positive |
| fix/Orders128.whole-reader | generated | hand | 118713.4 | 85865.1 | 0.72x | 86210.0 | 0.73x | +0.4% | 129336 | 129336 | 3 % | [-2.0%..+0.6%], 2 of 5 positive | +2.6% [-1.4%..+8.6%], 3 of 5 positive |
| feeds/stock-count.good.text | generated | hand | 37069.6 | 29986.0 | 0.81x | 29696.4 | 0.80x | -1.0% | 111312 | 111403 | 9 % | [-8.7%..+1.3%], 1 of 5 positive | -1.5% [-2.8%..+4.6%], 2 of 5 positive |
| feeds/stock-count.good.reader | generated | hand | 45180.1 | 38393.9 | 0.85x | 37978.4 | 0.84x | -1.1% | 111488 | 111579 | 6 % | [-7.6%..+0.5%], 1 of 5 positive | -0.2% [-1.8%..+7.2%], 1 of 5 positive |
| feeds/stock-count.good.reader64 | generated | hand | 46793.8 | 42717.5 | 0.91x | 42361.1 | 0.91x | -0.8% | 111536 | 111627 | 5 % | [-7.1%..+0.3%], 2 of 5 positive | +1.6% [-0.2%..+6.1%], 3 of 5 positive |
| web/url.plain | generated | hand | 97.4 | 227.0 | 2.33x | 231.4 | 2.38x | +2.0% | 360 | 360 | 3 % | [-1.7%..+3.7%], 4 of 5 positive | +0.5% [-0.6%..+2.9%], 4 of 5 positive |
| web/url.full | generated | hand | 169.3 | 300.8 | 1.78x | 302.5 | 1.79x | +0.6% | 536 | 536 | 5 % | [-3.6%..+7.5%], 2 of 5 positive | -1.0% [-5.5%..+4.1%], 2 of 5 positive |
| web/url.ipv4 | generated | hand | 112.6 | 238.0 | 2.11x | 236.4 | 2.10x | -0.7% | 384 | 384 | 4 % | [-4.5%..+2.0%], 4 of 5 positive | +1.2% [-1.5%..+10.7%], 4 of 5 positive |
| web/url.long-path | generated | hand | 179.3 | 435.3 | 2.43x | 428.9 | 2.39x | -1.5% | 512 | 512 | 70 % | [-1.5%..+9.1%], 4 of 5 positive | +1.4% [-1.1%..+1.5%], 3 of 5 positive |
| web/url.refused | generated | hand | 68.1 | 199.5 | 2.93x | 202.8 | 2.98x | +1.7% | 64 | 64 | 8 % | [-0.8%..+4.9%], 4 of 5 positive | -0.8% [-2.0%..+4.8%], 2 of 5 positive |
| web/url.relative | generated | hand | 76.9 | 202.7 | 2.63x | 202.8 | 2.64x | +0.1% | 312 | 312 | 13 % | [-11.5%..+1.2%], 2 of 5 positive | +0.2% [-1.1%..+2.3%], 3 of 5 positive |
| web/url.relative-dot-colon | generated | hand | 64.3 | 178.5 | 2.78x | 177.2 | 2.75x | -0.7% | 280 | 280 | 8 % | [-6.8%..+3.9%], 1 of 5 positive | +2.3% [-1.2%..+3.7%], 3 of 5 positive |
| web/url.relative-letters | generated | hand | 105.5 | 242.5 | 2.30x | 243.0 | 2.30x | +0.2% | 312 | 312 | 28 % | [-1.9%..+3.3%], 1 of 5 positive | +2.3% [-1.9%..+4.8%], 3 of 5 positive |
| web/json.object | generated | hand | 605.6 | 946.5 | 1.56x | 967.3 | 1.60x | +2.2% | 2584 | 2584 | 24 % | [-6.2%..+3.4%], 3 of 5 positive | +0.1% [-5.4%..+1.9%], 3 of 5 positive |
| web/json.array | generated | hand | 600.9 | 761.2 | 1.27x | 740.7 | 1.23x | -2.7% | 2400 | 2400 | 20 % | [-3.4%..-0.3%], 0 of 5 positive | +0.4% [-3.6%..+3.2%], 3 of 5 positive |
| web/media-type.plain | generated | control | 165.3 | 206.0 | 1.25x | 216.7 | 1.31x | +5.2% | 448 | 448 | 19 % | [-8.6%..+5.3%], 4 of 5 positive | -1.6% [-14.2%..+14.3%], 3 of 5 positive |
| web/media-type.quoted | generated | control | 267.7 | 304.3 | 1.14x | 331.3 | 1.24x | +8.9% | 816 | 816 | 24 % | [+0.5%..+8.9%], 5 of 5 positive | -2.6% [-10.2%..+5.4%], 3 of 5 positive |
| web/media-type.refused | generated | control | 46.1 | 83.0 | 1.80x | 82.1 | 1.78x | -1.1% | 64 | 64 | 21 % | [-1.6%..+1.9%], 3 of 5 positive | +0.1% [-0.5%..+1.2%], 4 of 5 positive |
| web/addr-spec.plain | generated | control | 89.2 | 121.8 | 1.37x | 124.6 | 1.40x | +2.3% | 176 | 176 | 14 % | [-2.6%..+2.5%], 2 of 5 positive | -3.2% [-3.9%..+1.4%], 1 of 5 positive |
| web/addr-spec.refused | generated | control | 25.1 | 60.8 | 2.43x | 60.6 | 2.42x | -0.4% | 64 | 64 | 16 % | [-1.9%..+0.3%], 1 of 5 positive | +0.8% [-0.2%..+13.7%], 4 of 5 positive |
| web/json-patch.full | generated | control | 1378.4 | 1445.0 | 1.05x | 1426.1 | 1.03x | -1.3% | 4552 | 4552 | 27 % | [-1.3%..+0.8%], 2 of 5 positive | +2.0% [-0.9%..+4.7%], 2 of 5 positive |
| web/link.full | generated | control | 1316.9 | 1419.1 | 1.08x | 1435.4 | 1.09x | +1.1% | 2968 | 2968 | 16 % | [-2.8%..+5.7%], 2 of 5 positive | +1.8% [-1.1%..+2.1%], 4 of 5 positive |
| web/link.refused | generated | control | 20.8 | 55.0 | 2.65x | 55.6 | 2.67x | +0.9% | 64 | 64 | 26 % | [-0.5%..+2.2%], 3 of 5 positive | +0.8% [-0.9%..+1.5%], 4 of 5 positive |
| web/sf.item | generated | control | 283.9 | 354.5 | 1.25x | 333.0 | 1.17x | -6.1% | 800 | 800 | 13 % | [-6.4%..+4.2%], 1 of 5 positive | -4.1% [-5.6%..-2.3%], 0 of 5 positive |
| web/sf.list | generated | control | 1223.8 | 1336.9 | 1.09x | 1320.5 | 1.08x | -1.2% | 3064 | 3064 | 12 % | [-2.6%..+2.1%], 3 of 5 positive | +1.4% [-0.3%..+2.1%], 4 of 5 positive |
| web/sf.dictionary | generated | control | 1254.8 | 1288.6 | 1.03x | 1316.2 | 1.05x | +2.1% | 3280 | 3280 | 9 % | [-6.1%..+3.4%], 3 of 5 positive | +0.2% [-5.4%..+0.2%], 2 of 5 positive |
| el/ladder | generated | hand | 1031.0 | 1769.4 | 1.72x | 1781.4 | 1.73x | +0.7% | 1776 | 1776 | 6 % | [+0.2%..+5.1%], 5 of 5 positive | +0.8% [+0.0%..+6.5%], 4 of 5 positive |
| el/ladder | immediate | hand | 1031.0 | 1223.9 | 1.19x | 1207.1 | 1.17x | -1.4% | 1784 | 1784 | 6 % | [-1.9%..+3.0%], 2 of 5 positive | -1.9% [-3.1%..+1.6%], 1 of 5 positive |
| el/terms1000 | generated | hand | 115529.1 | 159622.9 | 1.38x | 156244.8 | 1.35x | -2.1% | 169104 | 169104 | 8 % | [-4.4%..+6.3%], 3 of 5 positive | -0.3% [-1.4%..+0.8%], 1 of 5 positive |
| el/terms1000 | immediate | hand | 115529.1 | 129285.1 | 1.12x | 130977.4 | 1.13x | +1.3% | 177120 | 177123 | 8 % | [-0.1%..+2.0%], 4 of 5 positive | -0.8% [-1.9%..+1.5%], 1 of 5 positive |
| sql/select20 | generated | hand | 7232.0 | 19192.5 | 2.65x | 18601.3 | 2.57x | -3.1% | 21448 | 21448 | 16 % | [-4.8%..+2.2%], 2 of 5 positive | -0.3% [-2.6%..+9.7%], 2 of 5 positive |
| sql/refused-late | generated | hand | 2744.7 | 12240.5 | 4.46x | 12341.5 | 4.50x | +0.8% | 13552 | 13552 | 8 % | [-2.0%..+0.9%], 2 of 5 positive | -0.9% [-2.3%..+3.2%], 2 of 5 positive |
