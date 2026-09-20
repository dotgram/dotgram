# Paired stand, 2026-09-19 23:07

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.9 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 963.4 | 1019.1 | 1.06x | 1015.0 | 1.05x | -0.4% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 1043.0 | 1984.5 | 1.90x | 1962.1 | 1.88x | -1.1% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 1153.9 | 2388.5 | 2.07x | 2381.8 | 2.06x | -0.3% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 1067.2 | 1857.7 | 1.74x | 1172.3 | 1.10x | -36.9% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 1081.4 | 3049.6 | 2.82x | 2505.5 | 2.32x | -17.8% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 1071.1 | 4313.1 | 4.03x | 2918.3 | 2.72x | -32.3% | 1872 | 1872 |
