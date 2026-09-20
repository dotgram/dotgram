Median of 4 of 5 runs, each in a process of its own; control 31.8 ns (the runs' controls: 34.0, 31.6, 31.6, 31.9, 33.1).
Dropped for a control more than 5% off the median: run 1.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, a08cfca0+changes, 2026-09-20 03:24

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.8 ns.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 79.6 | generated | 56.8 | 0.71x | 256 | 136 | 3 % |
| fix/One.text | 79.6 | regex-lesser | 154.6 | 1.94x | 256 | 680 | 3 % |
| fix/One.text | 79.6 | regex-compiled-lesser | 128.5 | 1.61x | 256 | 680 | 3 % |
| fix/One.bytes | 82.5 | generated | 84.2 | 1.02x | 256 | 192 | 10 % |
