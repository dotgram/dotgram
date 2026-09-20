# Stand, 161d7641, 2026-09-19 21:02

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.6 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 651.8 | generated | 492.1 | 0.75x | 920 | 800 | 17 % |
| fix/Order.text | 651.8 | regex-lesser | 1379.9 | 2.12x | 920 | 5960 | 17 % |
| fix/Order.text | 651.8 | regex-compiled-lesser | 1142.9 | 1.75x | 920 | 5960 | 17 % |
| fix/Order.bytes | 662.3 | generated | 604.2 | 0.91x | 920 | 856 | 11 % |
| fix/Order.stream | 810.0 | generated | 2362.4 | 2.92x | 5088 | 992 | 19 % |
| web/url.full | 170.0 | generated | 239.7 | 1.41x | 328 | 472 | 12 % |
| web/url.full | 170.0 | regex | 1086.9 | 6.40x | 328 | 1336 | 12 % |
| web/url.full | 170.0 | regex-compiled | 411.5 | 2.42x | 328 | 1336 | 12 % |
| web/json.object | 509.8 | generated | 807.3 | 1.58x | 1976 | 2520 | 4 % |
| web/json.object | 509.8 | system-text-json | 473.2 | 0.93x | 1976 | 72 | 4 % |
| web/date-time.utc | 26.2 | generated | 55.0 | 2.10x | 112 | 112 | 7 % |
| web/date-time.utc | 26.2 | regex | 440.6 | 16.79x | 112 | 1192 | 7 % |
| web/date-time.utc | 26.2 | regex-compiled | 347.3 | 13.24x | 112 | 1192 | 7 % |
| feeds/stock-count.good.text | 41789.3 | generated | 29490.2 | 0.71x | 197664 | 111168 | 17 % |
| el/ladder | 994.2 | tape | 1823.7 | 1.83x | 1712 | 1616 | 17 % |
| el/ladder | 994.2 | immediate | 1110.2 | 1.12x | 1712 | 1624 | 17 % |
| sql/select20 | 7070.2 | generated | 73055.9 | 10.33x | 23976 | 21328 | 6 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 30376.9 | generated | 8386.5 | 0.28x | 61512 | 3280 | 18 % |
| tsql/select-join | 30376.9 | located | 12147.0 | 0.40x | 61512 | 3280 | 18 % |
