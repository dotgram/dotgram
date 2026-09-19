# Paired first calls, 2026-09-19 17:00

First call in a fresh process, median of 5: milliseconds, methods the runtime compiled during it, and the time it spent compiling them (JitInfo, no events). The hand reading is this build's own.

| row | reading | ms | methods | JIT ms |
| --- | --- | ---: | ---: | ---: |
| tsql/columns1000 | hand | 51.18 | 597 | 34.27 |
| tsql/columns1000 | before | 46.01 | 135 | 43.52 |
| tsql/columns1000 | after | 45.86 | 139 | 43.42 |
| tsql/conditions1000 | hand | 52.09 | 603 | 33.93 |
| tsql/conditions1000 | before | 52.58 | 151 | 48.86 |
| tsql/conditions1000 | after | 52.60 | 155 | 48.96 |
| sql/literal | hand | 1.54 | 23 | 1.40 |
| sql/literal | before | 13.16 | 39 | 12.80 |
| sql/literal | after | 12.94 | 39 | 12.59 |
| sql/conditions100 | hand | 13.25 | 160 | 12.55 |
| sql/conditions100 | before | 57.89 | 253 | 55.86 |
| sql/conditions100 | after | 57.25 | 256 | 55.28 |
| sql/conditions1000 | hand | 15.20 | 160 | 12.52 |
| sql/conditions1000 | before | 67.99 | 253 | 55.57 |
| sql/conditions1000 | after | 67.57 | 256 | 55.22 |
| tsql/comment | hand | 47.40 | 648 | 36.40 |
| tsql/comment | before | 52.07 | 163 | 50.82 |
| tsql/comment | after | 51.70 | 169 | 50.53 |
| sql/column | hand | 2.11 | 31 | 1.97 |
| sql/column | before | 34.81 | 55 | 34.39 |
| sql/column | after | 34.94 | 57 | 34.54 |
| sql/select20 | hand | 17.33 | 216 | 16.68 |
| sql/select20 | before | 60.09 | 353 | 58.91 |
| sql/select20 | after | 60.01 | 358 | 58.86 |
