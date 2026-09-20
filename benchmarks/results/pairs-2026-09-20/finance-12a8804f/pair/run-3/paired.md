# Paired stand, 2026-09-20 02:19

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5398.5 | 10719.0 | 1.99x | 10708.5 | 1.98x | -0.1% | 8384 | 8384 |
| web/url.plain | generated | hand | 169.0 | 353.6 | 2.09x | 335.9 | 1.99x | -5.0% | 360 | 360 |
| web/url.full | generated | hand | 305.2 | 417.4 | 1.37x | 476.5 | 1.56x | +14.2% | 536 | 536 |
| web/url.ipv4 | generated | hand | 114.5 | 238.9 | 2.09x | 228.9 | 2.00x | -4.2% | 384 | 384 |
| web/url.long-path | generated | hand | 174.6 | 409.5 | 2.35x | 404.8 | 2.32x | -1.2% | 512 | 512 |
| web/url.refused | generated | hand | 67.7 | 249.6 | 3.69x | 194.6 | 2.87x | -22.0% | 64 | 64 |
| web/url.relative | generated | hand | 78.0 | 185.6 | 2.38x | 183.4 | 2.35x | -1.2% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 63.5 | 154.9 | 2.44x | 159.2 | 2.51x | +2.8% | 280 | 280 |
| web/url.relative-letters | generated | hand | 105.3 | 209.7 | 1.99x | 222.1 | 2.11x | +6.0% | 312 | 312 |
