Median of 4 of 5 runs, each in a process of its own; control 31.3 ns (the runs' controls: 31.3, 31.3, 31.2, 31.3, 46.2).
Dropped for a control more than 5% off the median: run 5.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 01:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 74.4 | 79.3 | 1.07x | 80.5 | 1.08x | +1.5% | 192 | 192 |
| tsql/columns1000 | generated | scriptdom | 2751568.8 | 230475.0 | 0.08x | 228050.0 | 0.08x | -1.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 875719.1 | 304095.3 | 0.35x | 312694.5 | 0.36x | +2.8% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 600212.9 | 226766.0 | 0.38x | 224777.0 | 0.37x | -0.9% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7050.9 | 18494.9 | 2.62x | 18591.0 | 2.64x | +0.5% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6932.2 | 18683.0 | 2.70x | 18598.7 | 2.68x | -0.5% | 21440 | 21440 |
| sql/select20.scan | generated | control | 385.7 | 385.6 | 1.00x | 391.0 | 1.01x | +1.4% | 0 | 0 |
| tsql/select20.scan | generated | control | 383.9 | 381.6 | 0.99x | 384.8 | 1.00x | +0.8% | 0 | 0 |
| sql/select20.bool | generated | hand | 6848.8 | 18493.0 | 2.70x | 18360.5 | 2.68x | -0.7% | 21448 | 21392 |
| web/url.full | generated | hand | 158.1 | 281.5 | 1.78x | 279.2 | 1.77x | -0.8% | 536 | 536 |
| el/string | generated | hand | 278.5 | 965.6 | 3.47x | 987.2 | 3.54x | +2.2% | 1056 | 1056 |
| el/string | immediate | hand | 278.5 | 670.0 | 2.41x | 667.5 | 2.40x | -0.4% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 19840.7 | 1664.9 | 0.08x | 1621.7 | 0.08x | -2.6% | 1192 | 1192 |
| sql/select20 | generated | hand | 6943.9 | 18690.0 | 2.69x | 18787.6 | 2.71x | +0.5% | 21472 | 21472 |
