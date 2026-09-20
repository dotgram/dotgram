# Stand, 161d7641, 2026-09-19 21:06

IGOR-DESKTOP, .NET 8.0.31, pinned to 0-15, high priority, control 30.1 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 641.1 | generated | 506.4 | 0.79x | 1184 | 832 | 2 % |
| fix/Order.text | 641.1 | regex-lesser | 1215.9 | 1.90x | 1184 | 5960 | 2 % |
| fix/Order.text | 641.1 | regex-compiled-lesser | 975.8 | 1.52x | 1184 | 5960 | 2 % |
| fix/Order.bytes | 641.7 | generated | 623.9 | 0.97x | 1184 | 888 | 4 % |
| fix/Order.stream | 703.5 | generated | 2306.0 | 3.28x | 5088 | 992 | 14 % |
| web/url.full | 143.9 | generated | 227.1 | 1.58x | 328 | 472 | 13 % |
| web/url.full | 143.9 | regex | 973.7 | 6.77x | 328 | 1336 | 13 % |
| web/url.full | 143.9 | regex-compiled | 423.2 | 2.94x | 328 | 1336 | 13 % |
| web/json.object | 463.9 | generated | 798.8 | 1.72x | 1976 | 2520 | 7 % |
| web/json.object | 463.9 | system-text-json | 492.4 | 1.06x | 1976 | 72 | 7 % |
| web/date-time.utc | 24.4 | generated | 44.0 | 1.80x | 112 | 112 | 5 % |
| web/date-time.utc | 24.4 | regex | 399.1 | 16.34x | 112 | 1192 | 5 % |
| web/date-time.utc | 24.4 | regex-compiled | 316.8 | 12.97x | 112 | 1192 | 5 % |
| feeds/stock-count.good.text | 36985.0 | generated | 29366.4 | 0.79x | 197664 | 111168 | 4 % |
| el/ladder | 1052.6 | tape | 1877.9 | 1.78x | 1712 | 1616 | 6 % |
| el/ladder | 1052.6 | immediate | 1159.4 | 1.10x | 1712 | 1624 | 6 % |
| sql/select20 | 5865.6 | generated | 70427.7 | 12.01x | 24680 | 21328 | 4 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 60103.1 | generated | 8358.2 | 0.14x | 62800 | 3280 | 42 % |
| tsql/select-join | 60103.1 | located | 12458.7 | 0.21x | 62800 | 3280 | 42 % |
