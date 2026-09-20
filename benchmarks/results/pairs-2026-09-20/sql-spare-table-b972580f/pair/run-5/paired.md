# Paired stand, 2026-09-20 01:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 46.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 78.6 | 82.7 | 1.05x | 84.2 | 1.07x | +1.9% | 192 | 192 |
| tsql/columns1000 | generated | scriptdom | 2897706.2 | 303250.0 | 0.10x | 300068.8 | 0.10x | -1.0% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 1511740.6 | 519318.8 | 0.34x | 518134.4 | 0.34x | -0.2% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 1077764.1 | 394757.8 | 0.37x | 401121.9 | 0.37x | +1.6% | 296497 | 296497 |
| sql/select20.at | generated | hand | 12239.8 | 29683.5 | 2.43x | 28985.5 | 2.37x | -2.4% | 21416 | 21441 |
| sql/select20.window | generated | hand | 11981.0 | 29372.3 | 2.45x | 25729.6 | 2.15x | -12.4% | 21416 | 21416 |
| sql/select20.scan | generated | control | 591.1 | 594.0 | 1.00x | 604.2 | 1.02x | +1.7% | 0 | 0 |
| tsql/select20.scan | generated | control | 636.0 | 678.3 | 1.07x | 663.7 | 1.04x | -2.2% | 0 | 0 |
| sql/select20.bool | generated | hand | 10734.8 | 28522.8 | 2.66x | 28921.6 | 2.69x | +1.4% | 21448 | 21392 |
| web/url.full | generated | hand | 272.5 | 487.2 | 1.79x | 417.0 | 1.53x | -14.4% | 536 | 536 |
| el/string | generated | hand | 346.5 | 1317.3 | 3.80x | 1364.7 | 3.94x | +3.6% | 1056 | 1056 |
| el/string | immediate | hand | 346.5 | 954.8 | 2.76x | 920.9 | 2.66x | -3.6% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 33041.8 | 2611.0 | 0.08x | 2622.9 | 0.08x | +0.5% | 1192 | 1192 |
| sql/select20 | generated | hand | 12677.5 | 29605.5 | 2.34x | 24145.9 | 1.90x | -18.4% | 21472 | 21472 |
