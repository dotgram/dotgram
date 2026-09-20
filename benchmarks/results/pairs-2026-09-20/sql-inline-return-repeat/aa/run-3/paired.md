# Paired stand, 2026-09-20 00:53

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 1639175.0 | 167881.2 | 0.10x | 166787.5 | 0.10x | -0.7% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 887041.4 | 303711.7 | 0.34x | 302767.2 | 0.34x | -0.3% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 591057.8 | 222268.8 | 0.38x | 222375.8 | 0.38x | 0.0% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7195.5 | 18376.9 | 2.55x | 18334.1 | 2.55x | -0.2% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6934.8 | 18426.3 | 2.66x | 18387.8 | 2.65x | -0.2% | 21440 | 21440 |
| sql/select20.scan | generated | control | 384.7 | 383.9 | 1.00x | 387.0 | 1.01x | +0.8% | 0 | 0 |
| tsql/select20.scan | generated | control | 399.0 | 380.0 | 0.95x | 383.0 | 0.96x | +0.8% | 0 | 0 |
| sql/select20.bool | generated | hand | 6907.7 | 18130.7 | 2.62x | 18919.1 | 2.74x | +4.3% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 19484.8 | 1621.7 | 0.08x | 1617.7 | 0.08x | -0.2% | 1192 | 1192 |
| sql/select20 | generated | hand | 6853.8 | 18118.8 | 2.64x | 18352.8 | 2.68x | +1.3% | 21448 | 21448 |
