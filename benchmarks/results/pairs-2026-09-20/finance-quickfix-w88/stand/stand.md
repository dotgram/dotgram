Median of 6 of 7 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.4, 31.5, 33.4, 31.5, 31.4, 31.5, 31.7).
Dropped for a control more than 5% off the median: run 3.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, 003a79af, 2026-09-20 03:14

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.5 ns.

A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.


Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fixmsg/Order44.strict | 1919.0 | reference-QuickFIXn | 2841.7 | reference | 4192 | 6808 | 37 % |
| fixmsg/Order.parse | 1726.6 | — (N/A) | | | 3360 | | 4 % |
| fixmsg/Order.build | 1874.6 | — (N/A) | | | 3512 | | 3 % |
