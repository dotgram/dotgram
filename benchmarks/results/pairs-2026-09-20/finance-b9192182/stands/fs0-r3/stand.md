Median of 5 of 5 runs, each in a process of its own; control 32.3 ns (the runs' controls: 33.3, 32.2, 32.5, 32.3, 32.1).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, a08cfca0+changes, 2026-09-20 03:26

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 32.3 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 81.7 | generated | 57.4 | 0.70x | 256 | 136 | 23 % |
| fix/One.text | 81.7 | regex-lesser | 155.7 | 1.90x | 256 | 680 | 23 % |
| fix/One.text | 81.7 | regex-compiled-lesser | 128.5 | 1.57x | 256 | 680 | 23 % |
| fix/One.bytes | 78.7 | generated | 82.0 | 1.04x | 256 | 192 | 8 % |
