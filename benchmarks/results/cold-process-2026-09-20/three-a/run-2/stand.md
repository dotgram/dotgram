# Stand, a0d7dd98, 2026-09-20 04:45

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.6 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/date-time.refused | 16.9 | generated | 32.3 | 1.91x | 32 | 32 | 17 % |
| web/date-time.refused | 16.9 | regex | 21.3 | 1.26x | 32 | 0 | 17 % |
| web/date-time.refused | 16.9 | regex-compiled | 21.6 | 1.28x | 32 | 0 | 17 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/addr-spec.refused | 41.3 | regex | 148.1 | 3.58x | 0 | 0 | 3 % |
| web/addr-spec.refused | 41.3 | regex-compiled | 52.5 | 1.27x | 0 | 0 | 3 % |
| web/addr-spec.refused | 41.3 | reference-MailAddress | 41.0 | reference | 0 | 48 | 3 % |
| web/media-type.refused | 49.2 | regex | 60.0 | 1.22x | 0 | 0 | 12 % |
| web/media-type.refused | 49.2 | regex-compiled | 30.5 | 0.62x | 0 | 0 | 12 % |
| web/media-type.refused | 49.2 | reference-MediaTypeHeaderValue | 9.0 | reference | 0 | 0 | 12 % |
