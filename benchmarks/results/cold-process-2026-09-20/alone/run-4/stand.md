# Stand, d29170ba, 2026-09-20 04:44

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 130.6 ns.


Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/media-type.refused | 48.0 | regex | 61.6 | 1.28x | 0 | 0 | 11 % |
| web/media-type.refused | 48.0 | regex-compiled | 29.3 | 0.61x | 0 | 0 | 11 % |
| web/media-type.refused | 48.0 | reference-MediaTypeHeaderValue | 8.2 | reference | 0 | 0 | 11 % |
