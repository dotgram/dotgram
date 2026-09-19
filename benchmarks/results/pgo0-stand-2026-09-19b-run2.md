# Stand, 841ce7c7, 2026-09-19 14:49

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.0 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 113.3 | generated | 104.3 | 0.92x | 288 | 168 | 2 % |
| fix/One.text | 113.3 | regex-lesser | 172.9 | 1.53x | 288 | 680 | 2 % |
| fix/One.text | 113.3 | regex-compiled-lesser | 138.6 | 1.22x | 288 | 680 | 2 % |
| fix/One.bytes | 117.2 | generated | 258.7 | 2.21x | 288 | 224 | 2 % |
| fix/One.stream | 186.6 | generated | 374.9 | 2.01x | 4504 | 408 | 6 % |
| fix/Order.text | 1006.5 | generated | 927.4 | 0.92x | 952 | 832 | 8 % |
| fix/Order.text | 1006.5 | regex-lesser | 1629.7 | 1.62x | 952 | 5960 | 8 % |
| fix/Order.text | 1006.5 | regex-compiled-lesser | 1284.9 | 1.28x | 952 | 5960 | 8 % |
| fix/Order.bytes | 1028.7 | generated | 2345.5 | 2.28x | 952 | 888 | 6 % |
| fix/Order.stream | 1174.8 | generated | 3236.8 | 2.76x | 5088 | 992 | 9 % |
| fix/BinaryMany.text | 5525.5 | generated | 4408.0 | 0.80x | 5840 | 5720 | 5 % |
| fix/BinaryMany.bytes | 5608.9 | generated | 14403.9 | 2.57x | 5840 | 5776 | 3 % |
| fix/BinaryMany.stream | 6357.1 | generated | 23653.5 | 3.72x | 9552 | 5456 | 2 % |
| fix/Orders128.text | 121116.7 | generated | 111099.3 | 0.92x | 95440 | 95320 | 4 % |
| fix/Orders128.text | 121116.7 | regex-lesser | 194366.5 | 1.60x | 95440 | 742728 | 4 % |
| fix/Orders128.text | 121116.7 | regex-compiled-lesser | 152371.6 | 1.26x | 95440 | 742728 | 4 % |
| fix/Orders128.bytes | 123654.8 | generated | 286258.9 | 2.31x | 95440 | 95376 | 10 % |
| fix/Orders128.stream | 162377.9 | generated | 389070.1 | 2.40x | 88400 | 84304 | 3 % |
| fix/OrderMalformed.text | 1001.8 | generated | 919.3 | 0.92x | 1008 | 984 | 8 % |
| fix/OrderMalformed.bytes | 1013.8 | generated | 2298.2 | 2.27x | 1008 | 1040 | 6 % |
| fix/OrderMalformed.stream | 1125.9 | generated | 3183.5 | 2.83x | 5144 | 1144 | 13 % |
| fix/slope-0.text | 39.1 | generated | 32.2 | 0.82x | 152 | 32 | 3 % |
| fix/slope-0.text | 39.1 | ideal | 20.5 | 0.52x | 152 | 88 | 3 % |
| fix/slope-0.text | 39.1 | regex-lesser | 38.8 | 0.99x | 152 | 120 | 3 % |
| fix/slope-0.text | 39.1 | regex-compiled-lesser | 37.7 | 0.96x | 152 | 120 | 3 % |
| fix/slope-1.text | 131.8 | generated | 120.9 | 0.92x | 328 | 208 | 6 % |
| fix/slope-1.text | 131.8 | ideal | 76.2 | 0.58x | 328 | 264 | 6 % |
| fix/slope-1.text | 131.8 | regex-lesser | 172.6 | 1.31x | 328 | 680 | 6 % |
| fix/slope-1.text | 131.8 | regex-compiled-lesser | 138.5 | 1.05x | 328 | 680 | 6 % |
| fix/slope-2.text | 228.5 | generated | 209.6 | 0.92x | 464 | 344 | 4 % |
| fix/slope-2.text | 228.5 | ideal | 127.8 | 0.56x | 464 | 400 | 4 % |
| fix/slope-2.text | 228.5 | regex-lesser | 303.9 | 1.33x | 464 | 1184 | 4 % |
| fix/slope-2.text | 228.5 | regex-compiled-lesser | 235.0 | 1.03x | 464 | 1184 | 4 % |
| fix/slope-4.text | 402.2 | generated | 358.7 | 0.89x | 648 | 528 | 5 % |
| fix/slope-4.text | 402.2 | ideal | 221.8 | 0.55x | 648 | 584 | 5 % |
| fix/slope-4.text | 402.2 | regex-lesser | 598.0 | 1.49x | 648 | 2192 | 5 % |
| fix/slope-4.text | 402.2 | regex-compiled-lesser | 461.0 | 1.15x | 648 | 2192 | 5 % |
| fix/slope-8.text | 731.5 | generated | 646.7 | 0.88x | 1024 | 904 | 9 % |
| fix/slope-8.text | 731.5 | ideal | 394.4 | 0.54x | 1024 | 872 | 9 % |
| fix/slope-8.text | 731.5 | regex-lesser | 1195.2 | 1.63x | 1024 | 4296 | 9 % |
| fix/slope-8.text | 731.5 | regex-compiled-lesser | 935.0 | 1.28x | 1024 | 4296 | 9 % |
| fix/slope-16.text | 1460.9 | generated | 1249.7 | 0.86x | 1776 | 1656 | 8 % |
| fix/slope-16.text | 1460.9 | ideal | 778.0 | 0.53x | 1776 | 1712 | 8 % |
| fix/slope-16.text | 1460.9 | regex-lesser | 2315.2 | 1.58x | 1776 | 8480 | 8 % |
| fix/slope-16.text | 1460.9 | regex-compiled-lesser | 1799.4 | 1.23x | 1776 | 8480 | 8 % |
| fix/slope-0.bytes | 49.5 | generated | 102.1 | 2.06x | 176 | 136 | 1 % |
| fix/slope-1.bytes | 147.4 | generated | 289.2 | 1.96x | 360 | 296 | 6 % |
| fix/slope-2.bytes | 252.0 | generated | 491.7 | 1.95x | 504 | 440 | 27 % |
| fix/slope-4.bytes | 436.5 | generated | 878.5 | 2.01x | 704 | 640 | 7 % |
| fix/slope-8.bytes | 787.4 | generated | 1633.6 | 2.07x | 1104 | 1040 | 10 % |
| fix/slope-16.bytes | 1593.2 | generated | 3092.7 | 1.94x | 1912 | 1848 | 9 % |
| fix/slope-0.stream | 111.9 | generated | 95.6 | 0.85x | 4456 | 360 | 5 % |
| fix/slope-1.stream | 224.1 | generated | 431.9 | 1.93x | 4576 | 480 | 7 % |
| fix/slope-2.stream | 326.9 | generated | 719.9 | 2.20x | 4712 | 616 | 3 % |
| fix/slope-4.stream | 554.0 | generated | 1294.5 | 2.34x | 4896 | 800 | 4 % |
| fix/slope-8.stream | 940.1 | generated | 2407.3 | 2.56x | 5264 | 1168 | 3 % |
| fix/slope-16.stream | 1859.2 | generated | 4784.9 | 2.57x | 6008 | 1912 | 10 % |
| web/url.plain | 184.0 | generated | 218.5 | 1.19x | 152 | 296 | 2 % |
| web/url.plain | 184.0 | regex | 994.9 | 5.41x | 152 | 1064 | 2 % |
| web/url.plain | 184.0 | regex-compiled | 495.2 | 2.69x | 152 | 1064 | 2 % |
| web/url.full | 291.2 | generated | 317.3 | 1.09x | 328 | 472 | 6 % |
| web/url.full | 291.2 | regex | 969.1 | 3.33x | 328 | 1336 | 6 % |
| web/url.full | 291.2 | regex-compiled | 477.3 | 1.64x | 328 | 1336 | 6 % |
| web/url.ipv4 | 198.8 | generated | 218.1 | 1.10x | 176 | 320 | 15 % |
| web/url.ipv4 | 198.8 | regex | 912.1 | 4.59x | 176 | 1088 | 15 % |
| web/url.ipv4 | 198.8 | regex-compiled | 473.5 | 2.38x | 176 | 1088 | 15 % |
| web/url.long-path | 437.2 | generated | 428.7 | 0.98x | 304 | 448 | 3 % |
| web/url.long-path | 437.2 | regex | 1846.4 | 4.22x | 304 | 1216 | 3 % |
| web/url.long-path | 437.2 | regex-compiled | 616.8 | 1.41x | 304 | 1216 | 3 % |
| web/url.refused | 111.1 | generated | 595.7 | 5.36x | 0 | 88 | 3 % |
| web/url.refused | 111.1 | regex | 619.9 | 5.58x | 0 | 0 | 3 % |
| web/url.refused | 111.1 | regex-compiled | 123.3 | 1.11x | 0 | 0 | 3 % |
| web/json.object | 650.4 | generated | 1036.5 | 1.59x | 1976 | 2520 | 2 % |
| web/json.object | 650.4 | system-text-json | 569.9 | 0.88x | 1976 | 72 | 2 % |
| web/json.array | 766.3 | generated | 976.4 | 1.27x | 2112 | 2336 | 15 % |
| web/json.array | 766.3 | system-text-json | 748.2 | 0.98x | 2112 | 72 | 15 % |
| web/date-time.utc | 31.8 | generated | 72.0 | 2.26x | 112 | 112 | 6 % |
| web/date-time.utc | 31.8 | regex | 509.1 | 15.98x | 112 | 1192 | 6 % |
| web/date-time.utc | 31.8 | regex-compiled | 419.9 | 13.18x | 112 | 1192 | 6 % |
| web/date-time.offset | 43.8 | generated | 99.1 | 2.26x | 144 | 144 | 12 % |
| web/date-time.offset | 43.8 | regex | 583.4 | 13.31x | 144 | 1240 | 12 % |
| web/date-time.offset | 43.8 | regex-compiled | 485.1 | 11.06x | 144 | 1240 | 12 % |
| web/date-time.refused | 21.5 | generated | 91.6 | 4.26x | 32 | 64 | 10 % |
| web/date-time.refused | 21.5 | regex | 25.9 | 1.21x | 32 | 0 | 10 % |
| web/date-time.refused | 21.5 | regex-compiled | 26.1 | 1.22x | 32 | 0 | 10 % |
| feeds/stock-count.small.text | 259.3 | generated | 174.3 | 0.67x | 880 | 464 | 9 % |
| feeds/stock-count.small.reader | 350.5 | generated | 451.6 | 1.29x | 8992 | 568 | 5 % |
| feeds/stock-count.small.reader64 | 263.8 | generated | 431.5 | 1.64x | 928 | 568 | 2 % |
| feeds/stock-count.good.text | 68012.8 | generated | 39617.0 | 0.58x | 197664 | 111168 | 6 % |
| feeds/stock-count.good.reader | 68089.6 | generated | 98270.7 | 1.44x | 183440 | 111272 | 1 % |
| feeds/stock-count.good.reader64 | 69955.9 | generated | 97674.3 | 1.40x | 175376 | 111272 | 5 % |
| feeds/stock-count.broken.text | 65614.2 | generated | 44356.0 | 0.68x | 187584 | 107264 | 6 % |
| feeds/stock-count.broken.reader | 65267.7 | generated | 102583.0 | 1.57x | 174800 | 107368 | 2 % |
| feeds/stock-count.broken.reader64 | 68202.4 | generated | 103592.3 | 1.52x | 166736 | 107368 | 7 % |
| el/floor | 476.6 | tape | 1025.8 | 2.15x | 800 | 720 | 5 % |
| el/floor | 476.6 | immediate | 552.6 | 1.16x | 800 | 672 | 5 % |
| el/ladder | 1416.1 | tape | 2513.8 | 1.78x | 1264 | 1168 | 8 % |
| el/ladder | 1416.1 | immediate | 1677.6 | 1.18x | 1264 | 1048 | 8 % |
| el/nest7 | 1179.1 | tape | 2308.6 | 1.96x | 800 | 768 | 5 % |
| el/nest7 | 1179.1 | immediate | 1446.7 | 1.23x | 800 | 672 | 5 % |
| el/block | 1441.5 | tape | 2461.5 | 1.71x | 1880 | 1832 | 9 % |
| el/block | 1441.5 | immediate | 1585.3 | 1.10x | 1880 | 1736 | 9 % |
| el/loop | 3176.5 | tape | 5480.7 | 1.73x | 3360 | 4008 | 10 % |
| el/loop | 3176.5 | immediate | 3722.4 | 1.17x | 3360 | 3664 | 10 % |
| el/overloads | 3043.1 | tape | 4039.4 | 1.33x | 3800 | 3928 | 3 % |
| el/overloads | 3043.1 | immediate | 3181.6 | 1.05x | 3800 | 3816 | 3 % |
| el/string | 390.1 | tape | 1082.9 | 2.78x | 1080 | 864 | 6 % |
| el/string | 390.1 | immediate | 656.9 | 1.68x | 1080 | 840 | 6 % |
| el/interpolation | 1808.6 | tape | 4415.7 | 2.44x | 1888 | 1792 | 24 % |
| el/interpolation | 1808.6 | immediate | 3532.2 | 1.95x | 1888 | 1720 | 24 % |
| el/untyped | 20489.6 | tape | 23258.8 | 1.14x | 15976 | 16072 | 10 % |
| el/untyped | 20489.6 | immediate | 21341.3 | 1.04x | 15976 | 15856 | 10 % |
| el/refused-early | 550.5 | tape | 1512.0 | 2.75x | 800 | 584 | 6 % |
| el/refused-early | 550.5 | immediate | 1284.5 | 2.33x | 800 | 1208 | 6 % |
| el/refused-late | 1872.6 | tape | 2308.1 | 1.23x | 2008 | 584 | 8 % |
| el/refused-late | 1872.6 | immediate | 3859.7 | 2.06x | 2008 | 3192 | 8 % |
| sql/literal | 33.3 | generated | 140.4 | 4.21x | 40 | 40 | 11 % |
| sql/column | 145.2 | generated | 1182.1 | 8.14x | 392 | 272 | 5 % |
| sql/arithmetic | 2139.0 | generated | 14284.8 | 6.68x | 4208 | 3352 | 27 % |
| sql/nest8 | 3571.8 | generated | 36778.3 | 10.30x | 5976 | 6384 | 7 % |
| sql/condition | 2425.2 | generated | 14972.7 | 6.17x | 5696 | 4808 | 17 % |
| sql/select1 | 879.3 | generated | 5972.1 | 6.79x | 1576 | 1568 | 10 % |
| sql/select20 | 10047.1 | generated | 78118.7 | 7.78x | 23976 | 21328 | 7 % |
| sql/values | 658.3 | generated | 6652.0 | 10.10x | 1648 | 1784 | 14 % |
| sql/comment | 2680.9 | generated | 18187.8 | 6.78x | 5408 | 5016 | 17 % |
| sql/conditions100 | 70101.5 | generated | 439072.7 | 6.26x | 188160 | 161616 | 11 % |
| sql/conditions1000 | 687950.8 | generated | 4434712.5 | 6.45x | 1872904 | 1616016 | 29 % |
| sql/create | 978.7 | generated | 4771.6 | 4.88x | 1584 | 1448 | 14 % |
| sql/refused-late | 3616.7 | generated | 46706.4 | 12.91x | 8376 | 13432 | 14 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/addr-spec.plain | 113.6 | regex | 227.7 | 2.01x | 112 | 584 | 1 % |
| web/addr-spec.plain | 113.6 | regex-compiled | 160.0 | 1.41x | 112 | 584 | 1 % |
| web/addr-spec.tagged | 122.4 | regex | 257.6 | 2.11x | 144 | 616 | 6 % |
| web/addr-spec.tagged | 122.4 | regex-compiled | 169.6 | 1.39x | 144 | 616 | 6 % |
| web/addr-spec.refused | 144.3 | regex | 197.5 | 1.37x | 88 | 0 | 11 % |
| web/addr-spec.refused | 144.3 | regex-compiled | 55.5 | 0.38x | 88 | 0 | 11 % |
| web/media-type.plain | 189.9 | regex | 364.3 | 1.92x | 384 | 952 | 2 % |
| web/media-type.plain | 189.9 | regex-compiled | 291.9 | 1.54x | 384 | 952 | 2 % |
| web/media-type.quoted | 312.3 | regex | 593.8 | 1.90x | 752 | 1360 | 7 % |
| web/media-type.quoted | 312.3 | regex-compiled | 380.2 | 1.22x | 752 | 1360 | 7 % |
| web/media-type.refused | 172.9 | regex | 74.7 | 0.43x | 0 | 0 | 2 % |
| web/media-type.refused | 172.9 | regex-compiled | 34.9 | 0.20x | 0 | 0 | 2 % |
| web/cookie.full | 388.9 | — (N/A) | | | 2072 | | 2 % |
| web/cookie.short | 67.9 | — (N/A) | | | 272 | | 3 % |
| web/pointer.full | 268.1 | — (N/A) | | | 376 | | 3 % |
| web/pointer.short | 47.2 | — (N/A) | | | 80 | | 1 % |
| web/sf.item | 327.4 | — (N/A) | | | 736 | | 1 % |
| web/sf.list | 1362.3 | — (N/A) | | | 3000 | | 16 % |
| web/sf.dictionary | 1445.1 | — (N/A) | | | 3216 | | 5 % |
| fixmsg/Order.parse | 2788.6 | — (N/A) | | | 3360 | | 4 % |
| fixmsg/Order.build | 3254.8 | — (N/A) | | | 3512 | | 4 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 51163.7 | generated | 10356.0 | 0.20x | 63160 | 3280 | 6 % |
| tsql/select-join | 51163.7 | located | 14153.7 | 0.28x | 63160 | 3280 | 6 % |
| tsql/insert-values | 22484.4 | generated | 7075.5 | 0.31x | 48544 | 2360 | 3 % |
| tsql/insert-values | 22484.4 | located | 8641.4 | 0.38x | 48544 | 2360 | 3 % |
| tsql/create-table | 35339.9 | generated | 7722.7 | 0.22x | 56216 | 4992 | 7 % |
| tsql/create-table | 35339.9 | located | 9102.6 | 0.26x | 56216 | 4992 | 7 % |
| tsql/update-subquery | 34333.7 | generated | 7766.7 | 0.23x | 55192 | 2528 | 11 % |
| tsql/update-subquery | 34333.7 | located | 10957.0 | 0.32x | 55192 | 2528 | 11 % |
| tsql/select-long | 166856.8 | generated | 41388.3 | 0.25x | 142256 | 13224 | 3 % |
| tsql/select-long | 166856.8 | located | 58164.6 | 0.35x | 142256 | 13224 | 3 % |
| tsql/comment | 44804.6 | generated | 4457.6 | 0.10x | 54128 | 1072 | 4 % |
| tsql/comment | 44804.6 | located | 6408.1 | 0.14x | 54128 | 1072 | 4 % |

First call in a fresh process, median of three:

| row | reading | ms |
| --- | --- | ---: |
| fix/One.text | hand | 3.98 |
| fix/One.text | generated | 4.50 |
| fix/One.text | regex-lesser | 1.44 |
| fix/One.text | regex-compiled-lesser | 4.31 |
| fix/One.bytes | hand | 4.00 |
| fix/One.bytes | generated | 6.08 |
| el/floor | hand | 11.97 |
| el/floor | tape | 23.10 |
| el/floor | immediate | 14.81 |
| sql/literal | hand | 1.07 |
| sql/literal | generated | 13.35 |
| sql/select20 | hand | 16.16 |
| sql/select20 | generated | 57.13 |

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
