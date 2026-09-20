Median of 5 of 5 runs, each in a process of its own; control 31.1 ns (the runs' controls: 31.0, 31.4, 30.9, 31.1, 31.5).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-19 21:40

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 75.8 | 84.4 | 1.11x | 86.3 | 1.14x | +2.3% | 192 | 192 |
| tsql/script100 | generated | scriptdom | 668312.5 | 1166700.0 | 1.75x | 1163168.8 | 1.74x | -0.3% | 113600 | 113600 |
| tsql/script400 | generated | scriptdom | 2736093.8 | 17383943.8 | 6.35x | 17285368.8 | 6.32x | -0.6% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1714201.6 | 188773.4 | 0.11x | 176982.8 | 0.10x | -6.2% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 875021.1 | 332696.1 | 0.38x | 304858.6 | 0.35x | -8.4% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 595286.7 | 247314.8 | 0.42x | 219673.4 | 0.37x | -11.2% | 296472 | 296472 |
| sql/select20.at | generated | hand | 6974.2 | 19053.7 | 2.73x | 18451.5 | 2.65x | -3.2% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6863.0 | 18344.1 | 2.67x | 18160.6 | 2.65x | -1.0% | 21440 | 21440 |
| tsql/insert-values.at | generated | scriptdom | 8176.8 | 1492.3 | 0.18x | 1449.3 | 0.18x | -2.9% | 1328 | 1328 |
| sql/select20.scan | generated | control | 377.8 | 378.7 | 1.00x | 374.2 | 0.99x | -1.2% | 0 | 0 |
| tsql/select20.scan | generated | control | 374.0 | 378.2 | 1.01x | 379.5 | 1.01x | +0.3% | 0 | 0 |
| sql/select20.bool | generated | hand | 7014.3 | 18216.8 | 2.60x | 18173.7 | 2.59x | -0.2% | 21448 | 21392 |
| web/url.full | generated | hand | 156.5 | 284.6 | 1.82x | 283.9 | 1.81x | -0.3% | 536 | 536 |
| el/string | generated | hand | 265.9 | 956.6 | 3.60x | 960.4 | 3.61x | +0.4% | 1056 | 1056 |
| el/string | immediate | hand | 265.9 | 666.3 | 2.51x | 663.8 | 2.50x | -0.4% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 19578.7 | 1599.6 | 0.08x | 1570.6 | 0.08x | -1.8% | 1192 | 1192 |
| sql/select20 | generated | hand | 6888.0 | 18311.1 | 2.66x | 18358.9 | 2.67x | +0.3% | 21448 | 21448 |
