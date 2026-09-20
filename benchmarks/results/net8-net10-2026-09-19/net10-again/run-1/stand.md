# Stand, fff88873, 2026-09-19 21:09

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.1 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 678.7 | generated | 496.4 | 0.73x | 920 | 800 | 17 % |
| fix/Order.text | 678.7 | regex-lesser | 1393.7 | 2.05x | 920 | 5960 | 17 % |
| fix/Order.text | 678.7 | regex-compiled-lesser | 1068.5 | 1.57x | 920 | 5960 | 17 % |
| fix/Order.bytes | 672.6 | generated | 628.9 | 0.94x | 920 | 856 | 23 % |
| fix/Order.stream | 802.5 | generated | 2242.8 | 2.79x | 5088 | 992 | 2 % |
| web/url.full | 171.5 | generated | 229.0 | 1.33x | 328 | 472 | 3 % |
| web/url.full | 171.5 | regex | 1112.6 | 6.49x | 328 | 1336 | 3 % |
| web/url.full | 171.5 | regex-compiled | 434.0 | 2.53x | 328 | 1336 | 3 % |
| web/json.object | 524.8 | generated | 809.8 | 1.54x | 1976 | 2520 | 52 % |
| web/json.object | 524.8 | system-text-json | 476.8 | 0.91x | 1976 | 72 | 52 % |
| web/date-time.utc | 26.5 | generated | 55.1 | 2.08x | 112 | 112 | 3 % |
| web/date-time.utc | 26.5 | regex | 455.3 | 17.16x | 112 | 1192 | 3 % |
| web/date-time.utc | 26.5 | regex-compiled | 376.3 | 14.19x | 112 | 1192 | 3 % |
| feeds/stock-count.good.text | 38240.6 | generated | 27837.7 | 0.73x | 197664 | 111168 | 6 % |
| el/ladder | 973.0 | tape | 1819.3 | 1.87x | 1712 | 1616 | 14 % |
| el/ladder | 973.0 | immediate | 1052.8 | 1.08x | 1712 | 1624 | 14 % |
| sql/select20 | 7306.7 | generated | 71787.2 | 9.82x | 23976 | 21328 | 9 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 30010.0 | generated | 8451.9 | 0.28x | 61512 | 3280 | 12 % |
| tsql/select-join | 30010.0 | located | 12306.2 | 0.41x | 61512 | 3280 | 12 % |
