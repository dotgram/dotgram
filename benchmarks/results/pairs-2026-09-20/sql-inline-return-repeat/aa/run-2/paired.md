# Paired stand, 2026-09-20 00:53

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 1722756.2 | 176562.5 | 0.10x | 177418.8 | 0.10x | +0.5% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 889743.0 | 302375.8 | 0.34x | 303407.0 | 0.34x | +0.3% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 597028.9 | 225670.3 | 0.38x | 223173.4 | 0.37x | -1.1% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7089.7 | 18471.0 | 2.61x | 20412.0 | 2.88x | +10.5% | 21441 | 21441 |
| sql/select20.window | generated | hand | 7033.3 | 18899.2 | 2.69x | 20813.4 | 2.96x | +10.1% | 21440 | 21440 |
| sql/select20.scan | generated | control | 416.1 | 392.4 | 0.94x | 400.0 | 0.96x | +2.0% | 0 | 0 |
| tsql/select20.scan | generated | control | 376.2 | 390.4 | 1.04x | 388.1 | 1.03x | -0.6% | 0 | 0 |
| sql/select20.bool | generated | hand | 6890.6 | 18171.1 | 2.64x | 20224.5 | 2.94x | +11.3% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 20190.0 | 1710.9 | 0.08x | 1617.1 | 0.08x | -5.5% | 1192 | 1192 |
| sql/select20 | generated | hand | 6839.4 | 18117.8 | 2.65x | 20128.9 | 2.94x | +11.1% | 21448 | 21448 |
