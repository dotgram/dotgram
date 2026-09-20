Median of 5 of 5 runs, each in a process of its own; control 30.3 ns (the runs' controls: 30.2, 30.6, 30.7, 30.3, 30.1).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, 161d7641, 2026-09-19 21:04

IGOR-DESKTOP, .NET 8.0.31, pinned to 0-15, high priority, control 30.3 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 647.6 | generated | 506.4 | 0.78x | 1184 | 832 | 3 % |
| fix/Order.text | 647.6 | regex-lesser | 1241.2 | 1.92x | 1184 | 5960 | 3 % |
| fix/Order.text | 647.6 | regex-compiled-lesser | 994.6 | 1.54x | 1184 | 5960 | 3 % |
| fix/Order.bytes | 661.3 | generated | 634.1 | 0.96x | 1184 | 888 | 7 % |
| fix/Order.stream | 706.4 | generated | 2278.2 | 3.22x | 5088 | 992 | 7 % |
| web/url.full | 143.9 | generated | 219.5 | 1.52x | 328 | 472 | 4 % |
| web/url.full | 143.9 | regex | 957.7 | 6.65x | 328 | 1336 | 4 % |
| web/url.full | 143.9 | regex-compiled | 397.1 | 2.76x | 328 | 1336 | 4 % |
| web/json.object | 463.3 | generated | 786.3 | 1.70x | 1976 | 2520 | 4 % |
| web/json.object | 463.3 | system-text-json | 495.1 | 1.07x | 1976 | 72 | 4 % |
| web/date-time.utc | 24.6 | generated | 44.3 | 1.80x | 112 | 112 | 5 % |
| web/date-time.utc | 24.6 | regex | 408.6 | 16.64x | 112 | 1192 | 5 % |
| web/date-time.utc | 24.6 | regex-compiled | 318.9 | 12.99x | 112 | 1192 | 5 % |
| feeds/stock-count.good.text | 34497.7 | generated | 29366.4 | 0.85x | 197664 | 111168 | 11 % |
| el/ladder | 1090.7 | tape | 1906.1 | 1.75x | 1712 | 1616 | 5 % |
| el/ladder | 1090.7 | immediate | 1204.9 | 1.10x | 1712 | 1624 | 5 % |
| sql/select20 | 5892.5 | generated | 70427.7 | 11.95x | 24680 | 21328 | 3 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 58729.9 | generated | 8352.4 | 0.14x | 62800 | 3280 | 3 % |
| tsql/select-join | 58729.9 | located | 12345.3 | 0.21x | 62800 | 3280 | 3 % |
