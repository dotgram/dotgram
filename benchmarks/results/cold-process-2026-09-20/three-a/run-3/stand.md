# Stand, cb5fe962, 2026-09-20 04:45

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.6 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/date-time.refused | 17.4 | generated | 27.5 | 1.58x | 32 | 32 | 18 % |
| web/date-time.refused | 17.4 | regex | 21.4 | 1.23x | 32 | 0 | 18 % |
| web/date-time.refused | 17.4 | regex-compiled | 21.4 | 1.23x | 32 | 0 | 18 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/addr-spec.refused | 42.1 | regex | 147.1 | 3.49x | 0 | 0 | 4 % |
| web/addr-spec.refused | 42.1 | regex-compiled | 52.9 | 1.25x | 0 | 0 | 4 % |
| web/addr-spec.refused | 42.1 | reference-MailAddress | 41.0 | reference | 0 | 48 | 4 % |
| web/media-type.refused | 49.2 | regex | 59.8 | 1.22x | 0 | 0 | 8 % |
| web/media-type.refused | 49.2 | regex-compiled | 30.5 | 0.62x | 0 | 0 | 8 % |
| web/media-type.refused | 49.2 | reference-MediaTypeHeaderValue | 8.8 | reference | 0 | 0 | 8 % |
