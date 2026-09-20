# Paired stand, 2026-09-20 02:17

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5238.0 | 10565.4 | 2.02x | 10556.0 | 2.02x | -0.1% | 8384 | 8384 |
| web/url.plain | generated | hand | 100.7 | 219.3 | 2.18x | 219.8 | 2.18x | +0.2% | 360 | 360 |
| web/url.full | generated | hand | 167.3 | 301.1 | 1.80x | 293.2 | 1.75x | -2.6% | 536 | 536 |
| web/url.ipv4 | generated | hand | 111.5 | 226.8 | 2.03x | 225.6 | 2.02x | -0.5% | 384 | 384 |
| web/url.long-path | generated | hand | 176.9 | 412.4 | 2.33x | 408.0 | 2.31x | -1.1% | 512 | 512 |
| web/url.refused | generated | hand | 68.4 | 244.9 | 3.58x | 247.3 | 3.61x | +1.0% | 64 | 64 |
| web/url.relative | generated | hand | 75.4 | 180.4 | 2.39x | 180.9 | 2.40x | +0.3% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 64.1 | 158.8 | 2.48x | 154.2 | 2.41x | -2.9% | 280 | 280 |
| web/url.relative-letters | generated | hand | 105.9 | 212.7 | 2.01x | 207.1 | 1.96x | -2.6% | 312 | 312 |
