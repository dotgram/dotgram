Median of 5 of 5 runs, each in a process of its own; control 31.9 ns (the runs' controls: 32.0, 31.9, 32.0, 31.4, 31.5).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, 91e4118f, 2026-09-19 11:13

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.9 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/Order.text | 706.2 | generated | 1023.0 | 1.45x | 920 | 800 | 9 % |
| fix/Order.text | 706.2 | regex-lesser | 1429.8 | 2.02x | 920 | 5960 | 9 % |
| fix/Order.text | 706.2 | regex-compiled-lesser | 1164.8 | 1.65x | 920 | 5960 | 9 % |
| web/url.full | 176.2 | generated | 244.5 | 1.39x | 328 | 472 | 10 % |
| web/url.full | 176.2 | regex | 1140.1 | 6.47x | 328 | 1336 | 10 % |
| web/url.full | 176.2 | regex-compiled | 446.9 | 2.54x | 328 | 1336 | 10 % |
| el/ladder | 1014.9 | tape | 2079.8 | 2.05x | 1712 | 1616 | 7 % |
| el/ladder | 1014.9 | immediate | 1110.4 | 1.09x | 1712 | 1624 | 7 % |
| sql/select20 | 7447.5 | generated | 77938.4 | 10.46x | 23976 | 21328 | 6 % |
