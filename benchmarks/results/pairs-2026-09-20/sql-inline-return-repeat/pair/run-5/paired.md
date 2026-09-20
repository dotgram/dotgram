# Paired stand, 2026-09-20 00:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 1629962.5 | 167262.5 | 0.10x | 166200.0 | 0.10x | -0.6% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 883607.8 | 301343.0 | 0.34x | 303462.5 | 0.34x | +0.7% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 592753.1 | 229007.0 | 0.39x | 228039.8 | 0.38x | -0.4% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7024.6 | 18960.1 | 2.70x | 18806.7 | 2.68x | -0.8% | 21416 | 21460 |
| sql/select20.window | generated | hand | 6930.9 | 18365.5 | 2.65x | 18604.7 | 2.68x | +1.3% | 21440 | 21440 |
| sql/select20.scan | generated | control | 383.3 | 394.3 | 1.03x | 379.9 | 0.99x | -3.7% | 0 | 0 |
| tsql/select20.scan | generated | control | 381.4 | 386.5 | 1.01x | 382.5 | 1.00x | -1.0% | 0 | 0 |
| sql/select20.bool | generated | hand | 6855.7 | 18230.1 | 2.66x | 18333.5 | 2.67x | +0.6% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 19737.0 | 1692.9 | 0.09x | 1666.0 | 0.08x | -1.6% | 1192 | 1192 |
| sql/select20 | generated | hand | 6927.2 | 18227.3 | 2.63x | 18486.1 | 2.67x | +1.4% | 21448 | 21448 |
