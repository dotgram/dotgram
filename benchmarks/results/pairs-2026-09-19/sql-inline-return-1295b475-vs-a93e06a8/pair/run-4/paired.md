# Paired stand, 2026-09-19 21:39

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 77.3 | 87.1 | 1.13x | 86.3 | 1.12x | -0.9% | 192 | 192 |
| tsql/script100 | generated | scriptdom | 667006.2 | 1158206.2 | 1.74x | 1161893.8 | 1.74x | +0.3% | 113600 | 113600 |
| tsql/script400 | generated | scriptdom | 2656878.1 | 17139646.9 | 6.45x | 16911962.5 | 6.37x | -1.3% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1684728.1 | 188967.2 | 0.11x | 176982.8 | 0.11x | -6.3% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 915203.1 | 342258.6 | 0.37x | 313779.7 | 0.34x | -8.3% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 589783.6 | 248771.9 | 0.42x | 219673.4 | 0.37x | -11.7% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7489.6 | 19823.7 | 2.65x | 19500.2 | 2.60x | -1.6% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6931.9 | 18220.5 | 2.63x | 18437.0 | 2.66x | +1.2% | 21440 | 21440 |
| tsql/insert-values.at | generated | scriptdom | 8176.8 | 1504.8 | 0.18x | 1451.4 | 0.18x | -3.5% | 1328 | 1328 |
| sql/select20.scan | generated | control | 373.3 | 374.1 | 1.00x | 374.2 | 1.00x | 0.0% | 0 | 0 |
| tsql/select20.scan | generated | control | 372.0 | 372.0 | 1.00x | 375.3 | 1.01x | +0.9% | 0 | 0 |
| sql/select20.bool | generated | hand | 6702.6 | 18275.1 | 2.73x | 18631.1 | 2.78x | +1.9% | 21448 | 21392 |
| web/url.full | generated | hand | 161.9 | 298.3 | 1.84x | 288.6 | 1.78x | -3.3% | 536 | 536 |
| el/string | generated | hand | 264.4 | 952.4 | 3.60x | 932.8 | 3.53x | -2.0% | 1056 | 1056 |
| el/string | immediate | hand | 264.4 | 666.3 | 2.52x | 661.5 | 2.50x | -0.7% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 19323.0 | 1609.0 | 0.08x | 1570.6 | 0.08x | -2.4% | 1192 | 1192 |
| sql/select20 | generated | hand | 6888.0 | 18782.6 | 2.73x | 18752.7 | 2.72x | -0.2% | 21448 | 21448 |
