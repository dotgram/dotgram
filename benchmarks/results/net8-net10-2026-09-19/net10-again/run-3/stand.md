# Stand, fff88873, 2026-09-19 21:10

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.1 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 639.8 | generated | 488.5 | 0.76x | 920 | 800 | 7 % |
| fix/Order.text | 639.8 | regex-lesser | 1343.7 | 2.10x | 920 | 5960 | 7 % |
| fix/Order.text | 639.8 | regex-compiled-lesser | 1080.0 | 1.69x | 920 | 5960 | 7 % |
| fix/Order.bytes | 664.1 | generated | 599.3 | 0.90x | 920 | 856 | 6 % |
| fix/Order.stream | 796.8 | generated | 2223.7 | 2.79x | 5088 | 992 | 3 % |
| web/url.full | 176.1 | generated | 244.5 | 1.39x | 328 | 472 | 5 % |
| web/url.full | 176.1 | regex | 1132.4 | 6.43x | 328 | 1336 | 5 % |
| web/url.full | 176.1 | regex-compiled | 431.7 | 2.45x | 328 | 1336 | 5 % |
| web/json.object | 513.3 | generated | 829.2 | 1.62x | 1976 | 2520 | 10 % |
| web/json.object | 513.3 | system-text-json | 494.2 | 0.96x | 1976 | 72 | 10 % |
| web/date-time.utc | 27.5 | generated | 55.7 | 2.03x | 112 | 112 | 22 % |
| web/date-time.utc | 27.5 | regex | 442.1 | 16.08x | 112 | 1192 | 22 % |
| web/date-time.utc | 27.5 | regex-compiled | 347.4 | 12.63x | 112 | 1192 | 22 % |
| feeds/stock-count.good.text | 37948.4 | generated | 28333.0 | 0.75x | 197664 | 111168 | 17 % |
| el/ladder | 972.0 | tape | 1764.8 | 1.82x | 1712 | 1616 | 6 % |
| el/ladder | 972.0 | immediate | 1038.6 | 1.07x | 1712 | 1624 | 6 % |
| sql/select20 | 7111.6 | generated | 74015.3 | 10.41x | 23976 | 21328 | 7 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 33359.7 | generated | 8523.5 | 0.26x | 62448 | 3280 | 13 % |
| tsql/select-join | 33359.7 | located | 12386.4 | 0.37x | 62448 | 3280 | 13 % |
