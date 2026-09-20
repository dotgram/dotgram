# Paired stand, 2026-09-19 23:06

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 966.2 | 1015.5 | 1.05x | 1018.6 | 1.05x | +0.3% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 1047.9 | 2025.6 | 1.93x | 1999.6 | 1.91x | -1.3% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 1159.6 | 2464.2 | 2.12x | 2429.5 | 2.10x | -1.4% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 1080.9 | 1841.2 | 1.70x | 1203.1 | 1.11x | -34.7% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 1081.4 | 3166.1 | 2.93x | 2569.1 | 2.38x | -18.9% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 1088.4 | 4043.4 | 3.72x | 2935.1 | 2.70x | -27.4% | 1872 | 1872 |
