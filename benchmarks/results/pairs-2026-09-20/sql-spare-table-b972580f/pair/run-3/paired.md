# Paired stand, 2026-09-20 01:47

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 75.1 | 81.5 | 1.09x | 79.5 | 1.06x | -2.5% | 192 | 192 |
| tsql/columns1000 | generated | scriptdom | 2758837.5 | 228912.5 | 0.08x | 218462.5 | 0.08x | -4.6% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 875657.8 | 298831.2 | 0.34x | 302671.1 | 0.35x | +1.3% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 596146.9 | 222753.1 | 0.37x | 224037.5 | 0.38x | +0.6% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7095.4 | 18281.4 | 2.58x | 18284.9 | 2.58x | 0.0% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6884.7 | 18540.0 | 2.69x | 18569.9 | 2.70x | +0.2% | 21440 | 21440 |
| sql/select20.scan | generated | control | 392.3 | 385.5 | 0.98x | 390.1 | 0.99x | +1.2% | 0 | 0 |
| tsql/select20.scan | generated | control | 384.5 | 376.3 | 0.98x | 389.5 | 1.01x | +3.5% | 0 | 0 |
| sql/select20.bool | generated | hand | 6808.3 | 18168.6 | 2.67x | 18235.5 | 2.68x | +0.4% | 21448 | 21392 |
| web/url.full | generated | hand | 158.7 | 273.7 | 1.72x | 280.3 | 1.77x | +2.4% | 536 | 536 |
| el/string | generated | hand | 276.8 | 953.9 | 3.45x | 948.5 | 3.43x | -0.6% | 1056 | 1056 |
| el/string | immediate | hand | 276.8 | 674.1 | 2.44x | 645.8 | 2.33x | -4.2% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 20321.1 | 1687.8 | 0.08x | 1637.5 | 0.08x | -3.0% | 1192 | 1192 |
| sql/select20 | generated | hand | 6888.1 | 18514.0 | 2.69x | 18275.6 | 2.65x | -1.3% | 21472 | 21472 |
