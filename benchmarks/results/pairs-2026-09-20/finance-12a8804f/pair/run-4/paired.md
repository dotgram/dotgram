# Paired stand, 2026-09-20 02:20

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5238.3 | 10535.7 | 2.01x | 10565.3 | 2.02x | +0.3% | 8384 | 8384 |
| web/url.plain | generated | hand | 94.6 | 214.9 | 2.27x | 212.2 | 2.24x | -1.2% | 360 | 360 |
| web/url.full | generated | hand | 158.4 | 290.1 | 1.83x | 280.8 | 1.77x | -3.2% | 536 | 536 |
| web/url.ipv4 | generated | hand | 109.5 | 222.1 | 2.03x | 223.4 | 2.04x | +0.6% | 384 | 384 |
| web/url.long-path | generated | hand | 163.0 | 407.2 | 2.50x | 411.9 | 2.53x | +1.2% | 512 | 512 |
| web/url.refused | generated | hand | 60.7 | 250.0 | 4.12x | 195.5 | 3.22x | -21.8% | 64 | 64 |
| web/url.relative | generated | hand | 78.5 | 199.3 | 2.54x | 193.1 | 2.46x | -3.1% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 65.6 | 170.2 | 2.59x | 168.3 | 2.56x | -1.1% | 280 | 280 |
| web/url.relative-letters | generated | hand | 108.2 | 225.6 | 2.09x | 232.0 | 2.14x | +2.8% | 312 | 312 |
