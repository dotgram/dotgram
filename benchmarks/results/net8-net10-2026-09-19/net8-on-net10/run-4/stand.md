# Stand, 161d7641, 2026-09-19 21:08

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.6 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 569.0 | generated | 483.7 | 0.85x | 920 | 800 | 5 % |
| fix/Order.text | 569.0 | regex-lesser | 1209.6 | 2.13x | 920 | 5960 | 5 % |
| fix/Order.text | 569.0 | regex-compiled-lesser | 880.6 | 1.55x | 920 | 5960 | 5 % |
| fix/Order.bytes | 550.6 | generated | 521.9 | 0.95x | 920 | 856 | 8 % |
| fix/Order.stream | 647.6 | generated | 2079.7 | 3.21x | 5088 | 992 | 4 % |
| web/url.full | 138.6 | generated | 214.2 | 1.54x | 328 | 472 | 7 % |
| web/url.full | 138.6 | regex | 928.5 | 6.70x | 328 | 1336 | 7 % |
| web/url.full | 138.6 | regex-compiled | 372.7 | 2.69x | 328 | 1336 | 7 % |
| web/json.object | 418.6 | generated | 675.8 | 1.61x | 1976 | 2520 | 16 % |
| web/json.object | 418.6 | system-text-json | 413.7 | 0.99x | 1976 | 72 | 16 % |
| web/date-time.utc | 23.0 | generated | 60.5 | 2.64x | 112 | 112 | 16 % |
| web/date-time.utc | 23.0 | regex | 387.1 | 16.86x | 112 | 1192 | 16 % |
| web/date-time.utc | 23.0 | regex-compiled | 307.0 | 13.37x | 112 | 1192 | 16 % |
| feeds/stock-count.good.text | 31218.9 | generated | 22275.8 | 0.71x | 197664 | 111168 | 6 % |
| el/ladder | 897.0 | tape | 1716.5 | 1.91x | 1712 | 1616 | 20 % |
| el/ladder | 897.0 | immediate | 977.2 | 1.09x | 1712 | 1624 | 20 % |
| sql/select20 | 6086.1 | generated | 71680.6 | 11.78x | 23976 | 21328 | 14 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 29384.3 | generated | 8035.2 | 0.27x | 62448 | 3280 | 4 % |
| tsql/select-join | 29384.3 | located | 11910.2 | 0.41x | 62448 | 3280 | 4 % |
