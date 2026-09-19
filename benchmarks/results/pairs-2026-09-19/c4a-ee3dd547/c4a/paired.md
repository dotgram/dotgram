Median of 4 of 5 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.6, 31.3, 35.5, 31.3, 31.6).
Dropped for a control more than 5% off the median: run 3.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 09:48

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 76.1 | 155.4 | 2.04x | 163.5 | 2.15x | +5.2% | 192 | 192 |
| web/url.plain | generated | 89.9 | 220.2 | 2.45x | 220.5 | 2.45x | +0.1% | 360 | 360 |
| web/url.full | generated | 167.8 | 289.9 | 1.73x | 296.7 | 1.77x | +2.3% | 536 | 536 |
| web/url.ipv4 | generated | 108.7 | 231.1 | 2.13x | 236.3 | 2.17x | +2.3% | 384 | 384 |
| web/url.long-path | generated | 181.3 | 412.3 | 2.27x | 424.9 | 2.34x | +3.1% | 512 | 512 |
| web/url.refused | generated | 59.6 | 614.1 | 10.30x | 614.6 | 10.31x | +0.1% | 152 | 152 |
| web/json.object | generated | 516.7 | 1640.4 | 3.17x | 1654.9 | 3.20x | +0.9% | 2584 | 2584 |
| web/json.array | generated | 595.6 | 1762.4 | 2.96x | 1743.8 | 2.93x | -1.1% | 2424 | 2424 |
| web/cookie.full | generated | 324.6 | 355.2 | 1.09x | 341.5 | 1.05x | -3.8% | 2184 | 2136 |
| web/cookie.short | generated | 66.2 | 90.6 | 1.37x | 81.9 | 1.24x | -9.6% | 376 | 336 |
| web/pointer.full | generated | 333.6 | 381.3 | 1.14x | 377.2 | 1.13x | -1.1% | 440 | 440 |
| web/pointer.short | generated | 65.7 | 100.5 | 1.53x | 99.6 | 1.52x | -0.9% | 144 | 144 |
| web/media-type.plain | generated | 163.4 | 197.6 | 1.21x | 199.5 | 1.22x | +0.9% | 448 | 448 |
| web/media-type.quoted | generated | 291.9 | 334.4 | 1.15x | 328.0 | 1.12x | -1.9% | 816 | 816 |
| web/media-type.refused | generated | 138.1 | 170.0 | 1.23x | 166.6 | 1.21x | -2.0% | 64 | 64 |
| web/addr-spec.plain | generated | 92.6 | 117.1 | 1.26x | 117.4 | 1.27x | +0.3% | 176 | 176 |
| web/addr-spec.refused | generated | 117.8 | 148.4 | 1.26x | 146.7 | 1.25x | -1.2% | 152 | 152 |
| web/date-time.utc | generated | 151.4 | 186.2 | 1.23x | 149.0 | 0.98x | -20.0% | 608 | 392 |
| web/date-time.offset | generated | 218.0 | 251.6 | 1.15x | 216.2 | 0.99x | -14.1% | 832 | 600 |
| web/sf.item | generated | 251.0 | 318.9 | 1.27x | 318.9 | 1.27x | 0.0% | 800 | 800 |
| web/sf.list | generated | 1127.0 | 1218.2 | 1.08x | 1219.2 | 1.08x | +0.1% | 3064 | 3064 |
| web/sf.dictionary | generated | 1184.2 | 1199.1 | 1.01x | 1203.2 | 1.02x | +0.3% | 3280 | 3280 |
| el/floor | generated | 330.2 | 1013.7 | 3.07x | 993.1 | 3.01x | -2.0% | 1104 | 1104 |
| el/floor | immediate | 330.2 | 488.0 | 1.48x | 494.1 | 1.50x | +1.2% | 1120 | 1120 |
| el/ladder | generated | 1041.3 | 2246.4 | 2.16x | 2235.9 | 2.15x | -0.5% | 1776 | 1776 |
| el/ladder | immediate | 1041.3 | 1202.7 | 1.15x | 1190.3 | 1.14x | -1.0% | 1784 | 1784 |
| el/nest7 | generated | 707.9 | 1990.9 | 2.81x | 1974.3 | 2.79x | -0.8% | 1152 | 1152 |
| el/nest7 | immediate | 707.9 | 1073.3 | 1.52x | 1065.1 | 1.50x | -0.8% | 1120 | 1120 |
| el/block | generated | 1077.6 | 2213.2 | 2.05x | 2238.0 | 2.08x | +1.1% | 2344 | 2344 |
| el/block | immediate | 1077.6 | 1191.2 | 1.11x | 1179.7 | 1.09x | -1.0% | 2440 | 2440 |
| el/loop | generated | 2275.9 | 5078.2 | 2.23x | 5061.0 | 2.22x | -0.3% | 4776 | 4776 |
| el/loop | immediate | 2275.9 | 2644.6 | 1.16x | 2594.3 | 1.14x | -1.9% | 5200 | 4880 |
| el/terms100 | generated | 11807.5 | 33889.5 | 2.87x | 33614.8 | 2.85x | -0.8% | 17904 | 17904 |
| el/terms100 | immediate | 11807.5 | 12884.8 | 1.09x | 12802.6 | 1.08x | -0.6% | 18720 | 18720 |
| el/terms1000 | generated | 116380.9 | 1356295.9 | 11.65x | 1348303.1 | 11.59x | -0.6% | 169116 | 169116 |
| el/terms1000 | immediate | 116380.9 | 124028.9 | 1.07x | 124627.8 | 1.07x | +0.5% | 177120 | 177121 |
| el/overloads | generated | 1883.1 | 2923.1 | 1.55x | 2864.5 | 1.52x | -2.0% | 4248 | 4248 |
| el/overloads | immediate | 1883.1 | 2057.1 | 1.09x | 2047.5 | 1.09x | -0.5% | 4240 | 4200 |
| el/string | generated | 281.4 | 1091.4 | 3.88x | 1083.0 | 3.85x | -0.8% | 1056 | 1056 |
| el/string | immediate | 281.4 | 682.3 | 2.42x | 685.1 | 2.43x | +0.4% | 1032 | 1032 |
| el/interpolation | generated | 1812.2 | 4829.9 | 2.67x | 5149.4 | 2.84x | +6.6% | 2368 | 2368 |
| el/interpolation | immediate | 1812.2 | 4201.4 | 2.32x | 4058.7 | 2.24x | -3.4% | 2424 | 2424 |
| el/untyped | generated | 18618.9 | 25890.9 | 1.39x | 21069.2 | 1.13x | -18.6% | 16424 | 16424 |
| el/refused-early | generated | 459.3 | 1607.6 | 3.50x | 1589.2 | 3.46x | -1.1% | 1064 | 1064 |
| el/refused-early | immediate | 459.3 | 1230.6 | 2.68x | 1203.7 | 2.62x | -2.2% | 1944 | 1944 |
| el/refused-late | generated | 1439.8 | 2204.1 | 1.53x | 2235.0 | 1.55x | +1.4% | 1064 | 1064 |
| el/refused-late | immediate | 1439.8 | 3046.6 | 2.12x | 3066.3 | 2.13x | +0.6% | 3928 | 3928 |
| sql/select20 | generated | 9222.7 | 97858.3 | 10.61x | 95884.2 | 10.40x | -2.0% | 21448 | 21448 |
