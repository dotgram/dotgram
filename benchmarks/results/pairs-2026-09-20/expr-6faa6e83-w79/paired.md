Median of 4 of 4 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.4, 31.4, 31.7, 31.5).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 09:30

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| fix/One.text | generated | hand | 77.8 | 84.0 | 1.08x | 86.5 | 1.11x | +3.0% | 192 | 192 | 4 % | [+1.3%..+21.0%], 4 of 4 positive | +0.8% [+0.3%..+1.8%], 4 of 4 positive |
| fix/One.bytes | generated | hand | 75.6 | 98.1 | 1.30x | 99.9 | 1.32x | +1.8% | 248 | 248 | 8 % | [-4.3%..+3.8%], 3 of 4 positive |  |
| fix/One.stream | generated | hand | 156.0 | 551.4 | 3.53x | 538.2 | 3.45x | -2.4% | 888 | 888 | 51 % | [-6.2%..+7.6%], 3 of 4 positive |  |
| fix/Order.text | generated | hand | 696.7 | 678.1 | 0.97x | 691.0 | 0.99x | +1.9% | 1096 | 1096 | 34 % | [-1.0%..+4.2%], 3 of 4 positive | -3.9% [-4.8%..-1.4%], 0 of 4 positive |
| fix/Order.bytes | generated | hand | 678.8 | 762.8 | 1.12x | 761.1 | 1.12x | -0.2% | 1152 | 1152 | 5 % | [-1.0%..+0.6%], 1 of 4 positive |  |
| fix/Order.stream | generated | hand | 816.0 | 2831.2 | 3.47x | 2843.6 | 3.49x | +0.4% | 1712 | 1712 | 6 % | [-0.6%..+2.8%], 2 of 4 positive |  |
| fix/BinaryMany.text | generated | hand | 3640.0 | 3815.4 | 1.05x | 3824.3 | 1.05x | +0.2% | 7256 | 7256 | 3 % | [-0.9%..+1.3%], 2 of 4 positive |  |
| fix/BinaryMany.bytes | generated | hand | 3750.0 | 4646.6 | 1.24x | 4631.3 | 1.24x | -0.3% | 7312 | 7312 | 3 % | [-1.0%..+0.5%], 1 of 4 positive |  |
| fix/BinaryMany.stream | generated | hand | 4758.4 | 19337.2 | 4.06x | 18807.1 | 3.95x | -2.7% | 7448 | 7448 | 2 % | [-4.3%..+3.3%], 1 of 4 positive |  |
| fix/Orders128.text | generated | hand | 81069.5 | 78675.8 | 0.97x | 76632.4 | 0.95x | -2.6% | 129112 | 129112 | 4 % | [-3.2%..-0.2%], 0 of 4 positive |  |
| fix/Orders128.bytes | generated | hand | 80944.3 | 90191.7 | 1.11x | 88289.9 | 1.09x | -2.1% | 129168 | 129168 | 5 % | [-3.9%..-0.5%], 0 of 4 positive |  |
| fix/Orders128.stream | generated | hand | 117417.0 | 303026.2 | 2.58x | 305423.7 | 2.60x | +0.8% | 118552 | 118552 | 2 % | [-3.0%..+1.9%], 3 of 4 positive |  |
| fix/OrderMalformed.text | generated | hand | 679.8 | 659.6 | 0.97x | 681.2 | 1.00x | +3.3% | 1248 | 1248 | 7 % | [-1.1%..+6.9%], 3 of 4 positive |  |
| fix/OrderMalformed.bytes | generated | hand | 690.8 | 766.8 | 1.11x | 771.2 | 1.12x | +0.6% | 1304 | 1304 | 1 % | [+0.0%..+1.8%], 4 of 4 positive |  |
| fix/OrderMalformed.stream | generated | hand | 823.7 | 2872.0 | 3.49x | 2862.7 | 3.48x | -0.3% | 1864 | 1864 | 2 % | [-1.3%..+1.9%], 2 of 4 positive |  |
| fix/slope-0.text | generated | hand | 26.3 | 28.3 | 1.08x | 30.8 | 1.17x | +8.8% | 64 | 64 | 8 % | [+8.1%..+9.4%], 4 of 4 positive |  |
| fix/slope-1.text | generated | hand | 94.9 | 83.9 | 0.88x | 91.0 | 0.96x | +8.4% | 192 | 192 | 53 % | [-29.0%..+20.0%], 3 of 4 positive |  |
| fix/slope-2.text | generated | hand | 162.8 | 132.7 | 0.82x | 141.6 | 0.87x | +6.6% | 296 | 296 | 10 % | [+1.6%..+12.5%], 4 of 4 positive |  |
| fix/slope-4.text | generated | hand | 289.2 | 238.6 | 0.83x | 240.5 | 0.83x | +0.8% | 504 | 504 | 1 % | [-0.8%..+6.5%], 2 of 4 positive |  |
| fix/slope-8.text | generated | hand | 511.8 | 439.5 | 0.86x | 441.6 | 0.86x | +0.5% | 920 | 920 | 1 % | [-0.7%..+2.3%], 2 of 4 positive |  |
| fix/slope-16.text | generated | hand | 1008.1 | 815.2 | 0.81x | 826.8 | 0.82x | +1.4% | 1752 | 1752 | 5 % | [-0.8%..+2.6%], 3 of 4 positive |  |
| fix/slope-0.bytes | generated | hand | 31.4 | 35.7 | 1.14x | 38.1 | 1.21x | +6.6% | 120 | 120 | 6 % | [+5.9%..+7.1%], 4 of 4 positive |  |
| fix/slope-1.bytes | generated | hand | 98.0 | 97.6 | 1.00x | 99.6 | 1.02x | +2.0% | 248 | 248 | 4 % | [-2.6%..+2.4%], 3 of 4 positive |  |
| fix/slope-2.bytes | generated | hand | 167.8 | 153.7 | 0.92x | 153.9 | 0.92x | +0.1% | 352 | 352 | 4 % | [-0.2%..+1.5%], 3 of 4 positive |  |
| fix/slope-4.bytes | generated | hand | 292.5 | 273.2 | 0.93x | 273.1 | 0.93x | 0.0% | 560 | 560 | 4 % | [-0.2%..+0.7%], 1 of 4 positive |  |
| fix/slope-8.bytes | generated | hand | 518.5 | 498.8 | 0.96x | 497.4 | 0.96x | -0.3% | 976 | 976 | 2 % | [-0.8%..+0.3%], 3 of 4 positive |  |
| fix/slope-16.bytes | generated | hand | 996.1 | 926.0 | 0.93x | 926.0 | 0.93x | 0.0% | 1808 | 1808 | 1 % | [+0.0%..+0.2%], 2 of 4 positive |  |
| fix/slope-0.stream | generated | hand | 96.3 | 228.6 | 2.37x | 223.6 | 2.32x | -2.2% | 792 | 792 | 11 % | [-3.3%..-0.3%], 0 of 4 positive |  |
| fix/slope-1.stream | generated | hand | 173.3 | 536.0 | 3.09x | 533.5 | 3.08x | -0.5% | 888 | 888 | 3 % | [-3.4%..+1.5%], 1 of 4 positive |  |
| fix/slope-2.stream | generated | hand | 246.0 | 754.1 | 3.07x | 748.6 | 3.04x | -0.7% | 984 | 984 | 2 % | [-4.0%..+1.2%], 2 of 4 positive |  |
| fix/slope-4.stream | generated | hand | 375.6 | 1170.6 | 3.12x | 1161.0 | 3.09x | -0.8% | 1176 | 1176 | 1 % | [-4.6%..+1.9%], 2 of 4 positive |  |
| fix/slope-8.stream | generated | hand | 650.5 | 1997.7 | 3.07x | 1995.9 | 3.07x | -0.1% | 1560 | 1560 | 2 % | [-2.4%..+3.7%], 2 of 4 positive |  |
| fix/slope-16.stream | generated | hand | 1208.3 | 3605.6 | 2.98x | 3602.6 | 2.98x | -0.1% | 2328 | 2328 | 3 % | [-1.0%..+3.8%], 3 of 4 positive |  |
| fix/orders400.text | generated | hand | 251593.8 | 242539.5 | 0.96x | 239723.4 | 0.95x | -1.2% | 403288 | 403288 | 4 % | [-2.8%..+1.3%], 1 of 4 positive |  |
| fix/slope-1600.text | generated | hand | 80501.9 | 75323.4 | 0.94x | 75582.1 | 0.94x | +0.3% | 166488 | 166488 | 3 % | [-1.4%..+2.1%], 2 of 4 positive |  |
| web/json.array10000 | generated | hand | 172283.3 | 192036.1 | 1.11x | 196777.0 | 1.14x | +2.5% | 720048 | 720048 | 3 % | [-0.1%..+3.0%], 3 of 4 positive |  |
| web/json.object10000 | generated | hand | 764784.0 | 1182268.8 | 1.55x | 1319254.3 | 1.73x | +11.6% | 1599241 | 1599240 | 8 % | [-3.8%..+192.0%], 3 of 4 positive | +3.1% [+0.0%..+8.7%], 3 of 4 positive |
| web/url.path1000 | generated | hand | 9365.6 | 10900.0 | 1.16x | 10891.1 | 1.16x | -0.1% | 8384 | 8384 | 4 % | [-0.3%..+0.0%], 1 of 4 positive |  |
| web/media-type.params1000 | generated | control | 48850.8 | 47065.9 | 0.96x | 47157.0 | 0.97x | +0.2% | 151472 | 151472 | 8 % | [-0.9%..+6.0%], 2 of 4 positive |  |
| web/sf.list10000 | generated | control | 1178919.5 | 1187018.8 | 1.01x | 1185909.0 | 1.01x | -0.1% | 2720056 | 2720056 | 7 % | [-2.9%..+5.3%], 3 of 4 positive |  |
| fix/Order.span | generated | hand | 665.7 | 507.2 | 0.76x | 507.8 | 0.76x | +0.1% | 1008 | 1008 | 5 % | [-2.4%..+2.9%], 2 of 4 positive |  |
| feeds/streaming.1000 | generated | control | 201376.4 | 203761.3 | 1.01x | 202930.2 | 1.01x | -0.4% | 240760 | 240760 | 5 % | [-1.5%..+0.7%], 3 of 4 positive |  |
| fix/One.log-text | generated | hand | 82.7 | 174.9 | 2.11x | 180.6 | 2.18x | +3.3% | 192 | 192 | 5 % | [-0.7%..+9.9%], 3 of 4 positive |  |
| fix/One.log-bytes | generated | hand | 82.9 | 203.7 | 2.46x | 212.1 | 2.56x | +4.2% | 248 | 248 | 4 % | [-0.3%..+5.1%], 3 of 4 positive |  |
| fix/One.log-stream | generated | hand | 82.3 | 556.0 | 6.76x | 564.9 | 6.87x | +1.6% | 888 | 888 | 9 % | [-0.2%..+3.2%], 3 of 4 positive |  |
| fix/Order.log-text | generated | hand | 748.1 | 1463.1 | 1.96x | 1466.0 | 1.96x | +0.2% | 1096 | 1096 | 59 % | [-1.1%..+25.5%], 3 of 4 positive |  |
| fix/Order.log-bytes | generated | hand | 748.5 | 1637.5 | 2.19x | 1642.7 | 2.19x | +0.3% | 1152 | 1152 | 4 % | [-0.6%..+2.3%], 3 of 4 positive |  |
| fix/Order.log-stream | generated | hand | 752.4 | 2931.8 | 3.90x | 2961.5 | 3.94x | +1.0% | 1712 | 1712 | 6 % | [-0.4%..+2.2%], 2 of 4 positive |  |
| fix/Orders128.log-text | generated | hand | 91747.9 | 173564.7 | 1.89x | 172286.3 | 1.88x | -0.7% | 129112 | 129112 | 3 % | [-1.7%..+0.8%], 2 of 4 positive |  |
| fix/Orders128.log-bytes | generated | hand | 92373.1 | 192662.0 | 2.09x | 189381.4 | 2.05x | -1.7% | 129168 | 129168 | 3 % | [-2.0%..+0.1%], 1 of 4 positive |  |
| fix/Orders128.log-stream | generated | hand | 92706.4 | 320697.6 | 3.46x | 326537.4 | 3.52x | +1.8% | 118552 | 118552 | 3 % | [-0.4%..+4.1%], 3 of 4 positive |  |
| fix/OrderMalformed.log-text | generated | hand | 768.4 | 1458.3 | 1.90x | 1450.1 | 1.89x | -0.6% | 1256 | 1256 | 13 % | [-1.0%..+1.6%], 2 of 4 positive |  |
| fix/OrderMalformed.log-bytes | generated | hand | 771.8 | 1637.0 | 2.12x | 1653.9 | 2.14x | +1.0% | 1312 | 1312 | 3 % | [-0.6%..+2.0%], 3 of 4 positive |  |
| fix/OrderMalformed.log-stream | generated | hand | 768.4 | 3031.0 | 3.94x | 3044.8 | 3.96x | +0.5% | 1872 | 1872 | 4 % | [-1.0%..+1.8%], 3 of 4 positive |  |
| fix/slope-0.log-text | generated | hand | 23.3 | 66.4 | 2.85x | 69.2 | 2.97x | +4.1% | 88 | 88 | 8 % | [+2.6%..+5.2%], 4 of 4 positive |  |
| fix/slope-0.log-bytes | generated | hand | 23.5 | 73.6 | 3.13x | 77.3 | 3.29x | +5.1% | 144 | 144 | 8 % | [+3.9%..+17.6%], 4 of 4 positive |  |
| fix/slope-0.log-stream | generated | hand | 23.5 | 231.9 | 9.88x | 231.3 | 9.85x | -0.2% | 792 | 792 | 8 % | [-2.6%..+0.9%], 2 of 4 positive |  |
| fix/slope-4.log-text | generated | hand | 263.3 | 518.8 | 1.97x | 528.3 | 2.01x | +1.8% | 504 | 504 | 4 % | [-1.9%..+7.2%], 2 of 4 positive |  |
| fix/slope-4.log-bytes | generated | hand | 261.1 | 575.9 | 2.21x | 588.2 | 2.25x | +2.1% | 560 | 560 | 9 % | [+1.4%..+3.5%], 4 of 4 positive |  |
| fix/slope-4.log-stream | generated | hand | 263.9 | 1229.5 | 4.66x | 1246.7 | 4.72x | +1.4% | 1176 | 1176 | 6 % | [-0.9%..+2.9%], 3 of 4 positive |  |
| fix/slope-16.log-text | generated | hand | 965.8 | 1888.0 | 1.95x | 1891.2 | 1.96x | +0.2% | 1752 | 1752 | 7 % | [-0.4%..+4.2%], 2 of 4 positive |  |
| fix/slope-16.log-bytes | generated | hand | 981.9 | 2053.0 | 2.09x | 2073.1 | 2.11x | +1.0% | 1808 | 1808 | 7 % | [-0.1%..+1.7%], 3 of 4 positive |  |
| fix/slope-16.log-stream | generated | hand | 977.9 | 3652.0 | 3.73x | 3657.9 | 3.74x | +0.2% | 2328 | 2328 | 7 % | [-6.7%..+5.9%], 3 of 4 positive |  |
| fix/One.yield-reader | generated | hand | 200.9 | 557.6 | 2.78x | 559.0 | 2.78x | +0.3% | 856 | 856 | 26 % | [-9.4%..+1.5%], 2 of 4 positive |  |
| fix/One.yield-string | generated | hand | 79.8 | 239.6 | 3.00x | 243.0 | 3.05x | +1.4% | 272 | 272 | 85 % | [-2.7%..+6.5%], 2 of 4 positive |  |
| fix/One.yield-memory | generated | hand | 79.5 | 280.6 | 3.53x | 282.2 | 3.55x | +0.6% | 352 | 352 | 56 % | [-7.2%..+6.1%], 2 of 4 positive |  |
| fix/One.whole-stream | generated | hand | 155.2 | 222.1 | 1.43x | 232.1 | 1.49x | +4.5% | 448 | 448 | 76 % | [-4.0%..+7.2%], 3 of 4 positive |  |
| fix/One.whole-reader | generated | hand | 155.9 | 234.7 | 1.51x | 244.6 | 1.57x | +4.2% | 416 | 416 | 31 % | [+0.7%..+6.7%], 4 of 4 positive |  |
| fix/Order.yield-reader | generated | hand | 893.4 | 2910.6 | 3.26x | 2909.1 | 3.26x | -0.1% | 1680 | 1680 | 65 % | [-3.5%..+5.4%], 2 of 4 positive |  |
| fix/Order.yield-string | generated | hand | 688.3 | 2258.2 | 3.28x | 2221.1 | 3.23x | -1.6% | 1096 | 1096 | 95 % | [-5.9%..+4.3%], 2 of 4 positive |  |
| fix/Order.yield-memory | generated | hand | 688.1 | 2444.3 | 3.55x | 2497.3 | 3.63x | +2.2% | 1176 | 1176 | 89 % | [-3.0%..+3.1%], 2 of 4 positive |  |
| fix/Order.whole-stream | generated | hand | 824.5 | 919.1 | 1.11x | 930.6 | 1.13x | +1.3% | 1352 | 1352 | 3 % | [+0.4%..+10.5%], 4 of 4 positive |  |
| fix/Order.whole-reader | generated | hand | 828.3 | 948.3 | 1.14x | 949.2 | 1.15x | +0.1% | 1320 | 1320 | 4 % | [-3.4%..+7.1%], 1 of 4 positive |  |
| fix/BinaryMany.yield-reader | generated | hand | 5053.9 | 19109.6 | 3.78x | 19315.9 | 3.82x | +1.1% | 7416 | 7416 | 5 % | [-1.8%..+2.0%], 2 of 4 positive |  |
| fix/BinaryMany.yield-string | generated | hand | 3708.8 | 15767.4 | 4.25x | 15955.0 | 4.30x | +1.2% | 6832 | 6832 | 3 % | [-2.9%..+6.3%], 3 of 4 positive |  |
| fix/BinaryMany.yield-memory | generated | hand | 3718.6 | 18793.8 | 5.05x | 18414.3 | 4.95x | -2.0% | 6912 | 6912 | 3 % | [-3.8%..+3.9%], 1 of 4 positive |  |
| fix/BinaryMany.whole-stream | generated | hand | 4769.1 | 4842.3 | 1.02x | 4826.5 | 1.01x | -0.3% | 7512 | 7512 | 4 % | [-1.7%..+0.7%], 1 of 4 positive |  |
| fix/BinaryMany.whole-reader | generated | hand | 4742.4 | 5237.8 | 1.10x | 5275.5 | 1.11x | +0.7% | 7480 | 7480 | 4 % | [+0.7%..+12.1%], 4 of 4 positive |  |
| fix/Orders128.yield-reader | generated | hand | 139661.3 | 312735.9 | 2.24x | 314107.8 | 2.25x | +0.4% | 118520 | 118520 | 3 % | [-4.0%..+2.5%], 2 of 4 positive |  |
| fix/Orders128.yield-string | generated | hand | 83042.1 | 273889.6 | 3.30x | 273166.5 | 3.29x | -0.3% | 117936 | 117936 | 5 % | [-3.2%..+3.3%], 1 of 4 positive |  |
| fix/Orders128.yield-memory | generated | hand | 82410.9 | 298205.2 | 3.62x | 303139.3 | 3.68x | +1.7% | 118016 | 118016 | 1 % | [-3.2%..+5.5%], 2 of 4 positive |  |
| fix/Orders128.whole-stream | generated | hand | 119007.4 | 90186.8 | 0.76x | 90499.3 | 0.76x | +0.3% | 129368 | 129368 | 3 % | [-2.8%..+1.8%], 1 of 4 positive |  |
| fix/Orders128.whole-reader | generated | hand | 118994.9 | 92951.6 | 0.78x | 90949.6 | 0.76x | -2.2% | 129336 | 129336 | 4 % | [-3.5%..+0.3%], 1 of 4 positive |  |
| fix/slope-0.yield-reader | generated | hand | 137.7 | 249.2 | 1.81x | 245.1 | 1.78x | -1.7% | 760 | 760 | 18 % | [-2.7%..+3.8%], 2 of 4 positive |  |
| fix/slope-0.yield-string | generated | hand | 23.3 | 52.9 | 2.28x | 53.4 | 2.29x | +0.8% | 176 | 176 | 11 % | [-0.1%..+1.6%], 3 of 4 positive |  |
| fix/slope-0.yield-memory | generated | hand | 23.2 | 65.2 | 2.82x | 66.4 | 2.87x | +1.8% | 256 | 256 | 10 % | [+0.9%..+2.6%], 4 of 4 positive |  |
| fix/slope-0.whole-stream | generated | hand | 89.6 | 146.0 | 1.63x | 144.6 | 1.61x | -1.0% | 320 | 320 | 10 % | [-1.6%..+1.7%], 2 of 4 positive |  |
| fix/slope-0.whole-reader | generated | hand | 89.6 | 155.7 | 1.74x | 156.3 | 1.74x | +0.3% | 288 | 288 | 7 % | [-1.2%..+1.2%], 2 of 4 positive |  |
| fix/slope-16.yield-reader | generated | hand | 1183.6 | 3706.5 | 3.13x | 3646.8 | 3.08x | -1.6% | 2296 | 2296 | 2 % | [-4.9%..+1.8%], 1 of 4 positive |  |
| fix/slope-16.yield-string | generated | hand | 884.2 | 2954.3 | 3.34x | 2974.9 | 3.36x | +0.7% | 1712 | 1712 | 93 % | [-3.6%..+3.4%], 2 of 4 positive |  |
| fix/slope-16.yield-memory | generated | hand | 885.0 | 3269.0 | 3.69x | 3348.2 | 3.78x | +2.4% | 1792 | 1792 | 92 % | [-0.8%..+8.6%], 3 of 4 positive |  |
| fix/slope-16.whole-stream | generated | hand | 1084.7 | 1080.2 | 1.00x | 1087.7 | 1.00x | +0.7% | 2008 | 2008 | 96 % | [-0.1%..+2.8%], 3 of 4 positive |  |
| fix/slope-16.whole-reader | generated | hand | 1080.0 | 1116.3 | 1.03x | 1129.9 | 1.05x | +1.2% | 1976 | 1976 | 95 % | [-1.4%..+4.7%], 3 of 4 positive |  |
| feeds/stock-count.small.text | generated | hand | 170.3 | 226.4 | 1.33x | 230.6 | 1.35x | +1.8% | 608 | 608 | 4 % | [-0.1%..+2.5%], 3 of 4 positive |  |
| feeds/stock-count.small.reader | generated | hand | 358.5 | 416.2 | 1.16x | 417.2 | 1.16x | +0.2% | 784 | 784 | 5 % | [-1.6%..+4.2%], 2 of 4 positive |  |
| feeds/stock-count.small.reader64 | generated | hand | 332.5 | 415.2 | 1.25x | 422.3 | 1.27x | +1.7% | 832 | 832 | 10 % | [-2.7%..+3.5%], 2 of 4 positive |  |
| feeds/stock-count.good.text | generated | hand | 37059.1 | 28760.4 | 0.78x | 29060.1 | 0.78x | +1.0% | 111312 | 111403 | 5 % | [-3.5%..+1.4%], 3 of 4 positive | -0.8% [-1.0%..+0.0%], 1 of 4 positive |
| feeds/stock-count.good.reader | generated | hand | 70913.5 | 41405.4 | 0.58x | 41156.2 | 0.58x | -0.6% | 111488 | 111488 | 68 % | [-1.1%..+0.8%], 1 of 4 positive |  |
| feeds/stock-count.good.reader64 | generated | hand | 86971.3 | 46353.8 | 0.53x | 45718.8 | 0.53x | -1.4% | 111536 | 111536 | 70 % | [-2.7%..-0.1%], 0 of 4 positive |  |
| feeds/stock-count.broken.text | generated | hand | 36939.1 | 33066.9 | 0.90x | 32947.0 | 0.89x | -0.4% | 107408 | 107499 | 39 % | [-2.0%..+10.7%], 2 of 4 positive |  |
| feeds/stock-count.broken.reader | generated | hand | 68335.2 | 42507.0 | 0.62x | 42550.4 | 0.62x | +0.1% | 107584 | 107630 | 30 % | [-2.6%..+0.7%], 1 of 4 positive |  |
| feeds/stock-count.broken.reader64 | generated | hand | 83568.9 | 47051.5 | 0.56x | 47399.6 | 0.57x | +0.7% | 107632 | 107632 | 48 % | [-0.8%..+9.2%], 2 of 4 positive |  |
| feeds/recovering.good | generated | control | 66306.3 | 67002.4 | 1.01x | 65641.7 | 0.99x | -2.0% | 240352 | 240352 | 73 % | [-2.3%..+1.0%], 2 of 4 positive |  |
| feeds/recovering.broken | generated | control | 70972.8 | 73730.8 | 1.04x | 71049.3 | 1.00x | -3.6% | 241920 | 241920 | 53 % | [-3.8%..+5.2%], 1 of 4 positive |  |
| web/url.plain | generated | hand | 97.0 | 225.2 | 2.32x | 223.6 | 2.30x | -0.7% | 360 | 360 | 83 % | [-4.3%..+1.4%], 3 of 4 positive |  |
| web/url.full | generated | hand | 171.3 | 298.0 | 1.74x | 288.7 | 1.69x | -3.1% | 536 | 536 | 33 % | [-14.5%..-0.4%], 0 of 4 positive | -0.8% [-3.5%..+4.1%], 2 of 4 positive |
| web/url.ipv4 | generated | hand | 111.3 | 240.0 | 2.16x | 230.3 | 2.07x | -4.1% | 384 | 384 | 22 % | [-5.4%..-1.5%], 0 of 4 positive |  |
| web/url.long-path | generated | hand | 175.7 | 419.6 | 2.39x | 415.3 | 2.36x | -1.0% | 512 | 512 | 64 % | [-1.5%..+28.0%], 2 of 4 positive |  |
| web/url.refused | generated | hand | 68.2 | 260.6 | 3.82x | 252.8 | 3.71x | -3.0% | 64 | 64 | 5 % | [-6.4%..+0.6%], 1 of 4 positive |  |
| web/url.relative | generated | hand | 75.3 | 182.2 | 2.42x | 184.3 | 2.45x | +1.1% | 312 | 312 | 6 % | [-1.1%..+6.8%], 3 of 4 positive |  |
| web/url.relative-dot-colon | generated | hand | 62.6 | 160.4 | 2.56x | 158.9 | 2.54x | -0.9% | 280 | 280 | 4 % | [-1.8%..+1.9%], 2 of 4 positive |  |
| web/url.relative-letters | generated | hand | 108.4 | 211.1 | 1.95x | 212.4 | 1.96x | +0.6% | 312 | 312 | 10 % | [-0.7%..+5.6%], 2 of 4 positive |  |
| web/json.object | generated | hand | 587.4 | 928.8 | 1.58x | 946.6 | 1.61x | +1.9% | 2584 | 2584 | 6 % | [+0.0%..+4.0%], 3 of 4 positive | -0.5% [-1.7%..+2.6%], 2 of 4 positive |
| web/json.array | generated | hand | 588.5 | 736.7 | 1.25x | 749.1 | 1.27x | +1.7% | 2400 | 2400 | 5 % | [-0.6%..+3.0%], 3 of 4 positive |  |
| web/cookie.full | generated | control | 305.9 | 343.7 | 1.12x | 344.5 | 1.13x | +0.2% | 2136 | 2136 | 97 % | [-2.0%..+1.1%], 2 of 4 positive |  |
| web/cookie.short | generated | control | 52.7 | 82.0 | 1.56x | 81.9 | 1.55x | -0.1% | 336 | 336 | 91 % | [-0.5%..+0.4%], 3 of 4 positive | +0.3% [+0.2%..+0.6%], 4 of 4 positive |
| web/pointer.full | generated | control | 205.1 | 204.2 | 1.00x | 204.2 | 1.00x | 0.0% | 440 | 440 | 88 % | [-1.0%..+1.7%], 2 of 4 positive |  |
| web/pointer.short | generated | control | 37.0 | 66.8 | 1.81x | 66.0 | 1.78x | -1.2% | 144 | 144 | 84 % | [-1.5%..+1.6%], 2 of 4 positive |  |
| web/media-type.plain | generated | control | 165.6 | 202.5 | 1.22x | 205.8 | 1.24x | +1.6% | 448 | 448 | 95 % | [-3.3%..+4.0%], 3 of 4 positive | +0.3% [-1.8%..+10.7%], 2 of 4 positive |
| web/media-type.quoted | generated | control | 255.4 | 294.2 | 1.15x | 304.5 | 1.19x | +3.5% | 816 | 816 | 106 % | [+1.4%..+6.0%], 4 of 4 positive |  |
| web/media-type.refused | generated | control | 62.4 | 95.8 | 1.54x | 95.9 | 1.54x | 0.0% | 64 | 64 | 46 % | [-0.3%..+12.1%], 3 of 4 positive |  |
| web/addr-spec.plain | generated | control | 89.3 | 118.9 | 1.33x | 119.4 | 1.34x | +0.4% | 176 | 176 | 1 % | [-1.1%..+2.1%], 3 of 4 positive |  |
| web/addr-spec.refused | generated | control | 44.7 | 79.6 | 1.78x | 79.3 | 1.77x | -0.4% | 64 | 64 | 3 % | [-2.5%..+5.9%], 2 of 4 positive |  |
| web/date-time.utc | generated | control | 50.7 | 69.7 | 1.38x | 71.2 | 1.40x | +2.1% | 176 | 176 | 3 % | [+0.5%..+11.2%], 4 of 4 positive |  |
| web/date-time.offset | generated | control | 77.9 | 97.8 | 1.26x | 99.4 | 1.28x | +1.7% | 208 | 208 | 2 % | [+0.4%..+7.7%], 4 of 4 positive |  |
| web/language-tag.plain | generated | control | 116.9 | 160.1 | 1.37x | 159.7 | 1.37x | -0.2% | 360 | 360 | 9 % | [-6.1%..+7.7%], 2 of 4 positive |  |
| web/language-tag.full | generated | control | 435.4 | 484.3 | 1.11x | 488.0 | 1.12x | +0.8% | 720 | 720 | 9 % | [-1.5%..+2.7%], 3 of 4 positive |  |
| web/language-tag.refused | generated | control | 111.9 | 155.4 | 1.39x | 151.2 | 1.35x | -2.7% | 64 | 64 | 7 % | [-8.2%..+0.1%], 1 of 4 positive |  |
| web/date-time.refused | generated | control | 36.2 | 55.0 | 1.52x | 56.5 | 1.56x | +2.6% | 96 | 96 | 4 % | [-0.3%..+51.5%], 2 of 4 positive |  |
| web/content-disposition.full | generated | control | 435.3 | 480.0 | 1.10x | 474.5 | 1.09x | -1.2% | 1680 | 1680 | 6 % | [-6.4%..+1.7%], 1 of 4 positive |  |
| web/content-disposition.refused | generated | control | 27.4 | 60.5 | 2.21x | 60.8 | 2.22x | +0.4% | 64 | 64 | 3 % | [-0.5%..+1.4%], 3 of 4 positive |  |
| web/uri-template.full | generated | control | 767.5 | 792.8 | 1.03x | 805.5 | 1.05x | +1.6% | 1088 | 1088 | 83 % | [+0.7%..+4.0%], 4 of 4 positive |  |
| web/uri-template.refused | generated | control | 3598378825.0 | 3542100100.0 | 0.98x | 3571388900.0 | 0.99x | +0.8% | 64 | 64 | 2 % | [-1.2%..+2.2%], 2 of 4 positive |  |
| web/json-patch.full | generated | control | 1327.5 | 1398.1 | 1.05x | 1396.6 | 1.05x | -0.1% | 4552 | 4552 | 2 % | [-0.6%..+3.2%], 2 of 4 positive |  |
| web/forwarded.full | generated | control | 1594.7 | 1623.4 | 1.02x | 1613.6 | 1.01x | -0.6% | 2648 | 2648 | 4 % | [-2.7%..+1.6%], 1 of 4 positive |  |
| web/forwarded.refused | generated | control | 112.0 | 140.3 | 1.25x | 141.1 | 1.26x | +0.5% | 328 | 328 | 5 % | [-7.3%..+7.5%], 3 of 4 positive |  |
| web/link.full | generated | control | 1333.1 | 1397.3 | 1.05x | 1407.8 | 1.06x | +0.7% | 2968 | 2968 | 8 % | [-1.5%..+1.5%], 2 of 4 positive |  |
| web/link.refused | generated | control | 36.7 | 66.0 | 1.80x | 65.7 | 1.79x | -0.4% | 64 | 64 | 9 % | [-1.2%..+3.6%], 1 of 4 positive |  |
| web/cookie.refused | generated | control | 27.1 | 53.8 | 1.99x | 53.9 | 1.99x | +0.3% | 136 | 136 | 2 % | [-0.3%..+0.6%], 2 of 4 positive |  |
| web/sf.item | generated | control | 266.0 | 300.3 | 1.13x | 301.3 | 1.13x | +0.3% | 800 | 800 | 9 % | [-2.9%..+2.6%], 2 of 4 positive |  |
| web/sf.list | generated | control | 1192.5 | 1254.3 | 1.05x | 1247.0 | 1.05x | -0.6% | 3064 | 3064 | 2 % | [-0.6%..+2.1%], 1 of 4 positive |  |
| web/sf.dictionary | generated | control | 1212.4 | 1257.5 | 1.04x | 1252.8 | 1.03x | -0.4% | 3280 | 3280 | 5 % | [-3.4%..+3.9%], 2 of 4 positive |  |
