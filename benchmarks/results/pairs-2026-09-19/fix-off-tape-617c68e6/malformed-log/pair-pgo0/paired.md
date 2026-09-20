Median of 5 of 5 runs, each in a process of its own; control 31.0 ns (the runs' controls: 31.0, 31.1, 31.0, 31.0, 30.9).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 23:07

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 971.1 | 1023.9 | 1.05x | 1033.9 | 1.06x | +1.0% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 1035.8 | 1999.2 | 1.93x | 1971.6 | 1.90x | -1.4% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 1160.0 | 2394.1 | 2.06x | 2390.0 | 2.06x | -0.2% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 1069.0 | 1879.3 | 1.76x | 1203.1 | 1.13x | -36.0% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 1076.7 | 3080.1 | 2.86x | 2540.5 | 2.36x | -17.5% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 1075.5 | 3997.8 | 3.72x | 2918.3 | 2.71x | -27.0% | 1872 | 1872 |
