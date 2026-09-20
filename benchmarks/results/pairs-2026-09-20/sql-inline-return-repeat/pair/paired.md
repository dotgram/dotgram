Median of 5 of 5 runs, each in a process of its own; control 31.4 ns (the runs' controls: 31.4, 31.5, 31.1, 31.4, 31.4).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 00:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 1665143.8 | 168331.2 | 0.10x | 168637.5 | 0.10x | +0.2% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 881071.9 | 300675.8 | 0.34x | 306191.4 | 0.35x | +1.8% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 598664.1 | 226053.1 | 0.38x | 227776.6 | 0.38x | +0.8% | 296472 | 296472 |
| sql/select20.at | generated | hand | 6931.3 | 18793.1 | 2.71x | 18392.1 | 2.65x | -2.1% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6916.4 | 18729.1 | 2.71x | 18604.7 | 2.69x | -0.7% | 21441 | 21441 |
| sql/select20.scan | generated | control | 379.2 | 386.3 | 1.02x | 387.2 | 1.02x | +0.2% | 0 | 0 |
| tsql/select20.scan | generated | control | 381.4 | 390.1 | 1.02x | 383.2 | 1.00x | -1.8% | 0 | 0 |
| sql/select20.bool | generated | hand | 6855.7 | 18296.4 | 2.67x | 18333.5 | 2.67x | +0.2% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 19772.0 | 1635.7 | 0.08x | 1642.5 | 0.08x | +0.4% | 1192 | 1192 |
| sql/select20 | generated | hand | 6837.3 | 18455.0 | 2.70x | 18486.1 | 2.70x | +0.2% | 21448 | 21448 |
