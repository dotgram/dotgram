Median of 5 of 5 runs, each in a process of its own; control 30.7 ns (the runs' controls: 30.7, 30.6, 30.5, 31.2, 31.1).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, 2fa604b6, 2026-09-19 21:01

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.7 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 667.1 | generated | 492.1 | 0.74x | 920 | 800 | 8 % |
| fix/Order.text | 667.1 | regex-lesser | 1369.4 | 2.05x | 920 | 5960 | 8 % |
| fix/Order.text | 667.1 | regex-compiled-lesser | 1075.4 | 1.61x | 920 | 5960 | 8 % |
| fix/Order.bytes | 672.8 | generated | 604.2 | 0.90x | 920 | 856 | 9 % |
| fix/Order.stream | 806.9 | generated | 2362.4 | 2.93x | 5088 | 992 | 2 % |
| web/url.full | 169.8 | generated | 239.7 | 1.41x | 328 | 472 | 8 % |
| web/url.full | 169.8 | regex | 1095.3 | 6.45x | 328 | 1336 | 8 % |
| web/url.full | 169.8 | regex-compiled | 418.9 | 2.47x | 328 | 1336 | 8 % |
| web/json.object | 518.6 | generated | 808.6 | 1.56x | 1976 | 2520 | 12 % |
| web/json.object | 518.6 | system-text-json | 476.5 | 0.92x | 1976 | 72 | 12 % |
| web/date-time.utc | 26.6 | generated | 55.5 | 2.09x | 112 | 112 | 9 % |
| web/date-time.utc | 26.6 | regex | 450.0 | 16.95x | 112 | 1192 | 9 % |
| web/date-time.utc | 26.6 | regex-compiled | 353.7 | 13.32x | 112 | 1192 | 9 % |
| feeds/stock-count.good.text | 39429.5 | generated | 28637.3 | 0.73x | 197664 | 111168 | 10 % |
| el/ladder | 994.2 | tape | 1797.9 | 1.81x | 1712 | 1616 | 10 % |
| el/ladder | 994.2 | immediate | 1073.3 | 1.08x | 1712 | 1624 | 10 % |
| sql/select20 | 7070.2 | generated | 72584.3 | 10.27x | 23976 | 21328 | 2 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 32274.0 | generated | 8355.4 | 0.26x | 62448 | 3280 | 7 % |
| tsql/select-join | 32274.0 | located | 12166.7 | 0.38x | 62448 | 3280 | 7 % |
