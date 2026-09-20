# Stand, d29170ba, 2026-09-20 04:44

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 127.8 ns.


Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/media-type.refused | 48.8 | regex | 60.0 | 1.23x | 0 | 0 | 10 % |
| web/media-type.refused | 48.8 | regex-compiled | 30.1 | 0.62x | 0 | 0 | 10 % |
| web/media-type.refused | 48.8 | reference-MediaTypeHeaderValue | 8.5 | reference | 0 | 0 | 10 % |
