# Before and after: the pairs of 2026-09-18

Every change measured on the stand today, each as a **pair**: the build before the change and
the build after it, timed alternately in one process (`--stand-paired`), five runs each in a
process of its own, the median of those whose control held (none was dropped). The hand
parser is this tree's own and does not move, so it is the control; where there is no hand
parser (the FIX message layer) it is this tree's own layer. The last column is the hand's
spread between the five runs — a change smaller than that is not a change. First calls are
taken in a fresh process each, median of five. Raw results:
[`benchmarks/results/pairs-2026-09-18/`](../../benchmarks/results/pairs-2026-09-18/) (the
`.json` and `.md` of each pair, the anatomy runs, the bisect logs).

What the numbers say, in one line each:

| change | first call | steady state |
| --- | --- | --- |
| SQL:2023 over kinds (7d3ba2d5) | literal 26 → 82 ms, select20 109 → 131 ms (+14..+56 ms) | -4..-40%, all ten rows |
| C1, `IndexOf` for short stop sets (137600f4) | — | flat: no gain per field |
| FIX anatomy items 1, 2, 3 (finance-24) | — | item 1 +4..+13%, item 2 -5..-13%, item 3 -0..-15% but +5..+10% on BinaryMany |
| D17, lazy `Expected` arrays (46eba522) | SQL literal 79 → 16 ms; EL -4..-16%; FIX 0 | ±2.6%; nothing on the accepted path |
| `FixSchema.Type` as a table (41d23b96) | FIX field parser -0.6 ms (-7%), hand -17% | flat |
| `FixSchema` as lazy slots (da7e5a81) | message layer 41.0 → 18.8 ms (-54%) | flat (-1.9%, -0.9%) |
| Q7.1 C, folded runs of alternatives | — | T-SQL generation 1.04x |

## Pairs

### SQL:2023 over kinds: 4e366615 to 7d3ba2d5

sql-39. The standard's parser moved to `Lexical = true`. Steady state, generated ns; the hand parser of this tree is the control.

Median of 5 of 5 runs, each in a process of its own; control 31.7 ns (the runs' controls: 32.0, 31.3, 31.7, 32.5, 31.4).

| row | reading | hand ns | before ns | after ns | change | B before / after | hand spread |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/literal | generated | 51.1 | 263.2 | 211.3 | -19.7% | 184 / 160 | 4% |
| sql/column | generated | 137.4 | 2370.6 | 1431.5 | -39.6% | 464 / 392 | 8% |
| sql/arithmetic | generated | 1624.5 | 23830.1 | 17480.5 | -26.6% | 3592 / 3472 | 8% |
| sql/nest8 | generated | 2681.7 | 45721.7 | 42644.5 | -6.7% | 6528 / 6504 | 8% |
| sql/condition | generated | 1896.2 | 19484.8 | 17813.9 | -8.6% | 5096 / 4928 | 6% |
| sql/select1 | generated | 744.3 | 8960.6 | 6928.6 | -22.7% | 1736 / 1688 | 17% |
| sql/select20 | generated | 7437.7 | 125831.7 | 94941.0 | -24.5% | 21520 / 21448 | 6% |
| sql/values | generated | 485.4 | 8373.4 | 8009.9 | -4.3% | 1928 / 1904 | 15% |
| sql/create | generated | 833.8 | 6735.5 | 6273.2 | -6.9% | 1664 / 1568 | 9% |
| sql/refused-late | generated | 2851.8 | 88532.6 | 58782.7 | -33.6% | 14848 / 13832 | 15% |

First call in a fresh process, median of 5 (both builds loaded before the call is timed, called through reflection):

| row | hand ms | before ms | after ms | change |
| --- | ---: | ---: | ---: | ---: |
| sql/literal | 1.75 | 26.10 | 82.10 | +215% |
| sql/column | 2.27 | 53.96 | 96.60 | +79% |
| sql/arithmetic | 13.81 | 113.46 | 128.94 | +14% |
| sql/nest8 | 12.67 | 97.68 | 123.82 | +27% |
| sql/condition | 14.81 | 98.76 | 123.31 | +25% |
| sql/select1 | 17.69 | 105.19 | 125.24 | +19% |
| sql/select20 | 17.87 | 108.97 | 131.33 | +21% |
| sql/values | 12.03 | 72.91 | 123.25 | +69% |
| sql/create | 8.46 | 93.86 | 133.05 | +42% |
| sql/refused-late | 18.96 | 119.77 | 133.32 | +11% |

### C1: a value run of one to five stop characters read with IndexOf: 137600f4 against f7b0522f

performance-ff. FIX and the expression language's string bodies; SQL is byte for byte the same on both sides and is the control.

Median of 5 of 5 runs, each in a process of its own; control 31.4 ns (the runs' controls: 31.4, 31.4, 31.3, 31.1, 31.5).

| row | reading | hand ns | before ns | after ns | change | B before / after | hand spread |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 75.0 | 231.5 | 234.2 | +1.2% | 192 / 192 | 3% |
| fix/Order.text | generated | 659.3 | 1854.3 | 1843.8 | -0.6% | 1096 / 1096 | 4% |
| fix/slope-0.text | generated | 24.8 | 76.6 | 77.2 | +0.7% | 88 / 88 | 2% |
| fix/slope-1.text | generated | 90.1 | 236.3 | 237.5 | +0.5% | 192 / 192 | 4% |
| fix/slope-2.text | generated | 158.3 | 388.9 | 399.6 | +2.8% | 296 / 296 | 6% |
| fix/slope-4.text | generated | 286.6 | 714.3 | 703.4 | -1.5% | 504 / 504 | 6% |
| fix/slope-8.text | generated | 505.6 | 1323.5 | 1312.5 | -0.8% | 920 / 920 | 4% |
| fix/slope-16.text | generated | 981.6 | 2528.8 | 2545.6 | +0.7% | 1752 / 1752 | 4% |
| el/string | generated | 268.8 | 1087.4 | 1051.3 | -3.3% | 1056 / 1056 | 8% |
| el/string | immediate | 268.8 | 697.5 | 679.0 | -2.6% | 1032 / 1032 | 8% |
| el/interpolation | generated | 1853.5 | 4225.4 | 4927.3 | +16.6% | 2368 / 2368 | 28% |
| el/interpolation | immediate | 1853.5 | 3855.0 | 3867.4 | +0.3% | 2424 / 2424 | 28% |
| el/untyped | generated | 19322.4 | 20199.6 | 21175.0 | +4.8% | 16424 / 16424 | 11% |
| sql/literal | generated | 61.4 | 222.1 | 214.7 | -3.3% | 160 / 160 | 6% |
| sql/select20 | generated | 8762.0 | 93975.4 | 93502.6 | -0.5% | 21448 / 21448 | 5% |

Nothing measurable: the FIX slope rows within ±3% (no gain per field), the string rows within their spread, `el/interpolation` +16.6% on a row whose hand reading moves 28% between runs.

### FIX anatomy 1: the text value and the data tag straight from the grammar: a8d8952a against 42a99dca

finance-24. On its own a slowdown of 4-13% on 10 of 12 rows.

Median of 5 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.4, 31.4, 31.3, 31.0, 30.9).

| row | reading | hand ns | before ns | after ns | change | B before / after | hand spread |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 74.5 | 224.0 | 245.4 | +9.6% | 192 / 192 | 4% |
| fix/One.bytes | generated | 75.7 | 250.6 | 284.6 | +13.5% | 248 / 248 | 4% |
| fix/One.stream | generated | 144.3 | 566.6 | 589.4 | +4.0% | 888 / 888 | 4% |
| fix/Order.text | generated | 669.4 | 1785.8 | 1905.6 | +6.7% | 1096 / 1096 | 2% |
| fix/Order.bytes | generated | 662.8 | 2030.2 | 2162.8 | +6.5% | 1152 / 1152 | 5% |
| fix/Order.stream | generated | 790.3 | 2961.4 | 3229.7 | +9.1% | 1712 / 1712 | 3% |
| fix/BinaryMany.text | generated | 3674.7 | 13351.2 | 13056.0 | -2.2% | 7256 / 7256 | 10% |
| fix/BinaryMany.bytes | generated | 3716.1 | 15723.7 | 15410.1 | -2.0% | 7312 / 7312 | 5% |
| fix/BinaryMany.stream | generated | 4595.1 | 20020.7 | 20782.6 | +3.8% | 7448 / 7448 | 2% |
| fix/Orders128.text | generated | 80944.5 | 220320.3 | 238718.3 | +8.4% | 129112 / 129112 | 5% |
| fix/Orders128.bytes | generated | 81333.3 | 250433.1 | 268324.3 | +7.1% | 129168 / 129168 | 3% |
| fix/Orders128.stream | generated | 114311.0 | 323611.2 | 360092.7 | +11.3% | 118552 / 118552 | 2% |

### FIX anatomy 2: the separator inside Field, one construction and one Locate: 80d1704b against a8d8952a

finance-24. 5-13% faster on all twelve rows.

Median of 5 of 5 runs, each in a process of its own; control 31.8 ns (the runs' controls: 31.2, 31.8, 31.8, 31.8, 31.7).

| row | reading | hand ns | before ns | after ns | change | B before / after | hand spread |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 79.5 | 248.6 | 223.7 | -10.0% | 192 / 192 | 7% |
| fix/One.bytes | generated | 80.6 | 273.4 | 256.3 | -6.3% | 248 / 248 | 13% |
| fix/One.stream | generated | 152.0 | 616.1 | 585.8 | -4.9% | 888 / 888 | 7% |
| fix/Order.text | generated | 715.3 | 1958.4 | 1746.1 | -10.8% | 1096 / 1096 | 7% |
| fix/Order.bytes | generated | 699.9 | 2261.2 | 1992.6 | -11.9% | 1152 / 1152 | 9% |
| fix/Order.stream | generated | 811.3 | 3266.9 | 3024.7 | -7.4% | 1712 / 1712 | 3% |
| fix/BinaryMany.text | generated | 3593.6 | 13189.5 | 12011.7 | -8.9% | 7256 / 7256 | 5% |
| fix/BinaryMany.bytes | generated | 3777.4 | 15769.1 | 14171.0 | -10.1% | 7312 / 7312 | 2% |
| fix/BinaryMany.stream | generated | 4769.6 | 20549.7 | 19144.0 | -6.8% | 7448 / 7448 | 10% |
| fix/Orders128.text | generated | 83391.1 | 239020.9 | 210628.1 | -11.9% | 129112 / 129112 | 9% |
| fix/Orders128.bytes | generated | 83176.1 | 274719.3 | 239013.0 | -13.0% | 129168 / 129168 | 2% |
| fix/Orders128.stream | generated | 117876.0 | 361185.8 | 335483.6 | -7.1% | 118552 / 118552 | 5% |

### FIX anatomy 3: the guard gets the tag's digits: 7141660c against 80d1704b

finance-24. Wins on the plain rows and loses 5-10% on BinaryMany (the text row's spread is 35%).

Median of 5 of 5 runs, each in a process of its own; control 31.2 ns (the runs' controls: 31.1, 31.4, 31.4, 31.2, 30.5).

| row | reading | hand ns | before ns | after ns | change | B before / after | hand spread |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 73.7 | 223.3 | 207.2 | -7.2% | 192 / 192 | 2% |
| fix/One.bytes | generated | 74.0 | 243.1 | 232.3 | -4.4% | 248 / 248 | 5% |
| fix/One.stream | generated | 146.6 | 574.2 | 571.9 | -0.4% | 888 / 888 | 4% |
| fix/Order.text | generated | 689.1 | 1726.6 | 1613.4 | -6.6% | 1096 / 1096 | 6% |
| fix/Order.bytes | generated | 678.1 | 1934.1 | 1677.9 | -13.2% | 1152 / 1152 | 4% |
| fix/Order.stream | generated | 795.0 | 2945.3 | 2840.5 | -3.6% | 1712 / 1712 | 4% |
| fix/BinaryMany.text | generated | 3678.8 | 11900.0 | 13078.0 | +9.9% | 7256 / 7256 | 35% |
| fix/BinaryMany.bytes | generated | 3745.7 | 14122.1 | 14803.5 | +4.8% | 7312 / 7312 | 4% |
| fix/BinaryMany.stream | generated | 4594.0 | 19031.6 | 19932.8 | +4.7% | 7448 / 7448 | 5% |
| fix/Orders128.text | generated | 82113.5 | 211332.8 | 195678.4 | -7.4% | 129112 / 129112 | 6% |
| fix/Orders128.bytes | generated | 82712.0 | 234699.9 | 206138.4 | -12.2% | 129168 / 129168 | 5% |
| fix/Orders128.stream | generated | 117815.0 | 333676.8 | 302619.5 | -9.3% | 118552 / 118552 | 8% |

### FIX: the text value from the wire again: 98d230dc against eb52b240

finance-24 (branch fix-x). 0 to -12%, nothing slower.

Median of 5 of 5 runs, each in a process of its own; control 31.6 ns (the runs' controls: 31.4, 31.6, 31.4, 31.7, 32.0).

| row | reading | hand ns | before ns | after ns | change | B before / after | hand spread |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 78.5 | 235.2 | 206.2 | -12.3% | 192 / 192 | 11% |
| fix/One.bytes | generated | 76.3 | 254.5 | 247.1 | -2.9% | 248 / 248 | 11% |
| fix/One.stream | generated | 150.2 | 582.2 | 566.5 | -2.7% | 888 / 888 | 4% |
| fix/Order.text | generated | 689.3 | 1697.5 | 1555.5 | -8.4% | 1096 / 1096 | 11% |
| fix/Order.bytes | generated | 694.4 | 1950.4 | 1852.7 | -5.0% | 1152 / 1152 | 5% |
| fix/Order.stream | generated | 826.9 | 3022.1 | 2892.7 | -4.3% | 1712 / 1712 | 8% |
| fix/BinaryMany.text | generated | 3657.8 | 12025.4 | 11982.0 | -0.4% | 7256 / 7256 | 8% |
| fix/BinaryMany.bytes | generated | 3849.3 | 14351.9 | 14297.8 | -0.4% | 7312 / 7312 | 7% |
| fix/BinaryMany.stream | generated | 4756.6 | 19678.7 | 19072.1 | -3.1% | 7448 / 7448 | 6% |
| fix/Orders128.text | generated | 85793.3 | 215781.3 | 198604.9 | -8.0% | 129112 / 129112 | 8% |
| fix/Orders128.bytes | generated | 84286.2 | 244017.6 | 221761.6 | -9.1% | 129168 / 129168 | 11% |
| fix/Orders128.stream | generated | 118936.2 | 334558.7 | 304658.9 | -8.9% | 118552 / 118552 | 9% |

### FIX: item 3 reworked (value sliced after the digits): 0b2b8e08 against 98d230dc

finance-24. Bytes and stream win up to -15%; BinaryMany loses 5-10% on all three forms.

Median of 5 of 5 runs, each in a process of its own; control 31.2 ns (the runs' controls: 31.4, 31.2, 30.8, 31.3, 31.1).

| row | reading | hand ns | before ns | after ns | change | B before / after | hand spread |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 73.9 | 199.6 | 208.3 | +4.3% | 192 / 192 | 6% |
| fix/One.bytes | generated | 74.4 | 231.7 | 207.4 | -10.5% | 248 / 248 | 3% |
| fix/One.stream | generated | 150.3 | 553.7 | 523.6 | -5.4% | 888 / 888 | 3% |
| fix/Order.text | generated | 672.9 | 1533.2 | 1480.4 | -3.4% | 1096 / 1096 | 5% |
| fix/Order.bytes | generated | 671.0 | 1800.1 | 1521.7 | -15.5% | 1152 / 1152 | 7% |
| fix/Order.stream | generated | 793.4 | 2627.9 | 2560.7 | -2.6% | 1712 / 1712 | 5% |
| fix/BinaryMany.text | generated | 3628.1 | 11763.1 | 12945.1 | +10.0% | 7256 / 7256 | 5% |
| fix/BinaryMany.bytes | generated | 3766.8 | 14088.3 | 14836.7 | +5.3% | 7312 / 7312 | 3% |
| fix/BinaryMany.stream | generated | 4755.8 | 18705.7 | 20512.2 | +9.7% | 7448 / 7448 | 4% |
| fix/Orders128.text | generated | 81635.1 | 188393.9 | 174056.4 | -7.6% | 129112 / 129112 | 4% |
| fix/Orders128.bytes | generated | 80844.4 | 212518.5 | 181989.8 | -14.4% | 129168 / 129168 | 4% |
| fix/Orders128.stream | generated | 116392.4 | 286695.8 | 269447.1 | -6.0% | 118552 / 118552 | 3% |

D17: the Expected arrays lazy, 6f66053c against 46eba522. First calls (sql-39):

| row | hand ms | before ms | after ms | change |
| --- | ---: | ---: | ---: | ---: |
| fix/One.text | 4.99 | 7.54 | 7.55 | +0% |
| fix/One.bytes | 4.96 | 8.17 | 8.01 | -2% |
| fix/One.stream | 5.18 | 8.55 | 8.29 | -3% |
| fix/slope-1.text | 5.01 | 7.53 | 7.50 | -0% |
| el/floor | 12.52 | 26.69 | 25.30 | -5% |
| el/floor (immediate) | 12.52 | 17.62 | 15.29 | -13% |
| el/ladder | 16.24 | 29.79 | 28.47 | -4% |
| el/ladder (immediate) | 16.24 | 23.00 | 19.41 | -16% |
| el/string | 8.77 | 29.54 | 27.97 | -5% |
| el/string (immediate) | 8.77 | 20.06 | 18.82 | -6% |
| el/interpolation | 18.62 | 41.70 | 39.94 | -4% |
| el/interpolation (immediate) | 18.62 | 35.89 | 32.63 | -9% |
| sql/literal | 1.64 | 79.34 | 15.68 | -80% |
| sql/select1 | 16.96 | 127.24 | 65.20 | -49% |
| sql/select20 | 18.52 | 131.44 | 67.62 | -49% |

### D17 steady state

Nothing moves on the accepted path.

Median of 5 of 5 runs, each in a process of its own; control 31.7 ns (the runs' controls: 31.6, 31.7, 31.9, 31.6, 31.7).

| row | reading | hand ns | before ns | after ns | change | B before / after | hand spread |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 78.3 | 222.3 | 236.8 | +6.5% | 192 / 192 | 5% |
| fix/One.bytes | generated | 78.1 | 256.8 | 258.7 | +0.8% | 248 / 248 | 8% |
| fix/One.stream | generated | 151.6 | 582.4 | 574.3 | -1.4% | 888 / 888 | 4% |
| fix/Order.text | generated | 680.5 | 1723.0 | 1739.8 | +1.0% | 1096 / 1096 | 9% |
| el/floor | generated | 322.4 | 955.9 | 945.9 | -1.1% | 1104 / 1104 | 6% |
| el/floor | immediate | 322.4 | 494.9 | 489.1 | -1.2% | 1120 / 1120 | 6% |
| el/ladder | generated | 1023.0 | 1975.9 | 2026.8 | +2.6% | 1776 / 1776 | 12% |
| el/ladder | immediate | 1023.0 | 1200.2 | 1197.4 | -0.2% | 1784 / 1784 | 12% |
| el/string | generated | 287.2 | 1099.1 | 1118.3 | +1.7% | 1056 / 1056 | 12% |
| el/string | immediate | 287.2 | 716.7 | 731.0 | +2.0% | 1032 / 1032 | 12% |
| sql/literal | generated | 57.1 | 210.3 | 213.1 | +1.3% | 160 / 160 | 7% |
| sql/select20 | generated | 7094.8 | 102312.2 | 101212.6 | -1.1% | 21448 / 21448 | 8% |

### FixSchema as tables: 41d23b96 against 93fb4eb4, steady state

finance-24. `FixSchema.Type` is a byte table and no longer a 912-way switch (13.8 KB of IL). The two stream rows were rerun (`w6`): +1.3% and -0.9%.

Median of 5 of 5 runs, each in a process of its own; control 31.4 ns (the runs' controls: 31.3, 31.5, 31.4, 31.4, 31.5).

| row | reading | hand ns | before ns | after ns | change | B before / after | hand spread |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 73.4 | 209.4 | 205.2 | -2.0% | 192 / 192 | 1% |
| fix/One.bytes | generated | 75.0 | 241.9 | 241.2 | -0.3% | 248 / 248 | 3% |
| fix/One.stream | generated | 152.6 | 550.8 | 549.8 | -0.2% | 888 / 888 | 8% |
| fix/Order.text | generated | 674.1 | 1583.3 | 1567.1 | -1.0% | 1096 / 1096 | 5% |
| fix/Order.bytes | generated | 682.0 | 1804.1 | 1807.7 | +0.2% | 1152 / 1152 | 5% |
| fix/Order.stream | generated | 801.1 | 2761.1 | 2864.2 | +3.7% | 1712 / 1712 | 5% |
| fix/BinaryMany.text | generated | 3622.8 | 11732.8 | 11649.2 | -0.7% | 7256 / 7256 | 3% |
| fix/BinaryMany.bytes | generated | 3737.5 | 13840.4 | 13962.9 | +0.9% | 7312 / 7312 | 51% |
| fix/BinaryMany.stream | generated | 4664.8 | 18917.4 | 18478.1 | -2.3% | 7448 / 7448 | 40% |
| fix/Orders128.bytes | generated | 81773.2 | 212359.8 | 212844.0 | +0.2% | 129168 / 129168 | 3% |
| fix/Orders128.stream | generated | 116237.7 | 293703.9 | 310012.3 | +5.6% | 118552 / 118552 | 2% |

FixSchema as tables, first call of the field parser (93fb4eb4 against 41d23b96):

| row | hand ms | before ms | after ms | change |
| --- | ---: | ---: | ---: | ---: |
| fix/One.text | 4.26 | 7.14 | 6.51 | -9% |
| fix/One.bytes | 4.25 | 7.70 | 7.05 | -8% |
| fix/One.stream | 4.51 | 8.01 | 7.39 | -8% |

FixSchema as tables and lazy slots, first call of the field parser (93fb4eb4 against da7e5a81):

| row | hand ms | before ms | after ms | change |
| --- | ---: | ---: | ---: | ---: |
| fix/One.text | 4.26 | 7.13 | 6.64 | -7% |
| fix/One.bytes | 4.27 | 7.70 | 7.19 | -7% |
| fix/One.stream | 4.54 | 8.03 | 7.50 | -7% |

### FixSchema's arrays as lazy slots: da7e5a81 against 6fadcb64, steady state

finance-24. The message layer has no hand parser; `hand` here is this tree's own `FixMessages`, a control that does not move.

Median of 5 of 5 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.5, 31.5, 31.9, 31.7, 30.6).

| row | reading | hand ns | before ns | after ns | change | B before / after | hand spread |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 75.1 | 207.5 | 200.5 | -3.4% | 192 / 192 | 6% |
| fix/One.bytes | generated | 75.8 | 236.6 | 230.2 | -2.7% | 248 / 248 | 5% |
| fix/One.stream | generated | 148.6 | 544.5 | 532.9 | -2.1% | 888 / 888 | 2% |
| fix/Order.text | generated | 658.6 | 1536.5 | 1522.3 | -0.9% | 1096 / 1096 | 4% |
| fixmsg/Order.parse | generated | 3062.1 | 3187.9 | 3127.5 | -1.9% | 3400 / 3400 | 8% |
| fixmsg/Order.build | generated | 3175.9 | 3681.7 | 3648.3 | -0.9% | 3592 / 3592 | 7% |

The message layer's first call (the paired harness):

| row | hand ms | before ms | after ms | change |
| --- | ---: | ---: | ---: | ---: |
| fixmsg/Order.parse | 16.11 | 38.42 | 16.06 | -58% |
| fixmsg/Order.build | 16.21 | 37.75 | 16.35 | -57% |


### The first call of FIX by phase (`fixfirst`, five fresh processes, medians)

The anatomy tool of `benchmarks/FirstCall`, with the runtime's events on, so its milliseconds run
above the stand's; it is for what was compiled, not for the time. Methods compiled, IL bytes
and milliseconds, per build:

| build | field parser, generated | field parser, hand | `FixSchema` type initializer | first `FixMessages.Parse` | first `Build` |
| --- | --- | --- | --- | --- | --- |
| 93fb4eb4 | 8.28 ms, 87 methods, 25,470 B | 5.69 ms, 40, 18,207 B | — | — | — |
| 59648810 (table) | 7.60 ms, 87, 11,631 B | 4.69 ms, 40, 4,368 B | — | — | — |
| 41d23b96 | 7.83 ms, 87, 11,631 B | 4.74 ms, 40, 4,368 B | — | — | — |
| 6fadcb64 | — | — | 24.2 ms, 50, 85,622 B | 16.8 ms, 194, 32,793 B | 8.1 ms, 72, 20,140 B |
| da7e5a81 (lazy slots) | 7.98 ms, 87, 11,646 B | 4.65 ms, 40, 4,368 B | 0.29 ms, 48, 3,061 B | 18.5 ms, 212, 38,082 B | 9.5 ms, 90, 25,429 B |

The `.cctor` of `FixSchema` was one method of 82,539 bytes of IL, compiled at the first touch: 24
of the 41 ms of the first message parse. The lazy slots compile 18 more methods when the Order
touches them (+1.7 ms), and the type initializer is gone.

### The generator's time, and the gate

Q7.1 C (`b35eb2ee` against `853b1665`, expr) with `benchmarks/Gate-Generation.ps1`: base and head
alternately three times, on the timing cores, the generator's own report per host, ratio of
medians. **No host moved by more than 20% and 100 ms**: T-SQL 4,398 → 4,553 ms (1.04x),
T-SQL located 2,139 → 2,205 (1.03x), SQL:2023 3,480 → 3,475 (1.00x), SQL-92 723 → 766 (1.06x).
([gate-q71c.md](../../benchmarks/results/pairs-2026-09-18/gate-q71c.md))

The gate is a ratio taken in one run because the milliseconds are not stable: the T-SQL
generation was **4.1 s at 01:49, 4.4 s on a quiet machine at 22:50, and 4.6-5.7 s in single
builds on the evening's busy one**, for commits that differ by nothing that matters. The
bisect from the morning's base to the head, one build per commit, pinned, is in
[the logs](../../benchmarks/results/pairs-2026-09-18/tsql-generation-bisect-2.log):

| commit | T-SQL ms | located ms | GramGrammar ms |
| --- | ---: | ---: | ---: |
| d4c58dfd (the morning's base) | 4,996 | 3,301 | 93 |
| c4a79af9 | 5,650 | 3,494 | 94 |
| 96d86795 | 4,922 | 3,036 | 188 |
| 5c6d1bc6 | 4,955 | 3,146 | 122 |
| d4f9a45c | 6,981 | 4,109 | 173 |
| 1875588b | **114,136** | **109,422** | 119 |
| c8d44074 (the fix) | 4,782 | 3,067 | 117 |
| 6173c973 | 5,049 | 2,919 | 140 |
| c6e0348b | 5,383 | 2,735 | 117 |
| d4c58dfd again, last | 4,556 | 2,602 | 112 |

Apart from the defect of `1875588b` (its build took 560 s) there is no step and no growth with
the rules: the +47% the first gate named against the morning's report was machine state and a
single build's noise (±10-20%). A gate on one build of each side is inside that noise, which
is why the script takes three rounds.

### What a build alongside does to a timing run

The stand's four rows taken 8 times on a quiet machine and 56 times while `DotGram.Tests` was
rebuilt with its dependencies, other sessions building alongside unpinned (the pinned build the
experiment was meant to be did not pin: a PowerShell literal `0xFFFF0000` is a negative Int32).
The medians of the loaded runs were within -3% to +4% of the quiet ones on every reading; single
runs were up to +85% (`fix/Order.text` hand), +76% (`el/ladder` immediate), +77% (`web/url.full`
generated) and +28% (`sql/select20` hand). A control above 33 ns flagged 15 of the 56, and did not
see the worst of the rest. So a median of five survives a busy machine and a single run does not;
that is the rule the stand now keeps (`--repeat 5`, the control gate), and the pinned experiment
is still to be repeated with the mask `[IntPtr]0xFFFF0000L`.
