Median of 5 of 5 runs, each in a process of its own; control 30.7 ns (the runs' controls: 30.7, 30.7, 30.6, 30.6, 31.0).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, 161d7641, 2026-09-19 21:06

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.7 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 569.0 | generated | 423.2 | 0.74x | 920 | 800 | 14 % |
| fix/Order.text | 569.0 | regex-lesser | 1190.2 | 2.09x | 920 | 5960 | 14 % |
| fix/Order.text | 569.0 | regex-compiled-lesser | 880.6 | 1.55x | 920 | 5960 | 14 % |
| fix/Order.bytes | 562.1 | generated | 525.9 | 0.94x | 920 | 856 | 14 % |
| fix/Order.stream | 665.1 | generated | 2079.7 | 3.13x | 5088 | 992 | 7 % |
| web/url.full | 141.1 | generated | 207.9 | 1.47x | 328 | 472 | 3 % |
| web/url.full | 141.1 | regex | 933.7 | 6.62x | 328 | 1336 | 3 % |
| web/url.full | 141.1 | regex-compiled | 372.7 | 2.64x | 328 | 1336 | 3 % |
| web/json.object | 419.5 | generated | 672.1 | 1.60x | 1976 | 2520 | 8 % |
| web/json.object | 419.5 | system-text-json | 410.2 | 0.98x | 1976 | 72 | 8 % |
| web/date-time.utc | 22.4 | generated | 49.0 | 2.19x | 112 | 112 | 3 % |
| web/date-time.utc | 22.4 | regex | 387.1 | 17.27x | 112 | 1192 | 3 % |
| web/date-time.utc | 22.4 | regex-compiled | 307.0 | 13.69x | 112 | 1192 | 3 % |
| feeds/stock-count.good.text | 31218.9 | generated | 22275.8 | 0.71x | 197664 | 111168 | 2 % |
| el/ladder | 898.6 | tape | 1680.9 | 1.87x | 1712 | 1616 | 2 % |
| el/ladder | 898.6 | immediate | 979.8 | 1.09x | 1712 | 1624 | 2 % |
| sql/select20 | 6068.9 | generated | 70882.8 | 11.68x | 23976 | 21328 | 2 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 29871.6 | generated | 8179.1 | 0.27x | 62448 | 3280 | 8 % |
| tsql/select-join | 29871.6 | located | 12054.8 | 0.40x | 62448 | 3280 | 8 % |
