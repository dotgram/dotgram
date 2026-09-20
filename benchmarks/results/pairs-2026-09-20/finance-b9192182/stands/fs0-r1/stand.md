Median of 5 of 5 runs, each in a process of its own; control 32.1 ns (the runs' controls: 32.1, 32.0, 32.7, 33.0, 32.1).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, a08cfca0+changes, 2026-09-20 03:22

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 32.1 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 77.5 | generated | 57.6 | 0.74x | 256 | 136 | 5 % |
| fix/One.text | 77.5 | regex-lesser | 156.8 | 2.02x | 256 | 680 | 5 % |
| fix/One.text | 77.5 | regex-compiled-lesser | 129.1 | 1.66x | 256 | 680 | 5 % |
| fix/One.bytes | 78.9 | generated | 78.6 | 1.00x | 256 | 192 | 5 % |
