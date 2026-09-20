# Paired stand, 2026-09-20 01:48

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 76.0 | 79.9 | 1.05x | 82.5 | 1.09x | +3.3% | 192 | 192 |
| tsql/columns1000 | generated | scriptdom | 2739718.8 | 232037.5 | 0.08x | 229587.5 | 0.08x | -1.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 872407.8 | 299853.9 | 0.34x | 306454.7 | 0.35x | +2.2% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 597108.6 | 226600.0 | 0.38x | 224662.5 | 0.38x | -0.9% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7006.3 | 19495.3 | 2.78x | 18479.0 | 2.64x | -5.2% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6854.0 | 18682.3 | 2.73x | 18541.7 | 2.71x | -0.8% | 21440 | 21440 |
| sql/select20.scan | generated | control | 376.6 | 385.8 | 1.02x | 385.3 | 1.02x | -0.1% | 0 | 0 |
| tsql/select20.scan | generated | control | 375.3 | 379.2 | 1.01x | 379.9 | 1.01x | +0.2% | 0 | 0 |
| sql/select20.bool | generated | hand | 6801.9 | 18637.5 | 2.74x | 18258.1 | 2.68x | -2.0% | 21448 | 21392 |
| web/url.full | generated | hand | 155.8 | 283.4 | 1.82x | 272.7 | 1.75x | -3.8% | 536 | 536 |
| el/string | generated | hand | 274.4 | 979.1 | 3.57x | 962.2 | 3.51x | -1.7% | 1056 | 1056 |
| el/string | immediate | hand | 274.4 | 679.0 | 2.47x | 671.6 | 2.45x | -1.1% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 19671.2 | 1621.7 | 0.08x | 1636.9 | 0.08x | +0.9% | 1192 | 1192 |
| sql/select20 | generated | hand | 7782.3 | 19919.3 | 2.56x | 20518.3 | 2.64x | +3.0% | 21448 | 21448 |
