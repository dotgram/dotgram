# Stand, fff88873, 2026-09-19 21:10

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.1 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 639.4 | generated | 492.8 | 0.77x | 920 | 800 | 18 % |
| fix/Order.text | 639.4 | regex-lesser | 1336.8 | 2.09x | 920 | 5960 | 18 % |
| fix/Order.text | 639.4 | regex-compiled-lesser | 1063.0 | 1.66x | 920 | 5960 | 18 % |
| fix/Order.bytes | 676.4 | generated | 593.3 | 0.88x | 920 | 856 | 7 % |
| fix/Order.stream | 807.5 | generated | 2242.3 | 2.78x | 5088 | 992 | 19 % |
| web/url.full | 175.0 | generated | 241.9 | 1.38x | 328 | 472 | 22 % |
| web/url.full | 175.0 | regex | 1112.1 | 6.35x | 328 | 1336 | 22 % |
| web/url.full | 175.0 | regex-compiled | 427.8 | 2.44x | 328 | 1336 | 22 % |
| web/json.object | 503.8 | generated | 809.7 | 1.61x | 1976 | 2520 | 10 % |
| web/json.object | 503.8 | system-text-json | 481.3 | 0.96x | 1976 | 72 | 10 % |
| web/date-time.utc | 26.9 | generated | 55.0 | 2.05x | 112 | 112 | 7 % |
| web/date-time.utc | 26.9 | regex | 438.1 | 16.28x | 112 | 1192 | 7 % |
| web/date-time.utc | 26.9 | regex-compiled | 348.7 | 12.96x | 112 | 1192 | 7 % |
| feeds/stock-count.good.text | 37416.0 | generated | 28296.5 | 0.76x | 197664 | 111168 | 15 % |
| el/ladder | 1012.7 | tape | 1791.5 | 1.77x | 1712 | 1616 | 16 % |
| el/ladder | 1012.7 | immediate | 1057.1 | 1.04x | 1712 | 1624 | 16 % |
| sql/select20 | 7218.8 | generated | 74417.2 | 10.31x | 23976 | 21328 | 6 % |

Rows with no hand-written parser, so the base is the scriptdom reading:

| row | scriptdom ns | reading | ns | /scriptdom | scriptdom B | B | scriptdom spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| tsql/select-join | 30939.6 | generated | 8428.8 | 0.27x | 61512 | 3280 | 8 % |
| tsql/select-join | 30939.6 | located | 12396.3 | 0.40x | 61512 | 3280 | 8 % |
