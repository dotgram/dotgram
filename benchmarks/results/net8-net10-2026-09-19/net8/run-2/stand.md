# Stand, 161d7641, 2026-09-19 21:04

IGOR-DESKTOP, .NET 8.0.31, pinned to 0-15, high priority, control 30.6 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 650.5 | generated | 508.9 | 0.78x | 1184 | 832 | 6 % |
| fix/Order.text | 650.5 | regex-lesser | 1241.2 | 1.91x | 1184 | 5960 | 6 % |
| fix/Order.text | 650.5 | regex-compiled-lesser | 999.9 | 1.54x | 1184 | 5960 | 6 % |
| fix/Order.bytes | 686.1 | generated | 662.1 | 0.97x | 1184 | 888 | 17 % |
| fix/Order.stream | 721.6 | generated | 2261.7 | 3.13x | 5088 | 992 | 16 % |
| web/url.full | 149.1 | generated | 219.5 | 1.47x | 328 | 472 | 8 % |
| web/url.full | 149.1 | regex | 957.7 | 6.42x | 328 | 1336 | 8 % |
| web/url.full | 149.1 | regex-compiled | 392.5 | 2.63x | 328 | 1336 | 8 % |
| web/json.object | 463.3 | generated | 789.2 | 1.70x | 1976 | 2520 | 5 % |
| web/json.object | 463.3 | system-text-json | 508.9 | 1.10x | 1976 | 72 | 5 % |
| web/date-time.utc | 25.1 | generated | 45.0 | 1.79x | 112 | 112 | 16 % |
| web/date-time.utc | 25.1 | regex | 410.3 | 16.34x | 112 | 1192 | 16 % |
| web/date-time.utc | 25.1 | regex-compiled | 318.9 | 12.70x | 112 | 1192 | 16 % |
| feeds/stock-count.good.text | 35960.4 | generated | 29363.5 | 0.82x | 197664 | 111168 | 3 % |
| el/ladder | 1090.7 | tape | 1892.2 | 1.73x | 1712 | 1616 | 7 % |
| el/ladder | 1090.7 | immediate | 1204.9 | 1.10x | 1712 | 1624 | 7 % |
| sql/select20 | 5892.5 | generated | 70185.3 | 11.91x | 24680 | 21328 | 4 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 58723.6 | generated | 8214.5 | 0.14x | 62800 | 3280 | 4 % |
| tsql/select-join | 58723.6 | located | 12390.7 | 0.21x | 62800 | 3280 | 4 % |
