# Paired stand, 2026-09-20 02:16

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5198.4 | 10533.2 | 2.03x | 10672.2 | 2.05x | +1.3% | 8384 | 8384 |
| web/url.plain | generated | hand | 96.9 | 219.6 | 2.27x | 220.1 | 2.27x | +0.2% | 360 | 360 |
| web/url.full | generated | hand | 171.3 | 298.3 | 1.74x | 291.5 | 1.70x | -2.3% | 536 | 536 |
| web/url.ipv4 | generated | hand | 111.2 | 224.8 | 2.02x | 231.9 | 2.09x | +3.2% | 384 | 384 |
| web/url.long-path | generated | hand | 177.1 | 421.3 | 2.38x | 417.1 | 2.36x | -1.0% | 512 | 512 |
| web/url.refused | generated | hand | 68.3 | 245.4 | 3.59x | 257.7 | 3.77x | +5.0% | 64 | 64 |
| web/url.relative | generated | hand | 84.8 | 180.1 | 2.12x | 182.5 | 2.15x | +1.4% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 67.5 | 159.2 | 2.36x | 159.7 | 2.36x | +0.3% | 280 | 280 |
| web/url.relative-letters | generated | hand | 104.4 | 220.8 | 2.11x | 218.0 | 2.09x | -1.3% | 312 | 312 |
