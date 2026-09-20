# Stand, 161d7641, 2026-09-19 21:07

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.7 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 534.5 | generated | 423.2 | 0.79x | 920 | 800 | 7 % |
| fix/Order.text | 534.5 | regex-lesser | 1247.7 | 2.33x | 920 | 5960 | 7 % |
| fix/Order.text | 534.5 | regex-compiled-lesser | 963.1 | 1.80x | 920 | 5960 | 7 % |
| fix/Order.bytes | 615.8 | generated | 520.7 | 0.85x | 920 | 856 | 14 % |
| fix/Order.stream | 665.1 | generated | 2022.8 | 3.04x | 5088 | 992 | 8 % |
| web/url.full | 143.5 | generated | 198.9 | 1.39x | 328 | 472 | 8 % |
| web/url.full | 143.5 | regex | 939.4 | 6.55x | 328 | 1336 | 8 % |
| web/url.full | 143.5 | regex-compiled | 385.0 | 2.68x | 328 | 1336 | 8 % |
| web/json.object | 409.1 | generated | 658.8 | 1.61x | 1976 | 2520 | 6 % |
| web/json.object | 409.1 | system-text-json | 415.7 | 1.02x | 1976 | 72 | 6 % |
| web/date-time.utc | 22.3 | generated | 51.1 | 2.29x | 112 | 112 | 4 % |
| web/date-time.utc | 22.3 | regex | 400.0 | 17.93x | 112 | 1192 | 4 % |
| web/date-time.utc | 22.3 | regex-compiled | 312.6 | 14.01x | 112 | 1192 | 4 % |
| feeds/stock-count.good.text | 31221.1 | generated | 22476.0 | 0.72x | 197664 | 111168 | 9 % |
| el/ladder | 911.5 | tape | 1680.9 | 1.84x | 1712 | 1616 | 12 % |
| el/ladder | 911.5 | immediate | 979.8 | 1.07x | 1712 | 1624 | 12 % |
| sql/select20 | 6133.5 | generated | 70624.6 | 11.51x | 23976 | 21328 | 4 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 30018.6 | generated | 8278.5 | 0.28x | 62448 | 3280 | 4 % |
| tsql/select-join | 30018.6 | located | 12203.9 | 0.41x | 62448 | 3280 | 4 % |
