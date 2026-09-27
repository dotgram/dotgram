Median of 5 of 5 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.5, 31.8, 31.5, 31.5, 31.7).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, e44f7ea6+changes, 2026-09-27 01:39

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.5 ns.

A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 70.7 | generated | 53.5 | 0.76x | 264 | 136 | 13 % |
| fix/One.text | 70.7 | regex-lesser | 156.9 | 2.22x | 264 | 680 | 13 % |
| fix/One.text | 70.7 | regex-compiled-lesser | 129.2 | 1.83x | 264 | 680 | 13 % |
| fix/One.bytes | 72.5 | generated | 80.5 | 1.11x | 264 | 192 | 9 % |
| fix/One.stream | 150.7 | generated | 118.0 | 0.78x | 4512 | 392 | 3 % |
| fix/Order.text | 570.5 | generated | 406.3 | 0.71x | 880 | 752 | 9 % |
| fix/Order.text | 570.5 | regex-lesser | 1449.3 | 2.54x | 880 | 5960 | 9 % |
| fix/Order.text | 570.5 | regex-compiled-lesser | 1159.0 | 2.03x | 880 | 5960 | 9 % |
| fix/Order.bytes | 586.5 | generated | 527.5 | 0.90x | 880 | 808 | 6 % |
| fix/Order.stream | 742.7 | generated | 611.0 | 0.82x | 5048 | 928 | 4 % |
| fix/BinaryMany.text | 5096.6 | generated | 4065.4 | 0.80x | 8888 | 8760 | 3 % |
| fix/BinaryMany.bytes | 5373.7 | generated | 5049.5 | 0.94x | 8888 | 8816 | 5 % |
| fix/BinaryMany.stream | 7273.0 | generated | 5465.4 | 0.75x | 12120 | 8000 | 4 % |
| fix/Orders128.text | 68125.3 | generated | 47902.3 | 0.70x | 89272 | 89144 | 3 % |
| fix/Orders128.text | 68125.3 | regex-lesser | 176074.5 | 2.58x | 89272 | 742728 | 3 % |
| fix/Orders128.text | 68125.3 | regex-compiled-lesser | 142388.2 | 2.09x | 89272 | 742728 | 3 % |
| fix/Orders128.bytes | 70865.2 | generated | 62005.7 | 0.87x | 89272 | 89200 | 3 % |
| fix/Orders128.stream | 107953.4 | generated | 67751.8 | 0.63x | 82264 | 78144 | 3 % |
| fix/OrderMalformed.text | 587.2 | generated | 422.2 | 0.72x | 944 | 912 | 7 % |
| fix/OrderMalformed.bytes | 599.0 | generated | 544.0 | 0.91x | 944 | 968 | 8 % |
| fix/OrderMalformed.stream | 760.1 | generated | 617.2 | 0.81x | 5112 | 1088 | 4 % |
| fix/slope-0.text | 29.8 | generated | 18.8 | 0.63x | 160 | 32 | 28 % |
| fix/slope-0.text | 29.8 | ideal | 9.7 | 0.33x | 160 | 88 | 28 % |
| fix/slope-0.text | 29.8 | regex-lesser | 35.9 | 1.21x | 160 | 120 | 28 % |
| fix/slope-0.text | 29.8 | regex-compiled-lesser | 36.4 | 1.22x | 160 | 120 | 28 % |
| fix/slope-1.text | 87.0 | generated | 69.4 | 0.80x | 304 | 176 | 8 % |
| fix/slope-1.text | 87.0 | ideal | 56.9 | 0.65x | 304 | 232 | 8 % |
| fix/slope-1.text | 87.0 | regex-lesser | 156.3 | 1.80x | 304 | 680 | 8 % |
| fix/slope-1.text | 87.0 | regex-compiled-lesser | 128.4 | 1.48x | 304 | 680 | 8 % |
| fix/slope-2.text | 148.5 | generated | 117.9 | 0.79x | 440 | 312 | 4 % |
| fix/slope-2.text | 148.5 | ideal | 101.9 | 0.69x | 440 | 368 | 4 % |
| fix/slope-2.text | 148.5 | regex-lesser | 274.8 | 1.85x | 440 | 1184 | 4 % |
| fix/slope-2.text | 148.5 | regex-compiled-lesser | 217.8 | 1.47x | 440 | 1184 | 4 % |
| fix/slope-4.text | 257.5 | generated | 197.4 | 0.77x | 624 | 496 | 5 % |
| fix/slope-4.text | 257.5 | ideal | 176.2 | 0.68x | 624 | 552 | 5 % |
| fix/slope-4.text | 257.5 | regex-lesser | 523.9 | 2.03x | 624 | 2192 | 5 % |
| fix/slope-4.text | 257.5 | regex-compiled-lesser | 412.5 | 1.60x | 624 | 2192 | 5 % |
| fix/slope-8.text | 455.3 | generated | 355.0 | 0.78x | 1000 | 872 | 3 % |
| fix/slope-8.text | 455.3 | ideal | 311.0 | 0.68x | 1000 | 840 | 3 % |
| fix/slope-8.text | 455.3 | regex-lesser | 1041.5 | 2.29x | 1000 | 4296 | 3 % |
| fix/slope-8.text | 455.3 | regex-compiled-lesser | 851.1 | 1.87x | 1000 | 4296 | 3 % |
| fix/slope-16.text | 888.9 | generated | 663.9 | 0.75x | 1752 | 1624 | 5 % |
| fix/slope-16.text | 888.9 | ideal | 614.9 | 0.69x | 1752 | 1680 | 5 % |
| fix/slope-16.text | 888.9 | regex-lesser | 2050.7 | 2.31x | 1752 | 8480 | 5 % |
| fix/slope-16.text | 888.9 | regex-compiled-lesser | 1648.1 | 1.85x | 1752 | 8480 | 5 % |
| fix/slope-0.bytes | 32.1 | generated | 47.4 | 1.47x | 184 | 112 | 16 % |
| fix/slope-1.bytes | 95.0 | generated | 105.3 | 1.11x | 336 | 264 | 3 % |
| fix/slope-2.bytes | 160.0 | generated | 165.2 | 1.03x | 480 | 408 | 5 % |
| fix/slope-4.bytes | 268.1 | generated | 261.3 | 0.97x | 680 | 608 | 3 % |
| fix/slope-8.bytes | 482.3 | generated | 452.8 | 0.94x | 1080 | 1008 | 3 % |
| fix/slope-16.bytes | 938.6 | generated | 831.5 | 0.89x | 1888 | 1816 | 7 % |
| fix/slope-0.stream | 93.7 | generated | 77.2 | 0.82x | 4464 | 344 | 3 % |
| fix/slope-1.stream | 172.3 | generated | 142.5 | 0.83x | 4584 | 464 | 4 % |
| fix/slope-2.stream | 239.2 | generated | 205.7 | 0.86x | 4720 | 600 | 4 % |
| fix/slope-4.stream | 364.4 | generated | 318.7 | 0.87x | 4904 | 784 | 5 % |
| fix/slope-8.stream | 614.6 | generated | 518.0 | 0.84x | 5272 | 1152 | 3 % |
| fix/slope-16.stream | 1146.7 | generated | 938.4 | 0.82x | 6016 | 1896 | 2 % |
| web/url.plain | 90.3 | generated | 207.6 | 2.30x | 152 | 296 | 2 % |
| web/url.plain | 90.3 | regex | 1097.8 | 12.15x | 152 | 1064 | 2 % |
| web/url.plain | 90.3 | regex-compiled | 425.8 | 4.71x | 152 | 1064 | 2 % |
| web/url.full | 170.3 | generated | 273.0 | 1.60x | 328 | 472 | 6 % |
| web/url.full | 170.3 | regex | 1143.1 | 6.71x | 328 | 1336 | 6 % |
| web/url.full | 170.3 | regex-compiled | 435.7 | 2.56x | 328 | 1336 | 6 % |
| web/url.ipv4 | 109.3 | generated | 212.1 | 1.94x | 176 | 320 | 2 % |
| web/url.ipv4 | 109.3 | regex | 972.6 | 8.90x | 176 | 1088 | 2 % |
| web/url.ipv4 | 109.3 | regex-compiled | 419.6 | 3.84x | 176 | 1088 | 2 % |
| web/url.long-path | 179.3 | generated | 405.2 | 2.26x | 304 | 448 | 4 % |
| web/url.long-path | 179.3 | regex | 2288.0 | 12.76x | 304 | 1216 | 4 % |
| web/url.long-path | 179.3 | regex-compiled | 592.3 | 3.30x | 304 | 1216 | 4 % |
| web/url.refused | 58.4 | generated | 171.7 | 2.94x | 0 | 0 | 3 % |
| web/url.refused | 58.4 | regex | 781.1 | 13.37x | 0 | 0 | 3 % |
| web/url.refused | 58.4 | regex-compiled | 120.1 | 2.06x | 0 | 0 | 3 % |
| web/json.object | 540.8 | generated | 852.3 | 1.58x | 1976 | 2520 | 6 % |
| web/json.object | 540.8 | system-text-json | 494.5 | 0.91x | 1976 | 72 | 6 % |
| web/json.array | 606.8 | generated | 732.6 | 1.21x | 2112 | 2336 | 13 % |
| web/json.array | 606.8 | system-text-json | 589.0 | 0.97x | 2112 | 72 | 13 % |
| web/date-time.utc | 27.6 | generated | 50.4 | 1.83x | 112 | 112 | 3 % |
| web/date-time.utc | 27.6 | regex | 452.6 | 16.42x | 112 | 1192 | 3 % |
| web/date-time.utc | 27.6 | regex-compiled | 360.5 | 13.08x | 112 | 1192 | 3 % |
| web/date-time.offset | 49.9 | generated | 78.3 | 1.57x | 144 | 144 | 1 % |
| web/date-time.offset | 49.9 | regex | 526.0 | 10.54x | 144 | 1240 | 1 % |
| web/date-time.offset | 49.9 | regex-compiled | 416.6 | 8.34x | 144 | 1240 | 1 % |
| web/date-time.refused | 18.4 | generated | 33.7 | 1.83x | 32 | 32 | 1 % |
| web/date-time.refused | 18.4 | regex | 22.7 | 1.23x | 32 | 0 | 1 % |
| web/date-time.refused | 18.4 | regex-compiled | 22.2 | 1.21x | 32 | 0 | 1 % |
| feeds/stock-count.small.text | 169.6 | generated | 152.8 | 0.90x | 880 | 464 | 5 % |
| feeds/stock-count.small.reader | 263.2 | generated | 246.0 | 0.93x | 8992 | 576 | 6 % |
| feeds/stock-count.small.reader64 | 181.7 | generated | 223.5 | 1.23x | 928 | 576 | 4 % |
| feeds/stock-count.good.text | 37964.5 | generated | 29732.3 | 0.78x | 197664 | 111168 | 5 % |
| feeds/stock-count.good.reader | 40755.4 | generated | 36964.5 | 0.91x | 183440 | 111280 | 10 % |
| feeds/stock-count.good.reader64 | 42560.8 | generated | 41569.5 | 0.98x | 175376 | 111280 | 10 % |
| feeds/stock-count.broken.text | 36817.6 | generated | 33890.1 | 0.92x | 187584 | 107264 | 4 % |
| feeds/stock-count.broken.reader | 39747.4 | generated | 38305.6 | 0.96x | 174800 | 107376 | 8 % |
| feeds/stock-count.broken.reader64 | 41297.0 | generated | 42191.6 | 1.02x | 166736 | 107376 | 9 % |
| el/floor | 324.2 | tape | 790.3 | 2.44x | 920 | 864 | 7 % |
| el/floor | 324.2 | immediate | 793.8 | 2.45x | 920 | 864 | 7 % |
| el/ladder | 982.6 | tape | 1926.9 | 1.96x | 1384 | 1480 | 5 % |
| el/ladder | 982.6 | immediate | 1890.5 | 1.92x | 1384 | 1480 | 5 % |
| el/nest7 | 688.8 | tape | 2080.4 | 3.02x | 920 | 1416 | 4 % |
| el/nest7 | 688.8 | immediate | 2035.4 | 2.95x | 920 | 1416 | 4 % |
| el/block | 1020.6 | tape | 1861.5 | 1.82x | 2000 | 2024 | 3 % |
| el/block | 1020.6 | immediate | 1900.7 | 1.86x | 2000 | 2024 | 3 % |
| el/try | 5925.2 | tape | 6672.9 | 1.13x | 6536 | 5848 | 4 % |
| el/try | 5925.2 | immediate | 6679.3 | 1.13x | 6536 | 5848 | 4 % |
| el/loop | 2268.7 | tape | 4152.6 | 1.83x | 3816 | 4584 | 8 % |
| el/loop | 2268.7 | immediate | 4103.6 | 1.81x | 3816 | 4584 | 8 % |
| el/overloads | 4849.8 | tape | 4255.1 | 0.88x | 7136 | 5696 | 3 % |
| el/overloads | 4849.8 | immediate | 4215.5 | 0.87x | 7136 | 5696 | 3 % |
| el/string | 322.3 | tape | 982.7 | 3.05x | 1200 | 1008 | 8 % |
| el/string | 322.3 | immediate | 982.9 | 3.05x | 1200 | 1008 | 8 % |
| el/interpolation | 1354.0 | tape | 3981.7 | 2.94x | 2008 | 2008 | 47 % |
| el/interpolation | 1354.0 | immediate | 4177.5 | 3.09x | 2008 | 2008 | 47 % |
| el/untyped | 25893.5 | tape | 27465.3 | 1.06x | 24376 | 24568 | 19 % |
| el/untyped | 25893.5 | immediate | 28327.7 | 1.09x | 24376 | 24568 | 19 % |
| el/refused-early | 448.8 | tape | 1245.8 | 2.78x | 920 | 704 | 12 % |
| el/refused-early | 448.8 | immediate | 1238.6 | 2.76x | 920 | 704 | 12 % |
| el/refused-late | 1542.9 | tape | 2170.9 | 1.41x | 2128 | 704 | 7 % |
| el/refused-late | 1542.9 | immediate | 2112.6 | 1.37x | 2128 | 704 | 7 % |
| sql/literal | 22.7 | generated | 100.5 | 4.43x | 40 | 40 | 4 % |
| sql/literal | 22.7 | immediate | 88.7 | 3.91x | 40 | 40 | 4 % |
| sql/column | 134.8 | generated | 323.7 | 2.40x | 392 | 272 | 5 % |
| sql/column | 134.8 | immediate | 1859.7 | 13.80x | 392 | 272 | 5 % |
| sql/arithmetic | 1835.0 | generated | 3907.0 | 2.13x | 4208 | 3832 | 3 % |
| sql/arithmetic | 1835.0 | immediate | 12778.8 | 6.96x | 4208 | 3832 | 3 % |
| sql/nest8 | 2170.0 | generated | 5795.6 | 2.67x | 5976 | 6528 | 6 % |
| sql/nest8 | 2170.0 | immediate | 24938.5 | 11.49x | 5976 | 6528 | 6 % |
| sql/condition | 2267.9 | generated | 4470.2 | 1.97x | 5696 | 4680 | 5 % |
| sql/condition | 2267.9 | immediate | 15043.6 | 6.63x | 5696 | 4680 | 5 % |
| sql/select1 | 759.0 | generated | 1542.6 | 2.03x | 1576 | 1584 | 4 % |
| sql/select1 | 759.0 | immediate | 5872.0 | 7.74x | 1576 | 1584 | 4 % |
| sql/select20 | 9253.8 | generated | 21446.5 | 2.32x | 23976 | 21648 | 5 % |
| sql/select20 | 9253.8 | immediate | 73344.0 | 7.93x | 23976 | 21648 | 5 % |
| sql/values | 530.0 | generated | 1593.0 | 3.01x | 1648 | 1816 | 22 % |
| sql/values | 530.0 | immediate | 6995.7 | 13.20x | 1648 | 1816 | 22 % |
| sql/comment | 2499.3 | generated | 5155.1 | 2.06x | 5408 | 5064 | 15 % |
| sql/comment | 2499.3 | immediate | 16999.8 | 6.80x | 5408 | 5064 | 15 % |
| sql/conditions100 | 66874.2 | generated | 132995.6 | 1.99x | 188160 | 153696 | 33 % |
| sql/conditions100 | 66874.2 | immediate | 406387.9 | 6.08x | 188160 | 153696 | 33 % |
| sql/conditions1000 | 706478.1 | generated | 1349255.5 | 1.91x | 1872904 | 1536096 | 17 % |
| sql/conditions1000 | 706478.1 | immediate | 4235335.9 | 5.99x | 1872904 | 1536096 | 17 % |
| sql/create | 893.9 | generated | 2689.7 | 3.01x | 1584 | 1448 | 15 % |
| sql/create | 893.9 | immediate | 5124.1 | 5.73x | 1584 | 1448 | 15 % |
| sql/refused-late | 3401.7 | generated | 13875.0 | 4.08x | 8376 | 13240 | 19 % |
| sql/refused-late | 3401.7 | immediate | 45408.1 | 13.35x | 8376 | 13240 | 19 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/addr-spec.plain | 95.9 | regex | 213.1 | 2.22x | 112 | 584 | 4 % |
| web/addr-spec.plain | 95.9 | regex-compiled | 138.9 | 1.45x | 112 | 584 | 4 % |
| web/addr-spec.plain | 95.9 | reference-MailAddress | 388.8 | reference | 112 | 248 | 4 % |
| web/addr-spec.tagged | 102.7 | regex | 248.6 | 2.42x | 144 | 616 | 4 % |
| web/addr-spec.tagged | 102.7 | regex-compiled | 152.2 | 1.48x | 144 | 616 | 4 % |
| web/addr-spec.tagged | 102.7 | reference-MailAddress | 403.7 | reference | 144 | 312 | 4 % |
| web/addr-spec.refused | 27.2 | regex | 217.8 | 8.01x | 0 | 0 | 16 % |
| web/addr-spec.refused | 27.2 | regex-compiled | 52.9 | 1.95x | 0 | 0 | 16 % |
| web/addr-spec.refused | 27.2 | reference-MailAddress | 354.9 | reference | 0 | 112 | 16 % |
| web/media-type.plain | 169.7 | regex | 318.2 | 1.88x | 384 | 952 | 4 % |
| web/media-type.plain | 169.7 | regex-compiled | 245.4 | 1.45x | 384 | 952 | 4 % |
| web/media-type.plain | 169.7 | reference-MediaTypeHeaderValue | 49.3 | reference | 384 | 208 | 4 % |
| web/media-type.quoted | 267.7 | regex | 600.5 | 2.24x | 752 | 1360 | 6 % |
| web/media-type.quoted | 267.7 | regex-compiled | 336.3 | 1.26x | 752 | 1360 | 6 % |
| web/media-type.quoted | 267.7 | reference-MediaTypeHeaderValue | 82.1 | reference | 752 | 408 | 6 % |
| web/media-type.refused | 46.5 | regex | 69.8 | 1.50x | 0 | 0 | 10 % |
| web/media-type.refused | 46.5 | regex-compiled | 30.8 | 0.66x | 0 | 0 | 10 % |
| web/media-type.refused | 46.5 | reference-MediaTypeHeaderValue | 8.7 | reference | 0 | 0 | 10 % |
| web/cookie.full | 319.9 | reference-CookieContainer | 1003.4 | reference | 2072 | 1968 | 3 % |
| web/cookie.short | 55.2 | reference-CookieContainer | 450.4 | reference | 272 | 1440 | 12 % |
| web/cookie.refused | 30.4 | reference-CookieContainer | 2448.8 | reference | 72 | 1128 | 22 % |
| web/content-disposition.full | 444.6 | — (N/A) | | | 1616 | | 10 % |
| web/content-disposition.refused | 36.3 | — (N/A) | | | 0 | | 16 % |
| web/uri-template.full | 556.4 | — (N/A) | | | 976 | | 3 % |
| web/uri-template.refused | 292.4 | — (N/A) | | | 232 | | 7 % |
| web/json-patch.full | 1431.7 | — (N/A) | | | 4520 | | 3 % |
| web/forwarded.full | 1629.4 | — (N/A) | | | 2584 | | 8 % |
| web/forwarded.refused | 121.4 | — (N/A) | | | 264 | | 3 % |
| web/link.full | 1380.7 | — (N/A) | | | 2904 | | 4 % |
| web/link.refused | 26.7 | — (N/A) | | | 0 | | 19 % |
| web/pointer.full | 202.2 | — (N/A) | | | 376 | | 33 % |
| web/pointer.short | 40.1 | — (N/A) | | | 80 | | 1 % |
| web/sf.item | 276.6 | — (N/A) | | | 736 | | 6 % |
| web/sf.list | 1207.0 | — (N/A) | | | 3000 | | 5 % |
| web/sf.dictionary | 1237.6 | — (N/A) | | | 3216 | | 3 % |
| fixmsg/slope-8 | 432.9 | — (N/A) | | | 1296 | | 4 % |
| fixmsg/slope-12 | 635.9 | — (N/A) | | | 1864 | | 5 % |
| fixmsg/slope-13 | 690.0 | — (N/A) | | | 1952 | | 1 % |
| fixmsg/slope-20 | 1047.1 | — (N/A) | | | 3024 | | 4 % |
| fixmsg/slope-21 | 1098.0 | — (N/A) | | | 3120 | | 3 % |
| fixmsg/slope-36 | 1828.7 | — (N/A) | | | 5352 | | 4 % |
| fixmsg/slope-37 | 1869.3 | — (N/A) | | | 5448 | | 3 % |
| fixmsg/slope-68 | 3368.0 | — (N/A) | | | 9984 | | 3 % |
| fixmsg/slope-69 | 3373.9 | — (N/A) | | | 10080 | | 4 % |
| fixmsg/slope-132 | 6437.4 | — (N/A) | | | 19224 | | 3 % |
| fixmsg/Order44.parse | 1055.9 | reference-QuickFIXn | 1824.3 | reference | 2920 | 5784 | 5 % |
| fixmsg/Order44.strict | 1158.4 | generated-loaded | 1304.3 | 1.13x | 2952 | 2952 | 2 % |
| fixmsg/Order44.strict | 1158.4 | reference-QuickFIXn | 2857.9 | reference | 2952 | 6808 | 2 % |
| fixmsg/Order.parse | 960.8 | — (N/A) | | | 2864 | | 5 % |
| fixmsg/Order.build | 1082.2 | — (N/A) | | | 3016 | | 4 % |
| fixmsg/Order42.parse-string | 1089.1 | — (N/A) | | | 2648 | | 5 % |
| fixmsg/Order50.parse-string | 1706.3 | — (N/A) | | | 4424 | | 6 % |
| fixmsg/Report50.parse-string | 4325.4 | — (N/A) | | | 7976 | | 4 % |
| fixmsg/Order42.strict-string | 1185.8 | — (N/A) | | | 2680 | | 9 % |
| fixmsg/Order50.strict-string | 1903.6 | — (N/A) | | | 4456 | | 4 % |
| fixmsg/Report50.strict-string | 4849.2 | — (N/A) | | | 8008 | | 3 % |
| fixmsg/Order42.build-string | 1185.6 | — (N/A) | | | 2840 | | 3 % |
| fixmsg/Order50.build-string | 1891.7 | — (N/A) | | | 4648 | | 5 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 32544.9 | generated | 3622.1 | 0.11x | 62448 | 3280 | 8 % |
| tsql/select-join | 32544.9 | located | 4726.5 | 0.15x | 62448 | 3280 | 8 % |
| tsql/insert-values | 12398.0 | generated | 2501.7 | 0.20x | 48504 | 2360 | 3 % |
| tsql/insert-values | 12398.0 | located | 2936.8 | 0.24x | 48504 | 2360 | 3 % |
| tsql/create-table | 19446.7 | generated | 3642.5 | 0.19x | 55702 | 4608 | 10 % |
| tsql/create-table | 19446.7 | located | 4106.7 | 0.21x | 55702 | 4608 | 10 % |
| tsql/update-subquery | 20714.6 | generated | 2735.3 | 0.13x | 55152 | 2528 | 7 % |
| tsql/update-subquery | 20714.6 | located | 3600.9 | 0.17x | 55152 | 2528 | 7 % |
| tsql/select-long | 100638.4 | generated | 14059.5 | 0.14x | 140872 | 13224 | 9 % |
| tsql/select-long | 100638.4 | located | 18825.8 | 0.19x | 140872 | 13224 | 9 % |
| tsql/comment | 20057.2 | generated | 1520.8 | 0.08x | 53584 | 1072 | 6 % |
| tsql/comment | 20057.2 | located | 2119.0 | 0.11x | 53584 | 1072 | 6 % |

First call in a fresh process, median of three:

| row | reading | ms |
| --- | --- | ---: |
| fix/One.text | hand | 26.49 |
| fix/One.text | generated | 26.71 |
| fix/One.text | regex-lesser | 1.10 |
| fix/One.text | regex-compiled-lesser | 3.85 |
| fix/One.bytes | hand | 26.72 |
| fix/One.bytes | generated | 26.75 |
| el/floor | hand | 9.61 |
| el/floor | tape | 22.49 |
| el/floor | immediate | 21.52 |
| sql/literal | hand | 1.10 |
| sql/literal | generated | 10.95 |
| sql/literal | immediate | 6.44 |
| sql/select20 | hand | 16.83 |
| sql/select20 | generated | 36.84 |
| sql/select20 | immediate | 40.77 |
| fixmsg/Order44.strict | generated | 41.85 |
| fixmsg/Order44.strict | generated-loaded | 227.53 |
| fixmsg/Order44.strict | reference-QuickFIXn | 28.11 |

**`fixmsg/Order44.strict`, `reference-QuickFIXn`: its first call includes building the data dictionary (`new DataDictionary(path)` reads FIX44.xml, about a megabyte), and it is the cost of starting to use that reader, paid once per process and not per message.** An application that reads a million messages spreads it to nothing; dividing this number by ours reads as "a hundred times faster" and is the wrong quantity. The generated reading's first call builds no dictionary: its dictionary is compiled into the code and its 447 slots are filled as they are asked.

Held while 2,000,000 FIX fields are read lazily from a stream:

- hand: 4.5 KB above the floor, 2,500,000 fields
- generated: 4.4 KB above the floor, 2,500,000 fields

What the generator took, from the last build's reports:

None found: the last build compiled no grammar. Rebuild the projects (`-t:Rebuild`, or `--stand --rebuild`) to have them.

Generator time against the previous base, as history: the milliseconds move with the machine (the morning's base, rebuilt that evening, read 22-39% higher), so the gate to quote is `benchmarks/Gate-Generation.ps1`, which rebuilds the base alternately with the head in one run and holds their ratio:

Nothing to compare: this run has no generator report. Use `--rebuild`.
