# Stand, 161d7641, 2026-09-19 21:03

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.1 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 685.6 | generated | 505.3 | 0.74x | 920 | 800 | 13 % |
| fix/Order.text | 685.6 | regex-lesser | 1347.7 | 1.97x | 920 | 5960 | 13 % |
| fix/Order.text | 685.6 | regex-compiled-lesser | 1072.1 | 1.56x | 920 | 5960 | 13 % |
| fix/Order.bytes | 691.7 | generated | 600.6 | 0.87x | 920 | 856 | 19 % |
| fix/Order.stream | 800.4 | generated | 2329.0 | 2.91x | 5088 | 992 | 15 % |
| web/url.full | 182.6 | generated | 251.9 | 1.38x | 328 | 472 | 13 % |
| web/url.full | 182.6 | regex | 1160.9 | 6.36x | 328 | 1336 | 13 % |
| web/url.full | 182.6 | regex-compiled | 441.2 | 2.42x | 328 | 1336 | 13 % |
| web/json.object | 555.4 | generated | 933.0 | 1.68x | 1976 | 2520 | 24 % |
| web/json.object | 555.4 | system-text-json | 541.4 | 0.97x | 1976 | 72 | 24 % |
| web/date-time.utc | 28.7 | generated | 56.8 | 1.98x | 112 | 112 | 53 % |
| web/date-time.utc | 28.7 | regex | 450.0 | 15.68x | 112 | 1192 | 53 % |
| web/date-time.utc | 28.7 | regex-compiled | 357.5 | 12.46x | 112 | 1192 | 53 % |
| feeds/stock-count.good.text | 39291.4 | generated | 27725.6 | 0.71x | 197664 | 111168 | 4 % |
| el/ladder | 1014.4 | tape | 1797.9 | 1.77x | 1712 | 1616 | 12 % |
| el/ladder | 1014.4 | immediate | 1073.3 | 1.06x | 1712 | 1624 | 12 % |
| sql/select20 | 7034.8 | generated | 72852.0 | 10.36x | 23976 | 21328 | 3 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 32274.0 | generated | 8355.4 | 0.26x | 62448 | 3280 | 14 % |
| tsql/select-join | 32274.0 | located | 12274.4 | 0.38x | 62448 | 3280 | 14 % |
