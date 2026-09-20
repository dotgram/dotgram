# Paired stand, 2026-09-19 21:36

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.0 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 81.6 | 84.4 | 1.03x | 86.9 | 1.06x | +3.0% | 192 | 192 |
| tsql/script100 | generated | scriptdom | 657425.0 | 1158918.8 | 1.76x | 1166956.2 | 1.78x | +0.7% | 113600 | 113600 |
| tsql/script400 | generated | scriptdom | 2661478.1 | 17383943.8 | 6.53x | 17006862.5 | 6.39x | -2.2% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1729009.4 | 188773.4 | 0.11x | 175664.1 | 0.10x | -6.9% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 859415.6 | 330853.1 | 0.38x | 304858.6 | 0.35x | -7.9% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 595286.7 | 240128.9 | 0.40x | 218688.3 | 0.37x | -8.9% | 296472 | 296472 |
| sql/select20.at | generated | hand | 6974.2 | 19053.7 | 2.73x | 18451.5 | 2.65x | -3.2% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6863.0 | 18344.1 | 2.67x | 18139.7 | 2.64x | -1.1% | 21440 | 21440 |
| tsql/insert-values.at | generated | scriptdom | 8224.4 | 1492.3 | 0.18x | 1449.3 | 0.18x | -2.9% | 1328 | 1328 |
| sql/select20.scan | generated | control | 377.8 | 381.4 | 1.01x | 372.0 | 0.98x | -2.5% | 0 | 0 |
| tsql/select20.scan | generated | control | 374.0 | 378.2 | 1.01x | 382.4 | 1.02x | +1.1% | 0 | 0 |
| sql/select20.bool | generated | hand | 6759.5 | 18216.8 | 2.70x | 18023.6 | 2.67x | -1.1% | 21448 | 21392 |
| web/url.full | generated | hand | 155.5 | 280.3 | 1.80x | 273.8 | 1.76x | -2.3% | 536 | 536 |
| el/string | generated | hand | 268.9 | 966.8 | 3.60x | 960.4 | 3.57x | -0.7% | 1056 | 1056 |
| el/string | immediate | hand | 268.9 | 672.9 | 2.50x | 716.1 | 2.66x | +6.4% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 19868.2 | 1582.7 | 0.08x | 1626.9 | 0.08x | +2.8% | 1192 | 1192 |
| sql/select20 | generated | hand | 6844.8 | 18109.1 | 2.65x | 18168.2 | 2.65x | +0.3% | 21448 | 21448 |
