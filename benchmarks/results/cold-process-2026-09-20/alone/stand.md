Median of 5 of 5 runs, each in a process of its own; control 130.6 ns (the runs' controls: 127.8, 135.7, 135.0, 130.6, 125.5).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, d29170ba, 2026-09-20 04:44

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 130.6 ns.


Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| web/media-type.refused | 48.2 | regex | 61.1 | 1.27x | 0 | 0 | 2 % |
| web/media-type.refused | 48.2 | regex-compiled | 29.9 | 0.62x | 0 | 0 | 2 % |
| web/media-type.refused | 48.2 | reference-MediaTypeHeaderValue | 8.4 | reference | 0 | 0 | 2 % |
