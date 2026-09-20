# Paired stand, 2026-09-20 02:15

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5321.7 | 10630.7 | 2.00x | 10620.6 | 2.00x | -0.1% | 8384 | 8384 |
| web/url.plain | generated | hand | 98.9 | 222.0 | 2.24x | 218.1 | 2.21x | -1.7% | 360 | 360 |
| web/url.full | generated | hand | 173.7 | 295.4 | 1.70x | 277.3 | 1.60x | -6.1% | 536 | 536 |
| web/url.ipv4 | generated | hand | 112.8 | 228.4 | 2.02x | 224.8 | 1.99x | -1.6% | 384 | 384 |
| web/url.long-path | generated | hand | 177.6 | 414.8 | 2.34x | 404.0 | 2.27x | -2.6% | 512 | 512 |
| web/url.refused | generated | hand | 67.7 | 262.9 | 3.88x | 248.4 | 3.67x | -5.5% | 64 | 64 |
| web/url.relative | generated | hand | 76.0 | 201.7 | 2.65x | 186.3 | 2.45x | -7.6% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 63.7 | 179.8 | 2.82x | 160.6 | 2.52x | -10.7% | 280 | 280 |
| web/url.relative-letters | generated | hand | 104.7 | 232.7 | 2.22x | 222.9 | 2.13x | -4.2% | 312 | 312 |
