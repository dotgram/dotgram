# Stand, 841ce7c7, 2026-09-19 14:55

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.4 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 112.5 | generated | 101.8 | 0.90x | 288 | 168 | 6 % |
| fix/One.text | 112.5 | regex-lesser | 174.8 | 1.55x | 288 | 680 | 6 % |
| fix/One.text | 112.5 | regex-compiled-lesser | 139.6 | 1.24x | 288 | 680 | 6 % |
| fix/One.bytes | 113.3 | generated | 253.7 | 2.24x | 288 | 224 | 5 % |
| fix/One.stream | 186.1 | generated | 371.7 | 2.00x | 4504 | 408 | 5 % |
| fix/Order.text | 1010.5 | generated | 906.9 | 0.90x | 952 | 832 | 4 % |
| fix/Order.text | 1010.5 | regex-lesser | 1618.3 | 1.60x | 952 | 5960 | 4 % |
| fix/Order.text | 1010.5 | regex-compiled-lesser | 1291.4 | 1.28x | 952 | 5960 | 4 % |
| fix/Order.bytes | 1016.8 | generated | 2314.5 | 2.28x | 952 | 888 | 1 % |
| fix/Order.stream | 1147.9 | generated | 3231.8 | 2.82x | 5088 | 992 | 2 % |
| fix/BinaryMany.text | 5484.4 | generated | 4428.8 | 0.81x | 5840 | 5720 | 4 % |
| fix/BinaryMany.bytes | 5684.6 | generated | 14678.2 | 2.58x | 5840 | 5776 | 20 % |
| fix/BinaryMany.stream | 6452.5 | generated | 23539.3 | 3.65x | 9552 | 5456 | 2 % |
| fix/Orders128.text | 121444.0 | generated | 113875.3 | 0.94x | 95440 | 95320 | 5 % |
| fix/Orders128.text | 121444.0 | regex-lesser | 194977.8 | 1.61x | 95440 | 742728 | 5 % |
| fix/Orders128.text | 121444.0 | regex-compiled-lesser | 151059.7 | 1.24x | 95440 | 742728 | 5 % |
| fix/Orders128.bytes | 127230.5 | generated | 289171.8 | 2.27x | 95440 | 95376 | 13 % |
| fix/Orders128.stream | 163104.7 | generated | 394638.1 | 2.42x | 88400 | 84304 | 3 % |
| fix/OrderMalformed.text | 1014.6 | generated | 937.4 | 0.92x | 1008 | 984 | 7 % |
| fix/OrderMalformed.bytes | 1079.3 | generated | 2377.1 | 2.20x | 1008 | 1040 | 9 % |
| fix/OrderMalformed.stream | 1146.5 | generated | 3218.0 | 2.81x | 5144 | 1144 | 2 % |
| fix/slope-0.text | 39.4 | generated | 32.6 | 0.83x | 152 | 32 | 3 % |
| fix/slope-0.text | 39.4 | ideal | 20.3 | 0.52x | 152 | 88 | 3 % |
| fix/slope-0.text | 39.4 | regex-lesser | 39.1 | 0.99x | 152 | 120 | 3 % |
| fix/slope-0.text | 39.4 | regex-compiled-lesser | 38.0 | 0.97x | 152 | 120 | 3 % |
| fix/slope-1.text | 134.4 | generated | 121.1 | 0.90x | 328 | 208 | 5 % |
| fix/slope-1.text | 134.4 | ideal | 77.5 | 0.58x | 328 | 264 | 5 % |
| fix/slope-1.text | 134.4 | regex-lesser | 173.8 | 1.29x | 328 | 680 | 5 % |
| fix/slope-1.text | 134.4 | regex-compiled-lesser | 140.0 | 1.04x | 328 | 680 | 5 % |
| fix/slope-2.text | 230.6 | generated | 207.7 | 0.90x | 464 | 344 | 12 % |
| fix/slope-2.text | 230.6 | ideal | 128.0 | 0.56x | 464 | 400 | 12 % |
| fix/slope-2.text | 230.6 | regex-lesser | 300.6 | 1.30x | 464 | 1184 | 12 % |
| fix/slope-2.text | 230.6 | regex-compiled-lesser | 237.1 | 1.03x | 464 | 1184 | 12 % |
| fix/slope-4.text | 410.8 | generated | 365.6 | 0.89x | 648 | 528 | 15 % |
| fix/slope-4.text | 410.8 | ideal | 225.2 | 0.55x | 648 | 584 | 15 % |
| fix/slope-4.text | 410.8 | regex-lesser | 610.4 | 1.49x | 648 | 2192 | 15 % |
| fix/slope-4.text | 410.8 | regex-compiled-lesser | 474.6 | 1.16x | 648 | 2192 | 15 % |
| fix/slope-8.text | 743.1 | generated | 670.4 | 0.90x | 1024 | 904 | 4 % |
| fix/slope-8.text | 743.1 | ideal | 404.2 | 0.54x | 1024 | 872 | 4 % |
| fix/slope-8.text | 743.1 | regex-lesser | 1207.1 | 1.62x | 1024 | 4296 | 4 % |
| fix/slope-8.text | 743.1 | regex-compiled-lesser | 959.9 | 1.29x | 1024 | 4296 | 4 % |
| fix/slope-16.text | 1482.0 | generated | 1254.9 | 0.85x | 1776 | 1656 | 5 % |
| fix/slope-16.text | 1482.0 | ideal | 787.8 | 0.53x | 1776 | 1712 | 5 % |
| fix/slope-16.text | 1482.0 | regex-lesser | 2331.9 | 1.57x | 1776 | 8480 | 5 % |
| fix/slope-16.text | 1482.0 | regex-compiled-lesser | 1844.2 | 1.24x | 1776 | 8480 | 5 % |
| fix/slope-0.bytes | 50.2 | generated | 104.3 | 2.08x | 176 | 136 | 3 % |
| fix/slope-1.bytes | 147.5 | generated | 294.0 | 1.99x | 360 | 296 | 7 % |
| fix/slope-2.bytes | 251.8 | generated | 490.4 | 1.95x | 504 | 440 | 4 % |
| fix/slope-4.bytes | 436.6 | generated | 871.4 | 2.00x | 704 | 640 | 10 % |
| fix/slope-8.bytes | 772.2 | generated | 1583.2 | 2.05x | 1104 | 1040 | 7 % |
| fix/slope-16.bytes | 1558.9 | generated | 3020.5 | 1.94x | 1912 | 1848 | 4 % |
| fix/slope-0.stream | 103.7 | generated | 93.3 | 0.90x | 4456 | 360 | 11 % |
| fix/slope-1.stream | 212.2 | generated | 403.3 | 1.90x | 4576 | 480 | 3 % |
| fix/slope-2.stream | 317.9 | generated | 711.4 | 2.24x | 4712 | 616 | 6 % |
| fix/slope-4.stream | 533.6 | generated | 1273.7 | 2.39x | 4896 | 800 | 3 % |
| fix/slope-8.stream | 921.7 | generated | 2391.2 | 2.59x | 5264 | 1168 | 4 % |
| fix/slope-16.stream | 2039.8 | generated | 5339.6 | 2.62x | 6008 | 1912 | 37 % |
| web/url.plain | 211.1 | generated | 267.8 | 1.27x | 152 | 296 | 31 % |
| web/url.plain | 211.1 | regex | 1148.0 | 5.44x | 152 | 1064 | 31 % |
| web/url.plain | 211.1 | regex-compiled | 559.0 | 2.65x | 152 | 1064 | 31 % |
| web/url.full | 324.5 | generated | 354.9 | 1.09x | 328 | 472 | 21 % |
| web/url.full | 324.5 | regex | 1068.7 | 3.29x | 328 | 1336 | 21 % |
| web/url.full | 324.5 | regex-compiled | 492.8 | 1.52x | 328 | 1336 | 21 % |
| web/url.ipv4 | 230.2 | generated | 281.5 | 1.22x | 176 | 320 | 34 % |
| web/url.ipv4 | 230.2 | regex | 1085.7 | 4.72x | 176 | 1088 | 34 % |
| web/url.ipv4 | 230.2 | regex-compiled | 550.8 | 2.39x | 176 | 1088 | 34 % |
| web/url.long-path | 434.6 | generated | 441.8 | 1.02x | 304 | 448 | 4 % |
| web/url.long-path | 434.6 | regex | 1878.7 | 4.32x | 304 | 1216 | 4 % |
| web/url.long-path | 434.6 | regex-compiled | 614.7 | 1.41x | 304 | 1216 | 4 % |
| web/url.refused | 113.5 | generated | 612.1 | 5.39x | 0 | 88 | 7 % |
| web/url.refused | 113.5 | regex | 616.9 | 5.43x | 0 | 0 | 7 % |
| web/url.refused | 113.5 | regex-compiled | 124.0 | 1.09x | 0 | 0 | 7 % |
| web/json.object | 667.2 | generated | 1066.8 | 1.60x | 1976 | 2520 | 4 % |
| web/json.object | 667.2 | system-text-json | 585.1 | 0.88x | 1976 | 72 | 4 % |
| web/json.array | 748.3 | generated | 996.8 | 1.33x | 2112 | 2336 | 13 % |
| web/json.array | 748.3 | system-text-json | 768.0 | 1.03x | 2112 | 72 | 13 % |
| web/date-time.utc | 32.9 | generated | 74.5 | 2.26x | 112 | 112 | 8 % |
| web/date-time.utc | 32.9 | regex | 530.2 | 16.11x | 112 | 1192 | 8 % |
| web/date-time.utc | 32.9 | regex-compiled | 437.6 | 13.29x | 112 | 1192 | 8 % |
| web/date-time.offset | 42.8 | generated | 94.8 | 2.22x | 144 | 144 | 14 % |
| web/date-time.offset | 42.8 | regex | 618.8 | 14.47x | 144 | 1240 | 14 % |
| web/date-time.offset | 42.8 | regex-compiled | 501.2 | 11.72x | 144 | 1240 | 14 % |
| web/date-time.refused | 22.1 | generated | 92.4 | 4.19x | 32 | 64 | 5 % |
| web/date-time.refused | 22.1 | regex | 26.9 | 1.22x | 32 | 0 | 5 % |
| web/date-time.refused | 22.1 | regex-compiled | 26.2 | 1.19x | 32 | 0 | 5 % |
| feeds/stock-count.small.text | 314.5 | generated | 241.9 | 0.77x | 880 | 464 | 52 % |
| feeds/stock-count.small.reader | 576.9 | generated | 807.5 | 1.40x | 8992 | 568 | 38 % |
| feeds/stock-count.small.reader64 | 431.2 | generated | 641.1 | 1.49x | 928 | 568 | 30 % |
| feeds/stock-count.good.text | 94035.9 | generated | 53953.4 | 0.57x | 197664 | 111168 | 46 % |
| feeds/stock-count.good.reader | 115734.9 | generated | 160838.2 | 1.39x | 183440 | 111272 | 7 % |
| feeds/stock-count.good.reader64 | 114282.0 | generated | 134046.1 | 1.17x | 175376 | 111272 | 41 % |
| feeds/stock-count.broken.text | 68412.6 | generated | 46881.2 | 0.69x | 187584 | 107264 | 11 % |
| feeds/stock-count.broken.reader | 67731.1 | generated | 105029.7 | 1.55x | 174800 | 107368 | 15 % |
| feeds/stock-count.broken.reader64 | 69572.4 | generated | 106708.2 | 1.53x | 166736 | 107368 | 4 % |
| el/floor | 489.5 | tape | 1047.1 | 2.14x | 800 | 720 | 49 % |
| el/floor | 489.5 | immediate | 583.5 | 1.19x | 800 | 672 | 49 % |
| el/ladder | 1556.9 | tape | 2743.1 | 1.76x | 1264 | 1168 | 8 % |
| el/ladder | 1556.9 | immediate | 1796.8 | 1.15x | 1264 | 1048 | 8 % |
| el/nest7 | 1253.5 | tape | 2367.6 | 1.89x | 800 | 768 | 5 % |
| el/nest7 | 1253.5 | immediate | 1514.3 | 1.21x | 800 | 672 | 5 % |
| el/block | 1549.1 | tape | 2609.8 | 1.68x | 1880 | 1832 | 24 % |
| el/block | 1549.1 | immediate | 1638.3 | 1.06x | 1880 | 1736 | 24 % |
| el/loop | 3357.3 | tape | 5662.2 | 1.69x | 3360 | 4008 | 20 % |
| el/loop | 3357.3 | immediate | 3949.9 | 1.18x | 3360 | 3664 | 20 % |
| el/overloads | 3332.1 | tape | 4319.2 | 1.30x | 3800 | 3928 | 46 % |
| el/overloads | 3332.1 | immediate | 3416.1 | 1.03x | 3800 | 3816 | 46 % |
| el/string | 397.1 | tape | 1144.3 | 2.88x | 1080 | 864 | 5 % |
| el/string | 397.1 | immediate | 740.2 | 1.86x | 1080 | 840 | 5 % |
| el/interpolation | 2041.2 | tape | 4845.4 | 2.37x | 1888 | 1792 | 21 % |
| el/interpolation | 2041.2 | immediate | 3885.9 | 1.90x | 1888 | 1720 | 21 % |
| el/untyped | 23317.8 | tape | 23465.3 | 1.01x | 15976 | 16072 | 15 % |
| el/untyped | 23317.8 | immediate | 22934.0 | 0.98x | 15976 | 15856 | 15 % |
| el/refused-early | 696.2 | tape | 1756.0 | 2.52x | 800 | 584 | 23 % |
| el/refused-early | 696.2 | immediate | 1556.5 | 2.24x | 800 | 1208 | 23 % |
| el/refused-late | 2229.6 | tape | 2589.5 | 1.16x | 2008 | 584 | 30 % |
| el/refused-late | 2229.6 | immediate | 4856.9 | 2.18x | 2008 | 3192 | 30 % |
| sql/literal | 36.6 | generated | 152.4 | 4.16x | 40 | 40 | 10 % |
| sql/column | 178.7 | generated | 1254.3 | 7.02x | 392 | 272 | 24 % |
| sql/arithmetic | 2323.8 | generated | 15034.1 | 6.47x | 4208 | 3352 | 15 % |
| sql/nest8 | 3883.7 | generated | 37133.0 | 9.56x | 5976 | 6384 | 29 % |
| sql/condition | 2601.6 | generated | 14865.1 | 5.71x | 5696 | 4808 | 44 % |
| sql/select1 | 919.5 | generated | 5944.2 | 6.46x | 1576 | 1568 | 6 % |
| sql/select20 | 10937.5 | generated | 79767.1 | 7.29x | 23976 | 21328 | 9 % |
| sql/values | 698.6 | generated | 6741.4 | 9.65x | 1648 | 1784 | 9 % |
| sql/comment | 2936.2 | generated | 18719.2 | 6.38x | 5408 | 5016 | 19 % |
| sql/conditions100 | 78103.2 | generated | 464770.5 | 5.95x | 188160 | 161616 | 23 % |
| sql/conditions1000 | 741478.1 | generated | 4501618.8 | 6.07x | 1872904 | 1616016 | 15 % |
| sql/create | 1028.0 | generated | 4929.4 | 4.80x | 1584 | 1448 | 9 % |
| sql/refused-late | 3969.8 | generated | 47859.0 | 12.06x | 8376 | 13432 | 14 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/addr-spec.plain | 117.6 | regex | 228.2 | 1.94x | 112 | 584 | 3 % |
| web/addr-spec.plain | 117.6 | regex-compiled | 157.6 | 1.34x | 112 | 584 | 3 % |
| web/addr-spec.tagged | 126.1 | regex | 261.1 | 2.07x | 144 | 616 | 5 % |
| web/addr-spec.tagged | 126.1 | regex-compiled | 169.8 | 1.35x | 144 | 616 | 5 % |
| web/addr-spec.refused | 146.6 | regex | 199.5 | 1.36x | 88 | 0 | 8 % |
| web/addr-spec.refused | 146.6 | regex-compiled | 56.8 | 0.39x | 88 | 0 | 8 % |
| web/media-type.plain | 192.0 | regex | 356.9 | 1.86x | 384 | 952 | 4 % |
| web/media-type.plain | 192.0 | regex-compiled | 282.0 | 1.47x | 384 | 952 | 4 % |
| web/media-type.quoted | 312.4 | regex | 587.2 | 1.88x | 752 | 1360 | 3 % |
| web/media-type.quoted | 312.4 | regex-compiled | 376.4 | 1.20x | 752 | 1360 | 3 % |
| web/media-type.refused | 172.3 | regex | 72.9 | 0.42x | 0 | 0 | 9 % |
| web/media-type.refused | 172.3 | regex-compiled | 33.6 | 0.20x | 0 | 0 | 9 % |
| web/cookie.full | 378.4 | — (N/A) | | | 2072 | | 2 % |
| web/cookie.short | 67.4 | — (N/A) | | | 272 | | 7 % |
| web/pointer.full | 268.9 | — (N/A) | | | 376 | | 4 % |
| web/pointer.short | 47.0 | — (N/A) | | | 80 | | 4 % |
| web/sf.item | 342.3 | — (N/A) | | | 736 | | 6 % |
| web/sf.list | 1344.6 | — (N/A) | | | 3000 | | 11 % |
| web/sf.dictionary | 1429.6 | — (N/A) | | | 3216 | | 2 % |
| fixmsg/Order.parse | 2926.8 | — (N/A) | | | 3360 | | 15 % |
| fixmsg/Order.build | 3391.9 | — (N/A) | | | 3512 | | 9 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 52020.9 | generated | 10065.6 | 0.19x | 63160 | 3280 | 6 % |
| tsql/select-join | 52020.9 | located | 14304.6 | 0.27x | 63160 | 3280 | 6 % |
| tsql/insert-values | 22466.0 | generated | 7118.6 | 0.32x | 48544 | 2360 | 5 % |
| tsql/insert-values | 22466.0 | located | 8672.6 | 0.39x | 48544 | 2360 | 5 % |
| tsql/create-table | 34785.2 | generated | 7471.5 | 0.21x | 56216 | 4992 | 7 % |
| tsql/create-table | 34785.2 | located | 8921.9 | 0.26x | 56216 | 4992 | 7 % |
| tsql/update-subquery | 33632.2 | generated | 7502.1 | 0.22x | 55192 | 2528 | 6 % |
| tsql/update-subquery | 33632.2 | located | 10480.7 | 0.31x | 55192 | 2528 | 6 % |
| tsql/select-long | 179570.5 | generated | 41259.6 | 0.23x | 142256 | 13224 | 20 % |
| tsql/select-long | 179570.5 | located | 58523.0 | 0.33x | 142256 | 13224 | 20 % |
| tsql/comment | 49361.7 | generated | 4597.1 | 0.09x | 54128 | 1072 | 11 % |
| tsql/comment | 49361.7 | located | 6787.8 | 0.14x | 54128 | 1072 | 11 % |

First call in a fresh process, median of three:

| row | reading | ms |
| --- | --- | ---: |
| fix/One.text | hand | 4.04 |
| fix/One.text | generated | 4.50 |
| fix/One.text | regex-lesser | 1.41 |
| fix/One.text | regex-compiled-lesser | 4.19 |
| fix/One.bytes | hand | 3.94 |
| fix/One.bytes | generated | 6.07 |
| el/floor | hand | 11.97 |
| el/floor | tape | 23.21 |
| el/floor | immediate | 14.88 |
| sql/literal | hand | 1.10 |
| sql/literal | generated | 13.38 |
| sql/select20 | hand | 16.42 |
| sql/select20 | generated | 57.76 |

Held while 2,000,000 FIX fields are read lazily from a stream:

- hand: 4.4 KB above the floor, 2,000,000 fields
- generated: 4.4 KB above the floor, 2,000,000 fields

What the generator took, from the last build's reports:

| host | rules | MB of C# | ms | mode | written |
| --- | ---: | ---: | ---: | --- | --- |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 1141 | 12.84 | 3388 | lexical | 2026-09-19 14:08 |
| DotGram.Sql.TransactSql.TransactSqlParser | 1141 | 12.63 | 5501 | lexical | 2026-09-19 14:08 |
| DotGram.Sql.Standard.SqlStandardParser | 664 | 7.76 | 4515 | lexical | 2026-09-19 14:08 |
| DotGram.ExpressionLanguage.ExpressionParser | 183 | 1.72 | 908 | lexical | 2026-09-19 14:07 |
| DotGram.ExpressionLanguage.ExpressionParser.Immediate | 183 | 1.50 | 611 | lexical | 2026-09-19 14:07 |
| DotGram.Sql.Standard.Sql92Parser | 96 | 0.81 | 780 | lexical | 2026-09-19 14:08 |
| DotGram.Web.Rfc5322 | 126 | 0.72 | 196 | characters | 2026-09-19 14:08 |
| DotGram.Finance.Fix.FixGrammar | 14 | 0.47 | 286 | characters | 2026-09-19 14:07 |
| DotGram.Examples.Languages.GramGrammar | 62 | 0.46 | 179 | characters | 2026-09-19 14:08 |
| DotGram.Web.Rfc9651 | 30 | 0.24 | 20 | characters | 2026-09-19 14:08 |
| DotGram.Web.Rfc6265 | 30 | 0.21 | 41 | characters | 2026-09-19 14:08 |
| DotGram.Web.Rfc3986 | 36 | 0.21 | 55 | characters | 2026-09-19 14:08 |
| DotGram.Web.Rfc7239 | 23 | 0.17 | 18 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Expressions.Calculator | 12 | 0.16 | 213 | characters | 2026-09-19 14:08 |
| DotGram.Web.Rfc9110 | 15 | 0.14 | 9 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Languages.TokenizedQuery | 16 | 0.13 | 96 | lexical | 2026-09-19 14:08 |
| DotGram.Web.Rfc5646 | 26 | 0.13 | 21 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Languages.SqlReadOnly | 37 | 0.12 | 29 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Languages.Filter | 12 | 0.12 | 11 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Feeds.FeedReader | 9 | 0.11 | 35 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.Links | 16 | 0.10 | 14 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Feeds.StreamingFeedReader | 9 | 0.09 | 11 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Languages.Scoped | 11 | 0.09 | 17 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.JsonParser | 18 | 0.09 | 11 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Languages.Filters | 9 | 0.08 | 22 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.Config | 10 | 0.08 | 16 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Feeds.LoggingFeedReader | 9 | 0.08 | 7 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Feeds.StockCountReader | 7 | 0.08 | 6 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.MetricsLine | 15 | 0.08 | 16 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.XmlParser | 15 | 0.08 | 10 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.Config.Located | 10 | 0.08 | 14 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.IniParser | 15 | 0.07 | 7 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Languages.Selectors | 12 | 0.07 | 15 | characters | 2026-09-19 14:08 |
| DotGram.Web.Rfc8259 | 16 | 0.07 | 7 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.TypedCsv | 10 | 0.07 | 10 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.MarkdownParser | 14 | 0.07 | 8 | characters | 2026-09-19 14:08 |
| DotGram.Web.Rfc8288 | 14 | 0.07 | 7 | characters | 2026-09-19 14:08 |
| DotGram.Web.Rfc6570 | 16 | 0.07 | 7 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.YamlLite | 13 | 0.06 | 6 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Languages.SettingsFile | 10 | 0.06 | 18 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.HttpParser | 10 | 0.06 | 4 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Feeds.RecoveringFeedReader | 9 | 0.06 | 10 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Expressions.LocaleNumber | 5 | 0.06 | 5 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Expressions.ArithmeticTree | 9 | 0.06 | 19 | characters | 2026-09-19 14:08 |
| DotGram.Web.Rfc3339 | 10 | 0.06 | 178 | characters | 2026-09-19 14:08 |
| DotGram.Web.Rfc6266 | 10 | 0.06 | 8 | characters | 2026-09-19 14:08 |
| DotGram.Web.Rfc6901 | 10 | 0.05 | 4 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.FixedWidth | 16 | 0.05 | 9 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.FileNames | 5 | 0.05 | 5 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Languages.SqlDialect | 4 | 0.05 | 11 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Languages.FilterFile | 10 | 0.05 | 9 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Expressions.ClampedExample | 4 | 0.05 | 7 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Languages.Lexemes | 5 | 0.04 | 26 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.FixParser | 9 | 0.04 | 4 | characters | 2026-09-19 14:08 |
| DotGram.Examples.Formats.Netstrings | 5 | 0.04 | 3 | characters | 2026-09-19 14:08 |

Generator time against the previous base, as history: the milliseconds move with the machine (the morning's base, rebuilt that evening, read 22-39% higher), so the gate to quote is `benchmarks/Gate-Generation.ps1`, which rebuilds the base alternately with the head in one run and holds their ratio:

Nothing to compare: the base stand-2026-09-19.json has no generator report. Use `--rebuild`.
