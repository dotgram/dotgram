# Paired stand, 2026-09-19 23:05

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 975.1 | 1025.7 | 1.05x | 1035.5 | 1.06x | +1.0% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 1029.1 | 1994.0 | 1.94x | 1971.6 | 1.92x | -1.1% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 1160.0 | 2375.4 | 2.05x | 2383.1 | 2.05x | +0.3% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 1069.0 | 1890.3 | 1.77x | 1207.3 | 1.13x | -36.1% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 1069.0 | 3082.5 | 2.88x | 2543.5 | 2.38x | -17.5% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 1082.0 | 3997.8 | 3.69x | 2941.3 | 2.72x | -26.4% | 1872 | 1872 |
