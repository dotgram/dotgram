Median of 5 of 5 runs, each in a process of its own; control 31.7 ns (the runs' controls: 31.4, 31.6, 31.7, 31.7, 31.7).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 02:20

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5272.7 | 10568.0 | 2.00x | 10565.3 | 2.00x | 0.0% | 8384 | 8384 |
| web/url.plain | generated | hand | 96.4 | 214.9 | 2.23x | 218.5 | 2.27x | +1.7% | 360 | 360 |
| web/url.full | generated | hand | 169.6 | 290.1 | 1.71x | 283.7 | 1.67x | -2.2% | 536 | 536 |
| web/url.ipv4 | generated | hand | 112.8 | 225.9 | 2.00x | 228.9 | 2.03x | +1.4% | 384 | 384 |
| web/url.long-path | generated | hand | 177.4 | 414.0 | 2.33x | 411.9 | 2.32x | -0.5% | 512 | 512 |
| web/url.refused | generated | hand | 67.9 | 250.0 | 3.68x | 196.7 | 2.90x | -21.3% | 64 | 64 |
| web/url.relative | generated | hand | 78.5 | 183.0 | 2.33x | 186.5 | 2.37x | +1.9% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 64.2 | 155.9 | 2.43x | 160.0 | 2.49x | +2.6% | 280 | 280 |
| web/url.relative-letters | generated | hand | 108.2 | 216.7 | 2.00x | 231.6 | 2.14x | +6.8% | 312 | 312 |
