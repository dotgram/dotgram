Median of 5 of 5 runs, each in a process of its own; control 29.8 ns (the runs' controls: 29.8, 29.9, 29.8, 29.7, 29.7).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, 49ecdaf9, 2026-09-22 10:14

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 29.8 ns.

A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 62.0 | generated | 45.6 | 0.74x | 256 | 136 | 18 % |
| fix/One.text | 62.0 | regex-lesser | 126.1 | 2.03x | 256 | 680 | 18 % |
| fix/One.text | 62.0 | regex-compiled-lesser | 97.1 | 1.57x | 256 | 680 | 18 % |
| fix/One.bytes | 58.8 | generated | 65.1 | 1.11x | 256 | 192 | 3 % |
| fix/One.stream | 124.3 | generated | 101.3 | 0.82x | 4504 | 408 | 4 % |
| fix/Order.text | 553.5 | generated | 397.6 | 0.72x | 920 | 800 | 3 % |
| fix/Order.text | 553.5 | regex-lesser | 1175.3 | 2.12x | 920 | 5960 | 3 % |
| fix/Order.text | 553.5 | regex-compiled-lesser | 890.2 | 1.61x | 920 | 5960 | 3 % |
| fix/Order.bytes | 530.7 | generated | 516.4 | 0.97x | 920 | 856 | 6 % |
| fix/Order.stream | 652.5 | generated | 578.8 | 0.89x | 5088 | 992 | 3 % |
| fix/BinaryMany.text | 2950.5 | generated | 2591.3 | 0.88x | 5808 | 5688 | 6 % |
| fix/BinaryMany.bytes | 3006.3 | generated | 3022.9 | 1.01x | 5808 | 5744 | 5 % |
| fix/BinaryMany.stream | 3913.2 | generated | 3291.6 | 0.84x | 9552 | 5456 | 1 % |
| fix/Orders128.text | 66713.4 | generated | 48589.7 | 0.73x | 95408 | 95288 | 4 % |
| fix/Orders128.text | 66713.4 | regex-lesser | 148314.7 | 2.22x | 95408 | 742728 | 4 % |
| fix/Orders128.text | 66713.4 | regex-compiled-lesser | 111670.5 | 1.67x | 95408 | 742728 | 4 % |
| fix/Orders128.bytes | 63381.8 | generated | 60286.9 | 0.95x | 95408 | 95344 | 5 % |
| fix/Orders128.stream | 98729.3 | generated | 65776.4 | 0.67x | 88400 | 84304 | 4 % |
| fix/OrderMalformed.text | 554.8 | generated | 404.2 | 0.73x | 976 | 952 | 8 % |
| fix/OrderMalformed.bytes | 543.5 | generated | 516.4 | 0.95x | 976 | 1008 | 6 % |
| fix/OrderMalformed.stream | 673.1 | generated | 587.5 | 0.87x | 5144 | 1144 | 4 % |
| fix/slope-0.text | 19.5 | generated | 14.9 | 0.76x | 152 | 32 | 4 % |
| fix/slope-0.text | 19.5 | ideal | 7.0 | 0.36x | 152 | 88 | 4 % |
| fix/slope-0.text | 19.5 | regex-lesser | 23.9 | 1.23x | 152 | 120 | 4 % |
| fix/slope-0.text | 19.5 | regex-compiled-lesser | 23.4 | 1.20x | 152 | 120 | 4 % |
| fix/slope-1.text | 72.1 | generated | 55.8 | 0.77x | 296 | 176 | 4 % |
| fix/slope-1.text | 72.1 | ideal | 44.2 | 0.61x | 296 | 232 | 4 % |
| fix/slope-1.text | 72.1 | regex-lesser | 124.9 | 1.73x | 296 | 680 | 4 % |
| fix/slope-1.text | 72.1 | regex-compiled-lesser | 96.7 | 1.34x | 296 | 680 | 4 % |
| fix/slope-2.text | 127.0 | generated | 99.0 | 0.78x | 432 | 312 | 6 % |
| fix/slope-2.text | 127.0 | ideal | 81.1 | 0.64x | 432 | 368 | 6 % |
| fix/slope-2.text | 127.0 | regex-lesser | 224.7 | 1.77x | 432 | 1184 | 6 % |
| fix/slope-2.text | 127.0 | regex-compiled-lesser | 170.7 | 1.34x | 432 | 1184 | 6 % |
| fix/slope-4.text | 230.8 | generated | 176.0 | 0.76x | 616 | 496 | 6 % |
| fix/slope-4.text | 230.8 | ideal | 144.7 | 0.63x | 616 | 552 | 6 % |
| fix/slope-4.text | 230.8 | regex-lesser | 422.0 | 1.83x | 616 | 2192 | 6 % |
| fix/slope-4.text | 230.8 | regex-compiled-lesser | 316.1 | 1.37x | 616 | 2192 | 6 % |
| fix/slope-8.text | 410.2 | generated | 310.5 | 0.76x | 992 | 872 | 6 % |
| fix/slope-8.text | 410.2 | ideal | 259.5 | 0.63x | 992 | 840 | 6 % |
| fix/slope-8.text | 410.2 | regex-lesser | 864.5 | 2.11x | 992 | 4296 | 6 % |
| fix/slope-8.text | 410.2 | regex-compiled-lesser | 655.2 | 1.60x | 992 | 4296 | 6 % |
| fix/slope-16.text | 808.8 | generated | 585.0 | 0.72x | 1744 | 1624 | 6 % |
| fix/slope-16.text | 808.8 | ideal | 518.6 | 0.64x | 1744 | 1680 | 6 % |
| fix/slope-16.text | 808.8 | regex-lesser | 1704.1 | 2.11x | 1744 | 8480 | 6 % |
| fix/slope-16.text | 808.8 | regex-compiled-lesser | 1278.2 | 1.58x | 1744 | 8480 | 6 % |
| fix/slope-0.bytes | 23.0 | generated | 32.2 | 1.40x | 176 | 112 | 22 % |
| fix/slope-1.bytes | 75.7 | generated | 86.8 | 1.15x | 328 | 264 | 9 % |
| fix/slope-2.bytes | 127.3 | generated | 137.3 | 1.08x | 472 | 408 | 8 % |
| fix/slope-4.bytes | 222.7 | generated | 232.5 | 1.04x | 672 | 608 | 6 % |
| fix/slope-8.bytes | 395.9 | generated | 394.8 | 1.00x | 1072 | 1008 | 6 % |
| fix/slope-16.bytes | 764.2 | generated | 720.7 | 0.94x | 1880 | 1816 | 6 % |
| fix/slope-0.stream | 75.1 | generated | 58.1 | 0.77x | 4456 | 360 | 5 % |
| fix/slope-1.stream | 139.9 | generated | 118.2 | 0.84x | 4576 | 480 | 4 % |
| fix/slope-2.stream | 197.4 | generated | 171.9 | 0.87x | 4712 | 616 | 5 % |
| fix/slope-4.stream | 299.7 | generated | 271.3 | 0.91x | 4896 | 800 | 4 % |
| fix/slope-8.stream | 516.7 | generated | 452.6 | 0.88x | 5264 | 1168 | 5 % |
| fix/slope-16.stream | 967.7 | generated | 808.2 | 0.84x | 6008 | 1912 | 4 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fixmsg/slope-8 | 544.9 | — (N/A) | | | 1984 | | 5 % |
| fixmsg/slope-12 | 765.0 | — (N/A) | | | 2968 | | 4 % |
| fixmsg/slope-13 | 838.2 | — (N/A) | | | 3792 | | 3 % |
| fixmsg/slope-20 | 1222.2 | — (N/A) | | | 4960 | | 5 % |
| fixmsg/slope-21 | 1304.8 | — (N/A) | | | 6432 | | 4 % |
| fixmsg/slope-36 | 2082.4 | — (N/A) | | | 8952 | | 5 % |
| fixmsg/slope-37 | 2198.5 | — (N/A) | | | 11704 | | 5 % |
| fixmsg/slope-68 | 3824.0 | — (N/A) | | | 16912 | | 5 % |
| fixmsg/slope-69 | 3974.6 | — (N/A) | | | 22224 | | 5 % |
| fixmsg/slope-132 | 7283.9 | — (N/A) | | | 32808 | | 5 % |
| fixmsg/Order44.strict | 1146.1 | reference-QuickFIXn | 2398.6 | reference | 4256 | 6808 | 2 % |
| fixmsg/Order.parse | 1052.5 | — (N/A) | | | 3456 | | 2 % |
| fixmsg/Order.build | 1271.3 | — (N/A) | | | 3608 | | 4 % |
