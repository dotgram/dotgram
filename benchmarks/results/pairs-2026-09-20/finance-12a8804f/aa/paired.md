Median of 5 of 5 runs, each in a process of its own; control 31.6 ns (the runs' controls: 31.6, 31.6, 31.5, 31.6, 31.4).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 02:17

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web/url.path1000 | generated | hand | 5271.8 | 10565.4 | 2.00x | 10620.6 | 2.01x | +0.5% | 8384 | 8384 |
| web/url.plain | generated | hand | 96.9 | 219.3 | 2.26x | 218.1 | 2.25x | -0.5% | 360 | 360 |
| web/url.full | generated | hand | 167.3 | 298.3 | 1.78x | 292.1 | 1.75x | -2.1% | 536 | 536 |
| web/url.ipv4 | generated | hand | 111.2 | 226.8 | 2.04x | 231.3 | 2.08x | +2.0% | 384 | 384 |
| web/url.long-path | generated | hand | 176.9 | 412.9 | 2.33x | 416.7 | 2.36x | +0.9% | 512 | 512 |
| web/url.refused | generated | hand | 67.7 | 253.6 | 3.74x | 249.6 | 3.69x | -1.5% | 64 | 64 |
| web/url.relative | generated | hand | 76.0 | 180.4 | 2.37x | 180.9 | 2.38x | +0.3% | 312 | 312 |
| web/url.relative-dot-colon | generated | hand | 63.7 | 158.8 | 2.49x | 155.7 | 2.44x | -2.0% | 280 | 280 |
| web/url.relative-letters | generated | hand | 104.7 | 212.7 | 2.03x | 214.6 | 2.05x | +0.9% | 312 | 312 |
