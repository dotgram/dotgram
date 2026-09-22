Median of 5 of 5 runs, each in a process of its own; control 30.0 ns (the runs' controls: 29.8, 30.1, 30.0, 30.0, 30.0).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, 8f206d4b (run in a tree at aceedc78), 2026-09-22 10:53

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 30.0 ns.

A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.


Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fixmsg/Order44.parse | 940.4 | reference-QuickFIXn | 1463.6 | reference | 3112 | 5784 | 8 % |
| fixmsg/Order44.strict | 995.4 | reference-QuickFIXn | 2420.1 | reference | 3112 | 6808 | 10 % |
