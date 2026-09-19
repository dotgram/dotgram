# Paired first calls, 2026-09-19 16:14

First call in a fresh process, median of 5: milliseconds, methods the runtime compiled during it, and the time it spent compiling them (JitInfo, no events). The hand reading is this build's own.

| row | reading | ms | methods | JIT ms |
| --- | --- | ---: | ---: | ---: |
| tsql/columns1000 | hand | 52.07 | 597 | 34.92 |
| tsql/columns1000 | before | 54.45 | 199 | 51.79 |
| tsql/columns1000 | after | 45.90 | 135 | 43.50 |
| tsql/conditions1000 | hand | 52.74 | 603 | 34.31 |
| tsql/conditions1000 | before | 62.52 | 210 | 58.41 |
| tsql/conditions1000 | after | 52.34 | 151 | 48.67 |
| sql/literal | hand | 1.50 | 23 | 1.37 |
| sql/literal | before | 13.25 | 39 | 12.89 |
| sql/literal | after | 13.43 | 39 | 13.05 |
| sql/conditions100 | hand | 13.06 | 160 | 12.38 |
| sql/conditions100 | before | 61.72 | 293 | 59.66 |
| sql/conditions100 | after | 57.37 | 253 | 55.30 |
| sql/conditions1000 | hand | 15.24 | 160 | 12.53 |
| sql/conditions1000 | before | 72.73 | 293 | 60.12 |
| sql/conditions1000 | after | 67.60 | 253 | 55.50 |
| tsql/comment | hand | 47.22 | 648 | 36.20 |
| tsql/comment | before | 60.50 | 229 | 59.18 |
| tsql/comment | after | 51.92 | 163 | 50.76 |
| sql/column | hand | 2.10 | 31 | 1.95 |
| sql/column | before | 34.95 | 55 | 34.53 |
| sql/column | after | 34.89 | 55 | 34.46 |
| sql/select20 | hand | 17.38 | 216 | 16.71 |
| sql/select20 | before | 64.54 | 395 | 63.35 |
| sql/select20 | after | 60.22 | 353 | 59.04 |
