# Stand, 161d7641, 2026-09-19 21:02

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.5 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 667.1 | generated | 490.8 | 0.74x | 920 | 800 | 10 % |
| fix/Order.text | 667.1 | regex-lesser | 1369.4 | 2.05x | 920 | 5960 | 10 % |
| fix/Order.text | 667.1 | regex-compiled-lesser | 1075.4 | 1.61x | 920 | 5960 | 10 % |
| fix/Order.bytes | 672.8 | generated | 601.2 | 0.89x | 920 | 856 | 17 % |
| fix/Order.stream | 806.9 | generated | 2393.8 | 2.97x | 5088 | 992 | 20 % |
| web/url.full | 168.4 | generated | 233.0 | 1.38x | 328 | 472 | 4 % |
| web/url.full | 168.4 | regex | 1108.1 | 6.58x | 328 | 1336 | 4 % |
| web/url.full | 168.4 | regex-compiled | 415.3 | 2.47x | 328 | 1336 | 4 % |
| web/json.object | 518.6 | generated | 808.6 | 1.56x | 1976 | 2520 | 50 % |
| web/json.object | 518.6 | system-text-json | 469.3 | 0.90x | 1976 | 72 | 50 % |
| web/date-time.utc | 26.6 | generated | 55.8 | 2.10x | 112 | 112 | 7 % |
| web/date-time.utc | 26.6 | regex | 450.7 | 16.98x | 112 | 1192 | 7 % |
| web/date-time.utc | 26.6 | regex-compiled | 351.7 | 13.24x | 112 | 1192 | 7 % |
| feeds/stock-count.good.text | 37781.1 | generated | 27838.1 | 0.74x | 197664 | 111168 | 2 % |
| el/ladder | 962.4 | tape | 1745.9 | 1.81x | 1712 | 1616 | 3 % |
| el/ladder | 962.4 | immediate | 1030.4 | 1.07x | 1712 | 1624 | 3 % |
| sql/select20 | 7109.3 | generated | 72584.3 | 10.21x | 23976 | 21328 | 7 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 32441.8 | generated | 8296.4 | 0.26x | 62448 | 3280 | 4 % |
| tsql/select-join | 32441.8 | located | 12285.0 | 0.38x | 62448 | 3280 | 4 % |
