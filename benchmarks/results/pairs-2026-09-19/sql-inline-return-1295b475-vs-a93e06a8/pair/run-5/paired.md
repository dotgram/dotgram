# Paired stand, 2026-09-19 21:40

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 75.4 | 81.4 | 1.08x | 87.2 | 1.16x | +7.1% | 192 | 192 |
| tsql/script100 | generated | scriptdom | 684250.0 | 1166700.0 | 1.71x | 1159781.2 | 1.69x | -0.6% | 113600 | 113600 |
| tsql/script400 | generated | scriptdom | 2736137.5 | 17157243.8 | 6.27x | 17663865.6 | 6.46x | +3.0% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1708414.1 | 188267.2 | 0.11x | 178053.1 | 0.10x | -5.4% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 875021.1 | 332696.1 | 0.38x | 302801.6 | 0.35x | -9.0% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 611559.4 | 247314.8 | 0.40x | 221308.6 | 0.36x | -10.5% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7293.5 | 19128.1 | 2.62x | 19330.1 | 2.65x | +1.1% | 21441 | 21441 |
| sql/select20.window | generated | hand | 7352.6 | 19504.8 | 2.65x | 19050.5 | 2.59x | -2.3% | 21440 | 21440 |
| tsql/insert-values.at | generated | scriptdom | 8306.0 | 1508.7 | 0.18x | 1498.9 | 0.18x | -0.6% | 1328 | 1328 |
| sql/select20.scan | generated | control | 382.6 | 378.7 | 0.99x | 383.6 | 1.00x | +1.3% | 0 | 0 |
| tsql/select20.scan | generated | control | 380.6 | 386.3 | 1.01x | 379.5 | 1.00x | -1.7% | 0 | 0 |
| sql/select20.bool | generated | hand | 7014.3 | 18128.0 | 2.58x | 18173.7 | 2.59x | +0.3% | 21448 | 21392 |
| web/url.full | generated | hand | 156.5 | 284.6 | 1.82x | 286.4 | 1.83x | +0.6% | 536 | 536 |
| el/string | generated | hand | 265.9 | 972.8 | 3.66x | 971.4 | 3.65x | -0.1% | 1056 | 1056 |
| el/string | immediate | hand | 265.9 | 699.4 | 2.63x | 665.4 | 2.50x | -4.9% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 20403.4 | 1757.5 | 0.09x | 1639.2 | 0.08x | -6.7% | 1192 | 1192 |
| sql/select20 | generated | hand | 7026.1 | 18311.1 | 2.61x | 18358.9 | 2.61x | +0.3% | 21472 | 21472 |
