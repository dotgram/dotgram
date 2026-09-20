Median of 3 of 3 runs, each in a process of its own; control 31.1 ns (the runs' controls: 31.1, 31.1, 31.1).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, fff88873, 2026-09-19 21:09

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.1 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 639.8 | generated | 492.8 | 0.77x | 920 | 800 | 6 % |
| fix/Order.text | 639.8 | regex-lesser | 1343.7 | 2.10x | 920 | 5960 | 6 % |
| fix/Order.text | 639.8 | regex-compiled-lesser | 1068.5 | 1.67x | 920 | 5960 | 6 % |
| fix/Order.bytes | 672.6 | generated | 599.3 | 0.89x | 920 | 856 | 2 % |
| fix/Order.stream | 802.5 | generated | 2242.3 | 2.79x | 5088 | 992 | 1 % |
| web/url.full | 175.0 | generated | 241.9 | 1.38x | 328 | 472 | 3 % |
| web/url.full | 175.0 | regex | 1112.6 | 6.36x | 328 | 1336 | 3 % |
| web/url.full | 175.0 | regex-compiled | 431.7 | 2.47x | 328 | 1336 | 3 % |
| web/json.object | 513.3 | generated | 809.8 | 1.58x | 1976 | 2520 | 4 % |
| web/json.object | 513.3 | system-text-json | 481.3 | 0.94x | 1976 | 72 | 4 % |
| web/date-time.utc | 26.9 | generated | 55.1 | 2.05x | 112 | 112 | 4 % |
| web/date-time.utc | 26.9 | regex | 442.1 | 16.43x | 112 | 1192 | 4 % |
| web/date-time.utc | 26.9 | regex-compiled | 348.7 | 12.96x | 112 | 1192 | 4 % |
| feeds/stock-count.good.text | 37948.4 | generated | 28296.5 | 0.75x | 197664 | 111168 | 2 % |
| el/ladder | 973.0 | tape | 1791.5 | 1.84x | 1712 | 1616 | 4 % |
| el/ladder | 973.0 | immediate | 1052.8 | 1.08x | 1712 | 1624 | 4 % |
| sql/select20 | 7218.8 | generated | 74015.3 | 10.25x | 23976 | 21328 | 3 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 30939.6 | generated | 8451.9 | 0.27x | 61512 | 3280 | 11 % |
| tsql/select-join | 30939.6 | located | 12386.4 | 0.40x | 61512 | 3280 | 11 % |
