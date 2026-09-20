# Stand, d29170ba, 2026-09-20 04:44

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 125.5 ns.


Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/media-type.refused | 48.2 | regex | 61.2 | 1.27x | 0 | 0 | 10 % |
| web/media-type.refused | 48.2 | regex-compiled | 29.9 | 0.62x | 0 | 0 | 10 % |
| web/media-type.refused | 48.2 | reference-MediaTypeHeaderValue | 8.5 | reference | 0 | 0 | 10 % |
