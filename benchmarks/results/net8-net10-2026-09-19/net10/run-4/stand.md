# Stand, 161d7641, 2026-09-19 21:03

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.2 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 695.4 | generated | 514.8 | 0.74x | 920 | 800 | 28 % |
| fix/Order.text | 695.4 | regex-lesser | 1411.6 | 2.03x | 920 | 5960 | 28 % |
| fix/Order.text | 695.4 | regex-compiled-lesser | 1093.2 | 1.57x | 920 | 5960 | 28 % |
| fix/Order.bytes | 656.1 | generated | 615.1 | 0.94x | 920 | 856 | 30 % |
| fix/Order.stream | 820.4 | generated | 2398.3 | 2.92x | 5088 | 992 | 13 % |
| web/url.full | 169.8 | generated | 237.4 | 1.40x | 328 | 472 | 19 % |
| web/url.full | 169.8 | regex | 1089.1 | 6.41x | 328 | 1336 | 19 % |
| web/url.full | 169.8 | regex-compiled | 424.9 | 2.50x | 328 | 1336 | 19 % |
| web/json.object | 502.3 | generated | 806.6 | 1.61x | 1976 | 2520 | 8 % |
| web/json.object | 502.3 | system-text-json | 489.2 | 0.97x | 1976 | 72 | 8 % |
| web/date-time.utc | 26.5 | generated | 53.7 | 2.03x | 112 | 112 | 7 % |
| web/date-time.utc | 26.5 | regex | 437.1 | 16.51x | 112 | 1192 | 7 % |
| web/date-time.utc | 26.5 | regex-compiled | 353.7 | 13.36x | 112 | 1192 | 7 % |
| feeds/stock-count.good.text | 39429.5 | generated | 28637.3 | 0.73x | 197664 | 111168 | 13 % |
| el/ladder | 1064.6 | tape | 1807.6 | 1.70x | 1712 | 1616 | 9 % |
| el/ladder | 1064.6 | immediate | 1074.4 | 1.01x | 1712 | 1624 | 9 % |
| sql/select20 | 7177.8 | generated | 71415.8 | 9.95x | 23976 | 21328 | 16 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 32552.5 | generated | 8393.0 | 0.26x | 62448 | 3280 | 6 % |
| tsql/select-join | 32552.5 | located | 12166.7 | 0.37x | 62448 | 3280 | 6 % |
