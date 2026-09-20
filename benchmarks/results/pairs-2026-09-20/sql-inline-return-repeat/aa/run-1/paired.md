# Paired stand, 2026-09-20 00:52

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 1846937.5 | 181843.8 | 0.10x | 178887.5 | 0.10x | -1.6% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 909039.8 | 321989.8 | 0.35x | 335168.0 | 0.37x | +4.1% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 635525.0 | 236251.6 | 0.37x | 234117.2 | 0.37x | -0.9% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7268.8 | 18714.8 | 2.57x | 18551.7 | 2.55x | -0.9% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7624.7 | 19568.0 | 2.57x | 19191.4 | 2.52x | -1.9% | 21441 | 21441 |
| sql/select20.scan | generated | control | 384.2 | 412.7 | 1.07x | 390.7 | 1.02x | -5.3% | 0 | 0 |
| tsql/select20.scan | generated | control | 385.2 | 384.8 | 1.00x | 395.3 | 1.03x | +2.7% | 0 | 0 |
| sql/select20.bool | generated | hand | 6794.0 | 18256.2 | 2.69x | 18648.7 | 2.74x | +2.1% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 19596.7 | 1643.4 | 0.08x | 1657.3 | 0.08x | +0.8% | 1192 | 1192 |
| sql/select20 | generated | hand | 7029.0 | 18665.4 | 2.66x | 18891.9 | 2.69x | +1.2% | 21448 | 21448 |
