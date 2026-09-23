Median of 5 of 5 runs, each in a process of its own; control 30.2 ns (the runs' controls: 30.2, 30.1, 30.1, 30.5, 30.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, 237b11bf, 2026-09-22 22:38

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.2 ns.

A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.


Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fixmsg/Order44.parse | 1024.5 | reference-QuickFIXn | 1459.0 | reference | 3048 | 5784 | 81 % |
| fixmsg/Order44.strict | 1121.0 | generated-loaded | 1236.0 | 1.10x | 3048 | 3048 | 2 % |
| fixmsg/Order44.strict | 1121.0 | reference-QuickFIXn | 2436.8 | reference | 3048 | 6808 | 2 % |
