# Paired stand, 2026-09-19 23:05

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 971.1 | 1023.9 | 1.05x | 1046.4 | 1.08x | +2.2% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 1032.0 | 1999.2 | 1.94x | 1964.0 | 1.90x | -1.8% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 1168.8 | 2406.0 | 2.06x | 2390.0 | 2.04x | -0.7% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 1086.3 | 1879.3 | 1.73x | 1210.3 | 1.11x | -35.6% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 1076.7 | 3080.1 | 2.86x | 2540.5 | 2.36x | -17.5% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 1075.5 | 3996.3 | 3.72x | 2900.8 | 2.70x | -27.4% | 1872 | 1872 |
