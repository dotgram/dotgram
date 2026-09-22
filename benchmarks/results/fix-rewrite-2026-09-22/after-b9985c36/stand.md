Median of 5 of 5 runs, each in a process of its own; control 29.9 ns (the runs' controls: 30.0, 29.8, 29.9, 30.2, 29.9).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Stand, b9985c36 (run in a tree at 0e948b69), 2026-09-22 10:25

IGOR-DESKTOP, .NET 10.0.12, pinned to 0-15, high priority, control 29.9 ns.

A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.

| row | hand ns | reading | ns | /hand | hand B | B | hand spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | 61.4 | generated | 46.3 | 0.75x | 256 | 136 | 8 % |
| fix/One.text | 61.4 | regex-lesser | 126.2 | 2.06x | 256 | 680 | 8 % |
| fix/One.text | 61.4 | regex-compiled-lesser | 97.1 | 1.58x | 256 | 680 | 8 % |
| fix/One.bytes | 60.9 | generated | 67.4 | 1.11x | 256 | 192 | 8 % |
| fix/One.stream | 128.1 | generated | 102.6 | 0.80x | 4504 | 416 | 12 % |
| fix/Order.text | 535.1 | generated | 411.3 | 0.77x | 920 | 800 | 6 % |
| fix/Order.text | 535.1 | regex-lesser | 1183.6 | 2.21x | 920 | 5960 | 6 % |
| fix/Order.text | 535.1 | regex-compiled-lesser | 925.6 | 1.73x | 920 | 5960 | 6 % |
| fix/Order.bytes | 551.5 | generated | 532.1 | 0.96x | 920 | 856 | 4 % |
| fix/Order.stream | 669.2 | generated | 585.1 | 0.87x | 5088 | 1000 | 6 % |
| fix/BinaryMany.text | 2954.6 | generated | 2656.9 | 0.90x | 5808 | 5688 | 14 % |
| fix/BinaryMany.bytes | 2989.3 | generated | 3083.6 | 1.03x | 5808 | 5744 | 4 % |
| fix/BinaryMany.stream | 3983.8 | generated | 3305.0 | 0.83x | 9552 | 5464 | 4 % |
| fix/Orders128.text | 64715.9 | generated | 48692.3 | 0.75x | 95408 | 95288 | 6 % |
| fix/Orders128.text | 64715.9 | regex-lesser | 150055.1 | 2.32x | 95408 | 742728 | 6 % |
| fix/Orders128.text | 64715.9 | regex-compiled-lesser | 114146.1 | 1.76x | 95408 | 742728 | 6 % |
| fix/Orders128.bytes | 65352.5 | generated | 61591.8 | 0.94x | 95408 | 95344 | 4 % |
| fix/Orders128.stream | 99736.4 | generated | 66011.9 | 0.66x | 88400 | 84312 | 2 % |
| fix/OrderMalformed.text | 554.5 | generated | 409.0 | 0.74x | 976 | 952 | 4 % |
| fix/OrderMalformed.bytes | 554.6 | generated | 531.7 | 0.96x | 976 | 1008 | 8 % |
| fix/OrderMalformed.stream | 673.4 | generated | 591.8 | 0.88x | 5144 | 1152 | 4 % |
| fix/slope-0.text | 19.5 | generated | 15.2 | 0.78x | 152 | 32 | 7 % |
| fix/slope-0.text | 19.5 | ideal | 7.2 | 0.37x | 152 | 88 | 7 % |
| fix/slope-0.text | 19.5 | regex-lesser | 25.1 | 1.29x | 152 | 120 | 7 % |
| fix/slope-0.text | 19.5 | regex-compiled-lesser | 24.0 | 1.23x | 152 | 120 | 7 % |
| fix/slope-1.text | 70.4 | generated | 57.8 | 0.82x | 296 | 176 | 6 % |
| fix/slope-1.text | 70.4 | ideal | 42.9 | 0.61x | 296 | 232 | 6 % |
| fix/slope-1.text | 70.4 | regex-lesser | 127.8 | 1.81x | 296 | 680 | 6 % |
| fix/slope-1.text | 70.4 | regex-compiled-lesser | 98.5 | 1.40x | 296 | 680 | 6 % |
| fix/slope-2.text | 123.0 | generated | 100.5 | 0.82x | 432 | 312 | 5 % |
| fix/slope-2.text | 123.0 | ideal | 78.9 | 0.64x | 432 | 368 | 5 % |
| fix/slope-2.text | 123.0 | regex-lesser | 228.6 | 1.86x | 432 | 1184 | 5 % |
| fix/slope-2.text | 123.0 | regex-compiled-lesser | 175.4 | 1.43x | 432 | 1184 | 5 % |
| fix/slope-4.text | 220.2 | generated | 175.2 | 0.80x | 616 | 496 | 7 % |
| fix/slope-4.text | 220.2 | ideal | 140.5 | 0.64x | 616 | 552 | 7 % |
| fix/slope-4.text | 220.2 | regex-lesser | 431.0 | 1.96x | 616 | 2192 | 7 % |
| fix/slope-4.text | 220.2 | regex-compiled-lesser | 327.7 | 1.49x | 616 | 2192 | 7 % |
| fix/slope-8.text | 389.2 | generated | 311.1 | 0.80x | 992 | 872 | 7 % |
| fix/slope-8.text | 389.2 | ideal | 251.8 | 0.65x | 992 | 840 | 7 % |
| fix/slope-8.text | 389.2 | regex-lesser | 881.9 | 2.27x | 992 | 4296 | 7 % |
| fix/slope-8.text | 389.2 | regex-compiled-lesser | 672.3 | 1.73x | 992 | 4296 | 7 % |
| fix/slope-16.text | 769.4 | generated | 584.5 | 0.76x | 1744 | 1624 | 8 % |
| fix/slope-16.text | 769.4 | ideal | 512.6 | 0.67x | 1744 | 1680 | 8 % |
| fix/slope-16.text | 769.4 | regex-lesser | 1728.4 | 2.25x | 1744 | 8480 | 8 % |
| fix/slope-16.text | 769.4 | regex-compiled-lesser | 1307.2 | 1.70x | 1744 | 8480 | 8 % |
| fix/slope-0.bytes | 23.0 | generated | 32.9 | 1.43x | 176 | 112 | 22 % |
| fix/slope-1.bytes | 77.1 | generated | 88.2 | 1.14x | 328 | 264 | 4 % |
| fix/slope-2.bytes | 130.6 | generated | 140.5 | 1.08x | 472 | 408 | 6 % |
| fix/slope-4.bytes | 228.1 | generated | 235.8 | 1.03x | 672 | 608 | 5 % |
| fix/slope-8.bytes | 404.2 | generated | 403.1 | 1.00x | 1072 | 1008 | 6 % |
| fix/slope-16.bytes | 786.5 | generated | 738.0 | 0.94x | 1880 | 1816 | 7 % |
| fix/slope-0.stream | 79.6 | generated | 58.8 | 0.74x | 4456 | 368 | 12 % |
| fix/slope-1.stream | 144.8 | generated | 118.8 | 0.82x | 4576 | 488 | 7 % |
| fix/slope-2.stream | 200.9 | generated | 175.7 | 0.87x | 4712 | 624 | 8 % |
| fix/slope-4.stream | 306.6 | generated | 278.9 | 0.91x | 4896 | 808 | 7 % |
| fix/slope-8.stream | 527.4 | generated | 455.8 | 0.86x | 5264 | 1176 | 6 % |
| fix/slope-16.stream | 983.3 | generated | 820.6 | 0.83x | 6008 | 1920 | 4 % |

Rows with no hand-written parser, so the base is the generated reading:

| row | generated ns | reading | ns | /generated | generated B | B | generated spread |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| fixmsg/slope-8 | 412.9 | — (N/A) | | | 1248 | | 7 % |
| fixmsg/slope-12 | 577.8 | — (N/A) | | | 1600 | | 7 % |
| fixmsg/slope-13 | 620.8 | — (N/A) | | | 1688 | | 7 % |
| fixmsg/slope-20 | 957.8 | — (N/A) | | | 2352 | | 6 % |
| fixmsg/slope-21 | 995.3 | — (N/A) | | | 2448 | | 7 % |
| fixmsg/slope-36 | 1655.8 | — (N/A) | | | 3888 | | 7 % |
| fixmsg/slope-37 | 1692.4 | — (N/A) | | | 3984 | | 7 % |
| fixmsg/slope-68 | 3085.5 | — (N/A) | | | 6960 | | 9 % |
| fixmsg/slope-69 | 3125.8 | — (N/A) | | | 7056 | | 8 % |
| fixmsg/slope-132 | 5970.7 | — (N/A) | | | 13104 | | 9 % |
| fixmsg/Order44.strict | 949.6 | reference-QuickFIXn | 2426.3 | reference | 3112 | 6808 | 8 % |
| fixmsg/Order.parse | 909.2 | — (N/A) | | | 3048 | | 6 % |
| fixmsg/Order.build | 1119.5 | — (N/A) | | | 3200 | | 5 % |
