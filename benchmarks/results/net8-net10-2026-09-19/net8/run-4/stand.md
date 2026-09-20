# Stand, 161d7641, 2026-09-19 21:05

IGOR-DESKTOP, .NET 8.0.31, pinned to 0-15, high priority, control 30.3 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 658.2 | generated | 508.9 | 0.77x | 1184 | 832 | 7 % |
| fix/Order.text | 658.2 | regex-lesser | 1243.9 | 1.89x | 1184 | 5960 | 7 % |
| fix/Order.text | 658.2 | regex-compiled-lesser | 994.6 | 1.51x | 1184 | 5960 | 7 % |
| fix/Order.bytes | 673.1 | generated | 633.5 | 0.94x | 1184 | 888 | 56 % |
| fix/Order.stream | 706.4 | generated | 2377.7 | 3.37x | 5088 | 992 | 5 % |
| web/url.full | 144.9 | generated | 214.9 | 1.48x | 328 | 472 | 8 % |
| web/url.full | 144.9 | regex | 955.0 | 6.59x | 328 | 1336 | 8 % |
| web/url.full | 144.9 | regex-compiled | 397.1 | 2.74x | 328 | 1336 | 8 % |
| web/json.object | 449.9 | generated | 786.3 | 1.75x | 1976 | 2520 | 8 % |
| web/json.object | 449.9 | system-text-json | 498.2 | 1.11x | 1976 | 72 | 8 % |
| web/date-time.utc | 24.6 | generated | 44.3 | 1.80x | 112 | 112 | 18 % |
| web/date-time.utc | 24.6 | regex | 423.6 | 17.26x | 112 | 1192 | 18 % |
| web/date-time.utc | 24.6 | regex-compiled | 326.6 | 13.30x | 112 | 1192 | 18 % |
| feeds/stock-count.good.text | 33052.5 | generated | 29201.2 | 0.88x | 197664 | 111168 | 4 % |
| el/ladder | 1079.6 | tape | 1964.3 | 1.82x | 1712 | 1616 | 7 % |
| el/ladder | 1079.6 | immediate | 1208.6 | 1.12x | 1712 | 1624 | 7 % |
| sql/select20 | 6005.7 | generated | 70814.3 | 11.79x | 24680 | 21328 | 3 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 58729.9 | generated | 8352.4 | 0.14x | 62800 | 3280 | 6 % |
| tsql/select-join | 58729.9 | located | 12253.6 | 0.21x | 62800 | 3280 | 6 % |
