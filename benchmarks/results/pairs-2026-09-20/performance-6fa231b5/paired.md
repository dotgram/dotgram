Median of 5 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.4, 31.3, 31.3, 31.4, 31.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 07:26

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| fix/One.text | generated | hand | 89.9 | 97.1 | 1.08x | 91.5 | 1.02x | -5.7% | 192 | 192 | 3 % | [-11.4%..+13.4%], 1 of 5 positive | +1.0% [-8.6%..+3.9%], 4 of 5 positive |
| fix/One.bytes | generated | hand | 87.2 | 110.4 | 1.27x | 111.6 | 1.28x | +1.1% | 248 | 248 | 11 % | [-1.6%..+2.9%], 3 of 5 positive |  |
| fix/One.stream | generated | hand | 301.6 | 368.0 | 1.22x | 374.0 | 1.24x | +1.6% | 888 | 888 | 3 % | [-1.9%..+3.8%], 4 of 5 positive |  |
| fix/Order.text | generated | hand | 680.4 | 658.5 | 0.97x | 671.4 | 0.99x | +2.0% | 1096 | 1096 | 7 % | [-2.7%..+5.3%], 3 of 5 positive | +0.8% [-3.6%..+2.5%], 3 of 5 positive |
| fix/Order.bytes | generated | hand | 661.1 | 744.3 | 1.13x | 753.4 | 1.14x | +1.2% | 1152 | 1152 | 7 % | [-0.9%..+1.6%], 4 of 5 positive |  |
| fix/Order.stream | generated | hand | 803.2 | 1087.2 | 1.35x | 1083.4 | 1.35x | -0.3% | 1712 | 1712 | 3 % | [-1.2%..+0.8%], 2 of 5 positive |  |
| fix/BinaryMany.text | generated | hand | 3641.5 | 3827.0 | 1.05x | 3828.6 | 1.05x | 0.0% | 7256 | 7256 | 1 % | [-2.7%..+0.9%], 2 of 5 positive |  |
| fix/BinaryMany.bytes | generated | hand | 3734.4 | 4695.3 | 1.26x | 4629.3 | 1.24x | -1.4% | 7312 | 7312 | 1 % | [-2.7%..+0.1%], 1 of 5 positive |  |
| fix/BinaryMany.stream | generated | hand | 4745.0 | 5286.6 | 1.11x | 5267.4 | 1.11x | -0.4% | 7448 | 7448 | 1 % | [-3.7%..+0.8%], 2 of 5 positive |  |
| fix/Orders128.text | generated | hand | 81668.5 | 76387.4 | 0.94x | 77143.7 | 0.94x | +1.0% | 129112 | 129112 | 6 % | [-5.9%..+6.9%], 2 of 5 positive |  |
| fix/Orders128.bytes | generated | hand | 79432.2 | 87660.9 | 1.10x | 86954.7 | 1.09x | -0.8% | 129168 | 129168 | 9 % | [-3.3%..+1.9%], 1 of 5 positive |  |
| fix/Orders128.stream | generated | hand | 115928.2 | 96335.1 | 0.83x | 96129.4 | 0.83x | -0.2% | 118552 | 118552 | 2 % | [-0.8%..+0.7%], 2 of 5 positive |  |
| fix/OrderMalformed.text | generated | hand | 693.0 | 660.3 | 0.95x | 676.2 | 0.98x | +2.4% | 1248 | 1248 | 9 % | [-3.2%..+4.2%], 3 of 5 positive |  |
| fix/OrderMalformed.bytes | generated | hand | 674.8 | 752.6 | 1.12x | 762.1 | 1.13x | +1.3% | 1304 | 1304 | 9 % | [+0.9%..+2.2%], 5 of 5 positive |  |
| fix/OrderMalformed.stream | generated | hand | 816.1 | 1098.3 | 1.35x | 1097.6 | 1.34x | -0.1% | 1864 | 1864 | 2 % | [-1.2%..+1.2%], 2 of 5 positive |  |
| fix/slope-0.text | generated | hand | 26.2 | 28.3 | 1.08x | 30.8 | 1.18x | +8.7% | 64 | 64 | 2 % | [-8.3%..+10.2%], 4 of 5 positive |  |
| fix/slope-1.text | generated | hand | 93.0 | 82.9 | 0.89x | 85.6 | 0.92x | +3.2% | 192 | 192 | 10 % | [-0.2%..+4.0%], 4 of 5 positive |  |
| fix/slope-2.text | generated | hand | 160.4 | 132.6 | 0.83x | 135.6 | 0.85x | +2.3% | 296 | 296 | 10 % | [-1.5%..+5.2%], 3 of 5 positive |  |
| fix/slope-4.text | generated | hand | 286.3 | 238.3 | 0.83x | 240.9 | 0.84x | +1.1% | 504 | 504 | 11 % | [-1.6%..+5.0%], 3 of 5 positive |  |
| fix/slope-8.text | generated | hand | 510.2 | 438.7 | 0.86x | 442.0 | 0.87x | +0.8% | 920 | 920 | 13 % | [-2.7%..+3.2%], 3 of 5 positive |  |
| fix/slope-16.text | generated | hand | 997.0 | 814.0 | 0.82x | 826.4 | 0.83x | +1.5% | 1752 | 1752 | 14 % | [-2.6%..+3.3%], 3 of 5 positive |  |
| fix/slope-0.bytes | generated | hand | 31.4 | 35.6 | 1.13x | 37.7 | 1.20x | +6.0% | 120 | 120 | 1 % | [-6.9%..+8.0%], 4 of 5 positive |  |
| fix/slope-1.bytes | generated | hand | 97.6 | 100.0 | 1.02x | 100.0 | 1.02x | 0.0% | 248 | 248 | 9 % | [-3.3%..+4.1%], 3 of 5 positive |  |
| fix/slope-2.bytes | generated | hand | 164.9 | 155.0 | 0.94x | 153.9 | 0.93x | -0.7% | 352 | 352 | 9 % | [-1.6%..+1.9%], 2 of 5 positive |  |
| fix/slope-4.bytes | generated | hand | 289.4 | 270.7 | 0.94x | 273.8 | 0.95x | +1.2% | 560 | 560 | 10 % | [-1.5%..+1.4%], 4 of 5 positive |  |
| fix/slope-8.bytes | generated | hand | 512.7 | 499.0 | 0.97x | 499.3 | 0.97x | +0.1% | 976 | 976 | 14 % | [-0.7%..+1.2%], 3 of 5 positive |  |
| fix/slope-16.bytes | generated | hand | 999.2 | 935.4 | 0.94x | 929.6 | 0.93x | -0.6% | 1808 | 1808 | 15 % | [-0.9%..+0.4%], 1 of 5 positive |  |
| fix/slope-0.stream | generated | hand | 102.5 | 228.9 | 2.23x | 229.5 | 2.24x | +0.3% | 792 | 792 | 11 % | [-1.7%..+2.8%], 4 of 5 positive |  |
| fix/slope-1.stream | generated | hand | 180.2 | 335.9 | 1.86x | 342.4 | 1.90x | +1.9% | 888 | 888 | 5 % | [-2.3%..+4.0%], 4 of 5 positive |  |
| fix/slope-2.stream | generated | hand | 254.7 | 402.2 | 1.58x | 410.0 | 1.61x | +1.9% | 984 | 984 | 2 % | [-1.7%..+5.4%], 4 of 5 positive |  |
| fix/slope-4.stream | generated | hand | 384.1 | 531.8 | 1.38x | 543.5 | 1.41x | +2.2% | 1176 | 1176 | 1 % | [-3.0%..+5.1%], 4 of 5 positive |  |
| fix/slope-8.stream | generated | hand | 652.8 | 772.4 | 1.18x | 782.1 | 1.20x | +1.3% | 1560 | 1560 | 2 % | [-2.4%..+3.1%], 3 of 5 positive |  |
| fix/slope-16.stream | generated | hand | 1210.1 | 1252.3 | 1.03x | 1260.7 | 1.04x | +0.7% | 2328 | 2328 | 2 % | [-0.9%..+1.8%], 4 of 5 positive |  |
| fixmsg/Order.parse | generated | control | 1784.7 | 1831.3 | 1.03x | 1839.8 | 1.03x | +0.5% | 3400 | 3400 | 4 % | [-4.1%..+3.5%], 3 of 5 positive |  |
| fixmsg/Order.build | generated | control | 2094.2 | 2287.9 | 1.09x | 2324.6 | 1.11x | +1.6% | 3592 | 3592 | 2 % | [-2.7%..+2.8%], 3 of 5 positive |  |
| fix/orders400.text | generated | hand | 255383.8 | 241826.6 | 0.95x | 241292.0 | 0.94x | -0.2% | 403288 | 403288 | 7 % | [-5.8%..+7.0%], 2 of 5 positive |  |
| fix/slope-1600.text | generated | hand | 81027.7 | 75241.4 | 0.93x | 76849.5 | 0.95x | +2.1% | 166488 | 166488 | 20 % | [-3.9%..+2.9%], 4 of 5 positive |  |
| tsql/script100.bool | generated | scriptdom | 659618.8 | 125715.6 | 0.19x | 122021.9 | 0.18x | -2.9% | 113600 | 105600 | 4 % | [-7.1%..-0.1%], 0 of 5 positive |  |
| tsql/script100.boolboth | generated | scriptdom | 659168.0 | 123887.5 | 0.19x | 123000.8 | 0.19x | -0.7% | 105600 | 105600 | 4 % | [-6.2%..+1.6%], 1 of 5 positive |  |
| tsql/script100 | generated | scriptdom | 657141.4 | 125615.6 | 0.19x | 124443.8 | 0.19x | -0.9% | 113600 | 113600 | 5 % | [-6.8%..+0.4%], 1 of 5 positive |  |
| tsql/script400.bool | generated | scriptdom | 2650396.9 | 503025.0 | 0.19x | 492475.0 | 0.19x | -2.1% | 454400 | 422400 | 4 % | [-6.9%..-0.3%], 0 of 5 positive |  |
| tsql/script400.boolboth | generated | scriptdom | 2640006.2 | 499609.4 | 0.19x | 491840.6 | 0.19x | -1.6% | 422403 | 422403 | 5 % | [-6.9%..+1.3%], 1 of 5 positive |  |
| tsql/script400 | generated | scriptdom | 2657128.1 | 502425.0 | 0.19x | 496790.6 | 0.19x | -1.1% | 454400 | 454400 | 5 % | [-5.7%..+0.4%], 2 of 5 positive |  |
| tsql/columns1000 | generated | scriptdom | 1740503.1 | 188126.6 | 0.11x | 187596.9 | 0.11x | -0.3% | 200464 | 200464 | 6 % | [-0.9%..+2.3%], 2 of 5 positive |  |
| tsql/conditions1000 | generated | scriptdom | 897347.7 | 333028.1 | 0.37x | 333915.6 | 0.37x | +0.3% | 424424 | 424424 | 4 % | [-1.5%..+0.4%], 3 of 5 positive |  |
| tsql/rows1000 | generated | scriptdom | 585859.4 | 242365.6 | 0.41x | 243012.5 | 0.41x | +0.3% | 296472 | 296472 | 1 % | [-0.7%..+1.3%], 3 of 5 positive |  |
| web/json.array10000 | generated | hand | 170421.9 | 190217.8 | 1.12x | 190347.7 | 1.12x | +0.1% | 720048 | 720048 | 5 % | [-1.4%..+5.9%], 4 of 5 positive |  |
| web/json.object10000 | generated | hand | 737918.0 | 1248932.8 | 1.69x | 1269629.7 | 1.72x | +1.7% | 1599240 | 1599240 | 24 % | [-9.8%..+32.1%], 3 of 5 positive | +1.0% [-8.5%..+4.6%], 2 of 5 positive |
| web/url.path1000 | generated | hand | 9297.1 | 10817.6 | 1.16x | 10818.6 | 1.16x | 0.0% | 8384 | 8384 | 3 % | [-0.8%..+0.8%], 3 of 5 positive |  |
| web/media-type.params1000 | generated | control | 47892.5 | 47269.1 | 0.99x | 47207.8 | 0.99x | -0.1% | 151472 | 151472 | 3 % | [-7.7%..+2.3%], 3 of 5 positive |  |
| web/sf.list10000 | generated | control | 1174220.3 | 1220629.7 | 1.04x | 1200314.8 | 1.02x | -1.7% | 2720056 | 2720056 | 6 % | [-4.1%..+5.4%], 2 of 5 positive |  |
| sql/select20.at | generated | hand | 7180.4 | 17330.6 | 2.41x | 17071.4 | 2.38x | -1.5% | 21416 | 21416 | 2 % | [-2.0%..+0.6%], 1 of 5 positive | -1.1% [-4.4%..+0.6%], 1 of 5 positive |
| sql/select20.window | generated | hand | 7154.0 | 17825.5 | 2.49x | 17605.4 | 2.46x | -1.2% | 21416 | 21416 | 3 % | [-1.2%..+1.2%], 1 of 5 positive | -0.7% [-3.5%..+0.5%], 2 of 5 positive |
| tsql/insert-values.at | generated | scriptdom | 8227.6 | 1210.7 | 0.15x | 1190.1 | 0.14x | -1.7% | 1328 | 1328 | 1 % | [-5.1%..+1.5%], 2 of 5 positive |  |
| sql/select20.scan | generated | control | 374.6 | 381.8 | 1.02x | 382.0 | 1.02x | +0.1% | 0 | 0 | 1 % | [-0.5%..+0.6%], 4 of 5 positive | -0.5% [-4.6%..+1.3%], 2 of 5 positive |
| sql/conditions100.scan | generated | control | 3489.0 | 3500.1 | 1.00x | 3516.8 | 1.01x | +0.5% | 0 | 0 | 1 % | [-0.3%..+1.3%], 4 of 5 positive |  |
| tsql/select20.scan | generated | control | 375.6 | 379.4 | 1.01x | 378.8 | 1.01x | -0.1% | 0 | 0 | 2 % | [-1.8%..+1.9%], 3 of 5 positive | -0.5% [-2.5%..+1.2%], 2 of 5 positive |
| el/ladder.scan | generated | control | 137.4 | 137.4 | 1.00x | 138.1 | 1.01x | +0.5% | 0 | 0 | 3 % | [-0.3%..+1.8%], 3 of 5 positive | -0.1% [-0.8%..+1.0%], 2 of 5 positive |
| fixmsg/Order.parse-stream | generated | control | 1778.7 | 2173.8 | 1.22x | 2162.3 | 1.22x | -0.5% | 8224 | 8224 | 3 % | [-1.9%..+0.3%], 1 of 5 positive |  |
| fixmsg/Order.parse-reader | generated | control | 1800.5 | 2124.9 | 1.18x | 2167.5 | 1.20x | +2.0% | 12112 | 12112 | 5 % | [-3.4%..+6.5%], 4 of 5 positive |  |
| fixmsg/Order.parse-span | generated | control | 1767.4 | 1833.1 | 1.04x | 1864.7 | 1.06x | +1.7% | 3672 | 3672 | 4 % | [-3.7%..+3.2%], 3 of 5 positive |  |
| fixmsg/Order.read-stream100 | generated | control | 186229.7 | 203584.4 | 1.09x | 199796.7 | 1.07x | -1.9% | 389248 | 389248 | 8 % | [-2.7%..-0.5%], 0 of 5 positive |  |
| fixmsg/Order.read-reader100 | generated | control | 185697.1 | 190416.8 | 1.03x | 193944.1 | 1.04x | +1.9% | 375712 | 375712 | 4 % | [-4.9%..+3.1%], 3 of 5 positive |  |
| fix/Order.span | generated | hand | 670.9 | 504.6 | 0.75x | 514.5 | 0.77x | +2.0% | 1008 | 1008 | 6 % | [-3.9%..+5.3%], 3 of 5 positive |  |
| feeds/streaming.1000 | generated | control | 206579.7 | 203204.1 | 0.98x | 208697.1 | 1.01x | +2.7% | 240760 | 240760 | 3 % | [-1.2%..+5.2%], 4 of 5 positive |  |
| fix/One.log-text | generated | hand | 82.3 | 85.7 | 1.04x | 87.0 | 1.06x | +1.6% | 192 | 192 | 10 % | [-1.9%..+3.5%], 4 of 5 positive |  |
| fix/One.log-bytes | generated | hand | 82.4 | 109.9 | 1.33x | 111.0 | 1.35x | +1.0% | 248 | 248 | 10 % | [-3.2%..+3.0%], 4 of 5 positive |  |
| fix/One.log-stream | generated | hand | 82.2 | 347.5 | 4.23x | 348.6 | 4.24x | +0.3% | 888 | 888 | 11 % | [-3.4%..+2.7%], 3 of 5 positive |  |
| fix/Order.log-text | generated | hand | 757.3 | 674.2 | 0.89x | 688.9 | 0.91x | +2.2% | 1096 | 1096 | 6 % | [-0.1%..+5.0%], 4 of 5 positive |  |
| fix/Order.log-bytes | generated | hand | 749.9 | 856.6 | 1.14x | 856.2 | 1.14x | 0.0% | 1152 | 1152 | 6 % | [-2.3%..+3.4%], 2 of 5 positive |  |
| fix/Order.log-stream | generated | hand | 751.3 | 1199.9 | 1.60x | 1187.1 | 1.58x | -1.1% | 1712 | 1712 | 6 % | [-2.1%..+9.6%], 2 of 5 positive |  |
| fix/Orders128.log-text | generated | hand | 92109.2 | 78888.6 | 0.86x | 78763.2 | 0.86x | -0.2% | 129112 | 129112 | 7 % | [-3.6%..+2.8%], 3 of 5 positive |  |
| fix/Orders128.log-bytes | generated | hand | 92339.4 | 101337.5 | 1.10x | 100194.5 | 1.09x | -1.1% | 129168 | 129168 | 6 % | [-2.6%..+2.4%], 1 of 5 positive |  |
| fix/Orders128.log-stream | generated | hand | 92233.1 | 108393.5 | 1.18x | 108967.3 | 1.18x | +0.5% | 118552 | 118552 | 6 % | [-0.3%..+10.6%], 4 of 5 positive |  |
| fix/OrderMalformed.log-text | generated | hand | 761.1 | 678.9 | 0.89x | 691.7 | 0.91x | +1.9% | 1256 | 1256 | 7 % | [-1.3%..+4.6%], 4 of 5 positive |  |
| fix/OrderMalformed.log-bytes | generated | hand | 765.4 | 858.6 | 1.12x | 854.7 | 1.12x | -0.4% | 1312 | 1312 | 7 % | [-1.3%..+2.6%], 2 of 5 positive |  |
| fix/OrderMalformed.log-stream | generated | hand | 766.5 | 1195.6 | 1.56x | 1209.8 | 1.58x | +1.2% | 1872 | 1872 | 6 % | [-1.3%..+9.2%], 4 of 5 positive |  |
| fix/slope-0.log-text | generated | hand | 23.2 | 28.2 | 1.22x | 30.7 | 1.33x | +9.1% | 64 | 64 | 1 % | [-7.7%..+10.6%], 4 of 5 positive |  |
| fix/slope-0.log-bytes | generated | hand | 23.2 | 35.4 | 1.53x | 38.2 | 1.65x | +7.9% | 120 | 120 | 2 % | [-6.9%..+8.8%], 4 of 5 positive |  |
| fix/slope-0.log-stream | generated | hand | 23.2 | 228.5 | 9.86x | 230.9 | 9.97x | +1.1% | 792 | 792 | 1 % | [-5.8%..+1.6%], 3 of 5 positive |  |
| fix/slope-4.log-text | generated | hand | 260.2 | 245.5 | 0.94x | 247.1 | 0.95x | +0.7% | 504 | 504 | 13 % | [-1.5%..+1.7%], 3 of 5 positive |  |
| fix/slope-4.log-bytes | generated | hand | 259.2 | 310.0 | 1.20x | 309.5 | 1.19x | -0.2% | 560 | 560 | 14 % | [-2.0%..+0.7%], 1 of 5 positive |  |
| fix/slope-4.log-stream | generated | hand | 260.5 | 578.0 | 2.22x | 568.4 | 2.18x | -1.7% | 1176 | 1176 | 13 % | [-2.4%..+6.0%], 2 of 5 positive |  |
| fix/slope-16.log-text | generated | hand | 960.1 | 835.4 | 0.87x | 844.9 | 0.88x | +1.1% | 1752 | 1752 | 15 % | [-5.0%..+2.3%], 3 of 5 positive |  |
| fix/slope-16.log-bytes | generated | hand | 956.3 | 1078.3 | 1.13x | 1078.3 | 1.13x | 0.0% | 1808 | 1808 | 15 % | [-1.5%..+0.2%], 2 of 5 positive |  |
| fix/slope-16.log-stream | generated | hand | 958.9 | 1393.5 | 1.45x | 1382.6 | 1.44x | -0.8% | 2328 | 2328 | 14 % | [-1.0%..+9.6%], 1 of 5 positive |  |
| el/refused-early.bool | generated | hand | 342.3 | 1084.2 | 3.17x | 559.4 | 1.63x | -48.4% | 1064 | 624 | 10 % | [-48.9%..-46.3%], 0 of 5 positive |  |
| el/refused-late.bool | generated | hand | 1191.5 | 1740.7 | 1.46x | 905.1 | 0.76x | -48.0% | 1064 | 624 | 4 % | [-48.5%..-46.2%], 0 of 5 positive |  |
| el/ladder.bool | generated | hand | 998.0 | 1742.9 | 1.75x | 1725.4 | 1.73x | -1.0% | 1776 | 1720 | 4 % | [-2.6%..+1.2%], 2 of 5 positive | -1.5% [-4.8%..+1.5%], 2 of 5 positive |
| sql/refused-late.bool | generated | hand | 2660.4 | 11914.2 | 4.48x | 5765.3 | 2.17x | -51.6% | 13552 | 6616 | 2 % | [-53.1%..-50.7%], 0 of 5 positive |  |
| sql/select20.bool | generated | hand | 7173.1 | 17691.3 | 2.47x | 17608.8 | 2.45x | -0.5% | 21448 | 21392 | 2 % | [-1.4%..+0.9%], 1 of 5 positive | -0.4% [-3.9%..+1.8%], 3 of 5 positive |
| fix/One.yield-reader | generated | hand | 209.3 | 345.4 | 1.65x | 351.0 | 1.68x | +1.6% | 856 | 856 | 6 % | [-2.2%..+3.0%], 4 of 5 positive |  |
| fix/One.yield-string | generated | hand | 77.5 | 239.6 | 3.09x | 238.3 | 3.08x | -0.6% | 272 | 272 | 12 % | [-7.8%..+7.4%], 2 of 5 positive |  |
| fix/One.yield-memory | generated | hand | 77.2 | 120.6 | 1.56x | 121.6 | 1.57x | +0.8% | 352 | 352 | 12 % | [-2.7%..+2.0%], 4 of 5 positive |  |
| fix/One.whole-stream | generated | hand | 156.0 | 226.0 | 1.45x | 221.7 | 1.42x | -1.9% | 448 | 448 | 4 % | [-3.9%..+3.0%], 1 of 5 positive |  |
| fix/One.whole-reader | generated | hand | 157.0 | 231.3 | 1.47x | 241.7 | 1.54x | +4.5% | 416 | 416 | 4 % | [-5.6%..+12.2%], 3 of 5 positive |  |
| fix/Order.yield-reader | generated | hand | 904.2 | 1098.8 | 1.22x | 1107.2 | 1.22x | +0.8% | 1680 | 1680 | 5 % | [-2.7%..+4.2%], 3 of 5 positive |  |
| fix/Order.yield-string | generated | hand | 682.0 | 2311.2 | 3.39x | 2252.7 | 3.30x | -2.5% | 1096 | 1096 | 7 % | [-9.0%..+5.5%], 2 of 5 positive |  |
| fix/Order.yield-memory | generated | hand | 678.6 | 783.8 | 1.16x | 786.0 | 1.16x | +0.3% | 1176 | 1176 | 8 % | [-2.0%..+1.0%], 3 of 5 positive |  |
| fix/Order.whole-stream | generated | hand | 807.0 | 913.9 | 1.13x | 899.0 | 1.11x | -1.6% | 1352 | 1352 | 5 % | [-2.9%..+0.2%], 1 of 5 positive |  |
| fix/Order.whole-reader | generated | hand | 808.1 | 919.4 | 1.14x | 934.0 | 1.16x | +1.6% | 1320 | 1320 | 5 % | [-2.2%..+6.1%], 3 of 5 positive |  |
| fix/BinaryMany.yield-reader | generated | hand | 4975.3 | 5689.3 | 1.14x | 5680.8 | 1.14x | -0.2% | 7416 | 7416 | 1 % | [-2.4%..+1.3%], 2 of 5 positive |  |
| fix/BinaryMany.yield-string | generated | hand | 3621.8 | 15699.4 | 4.33x | 15838.7 | 4.37x | +0.9% | 6832 | 6832 | 1 % | [-5.3%..+7.0%], 2 of 5 positive |  |
| fix/BinaryMany.yield-memory | generated | hand | 3618.5 | 4801.0 | 1.33x | 4813.7 | 1.33x | +0.3% | 6912 | 6912 | 1 % | [-3.7%..+2.1%], 3 of 5 positive |  |
| fix/BinaryMany.whole-stream | generated | hand | 4725.7 | 4824.0 | 1.02x | 4777.0 | 1.01x | -1.0% | 7512 | 7512 | 1 % | [-2.2%..-0.2%], 0 of 5 positive |  |
| fix/BinaryMany.whole-reader | generated | hand | 4738.6 | 5203.4 | 1.10x | 5239.5 | 1.11x | +0.7% | 7480 | 7480 | 1 % | [-0.9%..+2.0%], 3 of 5 positive |  |
| fix/Orders128.yield-reader | generated | hand | 137415.3 | 96885.6 | 0.71x | 99351.6 | 0.72x | +2.5% | 118520 | 118520 | 4 % | [-2.8%..+3.2%], 3 of 5 positive |  |
| fix/Orders128.yield-string | generated | hand | 81781.4 | 285025.5 | 3.49x | 270571.6 | 3.31x | -5.1% | 117936 | 117936 | 7 % | [-9.2%..+5.3%], 2 of 5 positive |  |
| fix/Orders128.yield-memory | generated | hand | 81798.8 | 91123.1 | 1.11x | 90700.6 | 1.11x | -0.5% | 118016 | 118016 | 7 % | [-1.0%..+0.5%], 2 of 5 positive |  |
| fix/Orders128.whole-stream | generated | hand | 116337.4 | 87778.7 | 0.75x | 86965.3 | 0.75x | -0.9% | 129368 | 129368 | 5 % | [-2.5%..+1.6%], 2 of 5 positive |  |
| fix/Orders128.whole-reader | generated | hand | 116169.3 | 88966.9 | 0.77x | 90289.9 | 0.78x | +1.5% | 129336 | 129336 | 5 % | [-4.4%..+3.9%], 4 of 5 positive |  |
| fix/slope-0.yield-reader | generated | hand | 145.3 | 245.1 | 1.69x | 247.5 | 1.70x | +1.0% | 760 | 760 | 11 % | [-1.1%..+1.5%], 3 of 5 positive |  |
| fix/slope-0.yield-string | generated | hand | 23.2 | 51.4 | 2.21x | 51.3 | 2.21x | -0.2% | 176 | 176 | 1 % | [-0.4%..+1.1%], 3 of 5 positive |  |
| fix/slope-0.yield-memory | generated | hand | 23.2 | 62.6 | 2.70x | 64.2 | 2.77x | +2.5% | 256 | 256 | 2 % | [+2.2%..+3.5%], 5 of 5 positive |  |
| fix/slope-0.whole-stream | generated | hand | 93.0 | 142.4 | 1.53x | 143.8 | 1.55x | +1.0% | 320 | 320 | 6 % | [+0.2%..+1.3%], 5 of 5 positive |  |
| fix/slope-0.whole-reader | generated | hand | 92.5 | 152.1 | 1.64x | 152.9 | 1.65x | +0.5% | 288 | 288 | 6 % | [-0.2%..+1.2%], 4 of 5 positive |  |
| fix/slope-16.yield-reader | generated | hand | 1194.8 | 1280.1 | 1.07x | 1309.6 | 1.10x | +2.3% | 2296 | 2296 | 9 % | [-1.4%..+3.1%], 3 of 5 positive |  |
| fix/slope-16.yield-string | generated | hand | 864.9 | 3007.7 | 3.48x | 2888.0 | 3.34x | -4.0% | 1712 | 1712 | 16 % | [-13.1%..+9.1%], 2 of 5 positive |  |
| fix/slope-16.yield-memory | generated | hand | 865.5 | 986.5 | 1.14x | 981.4 | 1.13x | -0.5% | 1792 | 1792 | 16 % | [-1.9%..+0.4%], 3 of 5 positive |  |
| fix/slope-16.whole-stream | generated | hand | 1064.1 | 1060.3 | 1.00x | 1055.3 | 0.99x | -0.5% | 2008 | 2008 | 11 % | [-1.7%..+0.5%], 2 of 5 positive |  |
| fix/slope-16.whole-reader | generated | hand | 1063.0 | 1099.1 | 1.03x | 1107.2 | 1.04x | +0.7% | 1976 | 1976 | 11 % | [-1.2%..+5.8%], 3 of 5 positive |  |
| feeds/stock-count.small.text | generated | hand | 166.1 | 237.9 | 1.43x | 232.8 | 1.40x | -2.1% | 608 | 608 | 3 % | [-2.7%..+0.7%], 1 of 5 positive |  |
| feeds/stock-count.small.reader | generated | hand | 270.9 | 420.7 | 1.55x | 423.1 | 1.56x | +0.6% | 784 | 784 | 6 % | [-3.3%..+3.7%], 3 of 5 positive |  |
| feeds/stock-count.small.reader64 | generated | hand | 177.0 | 418.1 | 2.36x | 422.1 | 2.38x | +0.9% | 832 | 832 | 2 % | [-0.3%..+2.7%], 4 of 5 positive |  |
| feeds/stock-count.good.text | generated | hand | 36301.5 | 28549.9 | 0.79x | 28615.2 | 0.79x | +0.2% | 111312 | 111403 | 3 % | [+0.1%..+0.4%], 5 of 5 positive |  |
| feeds/stock-count.good.reader | generated | hand | 39860.8 | 40856.4 | 1.02x | 40922.0 | 1.03x | +0.2% | 111488 | 111579 | 9 % | [-0.6%..+0.4%], 3 of 5 positive |  |
| feeds/stock-count.good.reader64 | generated | hand | 41664.6 | 45507.7 | 1.09x | 45382.7 | 1.09x | -0.3% | 111536 | 111627 | 5 % | [-0.5%..+0.5%], 2 of 5 positive |  |
| feeds/stock-count.broken.text | generated | hand | 35916.6 | 32683.7 | 0.91x | 32692.8 | 0.91x | 0.0% | 107408 | 107499 | 5 % | [-1.1%..+1.9%], 3 of 5 positive |  |
| feeds/stock-count.broken.reader | generated | hand | 39381.2 | 41784.5 | 1.06x | 41792.0 | 1.06x | 0.0% | 107584 | 107675 | 9 % | [-0.6%..+0.5%], 3 of 5 positive |  |
| feeds/stock-count.broken.reader64 | generated | hand | 40950.5 | 45771.3 | 1.12x | 45732.4 | 1.12x | -0.1% | 107632 | 107632 | 6 % | [-0.5%..+0.7%], 2 of 5 positive |  |
| feeds/recovering.good | generated | control | 69141.7 | 69416.1 | 1.00x | 69428.5 | 1.00x | 0.0% | 240352 | 240352 | 1 % | [-1.4%..+0.9%], 4 of 5 positive |  |
| feeds/recovering.broken | generated | control | 74513.6 | 74939.5 | 1.01x | 75427.0 | 1.01x | +0.7% | 241920 | 241920 | 2 % | [-1.9%..+2.6%], 4 of 5 positive |  |
| web/url.plain | generated | hand | 94.7 | 219.8 | 2.32x | 218.4 | 2.31x | -0.7% | 360 | 360 | 4 % | [-1.7%..+0.8%], 2 of 5 positive |  |
| web/url.full | generated | hand | 165.6 | 295.7 | 1.79x | 285.8 | 1.73x | -3.3% | 536 | 536 | 1 % | [-6.4%..+1.3%], 1 of 5 positive | +1.3% [-2.5%..+4.2%], 4 of 5 positive |
| web/url.ipv4 | generated | hand | 110.7 | 231.4 | 2.09x | 227.8 | 2.06x | -1.5% | 384 | 384 | 3 % | [-3.2%..+3.8%], 2 of 5 positive |  |
| web/url.long-path | generated | hand | 175.4 | 414.2 | 2.36x | 412.6 | 2.35x | -0.4% | 512 | 512 | 9 % | [-1.0%..+0.1%], 3 of 5 positive |  |
| web/url.refused | generated | hand | 67.2 | 201.3 | 2.99x | 190.0 | 2.83x | -5.6% | 64 | 64 | 2 % | [-7.5%..-3.9%], 0 of 5 positive | -0.4% [-5.1%..+0.5%], 2 of 5 positive |
| web/url.relative | generated | hand | 75.3 | 191.3 | 2.54x | 193.1 | 2.56x | +0.9% | 312 | 312 | 2 % | [-1.3%..+2.5%], 3 of 5 positive |  |
| web/url.relative-dot-colon | generated | hand | 62.2 | 169.0 | 2.72x | 171.2 | 2.75x | +1.3% | 280 | 280 | 1 % | [-1.1%..+4.2%], 3 of 5 positive |  |
| web/url.relative-letters | generated | hand | 103.5 | 235.3 | 2.27x | 237.6 | 2.30x | +1.0% | 312 | 312 | 2 % | [-1.8%..+3.4%], 3 of 5 positive |  |
| web/json.object | generated | hand | 586.0 | 926.1 | 1.58x | 925.0 | 1.58x | -0.1% | 2584 | 2584 | 1 % | [-1.3%..+1.0%], 2 of 5 positive | +0.5% [-1.2%..+2.4%], 4 of 5 positive |
| web/json.array | generated | hand | 581.9 | 726.1 | 1.25x | 724.7 | 1.25x | -0.2% | 2400 | 2400 | 4 % | [-1.3%..+0.5%], 2 of 5 positive |  |
| web/cookie.full | generated | control | 305.8 | 337.9 | 1.11x | 339.8 | 1.11x | +0.6% | 2136 | 2136 | 2 % | [-0.6%..+1.4%], 4 of 5 positive |  |
| web/cookie.short | generated | control | 52.2 | 80.8 | 1.55x | 80.9 | 1.55x | +0.2% | 336 | 336 | 2 % | [+0.0%..+1.4%], 4 of 5 positive |  |
| web/pointer.full | generated | control | 204.6 | 200.1 | 0.98x | 199.8 | 0.98x | -0.1% | 440 | 440 | 2 % | [-2.4%..+4.8%], 3 of 5 positive |  |
| web/pointer.short | generated | control | 36.2 | 60.9 | 1.68x | 60.8 | 1.68x | -0.2% | 144 | 144 | 1 % | [-1.2%..+0.4%], 2 of 5 positive |  |
| web/media-type.plain | generated | control | 162.9 | 199.6 | 1.23x | 196.4 | 1.21x | -1.6% | 448 | 448 | 1 % | [-6.4%..+5.1%], 2 of 5 positive | +0.8% [-7.1%..+7.0%], 3 of 5 positive |
| web/media-type.quoted | generated | control | 252.5 | 293.6 | 1.16x | 292.1 | 1.16x | -0.5% | 816 | 816 | 6 % | [-3.9%..+3.9%], 3 of 5 positive |  |
| web/media-type.refused | generated | control | 61.7 | 93.6 | 1.52x | 77.9 | 1.26x | -16.8% | 64 | 64 | 2 % | [-17.1%..-16.6%], 0 of 5 positive | +0.4% [-0.8%..+1.1%], 3 of 5 positive |
| web/addr-spec.plain | generated | control | 89.2 | 116.7 | 1.31x | 117.9 | 1.32x | +1.1% | 176 | 176 | 5 % | [-2.0%..+3.7%], 3 of 5 positive |  |
| web/addr-spec.refused | generated | control | 43.7 | 77.5 | 1.77x | 57.1 | 1.31x | -26.4% | 64 | 64 | 1 % | [-26.8%..-26.3%], 0 of 5 positive | +0.8% [+0.2%..+13.9%], 5 of 5 positive |
| web/date-time.utc | generated | control | 50.1 | 68.7 | 1.37x | 69.0 | 1.38x | +0.5% | 176 | 176 | 1 % | [-0.6%..+5.2%], 4 of 5 positive |  |
| web/date-time.offset | generated | control | 77.2 | 97.2 | 1.26x | 97.2 | 1.26x | 0.0% | 208 | 208 | 0 % | [-1.0%..+3.4%], 1 of 5 positive |  |
| web/language-tag.plain | generated | control | 114.5 | 154.3 | 1.35x | 155.2 | 1.36x | +0.5% | 360 | 360 | 2 % | [-2.3%..+1.7%], 2 of 5 positive |  |
| web/language-tag.full | generated | control | 398.4 | 486.2 | 1.22x | 479.2 | 1.20x | -1.4% | 720 | 720 | 9 % | [-3.8%..+1.3%], 1 of 5 positive |  |
| web/language-tag.refused | generated | control | 109.4 | 146.8 | 1.34x | 134.5 | 1.23x | -8.4% | 64 | 64 | 1 % | [-9.0%..-7.4%], 0 of 5 positive | -1.5% [-2.2%..+0.2%], 1 of 5 positive |
| web/date-time.refused | generated | control | 35.6 | 54.2 | 1.52x | 54.3 | 1.53x | +0.2% | 96 | 96 | 2 % | [-0.2%..+12.0%], 2 of 5 positive | -0.1% [-1.4%..+0.5%], 2 of 5 positive |
| web/content-disposition.full | generated | control | 432.2 | 466.2 | 1.08x | 469.9 | 1.09x | +0.8% | 1680 | 1680 | 4 % | [-0.5%..+3.5%], 4 of 5 positive |  |
| web/content-disposition.refused | generated | control | 27.0 | 59.6 | 2.21x | 59.8 | 2.21x | +0.3% | 64 | 64 | 2 % | [+0.0%..+7.9%], 4 of 5 positive |  |
| web/uri-template.full | generated | control | 739.6 | 775.9 | 1.05x | 784.4 | 1.06x | +1.1% | 1088 | 1088 | 4 % | [-2.0%..+2.5%], 3 of 5 positive |  |
| web/uri-template.refused | generated | control | 3494498600.0 | 3556171950.0 | 1.02x | 3517116250.0 | 1.01x | -1.1% | 64 | 64 | 5 % | [-3.4%..+0.7%], 2 of 5 positive |  |
| web/json-patch.full | generated | control | 1337.1 | 1400.5 | 1.05x | 1399.0 | 1.05x | -0.1% | 4552 | 4552 | 3 % | [-1.0%..+0.5%], 2 of 5 positive |  |
| web/forwarded.full | generated | control | 1573.4 | 1641.8 | 1.04x | 1625.2 | 1.03x | -1.0% | 2648 | 2648 | 2 % | [-2.2%..+1.3%], 1 of 5 positive |  |
| web/forwarded.refused | generated | control | 111.7 | 141.5 | 1.27x | 141.0 | 1.26x | -0.3% | 328 | 328 | 3 % | [-0.4%..+1.2%], 4 of 5 positive |  |
| web/link.full | generated | control | 1334.7 | 1376.1 | 1.03x | 1399.5 | 1.05x | +1.7% | 2968 | 2968 | 4 % | [+0.1%..+2.7%], 5 of 5 positive |  |
| web/link.refused | generated | control | 36.2 | 65.4 | 1.80x | 51.2 | 1.41x | -21.7% | 64 | 64 | 2 % | [-22.4%..-21.2%], 0 of 5 positive |  |
| web/cookie.refused | generated | control | 27.0 | 53.6 | 1.98x | 53.7 | 1.99x | +0.2% | 136 | 136 | 1 % | [-0.1%..+1.6%], 4 of 5 positive |  |
| web/sf.item | generated | control | 261.6 | 300.0 | 1.15x | 296.9 | 1.13x | -1.0% | 800 | 800 | 4 % | [-4.0%..+3.7%], 2 of 5 positive |  |
| web/sf.list | generated | control | 1169.4 | 1241.0 | 1.06x | 1244.7 | 1.06x | +0.3% | 3064 | 3064 | 5 % | [-2.7%..+1.7%], 3 of 5 positive |  |
| web/sf.dictionary | generated | control | 1205.1 | 1252.3 | 1.04x | 1228.7 | 1.02x | -1.9% | 3280 | 3280 | 1 % | [-3.5%..+1.0%], 1 of 5 positive |  |
| el/floor | generated | hand | 325.5 | 810.3 | 2.49x | 820.8 | 2.52x | +1.3% | 1104 | 1104 | 12 % | [-5.9%..+3.9%], 4 of 5 positive |  |
| el/floor | immediate | hand | 325.5 | 482.1 | 1.48x | 475.9 | 1.46x | -1.3% | 1120 | 1120 | 12 % | [-5.0%..+7.4%], 1 of 5 positive |  |
| el/ladder | generated | hand | 998.9 | 1748.8 | 1.75x | 1765.6 | 1.77x | +1.0% | 1776 | 1776 | 4 % | [-0.7%..+2.6%], 4 of 5 positive | -0.9% [-5.0%..+2.5%], 3 of 5 positive |
| el/ladder | immediate | hand | 998.9 | 1178.7 | 1.18x | 1159.8 | 1.16x | -1.6% | 1784 | 1784 | 4 % | [-2.9%..+5.2%], 1 of 5 positive | -0.5% [-1.8%..+1.1%], 1 of 5 positive |
| el/nest7 | generated | hand | 650.5 | 1719.3 | 2.64x | 1704.8 | 2.62x | -0.8% | 1152 | 1152 | 7 % | [-2.0%..+4.3%], 1 of 5 positive |  |
| el/nest7 | immediate | hand | 650.5 | 1048.0 | 1.61x | 1055.3 | 1.62x | +0.7% | 1120 | 1120 | 7 % | [-0.6%..+3.1%], 3 of 5 positive |  |
| el/block | generated | hand | 1065.1 | 1813.4 | 1.70x | 1820.7 | 1.71x | +0.4% | 2344 | 2344 | 2 % | [-1.0%..+2.0%], 3 of 5 positive |  |
| el/block | immediate | hand | 1065.1 | 1174.8 | 1.10x | 1177.5 | 1.11x | +0.2% | 2440 | 2440 | 2 % | [-1.4%..+0.6%], 1 of 5 positive |  |
| el/try | generated | hand | 3268.2 | 6165.9 | 1.89x | 6120.0 | 1.87x | -0.7% | 5632 | 5632 | 1 % | [-1.5%..+3.0%], 1 of 5 positive |  |
| el/try | immediate | hand | 3268.2 | 4787.2 | 1.46x | 4725.0 | 1.45x | -1.3% | 5752 | 5752 | 1 % | [-2.5%..-0.2%], 0 of 5 positive |  |
| el/loop | generated | hand | 2236.9 | 3745.6 | 1.67x | 3732.1 | 1.67x | -0.4% | 4776 | 4776 | 1 % | [-1.6%..+1.2%], 2 of 5 positive |  |
| el/loop | immediate | hand | 2236.9 | 2575.8 | 1.15x | 2583.8 | 1.16x | +0.3% | 4880 | 4880 | 1 % | [-2.1%..+1.0%], 3 of 5 positive |  |
| el/terms100 | generated | hand | 11444.2 | 16325.3 | 1.43x | 16299.0 | 1.42x | -0.2% | 17904 | 17904 | 3 % | [-2.0%..+1.2%], 3 of 5 positive |  |
| el/terms100 | immediate | hand | 11444.2 | 13092.7 | 1.14x | 12929.1 | 1.13x | -1.2% | 18720 | 18720 | 3 % | [-4.4%..+2.2%], 1 of 5 positive |  |
| el/terms1000 | generated | hand | 110976.5 | 154845.8 | 1.40x | 154650.5 | 1.39x | -0.1% | 169104 | 169104 | 2 % | [-1.7%..+1.2%], 2 of 5 positive |  |
| el/terms1000 | immediate | hand | 110976.5 | 125245.7 | 1.13x | 124692.2 | 1.12x | -0.4% | 177120 | 177120 | 2 % | [-4.0%..+2.2%], 2 of 5 positive |  |
| el/overloads | generated | hand | 1875.4 | 2626.8 | 1.40x | 2577.4 | 1.37x | -1.9% | 4248 | 4248 | 1 % | [-2.3%..+1.4%], 1 of 5 positive |  |
| el/overloads | immediate | hand | 1875.4 | 2058.8 | 1.10x | 2063.4 | 1.10x | +0.2% | 4200 | 4200 | 1 % | [-1.3%..+2.3%], 2 of 5 positive |  |
| el/string | generated | hand | 301.2 | 964.7 | 3.20x | 964.1 | 3.20x | -0.1% | 1056 | 1056 | 7 % | [-1.4%..+1.1%], 1 of 5 positive |  |
| el/string | immediate | hand | 301.2 | 678.8 | 2.25x | 683.7 | 2.27x | +0.7% | 1032 | 1032 | 7 % | [-1.9%..+4.8%], 4 of 5 positive |  |
| el/interpolation | generated | hand | 2192.3 | 4886.6 | 2.23x | 4942.6 | 2.25x | +1.1% | 2368 | 2368 | 18 % | [-0.6%..+16.1%], 4 of 5 positive |  |
| el/interpolation | immediate | hand | 2192.3 | 4407.3 | 2.01x | 4060.9 | 1.85x | -7.9% | 2424 | 2424 | 18 % | [-12.7%..+12.9%], 2 of 5 positive |  |
| el/untyped | generated | hand | 16633.5 | 18346.1 | 1.10x | 18315.6 | 1.10x | -0.2% | 16424 | 16424 | 23 % | [-2.2%..+1.4%], 1 of 5 positive |  |
| el/refused-early | generated | hand | 520.2 | 1257.8 | 2.42x | 1301.9 | 2.50x | +3.5% | 1064 | 1064 | 9 % | [+1.8%..+7.0%], 5 of 5 positive |  |
| el/refused-early | immediate | hand | 520.2 | 1315.3 | 2.53x | 1302.5 | 2.50x | -1.0% | 1944 | 1944 | 9 % | [-8.8%..+1.6%], 3 of 5 positive |  |
| el/refused-late | generated | hand | 1563.2 | 1957.4 | 1.25x | 2008.1 | 1.28x | +2.6% | 1064 | 1064 | 14 % | [+1.0%..+5.4%], 5 of 5 positive |  |
| el/refused-late | immediate | hand | 1563.2 | 3492.9 | 2.23x | 3386.3 | 2.17x | -3.1% | 3928 | 3928 | 14 % | [-3.7%..+1.4%], 1 of 5 positive |  |
| sql/literal | generated | hand | 71.2 | 156.3 | 2.19x | 158.1 | 2.22x | +1.2% | 160 | 160 | 6 % | [-5.8%..+6.3%], 4 of 5 positive |  |
| sql/comment | generated | hand | 3038.4 | 5465.8 | 1.80x | 5429.3 | 1.79x | -0.7% | 5136 | 5136 | 6 % | [-1.8%..+2.5%], 3 of 5 positive |  |
| sql/conditions100 | generated | hand | 79377.3 | 140391.7 | 1.77x | 143458.0 | 1.81x | +2.2% | 161736 | 161736 | 10 % | [-0.3%..+2.2%], 4 of 5 positive |  |
| sql/conditions1000 | generated | hand | 778838.3 | 1446546.1 | 1.86x | 1444032.0 | 1.85x | -0.2% | 1616136 | 1616136 | 20 % | [-1.1%..+1.7%], 4 of 5 positive |  |
| tsql/comment | generated | scriptdom | 27191.6 | 1591.9 | 0.06x | 1588.5 | 0.06x | -0.2% | 1192 | 1192 | 6 % | [-3.5%..+1.8%], 2 of 5 positive |  |
| sql/column | generated | hand | 209.6 | 417.0 | 1.99x | 413.3 | 1.97x | -0.9% | 392 | 392 | 7 % | [-2.9%..+2.3%], 2 of 5 positive |  |
| sql/arithmetic | generated | hand | 2218.7 | 4614.8 | 2.08x | 4629.3 | 2.09x | +0.3% | 3472 | 3472 | 23 % | [-1.5%..+2.0%], 2 of 5 positive |  |
| sql/nest8 | generated | hand | 3620.0 | 12013.5 | 3.32x | 12034.1 | 3.32x | +0.2% | 6504 | 6504 | 7 % | [-0.2%..+0.9%], 4 of 5 positive |  |
| sql/condition | generated | hand | 2738.8 | 5083.6 | 1.86x | 5079.8 | 1.85x | -0.1% | 4928 | 4928 | 7 % | [-1.9%..+2.0%], 2 of 5 positive |  |
| sql/select1 | generated | hand | 905.4 | 1709.7 | 1.89x | 1692.0 | 1.87x | -1.0% | 1688 | 1688 | 6 % | [-2.2%..+2.9%], 2 of 5 positive |  |
| sql/select20 | generated | hand | 10836.1 | 21550.0 | 1.99x | 21233.2 | 1.96x | -1.5% | 21448 | 21448 | 7 % | [-1.5%..+0.8%], 3 of 5 positive | -0.8% [-4.1%..+0.9%], 1 of 5 positive |
| sql/values | generated | hand | 713.2 | 2060.9 | 2.89x | 1976.4 | 2.77x | -4.1% | 1904 | 1904 | 7 % | [-5.4%..-1.8%], 0 of 5 positive |  |
| sql/create | generated | hand | 1014.8 | 2806.4 | 2.77x | 2781.0 | 2.74x | -0.9% | 1568 | 1568 | 6 % | [-8.1%..+1.8%], 2 of 5 positive |  |
| sql/refused-late | generated | hand | 3514.6 | 13396.4 | 3.81x | 13656.6 | 3.89x | +1.9% | 13552 | 13552 | 31 % | [+0.1%..+4.7%], 5 of 5 positive |  |
