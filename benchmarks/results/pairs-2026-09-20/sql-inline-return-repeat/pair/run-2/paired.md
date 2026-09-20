# Paired stand, 2026-09-20 00:55

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 5945643.8 | 171037.5 | 0.03x | 171212.5 | 0.03x | +0.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 881071.9 | 300675.8 | 0.34x | 307036.7 | 0.35x | +2.1% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 616440.6 | 225699.2 | 0.37x | 231180.5 | 0.38x | +2.4% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7026.8 | 18793.1 | 2.67x | 20111.5 | 2.86x | +7.0% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6942.3 | 19277.2 | 2.78x | 20108.2 | 2.90x | +4.3% | 21441 | 21441 |
| sql/select20.scan | generated | control | 376.6 | 383.7 | 1.02x | 391.9 | 1.04x | +2.1% | 0 | 0 |
| tsql/select20.scan | generated | control | 377.7 | 392.8 | 1.04x | 391.2 | 1.04x | -0.4% | 0 | 0 |
| sql/select20.bool | generated | hand | 6906.6 | 18358.1 | 2.66x | 19562.5 | 2.83x | +6.6% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 19772.0 | 1691.8 | 0.09x | 1618.5 | 0.08x | -4.3% | 1192 | 1192 |
| sql/select20 | generated | hand | 6837.3 | 18455.0 | 2.70x | 19570.1 | 2.86x | +6.0% | 21448 | 21448 |
