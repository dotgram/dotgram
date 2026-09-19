# Paired first calls, 2026-09-19 15:55

First call in a fresh process, median of 5: milliseconds, methods the runtime compiled during it, and the time it spent compiling them (JitInfo, no events). The hand reading is this build's own.

| row | reading | ms | methods | JIT ms |
| --- | --- | ---: | ---: | ---: |
| tsql/columns1000 | hand | 51.26 | 597 | 34.28 |
| tsql/columns1000 | before | 54.97 | 199 | 52.34 |
| tsql/columns1000 | after | 36.81 | 209 | 34.47 |
| tsql/conditions1000 | hand | 51.89 | 603 | 33.91 |
| tsql/conditions1000 | before | 62.59 | 210 | 58.51 |
| tsql/conditions1000 | after | 37.77 | 223 | 34.31 |
| sql/literal | hand | 1.54 | 23 | 1.40 |
| sql/literal | before | 13.40 | 39 | 13.03 |
| sql/literal | after | 13.22 | 39 | 12.86 |
| sql/conditions100 | hand | 13.25 | 160 | 12.55 |
| sql/conditions100 | before | 62.01 | 293 | 59.91 |
| sql/conditions100 | after | 40.94 | 315 | 39.09 |
| sql/conditions1000 | hand | 15.42 | 160 | 12.62 |
| sql/conditions1000 | before | 73.52 | 293 | 60.62 |
| sql/conditions1000 | after | 49.19 | 315 | 38.74 |
| tsql/comment | hand | 47.66 | 648 | 36.33 |
| tsql/comment | before | 60.41 | 229 | 59.12 |
| tsql/comment | after | 37.16 | 243 | 35.84 |
| sql/column | hand | 2.12 | 31 | 1.98 |
| sql/column | before | 34.76 | 55 | 34.36 |
| sql/column | after | 19.28 | 59 | 18.86 |
| sql/select20 | hand | 17.61 | 216 | 16.93 |
| sql/select20 | before | 64.61 | 395 | 63.37 |
| sql/select20 | after | 44.10 | 434 | 42.90 |
