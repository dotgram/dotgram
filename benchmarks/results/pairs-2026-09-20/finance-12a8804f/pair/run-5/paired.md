# Paired stand, 2026-09-20 02:20

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5502.3 | 10940.7 | 1.99x | 10862.1 | 1.97x | -0.7% | 8384 | 8384 |
| web/url.plain | generated | hand | 98.1 | 221.9 | 2.26x | 218.5 | 2.23x | -1.5% | 360 | 360 |
| web/url.full | generated | hand | 171.5 | 298.2 | 1.74x | 282.7 | 1.65x | -5.2% | 536 | 536 |
| web/url.ipv4 | generated | hand | 116.9 | 226.3 | 1.94x | 232.9 | 1.99x | +2.9% | 384 | 384 |
| web/url.long-path | generated | hand | 181.1 | 423.4 | 2.34x | 417.5 | 2.31x | -1.4% | 512 | 512 |
| web/url.refused | generated | hand | 74.8 | 253.9 | 3.39x | 200.6 | 2.68x | -21.0% | 64 | 64 |
| web/url.relative | generated | hand | 85.4 | 181.2 | 2.12x | 183.1 | 2.14x | +1.1% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 66.3 | 157.7 | 2.38x | 160.0 | 2.41x | +1.4% | 280 | 280 |
| web/url.relative-letters | generated | hand | 110.3 | 216.7 | 1.97x | 223.2 | 2.02x | +3.0% | 312 | 312 |
