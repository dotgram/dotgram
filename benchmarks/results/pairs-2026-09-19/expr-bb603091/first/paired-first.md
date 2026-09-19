# Paired first calls, 2026-09-19 17:16

First call in a fresh process, median of 5: milliseconds, methods the runtime compiled during it, and the time it spent compiling them (JitInfo, no events). The hand reading is this build's own.

| row | reading | ms | methods | JIT ms |
| --- | --- | ---: | ---: | ---: |
| web/sf.list10000 | hand | 18.49 | 64 | 12.80 |
| web/sf.list10000 | before | 18.25 | 63 | 12.49 |
| web/sf.list10000 | after | 15.90 | 82 | 9.72 |
| el/ladder.bool | hand | 15.69 | 175 | 13.46 |
| el/ladder.bool | before | 27.21 | 235 | 24.71 |
| el/ladder.bool | after | 24.40 | 244 | 21.92 |
| sql/select20.bool | hand | 17.39 | 216 | 16.73 |
| sql/select20.bool | before | 44.01 | 434 | 42.74 |
| sql/select20.bool | after | 43.95 | 429 | 42.72 |
| web/addr-spec.plain | hand | 3.53 | 39 | 3.45 |
| web/addr-spec.plain | before | 3.31 | 38 | 3.19 |
| web/addr-spec.plain | after | 3.30 | 38 | 3.18 |
| web/sf.list | hand | 7.78 | 118 | 7.38 |
| web/sf.list | before | 7.46 | 117 | 7.02 |
| web/sf.list | after | 7.95 | 133 | 7.49 |
| el/ladder | hand | 15.94 | 174 | 13.69 |
| el/ladder | before | 27.38 | 235 | 24.88 |
| el/ladder | after | 24.91 | 251 | 22.35 |
| el/ladder | before-immediate | 18.91 | 208 | 16.45 |
| el/ladder | after-immediate | 18.80 | 208 | 16.38 |
| el/terms100 | hand | 14.54 | 165 | 12.40 |
| el/terms100 | before | 26.50 | 219 | 24.02 |
| el/terms100 | after | 23.82 | 229 | 21.37 |
| el/terms100 | before-immediate | 18.06 | 192 | 15.59 |
| el/terms100 | after-immediate | 17.99 | 192 | 15.55 |
| el/terms1000 | hand | 15.10 | 165 | 12.35 |
| el/terms1000 | before | 27.41 | 219 | 24.05 |
| el/terms1000 | after | 24.72 | 229 | 21.31 |
| el/terms1000 | before-immediate | 18.66 | 192 | 15.47 |
| el/terms1000 | after-immediate | 18.95 | 192 | 15.72 |
| sql/select20 | hand | 17.61 | 216 | 16.96 |
| sql/select20 | before | 44.68 | 434 | 43.43 |
| sql/select20 | after | 43.55 | 434 | 42.36 |
