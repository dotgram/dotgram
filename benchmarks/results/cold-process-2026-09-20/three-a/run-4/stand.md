# Stand, cb5fe962, 2026-09-20 04:46

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 32.1 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/date-time.refused | 17.4 | generated | 32.6 | 1.87x | 32 | 32 | 14 % |
| web/date-time.refused | 17.4 | regex | 21.3 | 1.23x | 32 | 0 | 14 % |
| web/date-time.refused | 17.4 | regex-compiled | 21.6 | 1.24x | 32 | 0 | 14 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/addr-spec.refused | 41.8 | regex | 149.3 | 3.57x | 0 | 0 | 7 % |
| web/addr-spec.refused | 41.8 | regex-compiled | 53.9 | 1.29x | 0 | 0 | 7 % |
| web/addr-spec.refused | 41.8 | reference-MailAddress | 40.6 | reference | 0 | 48 | 7 % |
| web/media-type.refused | 50.0 | regex | 62.3 | 1.25x | 0 | 0 | 6 % |
| web/media-type.refused | 50.0 | regex-compiled | 30.9 | 0.62x | 0 | 0 | 6 % |
| web/media-type.refused | 50.0 | reference-MediaTypeHeaderValue | 9.1 | reference | 0 | 0 | 6 % |
