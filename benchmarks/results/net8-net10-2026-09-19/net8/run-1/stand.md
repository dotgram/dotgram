# Stand, 161d7641, 2026-09-19 21:04

IGOR-DESKTOP, .NET 8.0.31, pinned to 0-15, high priority, control 30.2 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 644.7 | generated | 505.6 | 0.78x | 1184 | 832 | 8 % |
| fix/Order.text | 644.7 | regex-lesser | 1243.5 | 1.93x | 1184 | 5960 | 8 % |
| fix/Order.text | 644.7 | regex-compiled-lesser | 996.7 | 1.55x | 1184 | 5960 | 8 % |
| fix/Order.bytes | 661.3 | generated | 645.4 | 0.98x | 1184 | 888 | 6 % |
| fix/Order.stream | 745.5 | generated | 2278.2 | 3.06x | 5088 | 992 | 16 % |
| web/url.full | 143.5 | generated | 219.6 | 1.53x | 328 | 472 | 6 % |
| web/url.full | 143.5 | regex | 975.6 | 6.80x | 328 | 1336 | 6 % |
| web/url.full | 143.5 | regex-compiled | 382.2 | 2.66x | 328 | 1336 | 6 % |
| web/json.object | 468.3 | generated | 780.2 | 1.67x | 1976 | 2520 | 17 % |
| web/json.object | 468.3 | system-text-json | 495.1 | 1.06x | 1976 | 72 | 17 % |
| web/date-time.utc | 24.5 | generated | 43.8 | 1.78x | 112 | 112 | 6 % |
| web/date-time.utc | 24.5 | regex | 408.6 | 16.65x | 112 | 1192 | 6 % |
| web/date-time.utc | 24.5 | regex-compiled | 319.5 | 13.02x | 112 | 1192 | 6 % |
| feeds/stock-count.good.text | 33376.6 | generated | 29607.2 | 0.89x | 197664 | 111168 | 4 % |
| el/ladder | 1105.8 | tape | 1906.1 | 1.72x | 1712 | 1616 | 7 % |
| el/ladder | 1105.8 | immediate | 1197.5 | 1.08x | 1712 | 1624 | 7 % |
| sql/select20 | 5866.1 | generated | 70220.8 | 11.97x | 24680 | 21328 | 2 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 58121.1 | generated | 8142.0 | 0.14x | 62800 | 3280 | 15 % |
| tsql/select-join | 58121.1 | located | 12345.3 | 0.21x | 62800 | 3280 | 15 % |
