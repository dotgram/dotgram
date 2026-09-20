# Stand, d29170ba, 2026-09-20 04:44

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 135.7 ns.


Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/media-type.refused | 48.6 | regex | 60.6 | 1.25x | 0 | 0 | 11 % |
| web/media-type.refused | 48.6 | regex-compiled | 29.8 | 0.61x | 0 | 0 | 11 % |
| web/media-type.refused | 48.6 | reference-MediaTypeHeaderValue | 8.4 | reference | 0 | 0 | 11 % |
