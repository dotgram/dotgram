Median of 5 of 5 runs, each in a process of its own; control 31.1 ns (the runs' controls: 31.1, 31.1, 31.1, 31.2, 31.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 13:04

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 74.2 | 78.6 | 1.06x | 79.9 | 1.08x | +1.6% | 192 | 192 |
| fix/One.bytes | generated | 75.5 | 162.1 | 2.15x | 95.5 | 1.26x | -41.1% | 248 | 248 |
| fix/One.stream | generated | 152.0 | 551.4 | 3.63x | 542.3 | 3.57x | -1.6% | 888 | 888 |
| fix/Order.text | generated | 674.3 | 636.4 | 0.94x | 636.4 | 0.94x | 0.0% | 1096 | 1096 |
| fix/Order.bytes | generated | 670.7 | 1250.8 | 1.86x | 727.2 | 1.08x | -41.9% | 1152 | 1152 |
| fix/Order.stream | generated | 804.0 | 2801.2 | 3.48x | 2731.2 | 3.40x | -2.5% | 1712 | 1712 |
| fix/BinaryMany.bytes | generated | 3702.2 | 10231.9 | 2.76x | 4495.8 | 1.21x | -56.1% | 7312 | 7312 |
| fix/Orders128.bytes | generated | 80482.7 | 146568.0 | 1.82x | 86320.2 | 1.07x | -41.1% | 129168 | 129168 |
| fix/slope-0.bytes | generated | 31.1 | 70.0 | 2.25x | 36.4 | 1.17x | -48.0% | 144 | 120 |
| fix/slope-4.bytes | generated | 284.4 | 455.7 | 1.60x | 263.7 | 0.93x | -42.1% | 560 | 560 |
| fix/slope-16.bytes | generated | 975.8 | 1610.0 | 1.65x | 893.4 | 0.92x | -44.5% | 1808 | 1808 |
| fix/slope-0.stream | generated | 95.0 | 233.1 | 2.45x | 224.9 | 2.37x | -3.5% | 792 | 792 |
| fix/slope-16.stream | generated | 1197.4 | 3651.7 | 3.05x | 3489.4 | 2.91x | -4.4% | 2328 | 2328 |
| feeds/stock-count.small.text | generated | 167.1 | 224.9 | 1.35x | 222.2 | 1.33x | -1.2% | 608 | 608 |
| feeds/stock-count.small.reader | generated | 358.0 | 497.0 | 1.39x | 409.0 | 1.14x | -17.7% | 776 | 822 |
| feeds/stock-count.small.reader64 | generated | 318.4 | 510.3 | 1.60x | 409.3 | 1.29x | -19.8% | 824 | 824 |
| feeds/stock-count.good.text | generated | 36230.9 | 28276.8 | 0.78x | 28349.4 | 0.78x | +0.3% | 111312 | 111357 |
| feeds/stock-count.good.reader | generated | 69067.8 | 50543.9 | 0.73x | 40720.3 | 0.59x | -19.4% | 111480 | 111525 |
| feeds/stock-count.good.reader64 | generated | 85625.8 | 50686.6 | 0.59x | 40970.3 | 0.48x | -19.2% | 111528 | 111573 |
| feeds/stock-count.broken.text | generated | 36284.5 | 32511.1 | 0.90x | 32665.7 | 0.90x | +0.5% | 107408 | 107453 |
| feeds/stock-count.broken.reader | generated | 66165.2 | 56444.1 | 0.85x | 45840.0 | 0.69x | -18.8% | 107576 | 107621 |
| feeds/stock-count.broken.reader64 | generated | 81833.6 | 56551.0 | 0.69x | 46037.3 | 0.56x | -18.6% | 107624 | 107669 |
| el/string | generated | 263.9 | 1030.1 | 3.90x | 1029.7 | 3.90x | 0.0% | 1056 | 1056 |
| el/string | immediate | 263.9 | 655.0 | 2.48x | 652.0 | 2.47x | -0.5% | 1032 | 1032 |
| sql/select20 | generated | 6815.2 | 72493.7 | 10.64x | 71810.4 | 10.54x | -0.9% | 21448 | 21448 |
