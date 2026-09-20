# Stand, 161d7641, 2026-09-19 21:06

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.7 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 531.8 | generated | 427.6 | 0.80x | 920 | 800 | 30 % |
| fix/Order.text | 531.8 | regex-lesser | 1190.2 | 2.24x | 920 | 5960 | 30 % |
| fix/Order.text | 531.8 | regex-compiled-lesser | 879.7 | 1.65x | 920 | 5960 | 30 % |
| fix/Order.bytes | 535.9 | generated | 536.3 | 1.00x | 920 | 856 | 16 % |
| fix/Order.stream | 663.0 | generated | 2072.5 | 3.13x | 5088 | 992 | 3 % |
| web/url.full | 141.1 | generated | 207.9 | 1.47x | 328 | 472 | 3 % |
| web/url.full | 141.1 | regex | 933.7 | 6.62x | 328 | 1336 | 3 % |
| web/url.full | 141.1 | regex-compiled | 383.8 | 2.72x | 328 | 1336 | 3 % |
| web/json.object | 419.5 | generated | 672.1 | 1.60x | 1976 | 2520 | 21 % |
| web/json.object | 419.5 | system-text-json | 407.2 | 0.97x | 1976 | 72 | 21 % |
| web/date-time.utc | 22.4 | generated | 48.0 | 2.14x | 112 | 112 | 5 % |
| web/date-time.utc | 22.4 | regex | 407.4 | 18.18x | 112 | 1192 | 5 % |
| web/date-time.utc | 22.4 | regex-compiled | 310.3 | 13.85x | 112 | 1192 | 5 % |
| feeds/stock-count.good.text | 30793.0 | generated | 21842.2 | 0.71x | 197664 | 111168 | 3 % |
| el/ladder | 898.6 | tape | 1670.1 | 1.86x | 1712 | 1616 | 8 % |
| el/ladder | 898.6 | immediate | 1011.3 | 1.13x | 1712 | 1624 | 8 % |
| sql/select20 | 6067.7 | generated | 71684.2 | 11.81x | 23976 | 21328 | 8 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 30500.7 | generated | 8223.4 | 0.27x | 62448 | 3280 | 4 % |
| tsql/select-join | 30500.7 | located | 12286.1 | 0.40x | 62448 | 3280 | 4 % |
