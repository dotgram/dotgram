Median of 5 of 5 runs, each in a process of its own; control 31.6 ns (the runs' controls: 31.7, 31.6, 31.6, 32.1, 31.4).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, a0d7dd98, 2026-09-20 04:45

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.6 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/date-time.refused | 17.3 | generated | 32.3 | 1.87x | 32 | 32 | 3 % |
| web/date-time.refused | 17.3 | regex | 21.4 | 1.24x | 32 | 0 | 3 % |
| web/date-time.refused | 17.3 | regex-compiled | 21.4 | 1.24x | 32 | 0 | 3 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/addr-spec.refused | 41.8 | regex | 147.1 | 3.52x | 0 | 0 | 2 % |
| web/addr-spec.refused | 41.8 | regex-compiled | 52.9 | 1.26x | 0 | 0 | 2 % |
| web/addr-spec.refused | 41.8 | reference-MailAddress | 40.6 | reference | 0 | 48 | 2 % |
| web/media-type.refused | 49.2 | regex | 60.0 | 1.22x | 0 | 0 | 3 % |
| web/media-type.refused | 49.2 | regex-compiled | 30.5 | 0.62x | 0 | 0 | 3 % |
| web/media-type.refused | 49.2 | reference-MediaTypeHeaderValue | 8.8 | reference | 0 | 0 | 3 % |
