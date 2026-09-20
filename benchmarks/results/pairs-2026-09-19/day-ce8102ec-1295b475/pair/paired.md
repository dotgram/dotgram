Median of 4 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 35.4, 31.3, 31.3, 31.1, 31.5).
Dropped for a control more than 5% off the median: run 1.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 19:59

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | before B | after B |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | 75.9 | 83.2 | 1.10x | 84.9 | 1.12x | +2.1% | 192 | 192 |
| tsql/script100 | generated | 661118.8 | 1152803.1 | 1.74x | 1159412.5 | 1.75x | +0.6% | 113600 | 113600 |
| tsql/script400 | generated | 2681273.4 | 17315857.8 | 6.46x | 17382315.6 | 6.48x | +0.4% | 454400 | 454400 |
| tsql/columns1000 | generated | 1724628.1 | 187148.4 | 0.11x | 182271.9 | 0.11x | -2.6% | 200464 | 200464 |
| tsql/conditions1000 | generated | 876736.7 | 327694.1 | 0.37x | 311028.5 | 0.35x | -5.1% | 424424 | 424424 |
| tsql/rows1000 | generated | 587413.3 | 236350.8 | 0.40x | 221615.6 | 0.38x | -6.2% | 296472 | 296472 |
| sql/select20.bool | generated | 7000.1 | 34202.0 | 4.89x | 19068.3 | 2.72x | -44.2% | 21448 | 21392 |
| web/url.full | generated | 154.4 | 284.9 | 1.85x | 289.0 | 1.87x | +1.4% | 536 | 536 |
| el/string | generated | 271.3 | 1052.4 | 3.88x | 971.5 | 3.58x | -7.7% | 1056 | 1056 |
| el/string | immediate | 271.3 | 671.4 | 2.47x | 691.4 | 2.55x | +3.0% | 1032 | 1032 |
| tsql/comment | generated | 19335.6 | 1542.4 | 0.08x | 1559.7 | 0.08x | +1.1% | 1192 | 1192 |
| sql/select20 | generated | 7040.3 | 34447.0 | 4.89x | 18707.7 | 2.66x | -45.7% | 21472 | 21472 |
