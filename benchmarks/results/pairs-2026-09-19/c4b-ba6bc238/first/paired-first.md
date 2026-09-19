# Paired first calls, 2026-09-19 10:43

First call in a fresh process, median of 5: milliseconds, methods the runtime compiled during it, and the time it spent compiling them (JitInfo, no events). The hand reading is this build's own.

| row | reading | ms | methods | JIT ms |
| --- | --- | ---: | ---: | ---: |
| feeds/stock-count.small.text | hand | 1.22 | 22 | 1.07 |
| feeds/stock-count.small.text | before | 3.55 | 58 | 3.28 |
| feeds/stock-count.small.text | after | 2.93 | 53 | 2.67 |
| feeds/stock-count.small.reader | hand | 1.22 | 22 | 1.06 |
| feeds/stock-count.small.reader | before | 4.07 | 68 | 3.72 |
| feeds/stock-count.small.reader | after | 4.03 | 68 | 3.68 |
| feeds/stock-count.small.reader64 | hand | 1.20 | 22 | 1.05 |
| feeds/stock-count.small.reader64 | before | 4.09 | 68 | 3.74 |
| feeds/stock-count.small.reader64 | after | 4.04 | 68 | 3.69 |
| feeds/stock-count.good.reader | hand | 1.49 | 21 | 1.06 |
| feeds/stock-count.good.reader | before | 4.36 | 65 | 3.65 |
| feeds/stock-count.good.reader | after | 4.35 | 65 | 3.62 |
| feeds/stock-count.good.reader64 | hand | 1.48 | 21 | 1.06 |
| feeds/stock-count.good.reader64 | before | 4.31 | 65 | 3.59 |
| feeds/stock-count.good.reader64 | after | 4.26 | 65 | 3.56 |
| feeds/stock-count.broken.reader | hand | 1.47 | 22 | 1.07 |
| feeds/stock-count.broken.reader | before | 4.81 | 74 | 4.02 |
| feeds/stock-count.broken.reader | after | 4.74 | 74 | 3.98 |
| feeds/stock-count.broken.reader64 | hand | 1.49 | 22 | 1.07 |
| feeds/stock-count.broken.reader64 | before | 4.80 | 74 | 3.99 |
| feeds/stock-count.broken.reader64 | after | 4.79 | 74 | 3.97 |
| feeds/recovering.good | hand | 4.30 | 59 | 3.63 |
| feeds/recovering.good | before | 4.31 | 59 | 3.63 |
| feeds/recovering.good | after | 3.24 | 51 | 2.71 |
