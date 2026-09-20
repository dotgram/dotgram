# Paired stand, 2026-09-19 21:37

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 75.8 | 89.2 | 1.18x | 86.2 | 1.14x | -3.4% | 192 | 192 |
| tsql/script100 | generated | scriptdom | 668312.5 | 1173943.8 | 1.76x | 1163168.8 | 1.74x | -0.9% | 113600 | 113600 |
| tsql/script400 | generated | scriptdom | 2741953.1 | 17636971.9 | 6.43x | 17679840.6 | 6.45x | +0.2% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1777520.3 | 198042.2 | 0.11x | 181128.1 | 0.10x | -8.5% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 891593.0 | 336848.4 | 0.38x | 309665.6 | 0.35x | -8.1% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 597900.0 | 250666.4 | 0.42x | 228531.2 | 0.38x | -8.8% | 296472 | 296472 |
| sql/select20.at | generated | hand | 6902.9 | 18168.5 | 2.63x | 18005.6 | 2.61x | -0.9% | 21416 | 21416 |
| sql/select20.window | generated | hand | 6802.1 | 17982.0 | 2.64x | 17917.0 | 2.63x | -0.4% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 7994.9 | 1443.5 | 0.18x | 1438.6 | 0.18x | -0.3% | 1328 | 1328 |
| sql/select20.scan | generated | control | 391.3 | 390.2 | 1.00x | 408.6 | 1.04x | +4.7% | 0 | 0 |
| tsql/select20.scan | generated | control | 383.2 | 388.8 | 1.01x | 391.2 | 1.02x | +0.6% | 0 | 0 |
| sql/select20.bool | generated | hand | 7056.2 | 18047.7 | 2.56x | 18151.7 | 2.57x | +0.6% | 21448 | 21392 |
| web/url.full | generated | hand | 159.5 | 281.6 | 1.77x | 283.9 | 1.78x | +0.8% | 536 | 536 |
| el/string | generated | hand | 267.0 | 956.6 | 3.58x | 977.2 | 3.66x | +2.1% | 1056 | 1056 |
| el/string | immediate | hand | 267.0 | 654.0 | 2.45x | 663.8 | 2.49x | +1.5% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 19578.7 | 1580.9 | 0.08x | 1562.1 | 0.08x | -1.2% | 1192 | 1192 |
| sql/select20 | generated | hand | 7085.3 | 18604.5 | 2.63x | 18759.4 | 2.65x | +0.8% | 21448 | 21448 |
