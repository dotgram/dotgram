# Stand, 161d7641, 2026-09-19 21:09

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.0 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 581.5 | generated | 420.7 | 0.72x | 920 | 800 | 5 % |
| fix/Order.text | 581.5 | regex-lesser | 1188.0 | 2.04x | 920 | 5960 | 5 % |
| fix/Order.text | 581.5 | regex-compiled-lesser | 908.3 | 1.56x | 920 | 5960 | 5 % |
| fix/Order.bytes | 583.9 | generated | 536.8 | 0.92x | 920 | 856 | 22 % |
| fix/Order.stream | 691.5 | generated | 2141.4 | 3.10x | 5088 | 992 | 7 % |
| web/url.full | 139.0 | generated | 212.9 | 1.53x | 328 | 472 | 6 % |
| web/url.full | 139.0 | regex | 948.7 | 6.83x | 328 | 1336 | 6 % |
| web/url.full | 139.0 | regex-compiled | 371.0 | 2.67x | 328 | 1336 | 6 % |
| web/json.object | 443.9 | generated | 700.6 | 1.58x | 1976 | 2520 | 11 % |
| web/json.object | 443.9 | system-text-json | 410.2 | 0.92x | 1976 | 72 | 11 % |
| web/date-time.utc | 22.4 | generated | 49.0 | 2.19x | 112 | 112 | 8 % |
| web/date-time.utc | 22.4 | regex | 383.4 | 17.10x | 112 | 1192 | 8 % |
| web/date-time.utc | 22.4 | regex-compiled | 297.9 | 13.29x | 112 | 1192 | 8 % |
| feeds/stock-count.good.text | 31307.2 | generated | 22438.7 | 0.72x | 197664 | 111168 | 11 % |
| el/ladder | 892.5 | tape | 1690.5 | 1.89x | 1712 | 1616 | 14 % |
| el/ladder | 892.5 | immediate | 967.5 | 1.08x | 1712 | 1624 | 14 % |
| sql/select20 | 6068.9 | generated | 70882.8 | 11.68x | 23976 | 21328 | 4 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 29871.6 | generated | 8179.1 | 0.27x | 62448 | 3280 | 5 % |
| tsql/select-join | 29871.6 | located | 11806.9 | 0.40x | 62448 | 3280 | 5 % |
