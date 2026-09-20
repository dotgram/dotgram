# Stand, 1587b364, 2026-09-20 03:15

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.7 ns.

A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.


Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fixmsg/Order44.strict | 2611.2 | reference-QuickFIXn | 2797.3 | reference | 4192 | 6808 | 439 % |
| fixmsg/Order.parse | 1725.6 | — (N/A) | | | 3360 | | 13 % |
| fixmsg/Order.build | 1860.5 | — (N/A) | | | 3512 | | 3 % |
