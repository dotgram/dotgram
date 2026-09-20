# Stand, 161d7641, 2026-09-19 21:05

IGOR-DESKTOP, .NET 8.0.31, pinned to 0-15, high priority, control 30.7 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 647.6 | generated | 502.2 | 0.78x | 1184 | 832 | 12 % |
| fix/Order.text | 647.6 | regex-lesser | 1230.9 | 1.90x | 1184 | 5960 | 12 % |
| fix/Order.text | 647.6 | regex-compiled-lesser | 988.6 | 1.53x | 1184 | 5960 | 12 % |
| fix/Order.bytes | 648.6 | generated | 634.1 | 0.98x | 1184 | 888 | 8 % |
| fix/Order.stream | 693.9 | generated | 2246.8 | 3.24x | 5088 | 992 | 8 % |
| web/url.full | 143.6 | generated | 218.2 | 1.52x | 328 | 472 | 13 % |
| web/url.full | 143.6 | regex | 952.1 | 6.63x | 328 | 1336 | 13 % |
| web/url.full | 143.6 | regex-compiled | 426.3 | 2.97x | 328 | 1336 | 13 % |
| web/json.object | 447.9 | generated | 762.9 | 1.70x | 1976 | 2520 | 10 % |
| web/json.object | 447.9 | system-text-json | 483.8 | 1.08x | 1976 | 72 | 10 % |
| web/date-time.utc | 25.7 | generated | 44.3 | 1.72x | 112 | 112 | 7 % |
| web/date-time.utc | 25.7 | regex | 405.7 | 15.79x | 112 | 1192 | 7 % |
| web/date-time.utc | 25.7 | regex-compiled | 310.7 | 12.09x | 112 | 1192 | 7 % |
| feeds/stock-count.good.text | 34497.7 | generated | 30510.4 | 0.88x | 197664 | 111168 | 28 % |
| el/ladder | 1106.7 | tape | 1962.4 | 1.77x | 1712 | 1616 | 13 % |
| el/ladder | 1106.7 | immediate | 1232.3 | 1.11x | 1712 | 1624 | 13 % |
| sql/select20 | 6058.4 | generated | 71161.8 | 11.75x | 24680 | 21328 | 16 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 59825.3 | generated | 8403.7 | 0.14x | 62800 | 3280 | 5 % |
| tsql/select-join | 59825.3 | located | 12322.0 | 0.21x | 62800 | 3280 | 5 % |
