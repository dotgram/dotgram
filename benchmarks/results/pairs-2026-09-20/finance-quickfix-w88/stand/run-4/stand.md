# Stand, 1587b364, 2026-09-20 03:14

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 31.5 ns.

A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.


Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fixmsg/Order44.strict | 1905.6 | reference-QuickFIXn | 2850.4 | reference | 4192 | 6808 | 68 % |
| fixmsg/Order.parse | 1686.7 | — (N/A) | | | 3360 | | 9 % |
| fixmsg/Order.build | 1866.0 | — (N/A) | | | 3512 | | 13 % |
