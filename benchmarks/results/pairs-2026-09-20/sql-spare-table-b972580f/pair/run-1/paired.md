# Paired stand, 2026-09-20 01:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 73.7 | 78.8 | 1.07x | 80.8 | 1.10x | +2.5% | 192 | 192 |
| tsql/columns1000 | generated | scriptdom | 2752287.5 | 236150.0 | 0.09x | 226512.5 | 0.08x | -4.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 875780.5 | 337174.2 | 0.38x | 325374.2 | 0.37x | -3.5% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 603317.2 | 226932.0 | 0.38x | 225662.5 | 0.37x | -0.6% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7155.5 | 18488.6 | 2.58x | 18703.1 | 2.61x | +1.2% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6995.4 | 18683.8 | 2.67x | 18627.4 | 2.66x | -0.3% | 21440 | 21440 |
| sql/select20.scan | generated | control | 393.8 | 384.2 | 0.98x | 391.9 | 1.00x | +2.0% | 0 | 0 |
| tsql/select20.scan | generated | control | 385.5 | 394.7 | 1.02x | 404.3 | 1.05x | +2.4% | 0 | 0 |
| sql/select20.bool | generated | hand | 7108.6 | 18503.1 | 2.60x | 18462.8 | 2.60x | -0.2% | 21448 | 21392 |
| web/url.full | generated | hand | 168.0 | 279.6 | 1.66x | 278.0 | 1.66x | -0.6% | 536 | 536 |
| el/string | generated | hand | 285.9 | 969.6 | 3.39x | 1012.2 | 3.54x | +4.4% | 1056 | 1056 |
| el/string | immediate | hand | 285.9 | 666.0 | 2.33x | 663.5 | 2.32x | -0.4% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 19813.4 | 1642.0 | 0.08x | 1606.5 | 0.08x | -2.2% | 1192 | 1192 |
| sql/select20 | generated | hand | 6997.6 | 18668.7 | 2.67x | 18600.5 | 2.66x | -0.4% | 21472 | 21472 |
