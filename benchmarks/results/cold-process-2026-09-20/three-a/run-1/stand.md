# Stand, a0d7dd98, 2026-09-20 04:45

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.7 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/date-time.refused | 17.3 | generated | 33.4 | 1.94x | 32 | 32 | 5 % |
| web/date-time.refused | 17.3 | regex | 21.6 | 1.25x | 32 | 0 | 5 % |
| web/date-time.refused | 17.3 | regex-compiled | 20.9 | 1.21x | 32 | 0 | 5 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/addr-spec.refused | 41.4 | regex | 144.4 | 3.49x | 0 | 0 | 3 % |
| web/addr-spec.refused | 41.4 | regex-compiled | 52.9 | 1.28x | 0 | 0 | 3 % |
| web/addr-spec.refused | 41.4 | reference-MailAddress | 36.9 | reference | 0 | 48 | 3 % |
| web/media-type.refused | 48.5 | regex | 59.4 | 1.22x | 0 | 0 | 1 % |
| web/media-type.refused | 48.5 | regex-compiled | 30.1 | 0.62x | 0 | 0 | 1 % |
| web/media-type.refused | 48.5 | reference-MediaTypeHeaderValue | 8.8 | reference | 0 | 0 | 1 % |
