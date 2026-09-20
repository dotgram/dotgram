Median of 5 of 5 runs, each in a process of its own; control 31.9 ns (the runs' controls: 32.4, 32.3, 31.6, 31.9, 31.7).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, a08cfca0+changes, 2026-09-20 03:27

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.9 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 78.3 | generated | 57.1 | 0.73x | 256 | 136 | 8 % |
| fix/One.text | 78.3 | regex-lesser | 155.8 | 1.99x | 256 | 680 | 8 % |
| fix/One.text | 78.3 | regex-compiled-lesser | 128.7 | 1.64x | 256 | 680 | 8 % |
| fix/One.bytes | 77.8 | generated | 82.6 | 1.06x | 256 | 192 | 8 % |
