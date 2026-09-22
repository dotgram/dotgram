Median of 5 of 5 runs, each in a process of its own; control 30.5 ns (the runs' controls: 29.9, 30.2, 30.5, 30.8, 30.6).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, 156d16cd (run in a tree at 38ac14af), 2026-09-22 11:11

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.5 ns.

A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.


Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fixmsg/Order44.strict | 1020.5 | generated-loaded | 1039.2 | 1.02x | 3112 | 3112 | 3 % |
| fixmsg/Order44.strict | 1020.5 | reference-QuickFIXn | 2442.9 | reference | 3112 | 6808 | 3 % |
