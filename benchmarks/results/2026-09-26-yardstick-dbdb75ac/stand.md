Median of 5 of 5 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.8, 31.6, 31.5, 31.5, 31.5).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, dbdb75ac (run in a tree at 6aa98d13), 2026-09-26 23:46

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.5 ns.

A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 70.5 | generated | 52.9 | 0.75x | 264 | 136 | 9 % |
| fix/One.text | 70.5 | regex-lesser | 155.7 | 2.21x | 264 | 680 | 9 % |
| fix/One.text | 70.5 | regex-compiled-lesser | 126.5 | 1.79x | 264 | 680 | 9 % |
| fix/One.bytes | 73.1 | generated | 80.0 | 1.09x | 264 | 192 | 11 % |
| fix/One.stream | 148.9 | generated | 117.9 | 0.79x | 4512 | 392 | 3 % |
| fix/Order.text | 563.8 | generated | 396.1 | 0.70x | 880 | 752 | 3 % |
| fix/Order.text | 563.8 | regex-lesser | 1419.5 | 2.52x | 880 | 5960 | 3 % |
| fix/Order.text | 563.8 | regex-compiled-lesser | 1132.9 | 2.01x | 880 | 5960 | 3 % |
| fix/Order.bytes | 592.9 | generated | 524.9 | 0.89x | 880 | 808 | 3 % |
| fix/Order.stream | 731.8 | generated | 598.4 | 0.82x | 5048 | 928 | 3 % |
| fix/BinaryMany.text | 5167.7 | generated | 4250.7 | 0.82x | 8888 | 8760 | 5 % |
| fix/BinaryMany.bytes | 5422.9 | generated | 5212.9 | 0.96x | 8888 | 8816 | 3 % |
| fix/BinaryMany.stream | 7221.7 | generated | 5516.3 | 0.76x | 12120 | 8000 | 1 % |
| fix/Orders128.text | 66662.4 | generated | 46656.1 | 0.70x | 89272 | 89144 | 2 % |
| fix/Orders128.text | 66662.4 | regex-lesser | 173058.9 | 2.60x | 89272 | 742728 | 2 % |
| fix/Orders128.text | 66662.4 | regex-compiled-lesser | 138998.9 | 2.09x | 89272 | 742728 | 2 % |
| fix/Orders128.bytes | 70481.1 | generated | 61339.7 | 0.87x | 89272 | 89200 | 2 % |
| fix/Orders128.stream | 106068.9 | generated | 66421.9 | 0.63x | 82264 | 78144 | 2 % |
| fix/OrderMalformed.text | 584.7 | generated | 414.5 | 0.71x | 944 | 912 | 6 % |
| fix/OrderMalformed.bytes | 602.7 | generated | 542.0 | 0.90x | 944 | 968 | 3 % |
| fix/OrderMalformed.stream | 729.4 | generated | 604.6 | 0.83x | 5112 | 1088 | 3 % |
| fix/slope-0.text | 25.7 | generated | 18.7 | 0.73x | 160 | 32 | 9 % |
| fix/slope-0.text | 25.7 | ideal | 9.6 | 0.38x | 160 | 88 | 9 % |
| fix/slope-0.text | 25.7 | regex-lesser | 35.1 | 1.37x | 160 | 120 | 9 % |
| fix/slope-0.text | 25.7 | regex-compiled-lesser | 35.9 | 1.40x | 160 | 120 | 9 % |
| fix/slope-1.text | 85.7 | generated | 68.0 | 0.79x | 304 | 176 | 3 % |
| fix/slope-1.text | 85.7 | ideal | 56.1 | 0.65x | 304 | 232 | 3 % |
| fix/slope-1.text | 85.7 | regex-lesser | 152.9 | 1.78x | 304 | 680 | 3 % |
| fix/slope-1.text | 85.7 | regex-compiled-lesser | 125.1 | 1.46x | 304 | 680 | 3 % |
| fix/slope-2.text | 148.4 | generated | 120.7 | 0.81x | 440 | 312 | 3 % |
| fix/slope-2.text | 148.4 | ideal | 101.5 | 0.68x | 440 | 368 | 3 % |
| fix/slope-2.text | 148.4 | regex-lesser | 271.4 | 1.83x | 440 | 1184 | 3 % |
| fix/slope-2.text | 148.4 | regex-compiled-lesser | 215.1 | 1.45x | 440 | 1184 | 3 % |
| fix/slope-4.text | 256.5 | generated | 203.2 | 0.79x | 624 | 496 | 6 % |
| fix/slope-4.text | 256.5 | ideal | 173.2 | 0.68x | 624 | 552 | 6 % |
| fix/slope-4.text | 256.5 | regex-lesser | 506.5 | 1.97x | 624 | 2192 | 6 % |
| fix/slope-4.text | 256.5 | regex-compiled-lesser | 398.1 | 1.55x | 624 | 2192 | 6 % |
| fix/slope-8.text | 459.1 | generated | 361.8 | 0.79x | 1000 | 872 | 25 % |
| fix/slope-8.text | 459.1 | ideal | 302.4 | 0.66x | 1000 | 840 | 25 % |
| fix/slope-8.text | 459.1 | regex-lesser | 1029.0 | 2.24x | 1000 | 4296 | 25 % |
| fix/slope-8.text | 459.1 | regex-compiled-lesser | 818.5 | 1.78x | 1000 | 4296 | 25 % |
| fix/slope-16.text | 889.4 | generated | 675.0 | 0.76x | 1752 | 1624 | 4 % |
| fix/slope-16.text | 889.4 | ideal | 600.2 | 0.67x | 1752 | 1680 | 4 % |
| fix/slope-16.text | 889.4 | regex-lesser | 2002.3 | 2.25x | 1752 | 8480 | 4 % |
| fix/slope-16.text | 889.4 | regex-compiled-lesser | 1602.1 | 1.80x | 1752 | 8480 | 4 % |
| fix/slope-0.bytes | 31.6 | generated | 42.3 | 1.34x | 184 | 112 | 10 % |
| fix/slope-1.bytes | 96.2 | generated | 104.5 | 1.09x | 336 | 264 | 9 % |
| fix/slope-2.bytes | 161.0 | generated | 161.5 | 1.00x | 480 | 408 | 6 % |
| fix/slope-4.bytes | 269.4 | generated | 260.6 | 0.97x | 680 | 608 | 5 % |
| fix/slope-8.bytes | 483.8 | generated | 457.4 | 0.95x | 1080 | 1008 | 10 % |
| fix/slope-16.bytes | 923.0 | generated | 832.8 | 0.90x | 1888 | 1816 | 7 % |
| fix/slope-0.stream | 91.4 | generated | 75.3 | 0.82x | 4464 | 344 | 12 % |
| fix/slope-1.stream | 166.5 | generated | 139.5 | 0.84x | 4584 | 464 | 5 % |
| fix/slope-2.stream | 237.7 | generated | 202.9 | 0.85x | 4720 | 600 | 5 % |
| fix/slope-4.stream | 363.9 | generated | 313.7 | 0.86x | 4904 | 784 | 6 % |
| fix/slope-8.stream | 611.1 | generated | 512.1 | 0.84x | 5272 | 1152 | 5 % |
| fix/slope-16.stream | 1138.1 | generated | 917.7 | 0.81x | 6016 | 1896 | 2 % |
| web/url.plain | 90.5 | generated | 203.5 | 2.25x | 152 | 296 | 4 % |
| web/url.plain | 90.5 | regex | 1078.1 | 11.91x | 152 | 1064 | 4 % |
| web/url.plain | 90.5 | regex-compiled | 424.2 | 4.69x | 152 | 1064 | 4 % |
| web/url.full | 172.0 | generated | 273.6 | 1.59x | 328 | 472 | 5 % |
| web/url.full | 172.0 | regex | 1136.4 | 6.61x | 328 | 1336 | 5 % |
| web/url.full | 172.0 | regex-compiled | 432.6 | 2.52x | 328 | 1336 | 5 % |
| web/url.ipv4 | 108.6 | generated | 209.5 | 1.93x | 176 | 320 | 1 % |
| web/url.ipv4 | 108.6 | regex | 964.0 | 8.88x | 176 | 1088 | 1 % |
| web/url.ipv4 | 108.6 | regex-compiled | 413.9 | 3.81x | 176 | 1088 | 1 % |
| web/url.long-path | 179.0 | generated | 399.9 | 2.23x | 304 | 448 | 3 % |
| web/url.long-path | 179.0 | regex | 2287.4 | 12.78x | 304 | 1216 | 3 % |
| web/url.long-path | 179.0 | regex-compiled | 596.8 | 3.33x | 304 | 1216 | 3 % |
| web/url.refused | 58.3 | generated | 167.8 | 2.88x | 0 | 0 | 5 % |
| web/url.refused | 58.3 | regex | 770.0 | 13.20x | 0 | 0 | 5 % |
| web/url.refused | 58.3 | regex-compiled | 118.5 | 2.03x | 0 | 0 | 5 % |
| web/json.object | 542.5 | generated | 846.1 | 1.56x | 1976 | 2520 | 5 % |
| web/json.object | 542.5 | system-text-json | 502.3 | 0.93x | 1976 | 72 | 5 % |
| web/json.array | 606.7 | generated | 734.7 | 1.21x | 2112 | 2336 | 5 % |
| web/json.array | 606.7 | system-text-json | 589.5 | 0.97x | 2112 | 72 | 5 % |
| web/date-time.utc | 27.1 | generated | 52.5 | 1.93x | 112 | 112 | 5 % |
| web/date-time.utc | 27.1 | regex | 453.3 | 16.70x | 112 | 1192 | 5 % |
| web/date-time.utc | 27.1 | regex-compiled | 364.8 | 13.44x | 112 | 1192 | 5 % |
| web/date-time.offset | 48.9 | generated | 78.6 | 1.61x | 144 | 144 | 5 % |
| web/date-time.offset | 48.9 | regex | 518.4 | 10.60x | 144 | 1240 | 5 % |
| web/date-time.offset | 48.9 | regex-compiled | 406.2 | 8.30x | 144 | 1240 | 5 % |
| web/date-time.refused | 18.5 | generated | 34.9 | 1.88x | 32 | 32 | 3 % |
| web/date-time.refused | 18.5 | regex | 23.0 | 1.24x | 32 | 0 | 3 % |
| web/date-time.refused | 18.5 | regex-compiled | 22.2 | 1.20x | 32 | 0 | 3 % |
| feeds/stock-count.small.text | 174.3 | generated | 158.7 | 0.91x | 880 | 464 | 7 % |
| feeds/stock-count.small.reader | 268.7 | generated | 249.4 | 0.93x | 8992 | 576 | 6 % |
| feeds/stock-count.small.reader64 | 184.7 | generated | 231.8 | 1.26x | 928 | 576 | 6 % |
| feeds/stock-count.good.text | 39071.0 | generated | 31106.4 | 0.80x | 197664 | 111168 | 14 % |
| feeds/stock-count.good.reader | 43757.0 | generated | 38317.7 | 0.88x | 183440 | 111280 | 16 % |
| feeds/stock-count.good.reader64 | 43158.2 | generated | 43794.4 | 1.01x | 175376 | 111280 | 15 % |
| feeds/stock-count.broken.text | 38105.0 | generated | 35342.6 | 0.93x | 187584 | 107264 | 14 % |
| feeds/stock-count.broken.reader | 41671.0 | generated | 39786.3 | 0.95x | 174800 | 107376 | 15 % |
| feeds/stock-count.broken.reader64 | 42364.7 | generated | 45044.7 | 1.06x | 166736 | 107376 | 13 % |
| el/floor | 750.5 | tape | 818.3 | 1.09x | 1256 | 912 | 6 % |
| el/floor | 750.5 | immediate | 809.1 | 1.08x | 1256 | 912 | 6 % |
| el/ladder | 2863.1 | tape | 1961.9 | 0.69x | 2872 | 1528 | 4 % |
| el/ladder | 2863.1 | immediate | 1903.0 | 0.66x | 2872 | 1528 | 4 % |
| el/nest7 | 1873.3 | tape | 2050.2 | 1.09x | 1832 | 1464 | 6 % |
| el/nest7 | 1873.3 | immediate | 2024.1 | 1.08x | 1832 | 1464 | 6 % |
| el/block | 2181.9 | tape | 1891.8 | 0.87x | 2912 | 2072 | 8 % |
| el/block | 2181.9 | immediate | 1906.8 | 0.87x | 2912 | 2072 | 8 % |
| el/try | 8545.5 | tape | 6651.6 | 0.78x | 8568 | 5864 | 7 % |
| el/try | 8545.5 | immediate | 6750.4 | 0.79x | 8568 | 5864 | 7 % |
| el/loop | 5468.9 | tape | 4059.2 | 0.74x | 6328 | 4632 | 5 % |
| el/loop | 5468.9 | immediate | 4068.5 | 0.74x | 6328 | 4632 | 5 % |
| el/overloads | 5167.1 | tape | 4201.0 | 0.81x | 7472 | 5744 | 1 % |
| el/overloads | 5167.1 | immediate | 4211.9 | 0.82x | 7472 | 5744 | 1 % |
| el/string | 339.3 | tape | 1008.8 | 2.97x | 1248 | 1056 | 8 % |
| el/string | 339.3 | immediate | 994.0 | 2.93x | 1248 | 1056 | 8 % |
| el/interpolation | 2231.0 | tape | 3976.8 | 1.78x | 2632 | 2056 | 4 % |
| el/interpolation | 2231.0 | immediate | 4018.0 | 1.80x | 2632 | 2056 | 4 % |
| el/untyped | 27627.1 | tape | 27429.0 | 0.99x | 26112 | 24600 | 9 % |
| el/untyped | 27627.1 | immediate | 26755.3 | 0.97x | 26112 | 24600 | 9 % |
| el/refused-early | 895.6 | tape | 1270.8 | 1.42x | 1256 | 752 | 3 % |
| el/refused-early | 895.6 | immediate | 1237.6 | 1.38x | 1256 | 752 | 3 % |
| el/refused-late | 1936.1 | tape | 2150.6 | 1.11x | 2464 | 752 | 2 % |
| el/refused-late | 1936.1 | immediate | 2160.4 | 1.12x | 2464 | 752 | 2 % |
| sql/literal | 21.5 | generated | 100.3 | 4.66x | 40 | 40 | 4 % |
| sql/column | 131.3 | generated | 320.0 | 2.44x | 392 | 272 | 17 % |
| sql/arithmetic | 1745.5 | generated | 3681.2 | 2.11x | 4208 | 3832 | 11 % |
| sql/nest8 | 2037.8 | generated | 5610.3 | 2.75x | 5976 | 6528 | 12 % |
| sql/condition | 2139.6 | generated | 4328.7 | 2.02x | 5696 | 4680 | 10 % |
| sql/select1 | 720.7 | generated | 1503.4 | 2.09x | 1576 | 1584 | 8 % |
| sql/select20 | 8688.5 | generated | 20719.7 | 2.38x | 23976 | 21648 | 11 % |
| sql/values | 505.6 | generated | 1532.2 | 3.03x | 1648 | 1816 | 11 % |
| sql/comment | 2351.8 | generated | 5059.4 | 2.15x | 5408 | 5064 | 11 % |
| sql/conditions100 | 63165.2 | generated | 128135.9 | 2.03x | 188160 | 153696 | 11 % |
| sql/conditions1000 | 635156.2 | generated | 1332770.3 | 2.10x | 1872904 | 1536096 | 8 % |
| sql/create | 873.1 | generated | 2646.4 | 3.03x | 1584 | 1448 | 8 % |
| sql/refused-late | 3223.6 | generated | 13636.3 | 4.23x | 8376 | 13240 | 11 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/addr-spec.plain | 95.2 | regex | 213.8 | 2.25x | 112 | 584 | 9 % |
| web/addr-spec.plain | 95.2 | regex-compiled | 137.3 | 1.44x | 112 | 584 | 9 % |
| web/addr-spec.plain | 95.2 | reference-MailAddress | 388.2 | reference | 112 | 248 | 9 % |
| web/addr-spec.tagged | 102.6 | regex | 248.9 | 2.43x | 144 | 616 | 12 % |
| web/addr-spec.tagged | 102.6 | regex-compiled | 151.0 | 1.47x | 144 | 616 | 12 % |
| web/addr-spec.tagged | 102.6 | reference-MailAddress | 395.2 | reference | 144 | 312 | 12 % |
| web/addr-spec.refused | 26.3 | regex | 215.2 | 8.17x | 0 | 0 | 6 % |
| web/addr-spec.refused | 26.3 | regex-compiled | 53.8 | 2.04x | 0 | 0 | 6 % |
| web/addr-spec.refused | 26.3 | reference-MailAddress | 355.1 | reference | 0 | 112 | 6 % |
| web/media-type.plain | 166.6 | regex | 313.9 | 1.88x | 384 | 952 | 5 % |
| web/media-type.plain | 166.6 | regex-compiled | 241.0 | 1.45x | 384 | 952 | 5 % |
| web/media-type.plain | 166.6 | reference-MediaTypeHeaderValue | 49.7 | reference | 384 | 208 | 5 % |
| web/media-type.quoted | 268.8 | regex | 590.0 | 2.19x | 752 | 1360 | 6 % |
| web/media-type.quoted | 268.8 | regex-compiled | 331.6 | 1.23x | 752 | 1360 | 6 % |
| web/media-type.quoted | 268.8 | reference-MediaTypeHeaderValue | 82.8 | reference | 752 | 408 | 6 % |
| web/media-type.refused | 45.8 | regex | 71.4 | 1.56x | 0 | 0 | 5 % |
| web/media-type.refused | 45.8 | regex-compiled | 30.5 | 0.66x | 0 | 0 | 5 % |
| web/media-type.refused | 45.8 | reference-MediaTypeHeaderValue | 8.9 | reference | 0 | 0 | 5 % |
| web/cookie.full | 311.8 | reference-CookieContainer | 994.2 | reference | 2072 | 1968 | 4 % |
| web/cookie.short | 55.1 | reference-CookieContainer | 442.9 | reference | 272 | 1440 | 3 % |
| web/cookie.refused | 29.8 | reference-CookieContainer | 2458.9 | reference | 72 | 1128 | 3 % |
| web/content-disposition.full | 426.4 | — (N/A) | | | 1616 | | 6 % |
| web/content-disposition.refused | 36.8 | — (N/A) | | | 0 | | 6 % |
| web/uri-template.full | 560.5 | — (N/A) | | | 976 | | 2 % |
| web/uri-template.refused | 284.4 | — (N/A) | | | 232 | | 12 % |
| web/json-patch.full | 1415.7 | — (N/A) | | | 4520 | | 4 % |
| web/forwarded.full | 1640.7 | — (N/A) | | | 2584 | | 4 % |
| web/forwarded.refused | 120.8 | — (N/A) | | | 264 | | 7 % |
| web/link.full | 1359.1 | — (N/A) | | | 2904 | | 4 % |
| web/link.refused | 26.2 | — (N/A) | | | 0 | | 16 % |
| web/pointer.full | 202.1 | — (N/A) | | | 376 | | 6 % |
| web/pointer.short | 40.5 | — (N/A) | | | 80 | | 2 % |
| web/sf.item | 277.0 | — (N/A) | | | 736 | | 5 % |
| web/sf.list | 1218.1 | — (N/A) | | | 3000 | | 5 % |
| web/sf.dictionary | 1232.5 | — (N/A) | | | 3216 | | 5 % |
| fixmsg/slope-8 | 434.1 | — (N/A) | | | 1296 | | 4 % |
| fixmsg/slope-12 | 632.5 | — (N/A) | | | 1864 | | 4 % |
| fixmsg/slope-13 | 686.2 | — (N/A) | | | 1952 | | 4 % |
| fixmsg/slope-20 | 1043.8 | — (N/A) | | | 3024 | | 3 % |
| fixmsg/slope-21 | 1102.0 | — (N/A) | | | 3120 | | 4 % |
| fixmsg/slope-36 | 1819.7 | — (N/A) | | | 5352 | | 4 % |
| fixmsg/slope-37 | 1867.8 | — (N/A) | | | 5448 | | 5 % |
| fixmsg/slope-68 | 3368.2 | — (N/A) | | | 9984 | | 5 % |
| fixmsg/slope-69 | 3409.4 | — (N/A) | | | 10080 | | 6 % |
| fixmsg/slope-132 | 6508.1 | — (N/A) | | | 19224 | | 5 % |
| fixmsg/Order44.parse | 1056.0 | reference-QuickFIXn | 1777.3 | reference | 2920 | 5784 | 5 % |
| fixmsg/Order44.strict | 1179.8 | generated-loaded | 1318.1 | 1.12x | 2952 | 2952 | 4 % |
| fixmsg/Order44.strict | 1179.8 | reference-QuickFIXn | 2845.1 | reference | 2952 | 6808 | 4 % |
| fixmsg/Order.parse | 953.3 | — (N/A) | | | 2864 | | 6 % |
| fixmsg/Order.build | 1096.1 | — (N/A) | | | 3016 | | 4 % |
| fixmsg/Order42.parse-string | 1082.4 | — (N/A) | | | 2648 | | 4 % |
| fixmsg/Order50.parse-string | 1702.3 | — (N/A) | | | 4424 | | 5 % |
| fixmsg/Report50.parse-string | 4310.6 | — (N/A) | | | 7976 | | 5 % |
| fixmsg/Order42.strict-string | 1203.7 | — (N/A) | | | 2680 | | 4 % |
| fixmsg/Order50.strict-string | 1883.9 | — (N/A) | | | 4456 | | 4 % |
| fixmsg/Report50.strict-string | 4833.6 | — (N/A) | | | 8008 | | 1 % |
| fixmsg/Order42.build-string | 1190.4 | — (N/A) | | | 2840 | | 7 % |
| fixmsg/Order50.build-string | 1883.3 | — (N/A) | | | 4648 | | 2 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 30337.4 | generated | 3634.3 | 0.12x | 61512 | 3280 | 15 % |
| tsql/select-join | 30337.4 | located | 4768.3 | 0.16x | 61512 | 3280 | 15 % |
| tsql/insert-values | 12073.4 | generated | 2490.4 | 0.21x | 48504 | 2360 | 7 % |
| tsql/insert-values | 12073.4 | located | 2933.2 | 0.24x | 48504 | 2360 | 7 % |
| tsql/create-table | 19209.4 | generated | 3676.1 | 0.19x | 55656 | 4608 | 8 % |
| tsql/create-table | 19209.4 | located | 4107.2 | 0.21x | 55656 | 4608 | 8 % |
| tsql/update-subquery | 19035.0 | generated | 2771.8 | 0.15x | 54632 | 2528 | 14 % |
| tsql/update-subquery | 19035.0 | located | 3582.9 | 0.19x | 54632 | 2528 | 14 % |
| tsql/select-long | 92441.8 | generated | 14034.4 | 0.15x | 138064 | 13224 | 14 % |
| tsql/select-long | 92441.8 | located | 18786.6 | 0.20x | 138064 | 13224 | 14 % |
| tsql/comment | 20086.9 | generated | 1542.5 | 0.08x | 53584 | 1072 | 4 % |
| tsql/comment | 20086.9 | located | 2126.5 | 0.11x | 53584 | 1072 | 4 % |

First call in a fresh process, median of three:

| row | reading | ms |
| --- | --- | ---: |
| fix/One.text | hand | 26.89 |
| fix/One.text | generated | 26.17 |
| fix/One.text | regex-lesser | 1.08 |
| fix/One.text | regex-compiled-lesser | 3.82 |
| fix/One.bytes | hand | 25.99 |
| fix/One.bytes | generated | 27.12 |
| el/floor | hand | 34.00 |
| el/floor | tape | 22.52 |
| el/floor | immediate | 21.37 |
| sql/literal | hand | 1.12 |
| sql/literal | generated | 10.96 |
| sql/select20 | hand | 16.69 |
| sql/select20 | generated | 36.41 |
| fixmsg/Order44.strict | generated | 41.61 |
| fixmsg/Order44.strict | generated-loaded | 225.52 |
| fixmsg/Order44.strict | reference-QuickFIXn | 28.40 |

**`fixmsg/Order44.strict`, `reference-QuickFIXn`: its first call includes building the data dictionary (`new DataDictionary(path)` reads FIX44.xml, about a megabyte), and it is the cost of starting to use that reader, paid once per process and not per message.** An application that reads a million messages spreads it to nothing; dividing this number by ours reads as "a hundred times faster" and is the wrong quantity. The generated reading's first call builds no dictionary: its dictionary is compiled into the code and its 447 slots are filled as they are asked.

Held while 2,000,000 FIX fields are read lazily from a stream:

- hand: 4.5 KB above the floor, 2,500,000 fields
- generated: 4.4 KB above the floor, 2,500,000 fields

What the generator took, from the last build's reports:

None found: the last build compiled no grammar. Rebuild the projects (`-t:Rebuild`, or `--stand --rebuild`) to have them.

Generator time against the previous base, as history: the milliseconds move with the machine (the morning's base, rebuilt that evening, read 22-39% higher), so the gate to quote is `benchmarks/Gate-Generation.ps1`, which rebuilds the base alternately with the head in one run and holds their ratio:

Nothing to compare: this run has no generator report. Use `--rebuild`.
