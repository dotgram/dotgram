# Paired stand, 2026-09-20 02:16

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5272.9 | 10570.7 | 2.00x | 10629.5 | 2.02x | +0.6% | 8384 | 8384 |
| web/url.plain | generated | hand | 93.9 | 215.1 | 2.29x | 213.4 | 2.27x | -0.8% | 360 | 360 |
| web/url.full | generated | hand | 158.8 | 282.6 | 1.78x | 299.4 | 1.88x | +5.9% | 536 | 536 |
| web/url.ipv4 | generated | hand | 109.7 | 222.7 | 2.03x | 231.3 | 2.11x | +3.9% | 384 | 384 |
| web/url.long-path | generated | hand | 166.6 | 412.9 | 2.48x | 416.7 | 2.50x | +0.9% | 512 | 512 |
| web/url.refused | generated | hand | 60.1 | 253.6 | 4.22x | 249.6 | 4.16x | -1.5% | 64 | 64 |
| web/url.relative | generated | hand | 75.5 | 189.3 | 2.51x | 180.8 | 2.40x | -4.5% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 62.9 | 154.6 | 2.46x | 155.6 | 2.47x | +0.6% | 280 | 280 |
| web/url.relative-letters | generated | hand | 104.3 | 208.3 | 2.00x | 214.6 | 2.06x | +3.0% | 312 | 312 |
