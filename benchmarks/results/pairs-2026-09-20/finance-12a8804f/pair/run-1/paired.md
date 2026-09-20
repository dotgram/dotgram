# Paired stand, 2026-09-20 02:18

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5218.9 | 10568.0 | 2.02x | 10545.3 | 2.02x | -0.2% | 8384 | 8384 |
| web/url.plain | generated | hand | 96.3 | 213.4 | 2.21x | 219.3 | 2.28x | +2.8% | 360 | 360 |
| web/url.full | generated | hand | 166.9 | 286.3 | 1.72x | 287.6 | 1.72x | +0.5% | 536 | 536 |
| web/url.ipv4 | generated | hand | 112.7 | 218.6 | 1.94x | 220.7 | 1.96x | +1.0% | 384 | 384 |
| web/url.long-path | generated | hand | 177.4 | 414.0 | 2.33x | 405.4 | 2.29x | -2.1% | 512 | 512 |
| web/url.refused | generated | hand | 67.9 | 249.4 | 3.68x | 196.7 | 2.90x | -21.1% | 64 | 64 |
| web/url.relative | generated | hand | 77.0 | 182.0 | 2.36x | 186.5 | 2.42x | +2.4% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 63.5 | 155.9 | 2.45x | 158.1 | 2.49x | +1.4% | 280 | 280 |
| web/url.relative-letters | generated | hand | 105.8 | 208.7 | 1.97x | 231.6 | 2.19x | +10.9% | 312 | 312 |
