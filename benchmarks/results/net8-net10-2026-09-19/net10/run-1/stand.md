# Stand, 2fa604b6, 2026-09-19 21:01

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.7 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 638.7 | generated | 488.0 | 0.76x | 920 | 800 | 5 % |
| fix/Order.text | 638.7 | regex-lesser | 1338.6 | 2.10x | 920 | 5960 | 5 % |
| fix/Order.text | 638.7 | regex-compiled-lesser | 1055.7 | 1.65x | 920 | 5960 | 5 % |
| fix/Order.bytes | 718.0 | generated | 619.9 | 0.86x | 920 | 856 | 19 % |
| fix/Order.stream | 800.9 | generated | 2195.9 | 2.74x | 5088 | 992 | 15 % |
| web/url.full | 168.8 | generated | 245.0 | 1.45x | 328 | 472 | 10 % |
| web/url.full | 168.8 | regex | 1095.3 | 6.49x | 328 | 1336 | 10 % |
| web/url.full | 168.8 | regex-compiled | 418.9 | 2.48x | 328 | 1336 | 10 % |
| web/json.object | 562.2 | generated | 857.3 | 1.52x | 1976 | 2520 | 20 % |
| web/json.object | 562.2 | system-text-json | 476.5 | 0.85x | 1976 | 72 | 20 % |
| web/date-time.utc | 26.6 | generated | 55.5 | 2.08x | 112 | 112 | 10 % |
| web/date-time.utc | 26.6 | regex | 458.7 | 17.22x | 112 | 1192 | 10 % |
| web/date-time.utc | 26.6 | regex-compiled | 363.9 | 13.66x | 112 | 1192 | 10 % |
| feeds/stock-count.good.text | 40818.0 | generated | 31465.6 | 0.77x | 197664 | 111168 | 62 % |
| el/ladder | 960.7 | tape | 1771.0 | 1.84x | 1712 | 1616 | 10 % |
| el/ladder | 960.7 | immediate | 1063.2 | 1.11x | 1712 | 1624 | 10 % |
| sql/select20 | 7012.0 | generated | 72264.7 | 10.31x | 23976 | 21328 | 5 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 32151.3 | generated | 8352.0 | 0.26x | 62448 | 3280 | 10 % |
| tsql/select-join | 32151.3 | located | 12012.4 | 0.37x | 62448 | 3280 | 10 % |
