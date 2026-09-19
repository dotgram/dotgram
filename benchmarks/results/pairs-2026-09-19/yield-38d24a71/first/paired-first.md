# Paired first calls, 2026-09-19 18:10

First call in a fresh process, median of 5: milliseconds, methods the runtime compiled during it, and the time it spent compiling them (JitInfo, no events). The hand reading is this build's own.

| row | reading | ms | methods | JIT ms |
| --- | --- | ---: | ---: | ---: |
| fix/One.stream | hand | 4.52 | 51 | 4.40 |
| fix/One.stream | before | 7.05 | 105 | 6.71 |
| fix/One.stream | after | 4.88 | 78 | 4.62 |
| fix/Order.stream | hand | 5.73 | 85 | 5.55 |
| fix/Order.stream | before | 8.42 | 139 | 7.96 |
| fix/Order.stream | after | 6.25 | 112 | 5.89 |
| fix/slope-0.stream | hand | 1.88 | 22 | 1.82 |
| fix/slope-0.stream | before | 1.51 | 27 | 1.37 |
| fix/slope-0.stream | after | 1.51 | 27 | 1.37 |
| fix/slope-16.stream | hand | 4.64 | 51 | 4.51 |
| fix/slope-16.stream | before | 7.39 | 106 | 6.98 |
| fix/slope-16.stream | after | 5.05 | 79 | 4.75 |
| fix/One.yield-reader | hand | 4.57 | 51 | 4.46 |
| fix/One.yield-reader | before | 7.04 | 105 | 6.73 |
| fix/One.yield-reader | after | 4.88 | 78 | 4.66 |
| fix/slope-16.yield-reader | hand | 4.61 | 51 | 4.49 |
| fix/slope-16.yield-reader | before | 7.18 | 106 | 6.84 |
| fix/slope-16.yield-reader | after | 4.97 | 79 | 4.71 |
