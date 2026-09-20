# Paired stand, 2026-09-20 02:17

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5271.8 | 10546.3 | 2.00x | 10586.5 | 2.01x | +0.4% | 8384 | 8384 |
| web/url.plain | generated | hand | 93.9 | 215.1 | 2.29x | 216.8 | 2.31x | +0.8% | 360 | 360 |
| web/url.full | generated | hand | 161.8 | 300.4 | 1.86x | 292.1 | 1.81x | -2.8% | 536 | 536 |
| web/url.ipv4 | generated | hand | 108.8 | 230.0 | 2.11x | 240.7 | 2.21x | +4.7% | 384 | 384 |
| web/url.long-path | generated | hand | 164.9 | 411.5 | 2.50x | 432.0 | 2.62x | +5.0% | 512 | 512 |
| web/url.refused | generated | hand | 61.1 | 258.8 | 4.24x | 251.3 | 4.11x | -2.9% | 64 | 64 |
| web/url.relative | generated | hand | 76.7 | 179.5 | 2.34x | 180.2 | 2.35x | +0.4% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 63.3 | 158.7 | 2.51x | 155.7 | 2.46x | -1.9% | 280 | 280 |
| web/url.relative-letters | generated | hand | 105.4 | 211.9 | 2.01x | 210.3 | 2.00x | -0.8% | 312 | 312 |
