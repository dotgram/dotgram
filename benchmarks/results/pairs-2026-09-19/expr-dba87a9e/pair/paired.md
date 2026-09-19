Median of 5 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.4, 31.1, 31.3, 31.3, 31.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 16:28

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 74.3 | 78.5 | 1.06x | 80.5 | 1.08x | +2.5% | 192 | 192 |
| fix/One.bytes | generated | 76.0 | 169.5 | 2.23x | 173.1 | 2.28x | +2.2% | 248 | 248 |
| fix/Order.text | generated | 696.1 | 657.6 | 0.94x | 662.1 | 0.95x | +0.7% | 1096 | 1096 |
| fix/OrderMalformed.text | generated | 697.5 | 657.7 | 0.94x | 655.7 | 0.94x | -0.3% | 1248 | 1248 |
| tsql/columns1000 | generated | 1700681.2 | 220425.0 | 0.13x | 217881.2 | 0.13x | -1.2% | 200464 | 200464 |
| tsql/conditions1000 | generated | 865831.2 | 411921.1 | 0.48x | 414327.3 | 0.48x | +0.6% | 424449 | 424449 |
| web/json.object10000 | generated | 775586.7 | 1317353.9 | 1.70x | 1210267.2 | 1.56x | -8.1% | 1599240 | 1599240 |
| el/refused-early.bool | generated | 344.4 | 1283.9 | 3.73x | 661.1 | 1.92x | -48.5% | 1064 | 624 |
| el/refused-late.bool | generated | 1204.1 | 1948.0 | 1.62x | 1027.9 | 0.85x | -47.2% | 1064 | 624 |
| el/ladder.bool | generated | 1023.0 | 1959.9 | 1.92x | 1977.4 | 1.93x | +0.9% | 1776 | 1720 |
| sql/refused-late.bool | generated | 2617.1 | 23099.2 | 8.83x | 11254.2 | 4.30x | -51.3% | 13552 | 6616 |
| sql/select20.bool | generated | 6794.1 | 37157.5 | 5.47x | 37655.1 | 5.54x | +1.3% | 21448 | 21392 |
| web/url.full | generated | 156.0 | 286.5 | 1.84x | 288.3 | 1.85x | +0.6% | 536 | 536 |
| web/url.refused | generated | 60.0 | 611.1 | 10.19x | 618.5 | 10.31x | +1.2% | 152 | 152 |
| web/json.object | generated | 584.4 | 947.0 | 1.62x | 923.7 | 1.58x | -2.5% | 2584 | 2584 |
| web/cookie.full | generated | 318.0 | 346.3 | 1.09x | 346.1 | 1.09x | -0.1% | 2136 | 2136 |
| web/pointer.full | generated | 207.3 | 202.2 | 0.98x | 201.9 | 0.97x | -0.1% | 440 | 440 |
| web/media-type.refused | generated | 112.7 | 146.7 | 1.30x | 145.9 | 1.29x | -0.6% | 64 | 64 |
| web/addr-spec.refused | generated | 113.6 | 138.5 | 1.22x | 140.5 | 1.24x | +1.5% | 152 | 152 |
| web/language-tag.refused | generated | 261.2 | 305.9 | 1.17x | 297.4 | 1.14x | -2.8% | 152 | 152 |
| el/floor | generated | 325.8 | 937.5 | 2.88x | 946.6 | 2.91x | +1.0% | 1104 | 1104 |
| el/floor | immediate | 325.8 | 471.6 | 1.45x | 475.9 | 1.46x | +0.9% | 1120 | 1120 |
| el/ladder | generated | 1025.0 | 1968.4 | 1.92x | 1957.6 | 1.91x | -0.5% | 1776 | 1776 |
| el/ladder | immediate | 1025.0 | 1176.7 | 1.15x | 1194.0 | 1.16x | +1.5% | 1784 | 1784 |
| el/string | generated | 293.8 | 1058.8 | 3.60x | 1071.7 | 3.65x | +1.2% | 1056 | 1056 |
| el/string | immediate | 293.8 | 662.0 | 2.25x | 679.4 | 2.31x | +2.6% | 1032 | 1032 |
| el/refused-early | generated | 339.9 | 1277.6 | 3.76x | 1330.4 | 3.91x | +4.1% | 1064 | 1064 |
| el/refused-early | immediate | 339.9 | 984.4 | 2.90x | 996.8 | 2.93x | +1.3% | 1944 | 1944 |
| el/refused-late | generated | 1185.9 | 1938.4 | 1.63x | 2001.2 | 1.69x | +3.2% | 1064 | 1064 |
| el/refused-late | immediate | 1185.9 | 2733.6 | 2.31x | 2714.9 | 2.29x | -0.7% | 3928 | 3928 |
| sql/conditions100 | generated | 48445.7 | 213130.6 | 4.40x | 212466.5 | 4.39x | -0.3% | 161736 | 161736 |
| sql/conditions1000 | generated | 486702.3 | 2149553.1 | 4.42x | 2129271.1 | 4.37x | -0.9% | 1616203 | 1616160 |
| tsql/comment | generated | 19463.5 | 2110.5 | 0.11x | 2090.7 | 0.11x | -0.9% | 1192 | 1192 |
| sql/column | generated | 130.7 | 402.7 | 3.08x | 417.2 | 3.19x | +3.6% | 392 | 392 |
| sql/select20 | generated | 6738.0 | 36998.6 | 5.49x | 38169.2 | 5.66x | +3.2% | 21448 | 21448 |
| sql/refused-late | generated | 2498.3 | 23171.1 | 9.27x | 23933.1 | 9.58x | +3.3% | 13552 | 13552 |
