# Stand, cb5fe962, 2026-09-20 04:46

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.4 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/date-time.refused | 17.3 | generated | 32.3 | 1.87x | 32 | 32 | 5 % |
| web/date-time.refused | 17.3 | regex | 21.6 | 1.25x | 32 | 0 | 5 % |
| web/date-time.refused | 17.3 | regex-compiled | 20.6 | 1.19x | 32 | 0 | 5 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/addr-spec.refused | 42.0 | regex | 147.0 | 3.50x | 0 | 0 | 16 % |
| web/addr-spec.refused | 42.0 | regex-compiled | 54.6 | 1.30x | 0 | 0 | 16 % |
| web/addr-spec.refused | 42.0 | reference-MailAddress | 40.6 | reference | 0 | 48 | 16 % |
| web/media-type.refused | 49.4 | regex | 61.9 | 1.25x | 0 | 0 | 10 % |
| web/media-type.refused | 49.4 | regex-compiled | 31.7 | 0.64x | 0 | 0 | 10 % |
| web/media-type.refused | 49.4 | reference-MediaTypeHeaderValue | 8.4 | reference | 0 | 0 | 10 % |
