Median of 5 of 5 runs, each in a process of its own; control 31.4 ns (the runs' controls: 32.0, 32.1, 31.4, 31.4, 31.4).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, 46dec223, 2026-09-26 23:05

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.4 ns.

A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 71.5 | generated | 54.0 | 0.75x | 264 | 136 | 22 % |
| fix/One.text | 71.5 | regex-lesser | 157.0 | 2.19x | 264 | 680 | 22 % |
| fix/One.text | 71.5 | regex-compiled-lesser | 127.3 | 1.78x | 264 | 680 | 22 % |
| fix/One.bytes | 73.5 | generated | 85.8 | 1.17x | 264 | 192 | 13 % |
| fix/One.stream | 150.1 | generated | 118.8 | 0.79x | 4512 | 392 | 4 % |
| fix/Order.text | 568.3 | generated | 421.7 | 0.74x | 880 | 752 | 4 % |
| fix/Order.text | 568.3 | regex-lesser | 1429.4 | 2.52x | 880 | 5960 | 4 % |
| fix/Order.text | 568.3 | regex-compiled-lesser | 1163.9 | 2.05x | 880 | 5960 | 4 % |
| fix/Order.bytes | 590.8 | generated | 537.1 | 0.91x | 880 | 808 | 3 % |
| fix/Order.stream | 743.5 | generated | 592.2 | 0.80x | 5048 | 928 | 4 % |
| fix/BinaryMany.text | 5020.1 | generated | 4363.3 | 0.87x | 8888 | 8760 | 5 % |
| fix/BinaryMany.bytes | 5303.4 | generated | 5365.6 | 1.01x | 8888 | 8816 | 6 % |
| fix/BinaryMany.stream | 7326.0 | generated | 5390.2 | 0.74x | 12120 | 8000 | 3 % |
| fix/Orders128.text | 67390.2 | generated | 49374.7 | 0.73x | 89272 | 89144 | 4 % |
| fix/Orders128.text | 67390.2 | regex-lesser | 174736.9 | 2.59x | 89272 | 742728 | 4 % |
| fix/Orders128.text | 67390.2 | regex-compiled-lesser | 140422.8 | 2.08x | 89272 | 742728 | 4 % |
| fix/Orders128.bytes | 70013.9 | generated | 62031.2 | 0.89x | 89272 | 89200 | 4 % |
| fix/Orders128.stream | 106818.9 | generated | 66146.7 | 0.62x | 82264 | 78144 | 5 % |
| fix/OrderMalformed.text | 577.8 | generated | 433.1 | 0.75x | 944 | 912 | 2 % |
| fix/OrderMalformed.bytes | 599.5 | generated | 560.4 | 0.93x | 944 | 968 | 4 % |
| fix/OrderMalformed.stream | 744.6 | generated | 600.1 | 0.81x | 5112 | 1088 | 4 % |
| fix/slope-0.text | 26.5 | generated | 18.7 | 0.70x | 160 | 32 | 27 % |
| fix/slope-0.text | 26.5 | ideal | 9.6 | 0.36x | 160 | 88 | 27 % |
| fix/slope-0.text | 26.5 | regex-lesser | 35.9 | 1.36x | 160 | 120 | 27 % |
| fix/slope-0.text | 26.5 | regex-compiled-lesser | 36.9 | 1.39x | 160 | 120 | 27 % |
| fix/slope-1.text | 86.4 | generated | 70.4 | 0.81x | 304 | 176 | 4 % |
| fix/slope-1.text | 86.4 | ideal | 56.5 | 0.65x | 304 | 232 | 4 % |
| fix/slope-1.text | 86.4 | regex-lesser | 156.5 | 1.81x | 304 | 680 | 4 % |
| fix/slope-1.text | 86.4 | regex-compiled-lesser | 126.7 | 1.47x | 304 | 680 | 4 % |
| fix/slope-2.text | 147.9 | generated | 121.3 | 0.82x | 440 | 312 | 4 % |
| fix/slope-2.text | 147.9 | ideal | 103.3 | 0.70x | 440 | 368 | 4 % |
| fix/slope-2.text | 147.9 | regex-lesser | 275.9 | 1.87x | 440 | 1184 | 4 % |
| fix/slope-2.text | 147.9 | regex-compiled-lesser | 218.3 | 1.48x | 440 | 1184 | 4 % |
| fix/slope-4.text | 251.1 | generated | 200.5 | 0.80x | 624 | 496 | 5 % |
| fix/slope-4.text | 251.1 | ideal | 176.0 | 0.70x | 624 | 552 | 5 % |
| fix/slope-4.text | 251.1 | regex-lesser | 522.3 | 2.08x | 624 | 2192 | 5 % |
| fix/slope-4.text | 251.1 | regex-compiled-lesser | 419.4 | 1.67x | 624 | 2192 | 5 % |
| fix/slope-8.text | 448.8 | generated | 361.3 | 0.81x | 1000 | 872 | 3 % |
| fix/slope-8.text | 448.8 | ideal | 306.8 | 0.68x | 1000 | 840 | 3 % |
| fix/slope-8.text | 448.8 | regex-lesser | 1043.3 | 2.32x | 1000 | 4296 | 3 % |
| fix/slope-8.text | 448.8 | regex-compiled-lesser | 848.3 | 1.89x | 1000 | 4296 | 3 % |
| fix/slope-16.text | 880.2 | generated | 670.2 | 0.76x | 1752 | 1624 | 4 % |
| fix/slope-16.text | 880.2 | ideal | 611.7 | 0.69x | 1752 | 1680 | 4 % |
| fix/slope-16.text | 880.2 | regex-lesser | 2044.0 | 2.32x | 1752 | 8480 | 4 % |
| fix/slope-16.text | 880.2 | regex-compiled-lesser | 1650.6 | 1.88x | 1752 | 8480 | 4 % |
| fix/slope-0.bytes | 32.5 | generated | 43.1 | 1.33x | 184 | 112 | 28 % |
| fix/slope-1.bytes | 96.5 | generated | 105.4 | 1.09x | 336 | 264 | 4 % |
| fix/slope-2.bytes | 161.6 | generated | 164.2 | 1.02x | 480 | 408 | 5 % |
| fix/slope-4.bytes | 270.5 | generated | 262.7 | 0.97x | 680 | 608 | 4 % |
| fix/slope-8.bytes | 481.1 | generated | 462.1 | 0.96x | 1080 | 1008 | 2 % |
| fix/slope-16.bytes | 926.3 | generated | 841.0 | 0.91x | 1888 | 1816 | 4 % |
| fix/slope-0.stream | 91.9 | generated | 74.6 | 0.81x | 4464 | 344 | 5 % |
| fix/slope-1.stream | 169.7 | generated | 137.3 | 0.81x | 4584 | 464 | 7 % |
| fix/slope-2.stream | 241.0 | generated | 202.4 | 0.84x | 4720 | 600 | 4 % |
| fix/slope-4.stream | 368.6 | generated | 315.8 | 0.86x | 4904 | 784 | 6 % |
| fix/slope-8.stream | 627.9 | generated | 521.0 | 0.83x | 5272 | 1152 | 5 % |
| fix/slope-16.stream | 1150.4 | generated | 920.9 | 0.80x | 6016 | 1896 | 3 % |
| web/url.plain | 89.1 | generated | 206.6 | 2.32x | 152 | 296 | 2 % |
| web/url.plain | 89.1 | regex | 1080.6 | 12.12x | 152 | 1064 | 2 % |
| web/url.plain | 89.1 | regex-compiled | 422.3 | 4.74x | 152 | 1064 | 2 % |
| web/url.full | 170.8 | generated | 288.2 | 1.69x | 328 | 472 | 33 % |
| web/url.full | 170.8 | regex | 1143.2 | 6.69x | 328 | 1336 | 33 % |
| web/url.full | 170.8 | regex-compiled | 427.4 | 2.50x | 328 | 1336 | 33 % |
| web/url.ipv4 | 108.0 | generated | 212.5 | 1.97x | 176 | 320 | 9 % |
| web/url.ipv4 | 108.0 | regex | 993.5 | 9.20x | 176 | 1088 | 9 % |
| web/url.ipv4 | 108.0 | regex-compiled | 424.8 | 3.93x | 176 | 1088 | 9 % |
| web/url.long-path | 180.9 | generated | 400.0 | 2.21x | 304 | 448 | 4 % |
| web/url.long-path | 180.9 | regex | 2339.6 | 12.94x | 304 | 1216 | 4 % |
| web/url.long-path | 180.9 | regex-compiled | 618.1 | 3.42x | 304 | 1216 | 4 % |
| web/url.refused | 57.7 | generated | 171.5 | 2.97x | 0 | 0 | 5 % |
| web/url.refused | 57.7 | regex | 768.4 | 13.31x | 0 | 0 | 5 % |
| web/url.refused | 57.7 | regex-compiled | 121.1 | 2.10x | 0 | 0 | 5 % |
| web/json.object | 525.2 | generated | 843.2 | 1.61x | 1976 | 2520 | 9 % |
| web/json.object | 525.2 | system-text-json | 501.4 | 0.95x | 1976 | 72 | 9 % |
| web/json.array | 627.3 | generated | 748.7 | 1.19x | 2112 | 2336 | 16 % |
| web/json.array | 627.3 | system-text-json | 593.3 | 0.95x | 2112 | 72 | 16 % |
| web/date-time.utc | 27.9 | generated | 51.6 | 1.85x | 112 | 112 | 3 % |
| web/date-time.utc | 27.9 | regex | 451.2 | 16.18x | 112 | 1192 | 3 % |
| web/date-time.utc | 27.9 | regex-compiled | 360.5 | 12.92x | 112 | 1192 | 3 % |
| web/date-time.offset | 49.2 | generated | 77.8 | 1.58x | 144 | 144 | 6 % |
| web/date-time.offset | 49.2 | regex | 520.5 | 10.57x | 144 | 1240 | 6 % |
| web/date-time.offset | 49.2 | regex-compiled | 399.0 | 8.11x | 144 | 1240 | 6 % |
| web/date-time.refused | 18.6 | generated | 33.6 | 1.81x | 32 | 32 | 4 % |
| web/date-time.refused | 18.6 | regex | 23.0 | 1.24x | 32 | 0 | 4 % |
| web/date-time.refused | 18.6 | regex-compiled | 22.0 | 1.18x | 32 | 0 | 4 % |
| feeds/stock-count.small.text | 173.0 | generated | 157.0 | 0.91x | 880 | 464 | 15 % |
| feeds/stock-count.small.reader | 270.4 | generated | 251.7 | 0.93x | 8992 | 576 | 7 % |
| feeds/stock-count.small.reader64 | 186.2 | generated | 230.7 | 1.24x | 928 | 576 | 12 % |
| feeds/stock-count.good.text | 39313.7 | generated | 31383.6 | 0.80x | 197664 | 111168 | 15 % |
| feeds/stock-count.good.reader | 42326.4 | generated | 42405.9 | 1.00x | 183440 | 111280 | 12 % |
| feeds/stock-count.good.reader64 | 43901.1 | generated | 46575.5 | 1.06x | 175376 | 111280 | 11 % |
| feeds/stock-count.broken.text | 39034.5 | generated | 35536.3 | 0.91x | 187584 | 107264 | 12 % |
| feeds/stock-count.broken.reader | 41266.1 | generated | 43332.3 | 1.05x | 174800 | 107376 | 10 % |
| feeds/stock-count.broken.reader64 | 43023.6 | generated | 47152.3 | 1.10x | 166736 | 107376 | 8 % |
| el/floor | 341.2 | tape | 792.0 | 2.32x | 1056 | 1000 | 9 % |
| el/floor | 341.2 | immediate | 783.6 | 2.30x | 1056 | 1000 | 9 % |
| el/ladder | 1037.8 | tape | 1928.6 | 1.86x | 1744 | 1840 | 5 % |
| el/ladder | 1037.8 | immediate | 1893.5 | 1.82x | 1744 | 1840 | 5 % |
| el/nest7 | 695.2 | tape | 1992.8 | 2.87x | 1056 | 1552 | 11 % |
| el/nest7 | 695.2 | immediate | 1976.9 | 2.84x | 1056 | 1552 | 11 % |
| el/block | 1095.9 | tape | 1892.1 | 1.73x | 2296 | 2288 | 7 % |
| el/block | 1095.9 | immediate | 1912.6 | 1.75x | 2296 | 2288 | 7 % |
| el/try | 3309.5 | tape | 4919.8 | 1.49x | 4456 | 4472 | 5 % |
| el/try | 3309.5 | immediate | 4861.2 | 1.47x | 4456 | 4472 | 5 % |
| el/loop | 2332.7 | tape | 3979.6 | 1.71x | 4224 | 4816 | 6 % |
| el/loop | 2332.7 | immediate | 4046.9 | 1.73x | 4224 | 4816 | 6 % |
| el/overloads | 1884.1 | tape | 2672.6 | 1.42x | 3992 | 4192 | 4 % |
| el/overloads | 1884.1 | immediate | 2603.9 | 1.38x | 3992 | 4192 | 4 % |
| el/string | 302.0 | tape | 973.4 | 3.22x | 1144 | 952 | 12 % |
| el/string | 302.0 | immediate | 972.7 | 3.22x | 1144 | 952 | 12 % |
| el/interpolation | 1437.8 | tape | 4079.9 | 2.84x | 2336 | 2336 | 4 % |
| el/interpolation | 1437.8 | immediate | 4191.2 | 2.91x | 2336 | 2336 | 4 % |
| el/untyped | 18695.8 | tape | 20497.2 | 1.10x | 16232 | 16392 | 22 % |
| el/untyped | 18695.8 | immediate | 20043.8 | 1.07x | 16232 | 16392 | 22 % |
| el/refused-early | 464.8 | tape | 1256.3 | 2.70x | 1056 | 936 | 4 % |
| el/refused-early | 464.8 | immediate | 1233.1 | 2.65x | 1056 | 936 | 4 % |
| el/refused-late | 1565.1 | tape | 2036.7 | 1.30x | 2264 | 936 | 7 % |
| el/refused-late | 1565.1 | immediate | 2171.4 | 1.39x | 2264 | 936 | 7 % |
| sql/literal | 22.6 | generated | 100.6 | 4.45x | 40 | 40 | 2 % |
| sql/column | 134.7 | generated | 323.2 | 2.40x | 392 | 272 | 7 % |
| sql/arithmetic | 1819.3 | generated | 4217.7 | 2.32x | 4208 | 3832 | 4 % |
| sql/nest8 | 2146.1 | generated | 10753.1 | 5.01x | 5976 | 6528 | 4 % |
| sql/condition | 2242.1 | generated | 4697.8 | 2.10x | 5696 | 4680 | 5 % |
| sql/select1 | 756.0 | generated | 1630.6 | 2.16x | 1576 | 1584 | 9 % |
| sql/select20 | 9128.9 | generated | 21230.6 | 2.33x | 23976 | 21648 | 2 % |
| sql/values | 529.7 | generated | 1854.0 | 3.50x | 1648 | 1816 | 10 % |
| sql/comment | 2487.9 | generated | 5234.0 | 2.10x | 5408 | 5064 | 6 % |
| sql/conditions100 | 65485.3 | generated | 137054.1 | 2.09x | 188160 | 153696 | 19 % |
| sql/conditions1000 | 657178.9 | generated | 1388724.2 | 2.11x | 1872904 | 1536096 | 4 % |
| sql/create | 895.0 | generated | 2639.2 | 2.95x | 1584 | 1448 | 6 % |
| sql/refused-late | 3380.8 | generated | 14005.7 | 4.14x | 8376 | 13240 | 2 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/addr-spec.plain | 93.7 | regex | 215.0 | 2.30x | 112 | 584 | 3 % |
| web/addr-spec.plain | 93.7 | regex-compiled | 138.4 | 1.48x | 112 | 584 | 3 % |
| web/addr-spec.plain | 93.7 | reference-MailAddress | 391.0 | reference | 112 | 248 | 3 % |
| web/addr-spec.tagged | 100.7 | regex | 251.1 | 2.49x | 144 | 616 | 5 % |
| web/addr-spec.tagged | 100.7 | regex-compiled | 154.5 | 1.53x | 144 | 616 | 5 % |
| web/addr-spec.tagged | 100.7 | reference-MailAddress | 401.2 | reference | 144 | 312 | 5 % |
| web/addr-spec.refused | 26.0 | regex | 214.4 | 8.25x | 0 | 0 | 4 % |
| web/addr-spec.refused | 26.0 | regex-compiled | 53.1 | 2.04x | 0 | 0 | 4 % |
| web/addr-spec.refused | 26.0 | reference-MailAddress | 357.4 | reference | 0 | 112 | 4 % |
| web/media-type.plain | 167.3 | regex | 318.8 | 1.91x | 384 | 952 | 26 % |
| web/media-type.plain | 167.3 | regex-compiled | 245.0 | 1.47x | 384 | 952 | 26 % |
| web/media-type.plain | 167.3 | reference-MediaTypeHeaderValue | 49.7 | reference | 384 | 208 | 26 % |
| web/media-type.quoted | 271.3 | regex | 602.3 | 2.22x | 752 | 1360 | 16 % |
| web/media-type.quoted | 271.3 | regex-compiled | 340.1 | 1.25x | 752 | 1360 | 16 % |
| web/media-type.quoted | 271.3 | reference-MediaTypeHeaderValue | 81.9 | reference | 752 | 408 | 16 % |
| web/media-type.refused | 46.5 | regex | 71.9 | 1.55x | 0 | 0 | 5 % |
| web/media-type.refused | 46.5 | regex-compiled | 30.6 | 0.66x | 0 | 0 | 5 % |
| web/media-type.refused | 46.5 | reference-MediaTypeHeaderValue | 8.9 | reference | 0 | 0 | 5 % |
| web/cookie.full | 316.0 | reference-CookieContainer | 1010.5 | reference | 2072 | 1968 | 1 % |
| web/cookie.short | 55.1 | reference-CookieContainer | 451.8 | reference | 272 | 1440 | 4 % |
| web/cookie.refused | 30.0 | reference-CookieContainer | 2456.0 | reference | 72 | 1128 | 2 % |
| web/content-disposition.full | 426.0 | — (N/A) | | | 1616 | | 3 % |
| web/content-disposition.refused | 35.6 | — (N/A) | | | 0 | | 3 % |
| web/uri-template.full | 557.5 | — (N/A) | | | 976 | | 6 % |
| web/uri-template.refused | 286.0 | — (N/A) | | | 232 | | 5 % |
| web/json-patch.full | 1426.9 | — (N/A) | | | 4520 | | 6 % |
| web/forwarded.full | 1625.4 | — (N/A) | | | 2584 | | 3 % |
| web/forwarded.refused | 119.9 | — (N/A) | | | 264 | | 5 % |
| web/link.full | 1398.7 | — (N/A) | | | 2904 | | 4 % |
| web/link.refused | 25.8 | — (N/A) | | | 0 | | 3 % |
| web/pointer.full | 202.5 | — (N/A) | | | 376 | | 5 % |
| web/pointer.short | 39.9 | — (N/A) | | | 80 | | 6 % |
| web/sf.item | 274.1 | — (N/A) | | | 736 | | 6 % |
| web/sf.list | 1247.4 | — (N/A) | | | 3000 | | 7 % |
| web/sf.dictionary | 1238.2 | — (N/A) | | | 3216 | | 5 % |
| fixmsg/slope-8 | 445.0 | — (N/A) | | | 1296 | | 12 % |
| fixmsg/slope-12 | 653.5 | — (N/A) | | | 1864 | | 13 % |
| fixmsg/slope-13 | 706.0 | — (N/A) | | | 1952 | | 13 % |
| fixmsg/slope-20 | 1068.9 | — (N/A) | | | 3024 | | 14 % |
| fixmsg/slope-21 | 1143.9 | — (N/A) | | | 3120 | | 20 % |
| fixmsg/slope-36 | 1862.3 | — (N/A) | | | 5352 | | 17 % |
| fixmsg/slope-37 | 1898.4 | — (N/A) | | | 5448 | | 16 % |
| fixmsg/slope-68 | 3421.2 | — (N/A) | | | 9984 | | 12 % |
| fixmsg/slope-69 | 3475.1 | — (N/A) | | | 10080 | | 13 % |
| fixmsg/slope-132 | 6588.0 | — (N/A) | | | 19224 | | 14 % |
| fixmsg/Order44.parse | 1094.1 | reference-QuickFIXn | 1787.8 | reference | 2920 | 5784 | 6 % |
| fixmsg/Order44.strict | 1175.8 | generated-loaded | 1321.9 | 1.12x | 2952 | 2952 | 10 % |
| fixmsg/Order44.strict | 1175.8 | reference-QuickFIXn | 2835.4 | reference | 2952 | 6808 | 10 % |
| fixmsg/Order.parse | 955.9 | — (N/A) | | | 2864 | | 9 % |
| fixmsg/Order.build | 1111.6 | — (N/A) | | | 3016 | | 8 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 32875.3 | generated | 3645.9 | 0.11x | 62448 | 3280 | 10 % |
| tsql/select-join | 32875.3 | located | 4702.4 | 0.14x | 62448 | 3280 | 10 % |
| tsql/insert-values | 12026.1 | generated | 2501.7 | 0.21x | 48504 | 2360 | 3 % |
| tsql/insert-values | 12026.1 | located | 2945.2 | 0.24x | 48504 | 2360 | 3 % |
| tsql/create-table | 19125.2 | generated | 3687.8 | 0.19x | 55656 | 4608 | 6 % |
| tsql/create-table | 19125.2 | located | 4177.2 | 0.22x | 55656 | 4608 | 6 % |
| tsql/update-subquery | 20314.3 | generated | 2797.8 | 0.14x | 55152 | 2528 | 11 % |
| tsql/update-subquery | 20314.3 | located | 3682.0 | 0.18x | 55152 | 2528 | 11 % |
| tsql/select-long | 101866.4 | generated | 13995.9 | 0.14x | 140872 | 13224 | 12 % |
| tsql/select-long | 101866.4 | located | 18928.3 | 0.19x | 140872 | 13224 | 12 % |
| tsql/comment | 20151.1 | generated | 1529.1 | 0.08x | 53584 | 1072 | 4 % |
| tsql/comment | 20151.1 | located | 2108.3 | 0.10x | 53584 | 1072 | 4 % |

First call in a fresh process, median of three:

| row | reading | ms |
| --- | --- | ---: |
| fix/One.text | hand | 26.00 |
| fix/One.text | generated | 26.40 |
| fix/One.text | regex-lesser | 1.09 |
| fix/One.text | regex-compiled-lesser | 3.91 |
| fix/One.bytes | hand | 26.14 |
| fix/One.bytes | generated | 26.89 |
| el/floor | hand | 11.70 |
| el/floor | tape | 23.15 |
| el/floor | immediate | 22.64 |
| sql/literal | hand | 1.08 |
| sql/literal | generated | 10.88 |
| sql/select20 | hand | 16.60 |
| sql/select20 | generated | 35.98 |
| fixmsg/Order44.strict | generated | 41.43 |
| fixmsg/Order44.strict | generated-loaded | 792.20 |
| fixmsg/Order44.strict | reference-QuickFIXn | 28.26 |

**`fixmsg/Order44.strict`, `reference-QuickFIXn`: its first call includes building the data dictionary (`new DataDictionary(path)` reads FIX44.xml, about a megabyte), and it is the cost of starting to use that reader, paid once per process and not per message.** An application that reads a million messages spreads it to nothing; dividing this number by ours reads as "a hundred times faster" and is the wrong quantity. The generated reading's first call builds no dictionary: its dictionary is compiled into the code and its 447 slots are filled as they are asked.

Held while 2,000,000 FIX fields are read lazily from a stream:

- hand: 4.5 KB above the floor, 2,500,000 fields
- generated: 4.4 KB above the floor, 2,500,000 fields

What the generator took, from the last build's reports:

None found: the last build compiled no grammar. Rebuild the projects (`-t:Rebuild`, or `--stand --rebuild`) to have them.

Generator time against the previous base, as history: the milliseconds move with the machine (the morning's base, rebuilt that evening, read 22-39% higher), so the gate to quote is `benchmarks/Gate-Generation.ps1`, which rebuilds the base alternately with the head in one run and holds their ratio:

Nothing to compare: this run has no generator report. Use `--rebuild`.
