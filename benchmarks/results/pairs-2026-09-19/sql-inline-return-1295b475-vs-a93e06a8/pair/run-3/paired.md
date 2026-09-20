# Paired stand, 2026-09-19 21:38

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.9 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/One.text | generated | hand | 74.7 | 82.0 | 1.10x | 83.2 | 1.11x | +1.4% | 192 | 192 |
| tsql/script100 | generated | scriptdom | 710043.8 | 1291350.0 | 1.82x | 1228418.8 | 1.73x | -4.9% | 113600 | 113600 |
| tsql/script400 | generated | scriptdom | 2736093.8 | 18355437.5 | 6.71x | 17285368.8 | 6.32x | -5.8% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1714201.6 | 187417.2 | 0.11x | 174218.8 | 0.10x | -7.0% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 860062.5 | 328218.8 | 0.38x | 297962.5 | 0.35x | -9.2% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 589003.1 | 243762.5 | 0.41x | 216032.8 | 0.37x | -11.4% | 296472 | 296472 |
| sql/select20.at | generated | hand | 6688.6 | 18074.4 | 2.70x | 17675.3 | 2.64x | -2.2% | 21416 | 21416 |
| sql/select20.window | generated | hand | 6798.3 | 18583.7 | 2.73x | 18160.6 | 2.67x | -2.3% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8140.4 | 1466.8 | 0.18x | 1409.1 | 0.17x | -3.9% | 1328 | 1328 |
| sql/select20.scan | generated | control | 371.0 | 368.1 | 0.99x | 368.6 | 0.99x | +0.1% | 0 | 0 |
| tsql/select20.scan | generated | control | 371.2 | 373.6 | 1.01x | 372.3 | 1.00x | -0.3% | 0 | 0 |
| sql/select20.bool | generated | hand | 7158.4 | 19126.5 | 2.67x | 18331.5 | 2.56x | -4.2% | 21448 | 21392 |
| web/url.full | generated | hand | 154.5 | 298.6 | 1.93x | 273.3 | 1.77x | -8.5% | 536 | 536 |
| el/string | generated | hand | 253.0 | 922.2 | 3.65x | 924.2 | 3.65x | +0.2% | 1056 | 1056 |
| el/string | immediate | hand | 253.0 | 655.9 | 2.59x | 657.6 | 2.60x | +0.3% | 1032 | 1032 |
| tsql/comment | generated | scriptdom | 19467.6 | 1599.6 | 0.08x | 1560.0 | 0.08x | -2.5% | 1192 | 1192 |
| sql/select20 | generated | hand | 6794.6 | 17943.2 | 2.64x | 17671.5 | 2.60x | -1.5% | 21448 | 21448 |
