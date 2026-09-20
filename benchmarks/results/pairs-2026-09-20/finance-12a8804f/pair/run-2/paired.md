# Paired stand, 2026-09-20 02:19

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5272.7 | 10536.7 | 2.00x | 10430.0 | 1.98x | -1.0% | 8384 | 8384 |
| web/url.plain | generated | hand | 96.4 | 214.6 | 2.23x | 212.9 | 2.21x | -0.8% | 360 | 360 |
| web/url.full | generated | hand | 169.6 | 275.0 | 1.62x | 283.7 | 1.67x | +3.2% | 536 | 536 |
| web/url.ipv4 | generated | hand | 112.8 | 225.9 | 2.00x | 230.4 | 2.04x | +2.0% | 384 | 384 |
| web/url.long-path | generated | hand | 185.7 | 429.1 | 2.31x | 432.7 | 2.33x | +0.8% | 512 | 512 |
| web/url.refused | generated | hand | 71.4 | 260.9 | 3.66x | 221.4 | 3.10x | -15.1% | 64 | 64 |
| web/url.relative | generated | hand | 78.6 | 183.0 | 2.33x | 188.9 | 2.40x | +3.2% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 64.2 | 155.4 | 2.42x | 163.9 | 2.56x | +5.5% | 280 | 280 |
| web/url.relative-letters | generated | hand | 192.5 | 365.6 | 1.90x | 350.8 | 1.82x | -4.0% | 312 | 312 |
