Median of 5 of 5 runs, each in a process of its own; control 32.2 ns (the runs' controls: 32.9, 31.9, 32.2, 32.4, 31.9).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, a08cfca0+changes, 2026-09-20 03:25

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 32.2 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 78.8 | generated | 58.2 | 0.74x | 256 | 136 | 6 % |
| fix/One.text | 78.8 | regex-lesser | 155.1 | 1.97x | 256 | 680 | 6 % |
| fix/One.text | 78.8 | regex-compiled-lesser | 128.0 | 1.62x | 256 | 680 | 6 % |
| fix/One.bytes | 78.1 | generated | 81.1 | 1.04x | 256 | 192 | 5 % |
