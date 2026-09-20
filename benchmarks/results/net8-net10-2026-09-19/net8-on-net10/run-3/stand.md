# Stand, 161d7641, 2026-09-19 21:08

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.6 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 613.3 | generated | 419.8 | 0.68x | 920 | 800 | 15 % |
| fix/Order.text | 613.3 | regex-lesser | 1178.0 | 1.92x | 920 | 5960 | 15 % |
| fix/Order.text | 613.3 | regex-compiled-lesser | 877.5 | 1.43x | 920 | 5960 | 15 % |
| fix/Order.bytes | 562.1 | generated | 525.9 | 0.94x | 920 | 856 | 7 % |
| fix/Order.stream | 673.2 | generated | 2106.2 | 3.13x | 5088 | 992 | 10 % |
| web/url.full | 142.9 | generated | 202.2 | 1.42x | 328 | 472 | 7 % |
| web/url.full | 142.9 | regex | 923.4 | 6.46x | 328 | 1336 | 7 % |
| web/url.full | 142.9 | regex-compiled | 357.9 | 2.50x | 328 | 1336 | 7 % |
| web/json.object | 419.9 | generated | 662.2 | 1.58x | 1976 | 2520 | 7 % |
| web/json.object | 419.9 | system-text-json | 402.7 | 0.96x | 1976 | 72 | 7 % |
| web/date-time.utc | 22.4 | generated | 47.4 | 2.11x | 112 | 112 | 4 % |
| web/date-time.utc | 22.4 | regex | 379.0 | 16.90x | 112 | 1192 | 4 % |
| web/date-time.utc | 22.4 | regex-compiled | 295.9 | 13.20x | 112 | 1192 | 4 % |
| feeds/stock-count.good.text | 30922.9 | generated | 22039.5 | 0.71x | 197664 | 111168 | 6 % |
| el/ladder | 905.2 | tape | 1649.2 | 1.82x | 1712 | 1616 | 9 % |
| el/ladder | 905.2 | immediate | 980.5 | 1.08x | 1712 | 1624 | 9 % |
| sql/select20 | 6017.4 | generated | 69775.4 | 11.60x | 23976 | 21328 | 6 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 28088.1 | generated | 7977.9 | 0.28x | 61512 | 3280 | 18 % |
| tsql/select-join | 28088.1 | located | 12054.8 | 0.43x | 61512 | 3280 | 18 % |
