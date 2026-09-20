Median of 5 of 5 runs, each in a process of its own; control 31.4 ns (the runs' controls: 32.1, 31.4, 31.2, 31.5, 31.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 00:54

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 1733425.0 | 174100.0 | 0.10x | 173100.0 | 0.10x | -0.6% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 889743.0 | 303711.7 | 0.34x | 302767.2 | 0.34x | -0.3% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 597028.9 | 225670.3 | 0.38x | 224742.2 | 0.38x | -0.4% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7089.7 | 18478.4 | 2.61x | 18551.7 | 2.62x | +0.4% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6934.8 | 18849.6 | 2.72x | 18645.5 | 2.69x | -1.1% | 21440 | 21440 |
| sql/select20.scan | generated | control | 384.2 | 390.7 | 1.02x | 387.0 | 1.01x | -1.0% | 0 | 0 |
| tsql/select20.scan | generated | control | 379.8 | 381.7 | 1.01x | 388.1 | 1.02x | +1.7% | 0 | 0 |
| sql/select20.bool | generated | hand | 6890.6 | 18185.7 | 2.64x | 18648.7 | 2.71x | +2.5% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 19771.1 | 1643.4 | 0.08x | 1626.2 | 0.08x | -1.0% | 1192 | 1192 |
| sql/select20 | generated | hand | 6853.8 | 18235.8 | 2.66x | 18828.4 | 2.75x | +3.2% | 21448 | 21448 |
