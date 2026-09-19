# Paired first calls, 2026-09-19 13:04

First call in a fresh process, median of 5: milliseconds, methods the runtime compiled during it, and the time it spent compiling them (JitInfo, no events). The hand reading is this build's own.

| row | reading | ms | methods | JIT ms |
| --- | --- | ---: | ---: | ---: |
| fix/One.bytes | hand | 4.24 | 42 | 4.03 |
| fix/One.bytes | before | 6.36 | 81 | 6.13 |
| fix/One.bytes | after | 5.24 | 73 | 5.04 |
| fix/One.stream | hand | 4.52 | 51 | 4.41 |
| fix/One.stream | before | 7.57 | 107 | 7.22 |
| fix/One.stream | after | 7.60 | 107 | 7.25 |
| fix/Order.bytes | hand | 5.71 | 78 | 5.30 |
| fix/Order.bytes | before | 7.86 | 121 | 7.53 |
| fix/Order.bytes | after | 6.53 | 107 | 6.25 |
| fix/slope-0.bytes | hand | 1.68 | 14 | 1.53 |
| fix/slope-0.bytes | before | 3.03 | 38 | 2.93 |
| fix/slope-0.bytes | after | 2.12 | 31 | 2.02 |
| fix/slope-16.stream | hand | 4.52 | 51 | 4.40 |
| fix/slope-16.stream | before | 7.72 | 108 | 7.32 |
| fix/slope-16.stream | after | 7.79 | 108 | 7.38 |
| feeds/stock-count.small.reader | hand | 1.21 | 22 | 1.05 |
| feeds/stock-count.small.reader | before | 4.04 | 67 | 3.69 |
| feeds/stock-count.small.reader | after | 3.23 | 59 | 2.91 |
| feeds/stock-count.small.reader64 | hand | 1.20 | 22 | 1.05 |
| feeds/stock-count.small.reader64 | before | 4.03 | 67 | 3.68 |
| feeds/stock-count.small.reader64 | after | 3.20 | 59 | 2.89 |
| feeds/stock-count.good.reader | hand | 1.44 | 21 | 1.03 |
| feeds/stock-count.good.reader | before | 4.21 | 64 | 3.50 |
| feeds/stock-count.good.reader | after | 3.09 | 50 | 2.50 |
| feeds/stock-count.good.reader64 | hand | 1.47 | 21 | 1.04 |
| feeds/stock-count.good.reader64 | before | 4.24 | 64 | 3.55 |
| feeds/stock-count.good.reader64 | after | 3.12 | 50 | 2.54 |
| feeds/stock-count.broken.reader | hand | 1.46 | 22 | 1.05 |
| feeds/stock-count.broken.reader | before | 4.69 | 73 | 3.91 |
| feeds/stock-count.broken.reader | after | 3.56 | 59 | 2.91 |
| feeds/stock-count.broken.reader64 | hand | 1.46 | 22 | 1.05 |
| feeds/stock-count.broken.reader64 | before | 4.67 | 73 | 3.89 |
| feeds/stock-count.broken.reader64 | after | 3.59 | 59 | 2.92 |
