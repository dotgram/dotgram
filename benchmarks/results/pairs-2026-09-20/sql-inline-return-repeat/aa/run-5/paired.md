# Paired stand, 2026-09-20 00:54

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 3062875.0 | 169050.0 | 0.06x | 168081.2 | 0.05x | -0.6% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 892461.7 | 304957.0 | 0.34x | 301000.0 | 0.34x | -1.3% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 601287.5 | 226678.1 | 0.38x | 224742.2 | 0.37x | -0.9% | 296472 | 296472 |
| sql/select20.at | generated | hand | 6924.4 | 18813.3 | 2.72x | 18126.4 | 2.62x | -3.7% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6921.0 | 18849.6 | 2.72x | 18357.9 | 2.65x | -2.6% | 21440 | 21440 |
| sql/select20.scan | generated | control | 376.1 | 390.7 | 1.04x | 382.8 | 1.02x | -2.0% | 0 | 0 |
| tsql/select20.scan | generated | control | 379.8 | 381.7 | 1.01x | 393.8 | 1.04x | +3.2% | 0 | 0 |
| sql/select20.bool | generated | hand | 6913.4 | 18325.4 | 2.65x | 18325.7 | 2.65x | 0.0% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 20590.8 | 1699.8 | 0.08x | 1636.0 | 0.08x | -3.8% | 1192 | 1192 |
| sql/select20 | generated | hand | 6885.0 | 18303.7 | 2.66x | 18078.1 | 2.63x | -1.2% | 21448 | 21448 |
