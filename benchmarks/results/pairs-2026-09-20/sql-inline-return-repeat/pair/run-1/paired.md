# Paired stand, 2026-09-20 00:55

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 1636756.2 | 168693.8 | 0.10x | 168637.5 | 0.10x | 0.0% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 879637.5 | 299996.9 | 0.34x | 306591.4 | 0.35x | +2.2% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 595261.7 | 229188.3 | 0.39x | 225811.7 | 0.38x | -1.5% | 296472 | 296472 |
| sql/select20.at | generated | hand | 6912.3 | 18508.5 | 2.68x | 18315.6 | 2.65x | -1.0% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6821.7 | 18533.4 | 2.72x | 18536.8 | 2.72x | 0.0% | 21441 | 21441 |
| sql/select20.scan | generated | control | 383.7 | 390.8 | 1.02x | 381.7 | 0.99x | -2.3% | 0 | 0 |
| tsql/select20.scan | generated | control | 382.9 | 390.1 | 1.02x | 388.7 | 1.02x | -0.3% | 0 | 0 |
| sql/select20.bool | generated | hand | 6920.1 | 18296.4 | 2.64x | 18320.4 | 2.65x | +0.1% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 20207.9 | 1635.7 | 0.08x | 1642.5 | 0.08x | +0.4% | 1192 | 1192 |
| sql/select20 | generated | hand | 6990.3 | 18706.0 | 2.68x | 18909.8 | 2.71x | +1.1% | 21448 | 21448 |
