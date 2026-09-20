# Paired stand, 2026-09-20 01:47

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 73.7 | 78.4 | 1.06x | 80.2 | 1.09x | +2.2% | 192 | 192 |
| tsql/columns1000 | generated | scriptdom | 2750850.0 | 228187.5 | 0.08x | 237862.5 | 0.09x | +4.2% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 894044.5 | 308336.7 | 0.34x | 318934.4 | 0.36x | +3.4% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 604468.0 | 229494.5 | 0.38x | 224891.4 | 0.37x | -2.0% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7005.0 | 18501.3 | 2.64x | 19006.0 | 2.71x | +2.7% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6979.7 | 18734.9 | 2.68x | 18790.0 | 2.69x | +0.3% | 21440 | 21440 |
| sql/select20.scan | generated | control | 379.0 | 388.6 | 1.03x | 406.2 | 1.07x | +4.5% | 0 | 0 |
| tsql/select20.scan | generated | control | 383.2 | 384.1 | 1.00x | 380.1 | 0.99x | -1.0% | 0 | 0 |
| sql/select20.bool | generated | hand | 6889.4 | 18482.9 | 2.68x | 18916.7 | 2.75x | +2.3% | 21448 | 21392 |
| web/url.full | generated | hand | 157.5 | 295.9 | 1.88x | 286.1 | 1.82x | -3.3% | 536 | 536 |
| el/string | generated | hand | 280.2 | 961.6 | 3.43x | 1018.9 | 3.64x | +6.0% | 1056 | 1056 |
| el/string | immediate | hand | 280.2 | 664.7 | 2.37x | 684.5 | 2.44x | +3.0% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 19867.9 | 1722.5 | 0.09x | 1557.7 | 0.08x | -9.6% | 1192 | 1192 |
| sql/select20 | generated | hand | 6890.3 | 18711.2 | 2.72x | 18974.8 | 2.75x | +1.4% | 21472 | 21472 |
